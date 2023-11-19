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
using System.Windows.Shapes;
using VinylStudio.util;

namespace VinylStudio.ui
{
    /// <summary>
    /// Interaktionslogik für ErrorDialog.xaml
    /// </summary>
    public partial class ErrorDialog : Window
    {
        // Argg... tricky thing. See ShowDialog() method for explanation
        public static bool isShowing = false;

        /**
         * Constructor of this class
         */
        public ErrorDialog(VinylException exception)
        {
            InitializeComponent();

            labelMessage.Content = exception.Message;

            if (exception.InnerException != null)
            {
                labelInnerMessage.Content = exception.InnerException.Message;
            }

            Title = exception.GetTitle();

            Owner = App.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        /**
         * Okay, this is kinda tricky: This dialog must not be shown more than once. Therefore we
         * have a static flag isShown. When the dialog is active, this flag is set to true. If 
         * another thread tries to open this dialog, it won't open, because... yeah.. because it's
         * already showing.
         * Why the hell would different threads open this dialog? The answer is the Discogs API.
         * The request to Discogs are done asynchronously and if the user is fast and clicks
         * several times on Query... and immediatly closes the dialog and does the same, it might
         * happen that he reaches the Discogs limit of 60 request per minute (remember that there
         * is a built in limit for 10 requests in VinylStudio, but if he clicks 7 times on the
         * request button (why ever he would do that), and the requested album has many releases
         * on Discogs.... yes, then the limit is reached.
         */
        public new bool? ShowDialog()
        {
            if (isShowing)
            {
                return null;
            }

            isShowing = true;

            bool? result = base.ShowDialog();
            isShowing = false;
            return result;
        }

        /**
         * Is called if the user clicked the ok button
         */
        private void OnOkClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
