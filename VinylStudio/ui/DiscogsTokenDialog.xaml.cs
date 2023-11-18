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

namespace VinylStudio
{
    /// <summary>
    /// Interaktionslogik für DiscogsTokenDialog.xaml
    /// </summary>
    public partial class DiscogsTokenDialog : Window
    {
        private string? _token = null;

        /**
         * Constructor of this class
         */
        public DiscogsTokenDialog()
        {
            InitializeComponent();

            discogsLink.RequestNavigate += OpenDiscogsInBrowser;
        }

        private void OpenDiscogsInBrowser(object sender, RequestNavigateEventArgs e)
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
                MessageBox.Show($"Error while opening the web browser: {ex.Message}");
            }

            e.Handled = true;
        }

        /**
         * Is called if the user clicked on Save
         */
        private void OnSaveClicked(object sender, EventArgs e)
        {
            if (textboxToken.Text.Trim().Length > 0)
            {
                _token = textboxToken.Text;
            }
            Close();
        }

        /**
         * Is called if the user clicked on cancel
         */
        private void OnCancelClicked(object sender, EventArgs e)
        {
            Close();
        }

        public string? OpenDialog()
        {
            ShowDialog();
            return _token;
        }
    }
}
