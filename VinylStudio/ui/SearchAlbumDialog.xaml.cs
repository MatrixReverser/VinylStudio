using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using VinylStudio.model;
using VinylStudio.util;

namespace VinylStudio.ui
{
    /// <summary>
    /// Interaktionslogik für SearchAlbumDialog.xaml
    /// </summary>
    public partial class SearchAlbumDialog : Window
    {
        private AdvancedSearchFilter _filter;

        public SearchAlbumDialog(AdvancedSearchFilter filter, ObservableCollection<GenreModel> genreList)
        {
            InitializeComponent();

            _filter = filter;

            // fill combo boxes
            comboAlbumType.ItemsSource = Enum.GetValues(typeof(AlbumTypeEnum))
                    .Cast<AlbumTypeEnum>()
                    .Select(e => AlbumTypeTranslator.GetEnumDescription(e))
                    .ToList();
            comboAlbumType.SelectedItem = null;

            comboRating.ItemsSource = Enum.GetValues(typeof(AlbumRatingEnum))
                    .Cast<AlbumRatingEnum>()
                    .Select(e => AlbumRatingTranslator.GetEnumDescription(e))
                    .ToList();
            comboRating.SelectedItem = null;

            comboGenre.ItemsSource = genreList;

            // Fill in the current filter into UI
            FillControlsWithFilterOptions();
        }

        /**
         * Is called if the user pressed a key while AlbumType combobos has the focus
         */
        private void OnClearAlbumType(object sender, KeyEventArgs e)
        {
            e.Handled = false;

            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                e.Handled = true;
                comboAlbumType.SelectedItem = null;
            }
        }

        /**
         * Is called if the user pressed a key while AlbumRating combobos has the focus
         */
        private void OnClearRating(object sender, KeyEventArgs e)
        {
            e.Handled = false;

            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                e.Handled = true;
                comboRating.SelectedItem = null;
            }
        }

        /**
         * Is called if the user pressed a key while Genre combobos has the focus
         */
        private void OnClearGenre(object sender, KeyEventArgs e)
        {
            e.Handled = false;

            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                e.Handled = true;
                comboGenre.SelectedItem = null;
            }
        }

        /**
         * Is called to verify that the input of a year field is a number
         */
        private void OnPreviewNumberInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !AreAllValidNumericChars(e.Text);
        }

        /**
         * Checks if string is a number
         */
        private static bool AreAllValidNumericChars(string str)
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
         * Is called if the user wants to clear all filters
         */
        private void OnClearAllFilters(object sender, EventArgs e)
        {
            comboAlbumType.SelectedItem = null;
            textSongName.Text = string.Empty;
            comboRating.SelectedItem = null;
            textLocation.Text = string.Empty;
            textReleaseFrom.Text = string.Empty;
            textReleaseTo.Text = string.Empty;
            textPurchaseFrom.Text = string.Empty;
            textPurchaseTo.Text = string.Empty;
            textPriceFrom.Text = string.Empty;
            textPriceTo.Text = string.Empty;
            comboGenre.SelectedItem = null;
        }

        /**
         * Is called if the user clicked ok
         */
        private void OnOkClicked(object sender, EventArgs e)
        {
            _filter.AlbumType = comboAlbumType.SelectedItem == null ? null : AlbumTypeTranslator.GetEnumValue((string)comboAlbumType.SelectedItem);
            _filter.SongName = textSongName.Text == string.Empty ? null : textSongName.Text;
            _filter.AlbumRating = comboRating.SelectedItem == null ? null : AlbumRatingTranslator.GetEnumValue((string)comboRating.SelectedItem);
            _filter.Location = textLocation.Text == string.Empty ? null : textLocation.Text;
            _filter.ReleaseFrom = textReleaseFrom.Text == string.Empty ? null : int.Parse(textReleaseFrom.Text);
            _filter.ReleaseTo = textReleaseTo.Text == string.Empty ? null : int.Parse(textReleaseTo.Text);
            _filter.PurchaseFrom = textPurchaseFrom.Text == string.Empty ? null : int.Parse(textPurchaseFrom.Text);
            _filter.PurchaseTo = textPurchaseTo.Text == string.Empty ? null : int.Parse(textPurchaseTo.Text);
            _filter.PriceFrom = textPriceFrom.Text == string.Empty ? null : int.Parse(textPriceFrom.Text);
            _filter.PriceTo = textPriceTo.Text == string.Empty ? null : int.Parse(textPriceTo.Text);
            _filter.Genre = (GenreModel)comboGenre.SelectedItem;

            Close();
        }

        /**
         * Fills the UI controls with the parameters from the current filter
         */
        private void FillControlsWithFilterOptions()
        {
            if (_filter.AlbumType != null)
            {
                comboAlbumType.SelectedItem = AlbumTypeTranslator.GetEnumDescription((AlbumTypeEnum)_filter.AlbumType);
            }
            

            if (_filter.SongName != null)
            {
                textSongName.Text = _filter.SongName;
            }

            if (_filter.AlbumRating != null)
            {
                comboRating.SelectedItem = AlbumRatingTranslator.GetEnumDescription((AlbumRatingEnum)_filter.AlbumRating);
            }

            if (_filter.Location != null)
            {
                textLocation.Text = _filter.Location;
            }

            if (_filter.ReleaseFrom != null)
            {
                textReleaseFrom.Text = _filter.ReleaseFrom.ToString();
            }

            if (_filter.ReleaseTo != null) 
            {
                textReleaseTo.Text = _filter.ReleaseTo.ToString();
            }

            if (_filter.PurchaseFrom != null)
            {
                textPurchaseFrom.Text = _filter.PurchaseFrom.ToString();
            }

            if (_filter.PurchaseTo != null)
            {
                textPurchaseTo.Text = _filter.PurchaseTo.ToString();
            }

            if (_filter.PriceFrom != null)
            {
                textPriceFrom.Text = _filter.PriceFrom.ToString();
            }

            if (_filter.PriceTo != null)
            {
                textPriceTo.Text = _filter.PriceTo.ToString();
            }

            if (_filter.Genre != null)
            {
                comboGenre.SelectedItem = _filter.Genre;
            }
        }
    }
}
