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
			Log("starting Web at 'https://localhost:8001/wb'", ConsoleColor.White);
			StartWebServer();
			Log($"starting WCF at 'net.tcp://localhost:8000/WCFX/{typeof(IDossierService).FullName}' and 'https://localhost:8001/WCFX/{typeof(IDossierService).FullName}'", ConsoleColor.White);
			StartService<IDossierService, DossierService>();
		}


		private static void StartWebServer()
		{
			var (serverAddress, serverPort) = GetServerConfig();
			var uri = new Uri($"https://{serverAddress}:{serverPort + 1}");
			WebApp.Start(uri.ToString());
		}


		private static void StartService<TInterface, TImplementation>() where TImplementation : TInterface, new()
		{
			var (serverAddress, serverPort) = GetServerConfig();
			var maxReceivedMessageSize = long.Parse(ConfigurationManager.AppSettings["MaxReceivedMessageSize"]);

			WcfService.Host<TImplementation>()
				.AddNetTcpEndpoint<TInterface>(
					address: $"{serverAddress}:{serverPort}/WCFX/{typeof(TInterface).FullName}",
					maxReceivedMessageSize: maxReceivedMessageSize)
				.AddHttpsEndpoint<TInterface>(
					address: $"{serverAddress}:{serverPort + 1}/WCFX/{typeof(TInterface).FullName}",
					isMtomEnabled: false,
					maxReceivedMessageSize: maxReceivedMessageSize)
				.Start();
		}

		private static (string server, int port) GetServerConfig()
		{
			var serverAddress = ConfigurationManager.AppSettings["ServerAddress"];
			var serverPort = Int32.Parse(ConfigurationManager.AppSettings["ServerPort"]);
			return (serverAddress, serverPort);
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
