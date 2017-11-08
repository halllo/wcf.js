using System.Windows;

namespace WCFX.DesktopClient
{
	public partial class App : Application
	{
		public static readonly WcfClient ServiceProvider = new WcfClient();

		static App()
		{
		}
	}
}
