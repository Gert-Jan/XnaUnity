using System;
using System.Collections.Generic;

namespace XnaWrapper
{
	public static class Log
	{	
		private static List<string> buffer = new List<string>();

		public static Action<string> ExternalLogFunction = null;
		private static Action<string> InternalLogFunction = Variant_Chooser;
		
		private static void Variant_Chooser(string message)
		{
			if (UnityEngine.Application.isEditor)
				InternalLogFunction = Variant_Editor;
			else
			{
#if U_FUZE
				if (UnityEngine.Debug.isDebugBuild)
					InternalLogFunction = Variant_Write;
				else
					InternalLogFunction = Variant_ReleaseWrite;
#else
				if (UnityEngine.Debug.isDebugBuild)
					InternalLogFunction = Variant_WriteLine;
				else
					InternalLogFunction = Variant_ReleaseWriteLine;
#endif
			}
		}

		private static void Variant_Editor(string message)
		{
			UnityEngine.Debug.Log(message + '\n');
		}

		private static void Variant_WriteLine(string message)
		{
			ExternalLogFunction.Invoke(message);
            Console.WriteLine(message);
		}

		private static void Variant_Write(string message)
		{
			ExternalLogFunction.Invoke(message);
			Console.Write(message);
		}

		private static void Variant_ReleaseWrite(string message)
		{
			Console.Write(message);
		}

		private static void Variant_ReleaseWriteLine(string message)
		{
			Console.WriteLine(message);
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
				InternalLogFunction(string.Join("", buffer.ToArray()));
			buffer.Clear();
        }

		/// <summary>
		/// Flush any buffered logs
		/// </summary>
		public static void FlushT()
		{
			if (buffer.Count > 0)
				InternalLogFunction(Time() + " " + string.Join("", buffer.ToArray()));
			buffer.Clear();
		}

		/// <summary>
		/// Log
		/// </summary>
		public static void Write(object message)
		{
			InternalLogFunction(SafeToString(message));
		}

		/// <summary>
		/// Log formatted
		/// </summary>
		public static void Write(string message, object arg0, params object[] args)
		{
			InternalLogFunction(string.Format(message, ConvertArgs(arg0, args)));
		}
		
		/// <summary>
		/// Log, time prepended
		/// </summary>
		public static void WriteT(object message)
		{
			InternalLogFunction(Time() + SafeToString(message));
		}

		/// <summary>
		/// Log formatted, time prepended
		/// </summary>
		public static void WriteT(string message, object arg0, params object[] args)
		{
			InternalLogFunction(Time() + string.Format(message, ConvertArgs(arg0, args)));
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
