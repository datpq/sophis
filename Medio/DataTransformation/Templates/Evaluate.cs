using System;
using System.Globalization;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var colVal = args != null && args.Length > 0 ? args[0] : "";
            var lineVal = args != null && args.Length > 1 ? args[1] : "";
            solve(colVal, lineVal);
        } catch (IndexOutOfRangeException e) {
            Console.Out.Write("ERROR: " + e.Message);
        // do not catch FormatException as empty string datetime parsing should be ignored
        //} catch(FormatException e) {
        //    Console.Out.Write("ERROR: " + e.Message);
        } catch (Exception e) {
            Console.Out.Write("WARN: " + e.Message);
        }
    }

    //ExtraCode

    private static void solve(string colVal, string lineVal)
    {
        Console.Out.Write(123456789);
    }
}