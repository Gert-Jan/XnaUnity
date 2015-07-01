using UnityEngine;
using System;

public static class Print
{
	public static void Log(object message)
	{
		if (Application.isEditor)
			UnityEngine.Debug.Log(message.ToString());
		else
			Console.WriteLine(message.ToString());
	}

	public static void Log(object message, UnityEngine.Object context)
	{
		if (Application.isEditor)
			UnityEngine.Debug.Log(message.ToString(), context);
		else
			Console.WriteLine(message.ToString());
	}

	public static void LogError(object message)
	{
		if (Application.isEditor)
			UnityEngine.Debug.LogError(message.ToString());
		else
			Console.WriteLine(message.ToString());
	}

	public static void LogError(object message, UnityEngine.Object context)
	{
		if (Application.isEditor)
			UnityEngine.Debug.LogError(message.ToString(), context);
		else
			Console.WriteLine(message.ToString());
	}

	public static void LogWarning(object message)
	{
		if (Application.isEditor)
			UnityEngine.Debug.LogWarning(message.ToString());
		else
			Console.WriteLine(message.ToString());
	}

	public static void LogWarning(object message, UnityEngine.Object context)
	{
		if (Application.isEditor)
			UnityEngine.Debug.LogWarning(message.ToString(), context);
		else
			Console.WriteLine(message.ToString());
	}
}