using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
using VinylStudio.model.legacy;

namespace VinylStudio
{
    /// <summary>
    /// Interaktionslogik für AlbumEditDialog.xaml
    /// </summary>
    public partial class AlbumEditDialog : Window
    {
        private DataModel _dataModel;
        private string? _imagePath = null;
        private AlbumModel? _albumModel = null;

        internal AlbumEditDialog(DataModel dataModel)
        {
            InitializeComponent();

            _dataModel = dataModel;

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

            // Initialize Combo boxes with dynamic data objects (genre, interpret)
            comboboxGenre.ItemsSource = _dataModel.GenreList;
            comboboxGenre.SelectedIndex = (_dataModel.GenreList.Count > 0 ? 0 : -1);

            comboboxInterpret.ItemsSource = _dataModel.InterpretList;
            comboboxInterpret.SelectedIndex = (_dataModel.InterpretList.Count > 0 ? 0 : -1);
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
         * Checks if string is a currency
         */
        private static bool AreAllValidCurrencyChars(string str)
        {
            foreach (char c in str)
            {
                if (!char.IsDigit(c) && c != '.' && c != ',')
                {
                    return false;
                }
            }
            return true;
        }

        /**
         * Is called if the user wants to add an genre
         */
        private void OnAddGenre(object sender, EventArgs e)
        {
            GenreEditDialog dlg = new()
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            
            GenreModel? genre = dlg.OpenDialog();
            if (genre != null)
            {
                _dataModel.GenreList.Add(genre);
                comboboxGenre.SelectedItem = genre;
            }
        }

        /**
         * Is called if the user wants to add an interpret
         */
        private void OnAddInterpret(object sender, EventArgs e)
        {
            InterpretEditDialog dlg = new() 
            { 
                Owner = this, 
                WindowStartupLocation = WindowStartupLocation.CenterOwner 
            };

            InterpretModel? interpret = dlg.OpenDialog();
            if (interpret != null)
            {
                _dataModel.InterpretList.Add(interpret);
                comboboxInterpret.SelectedItem = interpret;
            }
        }

        /**
         * Is called if the user click the cover image button
         */
        private void OnSetCoverImage(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            openFileDialog.Filter = "Bilddateien|*.jpg;*.jpeg";
            openFileDialog.FilterIndex = 1;

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                _imagePath = openFileDialog.FileName;
                coverImage.Source = new BitmapImage(new Uri(_imagePath));
            }
        }

        /**
         * Is called if the user clicked Cancel
         */
        private void OnCancelClicked(object sender, EventArgs e)
        {
            Close();
        }

        /**
         * Is called if the user clicked Save
         */
        private void OnSaveClicked(object sender, EventArgs e)
        {
            // validate some important data
            string errorMessage = string.Empty;

            string title = textboxTitle.Text.Trim();
            if (title == string.Empty)
            {
                errorMessage += "Title has not been set\n";    
            }

            InterpretModel? interpret = comboboxInterpret.SelectedItem as InterpretModel;
            if (interpret == null)
            {
                errorMessage += "Interpret has not been set\n";
            }

            GenreModel? genre = comboboxGenre.SelectedItem as GenreModel;
            if (genre == null)
            {
                errorMessage += "Genre has not been set\n";
            }

            AlbumTypeEnum albumType = AlbumTypeTranslator.GetEnumValue((string)comboboxType.SelectedItem);
            AlbumRatingEnum albumRating = AlbumRatingTranslator.GetEnumValue((string)comboboxRating.SelectedItem);

            int releaseYear = -1;
            try
            {
                releaseYear = int.Parse(textReleased.Text.Trim());
                if (releaseYear < 1900 || releaseYear > 2200)
                {
                    errorMessage += "Release year '" + releaseYear + "' does not seem valid\n";
                }
            } catch (Exception ex)
            {
                errorMessage += "Release year has not been set\n";
            }
            
            int purchaseYear = -1;
            try
            {
                purchaseYear = int.Parse(textPurchased.Text.Trim());
                if (purchaseYear < 1900 || purchaseYear > 2200)
                {
                    errorMessage += "Purchase year '" + purchaseYear + "' does not seem valid\n";
                }
            } catch (Exception ex)
            {
                errorMessage += "Purchase year has not been set\n";
            }

            if (purchaseYear < releaseYear)
            {
                errorMessage += "Album was purchased before it was released? WTF?\n";
            }
            
            if (errorMessage != string.Empty)
            {
                ShowInvalidAlbumDataMsgBox(errorMessage);
                return;
            }

            double price = -1.0;
            string strPrice = textPrice.Text.Trim();
            CultureInfo userCulture = CultureInfo.CurrentCulture;

            try
            {
                price = double.Parse(strPrice, NumberStyles.Any, userCulture);
            } catch (Exception ex)
            {
                price = 0.0;
            }

            string location = textLocation.Text.Trim();
            
            // create a new album and fill it with the values
            AlbumModel album = new()
            {
                Name = title,
                Interpret = interpret,
                Genre = genre,
                AlbumType = albumType,
                AlbumRating = albumRating,
                ReleaseYear = releaseYear,
                PurchaseYear = purchaseYear,
                Price = price,
                Location = location
            };

            // copy _imagePath to the local database - delete old image before, if it exists
            if (_imagePath == null)
            {
                ShowInvalidAlbumDataMsgBox("Cover image has not been set");
                return;
            }

            Assembly? currentAssembly = Assembly.GetEntryAssembly();
            string? appPath = currentAssembly == null ? "" : System.IO.Path.GetDirectoryName(currentAssembly.Location);
            string targetPath = appPath + "/" + DataModel.DIR_THUMBNAIL + "/" + album.Id + ".jpg";

            CopyCover(_imagePath, targetPath);

            // append album to the database
            _dataModel.AlbumList.Add(album);

            // close windows
            _albumModel = album;
            Close();
        }

        /**
         * Shows this dialog and returns the newly created album or null if the user aborted the dialog
         */
        internal AlbumModel? OpenDialog()
        {
            _albumModel = null;
            ShowDialog();

            return _albumModel;
        }

        /**
         * Displays an error message that some data are missing or wrong
         */
        private void ShowInvalidAlbumDataMsgBox(string message)
        {
            MessageBox.Show(this, message, "Invalid album data", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /**
         * Copies the cover image from oldName to newName
         */
        private void CopyCover(string oldName, string newName)
        {           
            using (var originalImage = System.Drawing.Image.FromFile(oldName))
            {
                using (var scaledImage = new Bitmap(400, 400))
                using (var graphics = Graphics.FromImage(scaledImage))
                {
                    graphics.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, 400, 400));
                    scaledImage.Save(newName, ImageFormat.Jpeg);
                }
            }
        }
    }
}
