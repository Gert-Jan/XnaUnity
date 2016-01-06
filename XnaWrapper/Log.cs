using System;
using System.Collections.Generic;

namespace XnaWrapper
{
	public static class Log
	{	
		private static List<string> buffer = new List<string>();

		#region Log Destination

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
		}

		private static void Variant_Write(string message)
		{
			ExternalLogFunction.Invoke(message);
		}

		private static void Variant_ReleaseWrite(string message)
		{
			Console.Write(message);
		}

		private static void Variant_ReleaseWriteLine(string message)
		{
			Console.WriteLine(message);
		}

		#endregion

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
		/// Log (optionally formatted), Buffered
		/// </summary>
		public static void Buffer(params object[] args)
		{
			if (args.Length == 0)
				buffer.Add("");
			else if (args.Length == 1)
				buffer.Add(SafeToString(args[0]));
			else
				buffer.Add(string.Format(SafeToString(args[0]), ConvertExceptFirst(args)));
		}

		/// <summary>
		/// Log (optionally formatted)
		/// </summary>
		public static void Write(params object[] args)
		{
			if (args.Length == 0)
				InternalLogFunction("");
			else if (args.Length == 1)
				InternalLogFunction(SafeToString(args[0]));
			else
				InternalLogFunction(string.Format(SafeToString(args[0]), ConvertExceptFirst(args)));
		}
		
		/// <summary>
		/// Log (optionally formatted), time prepended
		/// </summary>
		public static void WriteT(params object[] args)
		{
			if (args.Length == 0)
				InternalLogFunction(Time());
			else if (args.Length == 1)
				InternalLogFunction(Time() + SafeToString(args[0]));
			else
				InternalLogFunction(Time() + string.Format(SafeToString(args[0]), ConvertExceptFirst(args)));
		}

		private static string[] ConvertExceptFirst(object[] objects)
		{
			string[] strings = new string[objects.Length - 1];
			for (int i = 0; i < strings.Length; ++i)
				strings[i] = SafeToString(objects[i + 1]);
			return strings;
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
