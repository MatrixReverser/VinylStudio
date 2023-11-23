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
using System.IO;
using VinylStudio.ui;
using System.Net;
using System.Net.Http;
using VinylStudio.util;

namespace VinylStudio
{
    /// <summary>
    /// Interaktionslogik für AlbumEditDialog.xaml
    /// </summary>
    public partial class AlbumEditDialog : Window
    {
        private const string TEMP_COVER_FILENAME = "temporaryCover.jpg";

        private DataModel _dataModel;
        private string? _imagePath = null;
        private AlbumModel? _albumModel = null;
        private UserSettings _userSettings;

        /**
         * Constructor of this class. Used when an album should be edited instead of being created
         */
        internal AlbumEditDialog(UserSettings settings, DataModel dataModel, AlbumModel album) : this(settings, dataModel)
        {
            _albumModel = album;
            
            // set data into the form
            _imagePath = album.ImagePath;
            coverImage.Source = album.ImageSource;
            textboxTitle.Text = album.Name;
            comboboxInterpret.SelectedItem = album.Interpret;
            comboboxGenre.SelectedItem = album.Genre;
            comboboxType.SelectedItem = AlbumTypeTranslator.GetEnumDescription(album.AlbumType);
            comboboxRating.SelectedItem = AlbumRatingTranslator.GetEnumDescription(album.AlbumRating);
            textReleased.Text = album.ReleaseYear.ToString();
            textPurchased.Text = album.PurchaseYear.ToString();
            textPrice.Text = album.Price.ToString();
            textLocation.Text = album.Location;
        }

        /**
         * Constructor of this class
         */
        internal AlbumEditDialog(UserSettings settings, DataModel dataModel)
        {
            InitializeComponent();

            _dataModel = dataModel;
            _userSettings = settings;

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
            bool isInEditMode = (_albumModel != null);

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
            } catch (Exception)
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
            } catch (Exception)
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
            } catch (Exception)
            {
                price = 0.0;
            }

            string location = textLocation.Text.Trim();

            if (_imagePath == null)
            {
                ShowInvalidAlbumDataMsgBox("Cover image has not been set");
                return;
            }

            // create a new album if we are not in edit mode, otherwise use the
            // existing album. Fill in the data
            if (!isInEditMode)
            {
                _albumModel = new();
            }

            if (_albumModel != null)
            {
                _albumModel.Name = title;
                _albumModel.Interpret = interpret;
                _albumModel.Genre = genre;
                _albumModel.AlbumType = albumType;
                _albumModel.AlbumRating = albumRating;
                _albumModel.ReleaseYear = releaseYear;
                _albumModel.PurchaseYear = purchaseYear;
                _albumModel.Price = price;
                _albumModel.Location = location;
            }

            // copy _imagePath to the local database - delete old image before, if it exists
            Assembly? currentAssembly = Assembly.GetEntryAssembly();
            string? appPath = currentAssembly == null ? "" : System.IO.Path.GetDirectoryName(currentAssembly.Location);
            string targetPath = appPath + "/" + DataModel.DIR_THUMBNAIL + "/" + _albumModel?.Id + ".jpg";

            if (_imagePath != targetPath)
            {
                CopyCover(_imagePath, targetPath);
                _albumModel?.FireImageChangedEvents();
            }

            // append album to the database if not in edit mode
            if (!isInEditMode && _albumModel != null)
            {
                _dataModel.AlbumList.Add(_albumModel);
            }

            // close windows
            Close();
        }

        /**
         * Shows this dialog and returns the newly created album or null if the user aborted the dialog
         */
        internal AlbumModel? OpenDialog()
        {
            ShowDialog();

            return _albumModel;
        }

        /**
         * Displays an error message that some data are missing or wrong
         */
        private void ShowInvalidAlbumDataMsgBox(string message)
        {
            VinylMessageBox msgBox = new(
                this,
                "Invalid Album Data",
                message,
                VinylMessageBoxType.ERROR,
                VinylMessageBoxButtons.OK);
            msgBox.OpenDialog();
        }

        /**
         * Copies the cover image from oldName to newName
         */
        private void CopyCover(string oldName, string newName)
        {           
            if (File.Exists(newName))
            {
                File.Delete(newName);
            }

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

        /**
         * Is called if the user wants to query a cover image from discogs
         */
        private void OnQueryCover(object? sender, EventArgs e)
        {
            // Get interpret name and title of the album
            InterpretModel? interpret = comboboxInterpret.SelectedItem as InterpretModel;
            if (interpret == null)
            {
                return;
            }

            string title = textboxTitle.Text.Trim();
            if (title == string.Empty)
            {
                return;
            }

            // open the dialog and check if the user has selected a cover image
            CoverSelectionDialog dialog = new(_userSettings, interpret.Name, title)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            string? imageUrl = dialog.OpenDialog();

            // when we have a new image url, download it and save it as temporary file
            if (imageUrl != null)
            {                
                DownloadImage(imageUrl, TEMP_COVER_FILENAME);                
            }
        }

        /**
         * Downloads a cover image from the web ans stores it as the new cover
         */
        private async void DownloadImage(string url, string temporaryPath)
        {            
            _imagePath = Directory.GetCurrentDirectory() + "/" + temporaryPath;

            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    // Lade das Bild herunter
                    byte[] data = await httpClient.GetByteArrayAsync(url);

                    // Erstelle einen BitmapImage aus den heruntergeladenen Daten
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = new MemoryStream(data);
                    bitmapImage.EndInit();

                    // Speichere das Bild als Datei (optional)
                    SaveImageToFile(bitmapImage, _imagePath);

                    // show the new image in the dialog
                    coverImage.Source = LoadBitmapImage(_imagePath); // new BitmapImage(new Uri(_imagePath));
                }
                catch (Exception ex)
                {                    
                    VinylMessageBox msgBox = new(
                        this,
                        "Error Retrieving Cover Image",
                        $"Error while retrieving cover image form Discogs: {ex.Message}",
                        VinylMessageBoxType.ERROR,
                        VinylMessageBoxButtons.OK);
                    msgBox.OpenDialog();
                }
            }
        }

        /**
         * Saves an image in memory to a file
         */
        private void SaveImageToFile(BitmapImage image, string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                BitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(fileStream);
            }
        }

        /**
         * Loads an image without blocking the file on the filesystem
         */
        private BitmapImage LoadBitmapImage(string path)
        {
            BitmapImage image = new BitmapImage();
            var stream = File.OpenRead(path);
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();
            stream.Close();
            stream.Dispose();

            return image;
        }
    }
}
