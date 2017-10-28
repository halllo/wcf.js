using System.Windows;

namespace WCFX.DesktopClient
{
	public partial class App : Application
	{
		public static readonly WcfServiceProvider ServiceProvider = new WcfServiceProvider();

		static App()
		{
		}
	}
}
