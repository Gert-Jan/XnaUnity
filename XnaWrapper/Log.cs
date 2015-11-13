using System;
using System.Collections.Generic;

namespace XnaWrapper
{
	public static class Log
	{
		private static List<string> buffer = new List<string>();

		private static void Internal_Write(string message)
		{
			if (PlatformInstances.IsEditor) // must query unity directly to allow this being used from editor scripts
				UnityEngine.Debug.Log(message + '\n');
			else
#if U_FUZE
				Console.Write(message);
#else
				Console.WriteLine(message);
#endif
		}

		/// <summary>
		/// Log, buffered
		/// </summary>
		public static void Buffer(object message)
		{
			buffer.Add(SafeToString(message));
        }

		/// <summary>
		/// Log formatted, buffered
		/// </summary>
		public static void Buffer(string message, object arg0, params object[] args)
		{
			buffer.Add(string.Format(message, ConvertArgs(arg0, args)));
		}

		/// <summary>
		/// Flush any buffered logs
		/// </summary>
		public static void Flush()
		{
			if (buffer.Count > 0)
				Internal_Write(string.Join("", buffer.ToArray()));
			buffer.Clear();
        }

		/// <summary>
		/// Flush any buffered logs
		/// </summary>
		public static void FlushT()
		{
			if (buffer.Count > 0)
				Internal_Write(Time() + " " + string.Join("", buffer.ToArray()));
			buffer.Clear();
		}

		/// <summary>
		/// Log
		/// </summary>
		public static void Write(object message)
		{
			Internal_Write(SafeToString(message));
		}

		/// <summary>
		/// Log formatted
		/// </summary>
		public static void Write(string message, object arg0, params object[] args)
		{
			Internal_Write(string.Format(message, ConvertArgs(arg0, args)));
		}
		
		/// <summary>
		/// Log, time prepended
		/// </summary>
		public static void WriteT(object message)
		{
			Internal_Write(Time() + SafeToString(message));
		}

		/// <summary>
		/// Log formatted, time prepended
		/// </summary>
		public static void WriteT(string message, object arg0, params object[] args)
		{
			Internal_Write(Time() + string.Format(message, ConvertArgs(arg0, args)));
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
