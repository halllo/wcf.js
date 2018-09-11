using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using CommandContext;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using WCFX.Common;
using WCFX.Common.Dtos;

namespace WCFX.DesktopClient
{
	public partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();

			this.CommandContext(DataContext = new MainWindowModel());
		}
	}

	public class MainWindowModel : ViewModel
	{
		public bool RunWithFullAccessRights { get; set; }
		public List<DossierDto> Akten { get; set; }
		public string JwtToken => App.ServiceProvider.Token;
		public string SamlJwtToken => TokenStuff.WrapJwt(App.ServiceProvider.Token ?? string.Empty).ToTokenXmlString();

		static string _tenant = ConfigurationManager.AppSettings["ida:Tenant"];
		static string _clientId = ConfigurationManager.AppSettings["ida:ClientId"];
		static string _audience = ConfigurationManager.AppSettings["ida:Audience"];
		static Uri _redirectUri = new Uri(ConfigurationManager.AppSettings["ida:RedirectUri"]);
		AuthenticationContext authContext = new AuthenticationContext($"https://login.microsoftonline.com/{_tenant}");


		public bool CanAktenLaden() => !string.IsNullOrWhiteSpace(App.ServiceProvider.Token);
		public void AktenLaden()
		{
			try
			{
				Akten = App.ServiceProvider.Execute<IDossierService, List<DossierDto>>(s => s.GetAll(RunWithFullAccessRights));
				OnPropertyChanged(nameof(Akten));
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString());
			}
		}

		public async void SignIn(Button button)
		{
			try
			{
				var result = await authContext.AcquireTokenAsync(_audience, _clientId, _redirectUri, new PlatformParameters(PromptBehavior.Auto));
				button.Content = $"Hallo {result.UserInfo.GivenName}";
				button.IsEnabled = false;

				App.ServiceProvider.Token = result.AccessToken;
				OnPropertyChanged(nameof(JwtToken));
				OnPropertyChanged(nameof(SamlJwtToken));
			}
			catch (AdalException ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
	}
}
