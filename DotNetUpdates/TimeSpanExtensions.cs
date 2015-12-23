using System;

public static class TimeSpanExtensions
{
	public static string ToString(this TimeSpan timeSpan, string format)
	{
		if (format == "mm\\:ss")
			return string.Concat(timeSpan.Minutes.ToString().PadLeft(2, '0'), ":",  timeSpan.Seconds.ToString().PadLeft(2, '0'));
		if (format == @"mm\.ss\:ff")
			return string.Concat(timeSpan.Minutes.ToString().PadLeft(2, '0'), ".", timeSpan.Seconds.ToString().PadLeft(2, '0'), ":", timeSpan.Milliseconds.ToString().PadLeft(2, '0'));

		throw new NotImplementedException();
	}
}
