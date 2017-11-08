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
		public string AccessToken { get; private set; }

		static string _tenant = ConfigurationManager.AppSettings["ida:Tenant"];
		static string _clientId = ConfigurationManager.AppSettings["ida:ClientId"];
		static string _audience = ConfigurationManager.AppSettings["ida:Audience"];
		static Uri _redirectUri = new Uri(ConfigurationManager.AppSettings["ida:RedirectUri"]);
		AuthenticationContext authContext = new AuthenticationContext($"https://login.microsoftonline.com/{_tenant}");
		

		public bool CanAktenLaden() => !string.IsNullOrWhiteSpace(AccessToken);
		public void AktenLaden()
		{
			try
			{
				Akten = App.ServiceProvider.Execute<IDossierService, List<DossierDto>>(AccessToken, s => s.GetAll(RunWithFullAccessRights));
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
				AccessToken = result.AccessToken;
				OnPropertyChanged(nameof(AccessToken));
				button.IsEnabled = false;
			}
			catch (AdalException ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
	}
}
