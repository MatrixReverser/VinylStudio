using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VinylStudio.ui
{
    /// <summary>
    /// Interaktionslogik für AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();
            projectLink.RequestNavigate += OpenInBrowser;
        }

        private void OpenInBrowser(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c start {e.Uri.AbsoluteUri}",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                VinylMessageBox msgBox = new(
                    this,
                    "Error Opening Web Browser",
                    $"Error while opening the web browser: {ex.Message}",
                    VinylMessageBoxType.ERROR,
                    VinylMessageBoxButtons.OK);
                msgBox.OpenDialog();
            }

            e.Handled = true;
        }
    }
}
