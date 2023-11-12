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
using VinylStudio.model;

namespace VinylStudio
{
    /// <summary>
    /// Interaktionslogik für AlbumEditDialog.xaml
    /// </summary>
    public partial class AlbumEditDialog : Window
    {
        public AlbumEditDialog()
        {
            InitializeComponent();

            // initialize combo boxes with enums
            comboboxType.ItemsSource = Enum.GetValues(typeof(AlbumTypeEnum))
                    .Cast<AlbumTypeEnum>()
                    .Select(e => AlbumTypeTranslator.GetEnumDescription(e))
                    .ToList();
            comboboxType.SelectedItem = AlbumTypeTranslator.GetEnumDescription(AlbumTypeEnum.LP);

            comboboxRating.ItemsSource = Enum.GetValues(typeof(AlbumRatingEnum))
                    .Cast<AlbumRatingEnum>()
                    .Select(e => AlbumRatingTranslator.GetEnumDescription(e))
                    .ToList();
            comboboxRating.SelectedIndex = 0;
        }

        /**
         * Is called to verify that the input of a year field is a number
         */
        private void OnPreviewNumberInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !AreAllValidNumericChars(e.Text);
        }

        /**
         * Is called to verify that the input of a textfield is a currency
         */
        private void OnPreviewCurrencyInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !AreAllValidCurrencyChars(e.Text);
        }

        /**
         * Checks if string is a number
         */
        private bool AreAllValidNumericChars(string str)
        {
            foreach (char c in str)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }

        /**
         * Checks if string is a currency
         */
        private bool AreAllValidCurrencyChars(string str)
        {
            foreach (char c in str)
            {
                if (!char.IsDigit(c) && c != '.')
                {
                    return false;
                }
            }
            return true;
        }

    }
}
