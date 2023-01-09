using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;



namespace SophisETL.Common.GlobalSettings
{

	/// <summary>
	/// Manager class for the Variable String API
	/// (implemented as a Singleton)
	/// Provides StringFiller implementations and Token Repository acces
	/// </summary>
	public class CSxVariableStringManager
	{
		private static CSxVariableStringManager instance = null;
		private CSxVariableStringFiller stringFiller;
		private CSxVariableTokenRepository tokenRepository;

		public static CSxVariableStringManager Instance
		{
			get
			{
				if ( null == instance )
					instance = new CSxVariableStringManager();
				return instance;
			}
		}

		private CSxVariableStringManager()
		{
			tokenRepository = new CSxVariableTokenRepositoryDefault();
			stringFiller = new CSxVariableStringFillerImpl();
			stringFiller.TokenRepository = tokenRepository;
		}

		public CSxVariableStringFiller GetStringFiller()
		{
			return stringFiller;
		}

		public CSxVariableTokenRepository GetTokenRepository()
		{
			return tokenRepository;
		}
	}


	/// <summary>
	/// Definition of a standard token
	/// </summary>
	public interface CSxVariableStringToken
	{
		string Code { get; }
		string Value { get; }
	}

	/// <summary>
	/// Default implementation of a Standard Token (code = value)
	/// </summary>
	public class CSxVariableStringTokenImpl : CSxVariableStringToken
	{
		private string fCode, fVal;

		public CSxVariableStringTokenImpl(string code, string val)
		{
			fCode = code;
			fVal = val;
		}

		#region CSxVariableStringToken Members
		public string Code	{ get { return fCode; } }
		public string Value	{ get { return fVal; } }
		#endregion
	}

	/// <summary>
	/// Specialized Token : current day
	/// </summary>
	public class CSxVariableStringTokenDay : CSxVariableStringToken
	{
		public string Code	{ get { return "day"; } }
		public string Value	{ get { return DateTime.Now.Day.ToString("00"); } }
	}

	/// <summary>
	/// Specialized Token : current month
	/// </summary>
	public class CSxVariableStringTokenMonth : CSxVariableStringToken
	{
		public string Code	{ get { return "month"; } }
		public string Value	{ get { return DateTime.Now.Month.ToString("00"); } }
	}

	/// <summary>
	/// Specialized Token : current year
	/// </summary>
	public class CSxVariableStringTokenYear : CSxVariableStringToken
	{
		public string Code	{ get { return "year"; } }
		public string Value	{ get { return DateTime.Now.Year.ToString("0000"); } }
	}

	/// <summary>
	/// Specialized Token : current hour
	/// </summary>
	public class CSxVariableStringTokenHour : CSxVariableStringToken
	{
		public string Code	{ get { return "hour"; } }
		public string Value	{ get { return DateTime.Now.Hour.ToString("00"); } }
	}

	/// <summary>
	/// Specialized Token : current minute
	/// </summary>
	public class CSxVariableStringTokenMinute : CSxVariableStringToken
	{
		public string Code	{ get { return "minute"; } }
		public string Value	{ get { return DateTime.Now.Minute.ToString("00"); } }
	}

	/// <summary>
	/// Specialized Token : current second
	/// </summary>
	public class CSxVariableStringTokenSecond : CSxVariableStringToken
	{
		public string Code	{ get { return "second"; } }
		public string Value	{ get { return DateTime.Now.Second.ToString("00"); } }
	}



	/// <summary>
	/// Definition of a Token Repository (i.e. the active token container)
	/// </summary>
	public interface CSxVariableTokenRepository
	{
		CSxVariableStringToken GetToken(string code);
		
		string GetTokenValue(string code);
		string GetTokenValue(string code, string deflt);

        void AddToken( CSxVariableStringToken token );
        void RemoveToken(CSxVariableStringToken token);
        void RemoveToken(string code);
	}

	public class CSxVariableTokenRepositoryDefault : CSxVariableTokenRepository
	{
		private IDictionary fTokensDictionary;

		public CSxVariableTokenRepositoryDefault()
		{
			// Initialize the tokens dictionary
			fTokensDictionary = new Hashtable();
			AddToken( new CSxVariableStringTokenYear() );
			AddToken( new CSxVariableStringTokenMonth() );
			AddToken( new CSxVariableStringTokenDay() );
			AddToken( new CSxVariableStringTokenHour() );
			AddToken( new CSxVariableStringTokenMinute() );
			AddToken( new CSxVariableStringTokenSecond() );
			AddToken( new CSxVariableStringTokenImpl("user", Environment.UserName) );
		}

		public void AddToken(CSxVariableStringToken token)
		{
		    if (fTokensDictionary.Contains(token.Code))
		        fTokensDictionary[token.Code] = token;
		    else
		        fTokensDictionary.Add(token.Code, token);
		}

        public void RemoveToken(CSxVariableStringToken token)
        {
            fTokensDictionary.Remove(token.Code);
        }

        public void RemoveToken(string code)
        {
            fTokensDictionary.Remove(code);
        }

		#region CSxVariableTokenRepository Members
		public CSxVariableStringToken GetToken(string code)
		{
			return (CSxVariableStringToken) fTokensDictionary[ code ];
		}

		public string GetTokenValue(string code)
		{
			CSxVariableStringToken token = (CSxVariableStringToken) fTokensDictionary[ code ];
			return token != null ? token.Value : null;
		}

		string CSxVariableTokenRepository.GetTokenValue(string code, string deflt)
		{
			CSxVariableStringToken token = (CSxVariableStringToken) fTokensDictionary[ code ];
			return token != null ? token.Value : deflt;
		}
		#endregion
	}
	

	/// <summary>
	/// Allows quick definition of "Variable" Strings, i.e. Strings with
	/// specific tokens that are replaced at run-time with a specific value.
	/// </summary>
	public interface CSxVariableStringFiller
	{
		string FillVariableString(string variableString);
		CSxVariableTokenRepository TokenRepository { get; set; }
	}


	public class CSxVariableStringFillerImpl :CSxVariableStringFiller
	{
		private const string tokenPattern = "%%(?<token>[a-zA-Z0-9_-]+)%%";
		CSxVariableTokenRepository fTokenRepository;

		#region CSxVariableStringFiller Members
		public string FillVariableString(string variableString)
		{
			// Lookup groups inside the variable string
			Regex tokenRegex = new Regex( tokenPattern );
			return tokenRegex.Replace( variableString, new MatchEvaluator(TokenEvaluator) );
		}

		private string TokenEvaluator(Match currentMatch)
		{
			string token = currentMatch.Groups[ "token" ].Value;
			// get the value of the current Token using the Token Repository
			return fTokenRepository.GetTokenValue( token, "error" );
		}

		public CSxVariableTokenRepository TokenRepository
		{
			get
			{
				return fTokenRepository;
			}
			set
			{
				fTokenRepository = value;
			}
		}

		#endregion
	}
}
