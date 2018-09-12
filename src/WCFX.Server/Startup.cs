using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(WCFX.Server.Startup))]

namespace WCFX.Server
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			app.UseMyWcfServices();
		}
	}
}
