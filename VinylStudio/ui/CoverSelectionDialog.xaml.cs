using discogsharp.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaktionslogik für CoverSelectionDialog.xaml
    /// </summary>
    public partial class CoverSelectionDialog : Window
    {
        private SolidColorBrush BRUSH_ACTIVE = new(Colors.Green);
        private SolidColorBrush BRUSH_INACTIVE = new(Colors.Gray);
        private Thickness BORDER_THICK = new(2);
        private Thickness BORDER_THIN = new(1);

        private ObservableCollection<Release> _releaseList;
        private ObservableCollection<string> _releaseNameList;
        private List<System.Windows.Controls.Image> _imageRef = new();
        private int _selectedImageIndex = -1;
        private string? _selectedImageUrl = null;

        /**
         * Constructor of this class
         */
        public CoverSelectionDialog(UserSettings settings, string interpret, string album)
        {
            InitializeComponent();

            // create a list with refs to the image controls in the ui
            _imageRef.Add(image1);
            _imageRef.Add(image2);
            _imageRef.Add(image3);
            _imageRef.Add(image4);

            // Load release list from discogs
            DiscogsClient client = new(settings.DiscogsToken, interpret, album);
            _releaseList = client.ReleaseList;
            _releaseNameList = client.ReleaseNameList;

            // Fill Combobox with release Names
            comboboxRelease.ItemsSource = _releaseNameList;
        }

        /**
         * Shows this dialog and returns the url of the selected image, when the
         * user has clicked save. Otherwise, null is returned
         */
        public string? OpenDialog()
        {
            ShowDialog();

            return _selectedImageUrl;
        }

        /**
         * Is called if the user selected another release in the combo box
         */
        private void OnReleaseChanged(object sender, RoutedEventArgs e)
        {
            // clear all current images from the ui
            foreach (System.Windows.Controls.Image image in _imageRef)
            {
                image.Source = null;
            }
            _selectedImageIndex = -1;
            PaintImageBorders();

            int index = comboboxRelease.SelectedIndex;
            if (index >= 0)
            {
                Release release = _releaseList[index];
                int imageCount = release.Images.Count;
                if (imageCount > 4)
                {
                    imageCount = 4;
                }

                for (int i=0; i<imageCount; i++)
                {
                    string imageUrl = release.Images[i].ResourceUrl;
                    _imageRef[i].Source = new BitmapImage(new Uri(imageUrl));
                }
            }
        }

        /**
         * Is called if the user selected an image
         */
        private void OnImageSelected(object sender, RoutedEventArgs e)
        {
            for (int i=0; i<_imageRef.Count; i++)
            {
                if (_imageRef[i] == sender)
                {
                    if (_selectedImageIndex == i)
                    {
                        _selectedImageIndex = -1;
                        _selectedImageUrl = null;
                        btnSave.IsEnabled = false;                        
                    }
                    else
                    {
                        _selectedImageIndex = i;
                        _selectedImageUrl = _releaseList[comboboxRelease.SelectedIndex].Images[i].ResourceUrl;
                        btnSave.IsEnabled = true;
                    }
                    break;
                }
            }

            PaintImageBorders();
        }

        /**
         * Paints the borders of the image, depending on the currently selected image
         */
        private void PaintImageBorders()
        {
            for (int i=0; i<_imageRef.Count; i++)
            {
                Border border = (Border)_imageRef[i].Parent;

                if (_selectedImageIndex == i)
                {
                    border.BorderBrush = BRUSH_ACTIVE;
                    border.BorderThickness = BORDER_THICK;
                } else
                {
                    border.BorderBrush = BRUSH_INACTIVE;
                    border.BorderThickness = BORDER_THIN;
                }
            }
        }

        /**
         * Is called if the user clicked on save
         */
        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            Close();    
        }

        /**
         * Is called if the user clicked on cancel
         */
        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            _selectedImageUrl = null;
            Close();
        }
    }
}
