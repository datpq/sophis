using System;
using System.IO;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using NLog;
using System.Configuration;

namespace DataTransformation
{
    public static class Compiler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static Dictionary<string, string> CacheExpressionExeFiles = new Dictionary<string, string>();
        private static Dictionary<string, string> CacheExpressionValues = new Dictionary<string, string>();
        private static string EvaluateTemplateCode = null;

        public static string Evaluate(string expression, string colVal, string lineVal = "")
        {
            var key = string.Format("expression={0}@colVal={1}@lineVal={2}", expression, colVal, lineVal);
            if (CacheExpressionValues.ContainsKey(key))
            {
                Logger.Debug(string.Format("Return value from Cache: {0}", CacheExpressionValues[key]));
                return CacheExpressionValues[key];
            }
            var keepSourceFile = false;
            bool.TryParse(ConfigurationManager.AppSettings["KeepEvaluationSourceFile"], out keepSourceFile);

            if (!CacheExpressionExeFiles.ContainsKey(expression) || !File.Exists(CacheExpressionExeFiles[expression]))
            {
                var tempPath = Path.GetTempPath();
                var fileName = Guid.NewGuid().ToString();
                var sourceFile = Path.Combine(tempPath, fileName + ".cs");
                String destFile = Path.Combine(tempPath, fileName + ".exe");
                if (EvaluateTemplateCode == null)
                {
                    EvaluateTemplateCode = Utils.ReadAllTextFile("Evaluate.cs"); // read from template
                    Logger.Debug($"Reading code from template: \n{EvaluateTemplateCode}");
                }
                string fileContent = EvaluateTemplateCode.Replace("123456789", expression);
                File.WriteAllText(sourceFile, fileContent);
                CompileExecutable(sourceFile, destFile);

                if (keepSourceFile)
                {
                    Logger.Debug(string.Format("sourceFile: {0}", sourceFile));
                }
                else
                {
                    File.Delete(sourceFile);
                }
                CacheExpressionExeFiles.Add(expression, destFile);
            }
            var exeFile = CacheExpressionExeFiles[expression];

            // Start the child process.
            var p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = exeFile;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.Arguments = string.Format("\"{0}\" \"{1}\"", colVal.Replace("\"", "\\\""), lineVal.Replace("\"", "\\\""));
            if (keepSourceFile)
            {
                Logger.Debug(string.Format("Arguments: {0}", p.StartInfo.Arguments));
            }
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            CacheExpressionValues.Add(key, output);
            return output;
        }

        public static void CleanUp()
        {
            foreach (var exeFile in CacheExpressionExeFiles.Values)
            {
                File.Delete(exeFile);
            }
            CacheExpressionExeFiles.Clear();
        }

        public static void CompileExecutable(string sourceFile, string destFile)
        {
            CodeDomProvider provider = null;

            // Select the code provider based on the input file extension.
            if (Path.GetExtension(sourceFile).ToUpper() == ".CS")
            {
                provider = CodeDomProvider.CreateProvider("CSharp");
            }
            else if (Path.GetExtension(sourceFile).ToUpper() == ".VB")
            {
                provider = CodeDomProvider.CreateProvider("VisualBasic");
            }
            else
            {
                throw new ArgumentException("Source file must have a .cs or .vb extension");
            }

            CompilerParameters cp = new CompilerParameters();

            cp.ReferencedAssemblies.Add("System.dll"); //Regex

            // Generate an executable instead of
            // a class library.
            cp.GenerateExecutable = true;

            // Specify the assembly file name to generate.
            cp.OutputAssembly = destFile;

            // Save the assembly as a physical file.
            cp.GenerateInMemory = false;

            // Set whether to treat all warnings as errors.
            cp.TreatWarningsAsErrors = false;

            // Invoke compilation of the source file.
            CompilerResults cr = provider.CompileAssemblyFromFile(cp, sourceFile);
            if (cr.Errors.Count > 0)
            {
                // Display compilation errors.
                var errorMsg = new StringBuilder();
                foreach (CompilerError ce in cr.Errors)
                {
                    errorMsg.AppendLine(string.Format("  {0}", ce.ToString()));
                }
                throw new ArgumentException(string.Format("Errors building {0} into {1}: {2}", sourceFile, cr.PathToAssembly, errorMsg.ToString()));
            }
        }
    }
}
