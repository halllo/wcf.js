using System;
using System.Configuration;
using Microsoft.Owin.Hosting;
using WCFX.Common;
using WCFX.Server.WCF;

namespace WCFX.Server
{
	public class Program
	{
		[ThreadStatic]
		public static string CurrentUser;

		static Program()
		{
			var (serverAddress, netTcpPort, httpsPort) = GetServerConfig();

			Log($"starting Web at 'https://{serverAddress}:{httpsPort}/wb'", ConsoleColor.White);
			//StartWebServer();
			Log($"starting WCF at 'net.tcp://{serverAddress}:{netTcpPort}/WCFX/{typeof(IDossierService).FullName}' and 'https://{serverAddress}:{httpsPort}/WCFX/{typeof(IDossierService).FullName}'", ConsoleColor.White);
			StartService<IDossierService, DossierService>();
		}


		private static void StartWebServer()
		{
			var (serverAddress, netTcpPort, httpsPort) = GetServerConfig();
			var uri = new Uri($"https://{serverAddress}:{httpsPort}");
			WebApp.Start(uri.ToString());
		}


		private static void StartService<TInterface, TImplementation>() where TImplementation : TInterface, new()
		{
			var (serverAddress, netTcpPort, httpsPort) = GetServerConfig();
			var maxReceivedMessageSize = long.Parse(ConfigurationManager.AppSettings["MaxReceivedMessageSize"]);

			WcfService.Host<TImplementation>()
				.AddNetTcpEndpoint<TInterface>(
					address: $"{serverAddress}:{netTcpPort}/WCFX/{typeof(TInterface).FullName}",
					maxReceivedMessageSize: maxReceivedMessageSize)
				.AddHttpsEndpoint<TInterface>(
					address: $"{serverAddress}:{httpsPort}/WCFX/{typeof(TInterface).FullName}",
					isMtomEnabled: false,
					maxReceivedMessageSize: maxReceivedMessageSize)
				.Start();
		}

		private static (string server, int netTcpPort, int httpsPort) GetServerConfig()
		{
			var serverAddress = ConfigurationManager.AppSettings["ServerAddress"];
			var netTcpPort = Int32.Parse(ConfigurationManager.AppSettings["NetTcpPort"]);
			var httpsPort = Int32.Parse(ConfigurationManager.AppSettings["HttpsPort"]);
			return (serverAddress, netTcpPort, httpsPort);
		}


		public static void Main(string[] args)
		{
			Log("Press Enter to exit.");
			Console.ReadLine();
		}

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
