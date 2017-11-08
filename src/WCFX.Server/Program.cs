using System;
using System.Configuration;
using Microsoft.Owin.Hosting;
using WCFX.Common;
using WCFX.Server.WCF;
using System.IdentityModel.Configuration;
using System.ServiceModel.Description;

namespace WCFX.Server
{
	public class Program
	{
		static Program()
		{
			var (serverAddress, netTcpPort, httpsPort) = GetServerConfig();

			Log($"starting Web at 'https://{serverAddress}:{httpsPort}/wb'", ConsoleColor.White);
			StartWebServer();
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

			var wcfFactory = new WcfService(urlInfix: "WCFX");
			wcfFactory.HostNetTcp<TImplementation, TInterface>(serverAddress, netTcpPort, maxReceivedMessageSize);
			wcfFactory.HostWS2007FederationHttp<TImplementation, TInterface>(serverAddress, httpsPort, maxReceivedMessageSize);
		}

		private static (string server, int netTcpPort, int httpsPort) GetServerConfig()
		{
			var serverAddress = ConfigurationManager.AppSettings["ServerAddress"];
			var netTcpPort = int.Parse(ConfigurationManager.AppSettings["NetTcpPort"]);
			var httpsPort = int.Parse(ConfigurationManager.AppSettings["HttpsPort"]);
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
