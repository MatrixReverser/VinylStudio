using System;
using System.Collections.Generic;
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

namespace VinylStudio.ui
{
    public enum VinylMessageBoxButtons
    {
        OK,
        YES_NO,
        YES_NO_CANCEL
    }

    public enum VinylMessageBoxResult
    {
        OK,
        YES,
        NO,
        CANCEL
    }

    public enum VinylMessageBoxType
    {
        WARNING,
        ERROR,
        QUESTION
    }

    /// <summary>
    /// Interaktionslogik für VinylMessageBox.xaml
    /// </summary>
    public partial class VinylMessageBox : Window
    {
        private static Thickness _buttonMargin = new(5);
        private Button _btnOk = new() { Content = "OK", Margin = _buttonMargin };
        private Button _btnYes = new() { Content = "Yes", Margin = _buttonMargin };
        private Button _btnNo = new() { Content = "No", Margin = _buttonMargin };
        private Button _btnCancel = new() { Content = "Cancel", Margin = _buttonMargin };

        private VinylMessageBoxResult? _result = null;
        private VinylMessageBoxButtons _buttonType;

        /**
         * Constructor of this class
         */
        public VinylMessageBox(Window owner, string caption, string message, VinylMessageBoxType msgType, VinylMessageBoxButtons buttonType)
        {
            InitializeComponent();

            _buttonType = buttonType;

            this.Owner = owner;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.Title = caption;
            textboxCaption.Text = caption;
            textboxMessage.Text = message;

            SetIcon(msgType);
            SetButtons(buttonType);
        }

        /**
         * Shows this dialog and returns the button that has been pressed to close the dialog
         */
        public VinylMessageBoxResult OpenDialog()
        {
            ShowDialog();
            
            return (VinylMessageBoxResult)(_result == null ? VinylMessageBoxResult.CANCEL : _result);
        }

        /**
         * Called if the window is closed
         */
        private void OnWindowClose(object sender, EventArgs e)
        {
            if (_result == null)
            {
                switch(_buttonType)
                {
                    case VinylMessageBoxButtons.OK:
                        _result = VinylMessageBoxResult.OK;
                        break;
                    case VinylMessageBoxButtons.YES_NO:
                        _result = VinylMessageBoxResult.NO;
                        break;
                    case VinylMessageBoxButtons.YES_NO_CANCEL:
                        _result = VinylMessageBoxResult.CANCEL;
                        break;
                }

            }
        }

        /**
         * Sets the buttons for this message box
         */
        private void SetButtons(VinylMessageBoxButtons buttonType)
        {
            // define event handlers for buttons
            _btnOk.Click += (s, e) =>
            {
                _result = VinylMessageBoxResult.OK;
                Close();
            };

            _btnCancel.Click += (s, e) =>
            {
                _result = VinylMessageBoxResult.CANCEL;
                Close();
            };

            _btnYes.Click += (s, e) =>
            {
                _result = VinylMessageBoxResult.YES;
                Close();
            };

            _btnNo.Click += (s, e) =>
            {
                _result = VinylMessageBoxResult.NO;
                Close();
            };

            // add buttons to gui
            if (buttonType == VinylMessageBoxButtons.OK)
            {
                buttonPanel.Children.Add(_btnOk);
            }

            if (buttonType == VinylMessageBoxButtons.YES_NO || buttonType == VinylMessageBoxButtons.YES_NO_CANCEL)
            {
                buttonPanel.Children.Add(_btnYes);
                buttonPanel.Children.Add(_btnNo);
            }

            if (buttonType == VinylMessageBoxButtons.YES_NO_CANCEL)
            {
                buttonPanel.Children.Add(_btnCancel);
            }
        }

        /**
         * Sets the icon for this message box
         */
        private void SetIcon(VinylMessageBoxType msgType)
        { 
            switch (msgType)
            {
                case VinylMessageBoxType.WARNING:
                    iconPlaceholder.Kind = MaterialDesignThemes.Wpf.PackIconKind.Alert;
                    break;
                case VinylMessageBoxType.ERROR:
                    iconPlaceholder.Kind = MaterialDesignThemes.Wpf.PackIconKind.AlertOctagram;
                    break;
                case VinylMessageBoxType.QUESTION:
                    iconPlaceholder.Kind = MaterialDesignThemes.Wpf.PackIconKind.MessageQuestion;
                    break;
            }
        }
    }
}
