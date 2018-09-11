using System;

namespace WCFX.Server
{
	public static class Logger
	{
		public static void Log(string text, ConsoleColor? color = null)
		{
			if (color != null)
			{
				var colorVorher = Console.ForegroundColor;
				Console.ForegroundColor = color.Value;
				Console.WriteLine(text);
				Console.ForegroundColor = colorVorher;
			}
			else
			{
				Console.WriteLine(text);
			}
		}
	}
}