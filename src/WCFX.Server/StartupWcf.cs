using System.Configuration;
using Owin;
using WCFX.Common;
using WCFX.Server.wcf;

namespace WCFX.Server
{
	public static class StartupWcf
	{
		public static void UseMyWcfServices(this IAppBuilder app)
		{
			StartService<IDossierService, DossierService>();
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
	}
}
