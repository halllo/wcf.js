using CommandContext;
using System.Collections.Generic;
using System.Windows;
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

		public void AktenLaden()
		{
			try
			{
				Akten = App.ServiceProvider.Execute<IDossierService, List<DossierDto>>(s => s.GetAll(RunWithFullAccessRights));
				OnPropertyChanged(nameof(Akten));
			}
			catch (System.Exception e)
			{
				MessageBox.Show(e.ToString());
			}
		}
	}
}
