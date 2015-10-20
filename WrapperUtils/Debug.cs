using System;
using UDebug = UnityEngine.Debug;

namespace XnaWrapper
{
	public static class Debug
	{
		public static void Log(object message)
		{	
			if (PlatformData.IsEditor)
				UDebug.Log(SafeToString(message) + '\n');
			else
				Console.WriteLine(message);
		}

		public static void Log(string message, object arg0, params object[] args)
		{
			Log(string.Format(message, ConvertArgs(arg0, args)));
		}

		public static void LogT(object message)
		{
			Log(Time() + SafeToString(message));
		}

		public static void LogT(string message, object arg0, params object[] args)
		{
			Log(Time() + string.Format(message, ConvertArgs(arg0, args)));
		}

		public static string[] ConvertArgs(object arg0, params object[] args)
		{
			string[] _args = new string[args.Length + 1];
			for (int i = 0; i < args.Length; ++i)
				_args[i + 1] = SafeToString(args[i]);
			_args[0] = SafeToString(arg0);
			return _args;
		}
		
		private static string SafeToString(object obj)
		{
			return obj == null ? "null" : obj.ToString();
		}

		private static string Time()
		{
			return UnityEngine.Time.time.ToString("F3") + ' ';
		}
	}
}
