using System.Diagnostics;

namespace WCFX.Server
{
	public static class Log
	{
		public static void Info(string text) => Trace.TraceInformation(text);
		public static void NotImportant(string text) => Trace.Write(text, "Debug");
		public static void Emphasized(string text) => Trace.TraceInformation(text);
		public static void Warn(string text) => Trace.TraceWarning(text);
		public static void Error(string text) => Trace.TraceError(text);
		public static void Succes(string text) => Trace.TraceInformation(text);
	}
}