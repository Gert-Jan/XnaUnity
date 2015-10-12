using System;
using UDebug = UnityEngine.Debug;

namespace XnaWrapper
{
	public static class Debug
	{
		public static void Log(object message)
		{
			if (UnityEngine.Application.isEditor)
				UnityEngine.Debug.Log((message != null ? message.ToString() : "null") + '\n');
			else
				Console.WriteLine(message);
		}

		public static void Log(string message, object arg0, params object[] args)
		{
			object[] _args = new object[args.Length + 1];
			Array.Copy(args, 0, _args, 1, args.Length);
			_args[0] = arg0;
			Log(String.Format(message, _args));
		}
	}
}
