using System;
using System.Collections.Generic;
using UDebug = UnityEngine.Debug;

namespace XnaWrapper
{
	public static class Debug
	{
		private static List<string> buffer = new List<string>();

		private static void Log_internal(string message)
		{
			if (UnityEngine.Application.isEditor) // must query unity directly to allow this being used from editor scripts
				UDebug.Log(message + '\n');
			else
			{
				switch (PlatformData.ActivePlatform)
				{
					case PlatformID.XboxOne:
						Console.WriteLine(message);
						break;
					default:
						Console.Write(message);
						break;
				}

			}
		}
		
		/// <summary>
		/// Log, buffered
		/// </summary>
		public static void LogB(object message)
		{
			buffer.Add(SafeToString(message));
        }

		/// <summary>
		/// Log formatted, buffered
		/// </summary>
		public static void LogB(string message, object arg0, params object[] args)
		{
			buffer.Add(string.Format(message, ConvertArgs(arg0, args)));
		}

		/// <summary>
		/// Flush any buffered logs
		/// </summary>
		public static void LogF()
		{
			if (buffer.Count > 0)
				Log_internal(Time() + " " + string.Join("", buffer.ToArray()));
			buffer.Clear();
        }

		/// <summary>
		/// Log
		/// </summary>
		public static void Log(object message)
		{
			Log_internal(SafeToString(message));
		}

		/// <summary>
		/// Log formatted
		/// </summary>
		public static void Log(string message, object arg0, params object[] args)
		{
			Log_internal(string.Format(message, ConvertArgs(arg0, args)));
		}
		
		/// <summary>
		/// Log, time prepended
		/// </summary>
		public static void LogT(object message)
		{
			Log_internal(Time() + SafeToString(message));
		}

		/// <summary>
		/// Log formatted, time prepended
		/// </summary>
		public static void LogT(string message, object arg0, params object[] args)
		{
			Log_internal(Time() + string.Format(message, ConvertArgs(arg0, args)));
		}

		private static string[] ConvertArgs(object arg0, params object[] args)
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
