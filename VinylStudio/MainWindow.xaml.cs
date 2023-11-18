using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using VinylStudio.model;
using VinylStudio.model.legacy;
using VinylStudio.ui;
using VinylStudio.util;

namespace VinylStudio
{
    // TODO: Export functions for excel
    
    public enum SortingEnum
    {
        NONE,
        RANDOM,
        ALBUM,
        GENRE,
        RATING,
        LENGTH,
        PRICE,
        RELEASED,
        PURCHASED
    }

    public static class SortingEnumTranslator
    {
        public static string GetEnumTranslation(SortingEnum sortingEnum)
        {
            switch(sortingEnum)
            {
                case SortingEnum.NONE: return "none";
                case SortingEnum.ALBUM: return "by album";
                case SortingEnum.GENRE: return "by genre";
                case SortingEnum.RATING: return "by rating";
                case SortingEnum.LENGTH: return "by length";
                case SortingEnum.PRICE: return "by price";
                case SortingEnum.RANDOM: return "random";
                case SortingEnum.RELEASED: return "by release date";
                case SortingEnum.PURCHASED: return "by purchase date";
                default: return "none";
            }
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UserSettings _userSettings;
        private DataModel _dataModel;
        private CollectionView? _thumbnailView;
        private CollectionView? _interpretView;
        private CollectionView? _genreView;

        public MainWindow()
        {
            InitializeComponent();    
            _dataModel = new DataModel();
            _dataModel.Load();

            ConnectDataModel();
            InitViews();
            
            // if there is at least one album in the list, select this album
            if (_dataModel.AlbumList.Count > 0)
            {
                AlbumModel album = _dataModel.AlbumList[0];
                detailPanel.DataContext = album;
                songTable.ItemsSource = album.Songs;
            }

            this.Closing += OnWindowClosing;
            detailPanel.DataContextChanged += (sender, e) =>
            {
                buttonDeleteAlbum.IsEnabled = (detailPanel.DataContext != null);
            };

            // set values for the sorting enum
            comboSorting.ItemsSource = Enum.GetValues(typeof(SortingEnum))
                    .Cast<SortingEnum>()
                    .Select(e => SortingEnumTranslator.GetEnumTranslation(e))
                    .ToList();
            comboSorting.SelectedIndex = 0;

            UpdateStatusLine();

            _userSettings = new();            
        }

        /**
         * Is called if the main window has opened
         */
        private void OnMainWindowOpened(object sender, EventArgs e)
        {
            if (_userSettings.DiscogsToken == null)
            {
                _userSettings.DiscogsToken = AskForDiscogsToken();
                if (_userSettings.DiscogsToken == null)
                {
                    _userSettings.DiscogsToken = string.Empty;
                    return;
                }
            }
            
        }

        /**
         * Initializes the views of the main window for sorting and filtering
         */
        private void InitViews()
        {
            // We add a collection view in order to sort the interprets all the time
            _interpretView = (CollectionView)CollectionViewSource.GetDefaultView(_dataModel.InterpretList);
            _interpretView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            // Although we don't display genres in this window, we sort them for later use (e.g. in AlbumEditDialog)
            _genreView = (CollectionView)CollectionViewSource.GetDefaultView(_dataModel.GenreList);
            _genreView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            // We add a collection view for the thumbnails in order to sort and filter all Thumbnails (triggered by the user via the UI)
            _thumbnailView = (CollectionView)CollectionViewSource.GetDefaultView(_dataModel.AlbumList);
        }

        /**
         * Is called if the window is about to close
         */
        private void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            if (_dataModel.IsModified)
            {
                _dataModel.Save();                
            }
            _userSettings.Save();
        }

        /**
         * Connects the data model to the UI.
         * This has to be done after start of the application or if the
         * complete data model has changed (e.g. after doing an legacy import)
         */
        private void ConnectDataModel()
        {
            thumbnailsContainer.ItemsSource = _dataModel.AlbumList;
            interpretList.ItemsSource = _dataModel.InterpretList;

            InitViews();
        }

        /**
         * Closing the application
         */
        private void OnMenuFileExit(object sender, EventArgs e)
        {            
            if (_dataModel.IsModified)
            {
                _dataModel.Save();
            }
            _userSettings.Save();

            // we don't want to ask the user again, when the window is closing
            // therefore we remove the event handler
            this.Closing -= OnWindowClosing; 
            Close();
        }

        /**
         * Starts a migration form data that exists in the form of the old VinylShelf 0.2 application
         */
        private void OnMenuFileMigrateFromShelf(object sender, EventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Warning!!! Migrating from the legacy system will erase\nyour current database. This proccess is irreversible!!!\nDo you really want to do this?",
                "All data will be erased",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (result == MessageBoxResult.No) 
            {
                return;
            }

            Microsoft.Win32.OpenFileDialog fileDialog = new() { Title = "Import from VinylShelf", Filter="JSON-Files|*.json" };
            if (fileDialog.ShowDialog(this) == true)
            {
                string filename = fileDialog.FileName;
                if (filename != null)
                {
                    LegacyImporter importer = new(filename);
                    DataModel? model = importer.Import();

                    if (model == null)
                    {
                        MessageBox.Show(
                            "Something went wrong. Since migration is a highly risky\n action, I'm not sure what happened. I'm sorry",
                            "Migration failed",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }
                    
                    _dataModel = model;
                    ConnectDataModel();
                }
            }
        }

        /**
         * Is called when the user clicked on a thumbnail to show details in the AlbumContainer
         */ 
        private void OnThumbnailClicked(object sender, MouseButtonEventArgs e)
        {   
            if (sender is Image clickedThumbnail)
            {
                if (clickedThumbnail.DataContext is AlbumModel album)
                {
                    EnableSongTableEditing(false); 
                    detailPanel.DataContext = album;
                    songTable.ItemsSource = album.Songs;                    

                    // when double clicked, open the album in the editor
                    if (e.ClickCount == 2)
                    {
                        EditAlbum(album);                        
                    }
                }
            }
        }

        /**
         * Starts editing the album in the AlbumEditDialog
         */
        private void EditAlbum(AlbumModel album)
        {   
            AlbumEditDialog dlg = new(_userSettings, _dataModel, album)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            dlg.OpenDialog();
            _dataModel.Save();
        }

        /**
         * is called if the user has clicked on an interpret in the interpret list
         */
        private void OnInterpretSelected(object sender, SelectionChangedEventArgs e)
        {            
            FilterThumbnails();            
        }

        /**
         * Is called to filter the interprets after the TextBox for the interpret filter
         * has lost the focus
         */
        private void OnFilterInterprets(object sender, EventArgs e)
        {
            string? filter = interpretFilter.Text.Trim();
            if (filter.Length == 0)
            {
                filter = null;
            }
            SetInterpretFilter(filter);

            SelectFirstAlbumInThumbnailList();
        }

        /**
         * Selects the first album in the thumbnail list. If no album is available in the
         * thumbnail list, then no album is selected
         */
        private void SelectFirstAlbumInThumbnailList()
        {
            // if there is at least one album in the filtered list, we want to select the first album
            CollectionView thumbnailView = (CollectionView)CollectionViewSource.GetDefaultView(_dataModel.AlbumList);

            EnableSongTableEditing(false);
            if (thumbnailView != null && thumbnailView.Count > 0)
            {
                AlbumModel album = (AlbumModel)thumbnailView.GetItemAt(0);
                detailPanel.DataContext = album;
                songTable.ItemsSource = album.Songs;
            }
            else
            {
                detailPanel.DataContext = null;
                songTable.ItemsSource = null;
            }            
        }

        private void OnKeyUpInFilterInterprets(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Keyboard.FocusedElement is UIElement nextControl)
                {
                    nextControl.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
            }
        }
        
        /**
         * Filters the interpret list by a user defined filter string
         */
        private void SetInterpretFilter(string? filter)
        {
            if (_interpretView == null || _thumbnailView == null)
            {
                return;
            }

            if (filter != null)
            {
                _interpretView.Filter = obj =>
                {
                    InterpretModel interpret = (InterpretModel)obj;
                    return interpret.Name.ToLower().Contains(filter.ToLower());
                };

                // Update the thumbnail grid with the currently shown interprets
                List<InterpretModel> interpretList = new();
                foreach (var interpret in _interpretView)
                {
                    interpretList.Add((InterpretModel)interpret);
                }
                FilterThumbnails();
            }
            else
            {
                _interpretView.Filter = null;
                _thumbnailView.Filter = null;
                UpdateStatusLine();
            }
        }

        /**
         * Is called if the user clicks on the AddAlbum button
         */
        private void OnAddAlbum(object sender, EventArgs e)
        {
            AlbumEditDialog dlg = new(_userSettings, _dataModel)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            AlbumModel? album = dlg.OpenDialog();
            
            if (album != null) 
            {
                detailPanel.DataContext = album;
                songTable.ItemsSource = album.Songs;
            }
            _dataModel.Save();
        }

        /**
         * Is called if the user wants to delete the current album
         */
        private void OnDeleteAlbum(object sender, EventArgs e)
        {
            AlbumModel? album = detailPanel.DataContext as AlbumModel;

            if (album != null)
            {
                string message = "Do you really want to delete the album\n\"" + album.Name + "\" from \"" + album.Interpret?.Name + "?";
                MessageBoxResult result = MessageBox.Show(this, message, "Delete album", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // delete album and thumbnail image
                    _dataModel.AlbumList.Remove(album);
                    string thumbnailPath = album.ImagePath;
                    if (File.Exists(thumbnailPath))
                    {
                        File.Delete(thumbnailPath);
                    }

                    // we really have to save the data. Because if the user does not save, the next start will cause
                    // an exception because the album references an image that already has been deleted.
                    _dataModel.Save();

                    // Select next album from the thumbnail panel to display in the detail panel
                    AlbumModel? nextAlbum = null;                    
                    if (_thumbnailView?.Count > 0)
                    {
                        nextAlbum = (AlbumModel)_thumbnailView.GetItemAt(0);
                    }
                    detailPanel.DataContext = nextAlbum;
                    songTable.ItemsSource = nextAlbum?.Songs;

                    // Delete orphan interprets?
                    InterpretModel? interpret = album.Interpret;
                    if (_dataModel.IsOrphan(interpret))
                    {
                        message = "There are no more albums by interpret \"" + interpret + "\".\nDelete interpret?";
                        result = MessageBox.Show(this, message, "Delete album", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes && interpret != null)
                        {
                            _dataModel.InterpretList.Remove(interpret);
                            _dataModel.Save();
                        }
                    }                    
                }
            }            
        }

        /**
         * Is called if the user chose another sorting
         */
        private void OnSortingChanged(object sender, SelectionChangedEventArgs e)
        {
            _thumbnailView?.SortDescriptions.Clear();
            SortDescription description = new SortDescription("Id", ListSortDirection.Ascending);

            switch ((SortingEnum)comboSorting.SelectedIndex)
            {
                case SortingEnum.ALBUM:
                    description = new SortDescription("Name", ListSortDirection.Ascending);
                    break;
                case SortingEnum.GENRE:
                    description = new SortDescription("Genre.Name", ListSortDirection.Ascending);
                    break;
                case SortingEnum.RATING:
                    description = new SortDescription("AlbumRating", ListSortDirection.Ascending);
                    break;
                case SortingEnum.LENGTH:
                    description = new SortDescription("CummulatedLength", ListSortDirection.Ascending);
                    break;
                case SortingEnum.PRICE:
                    description = new SortDescription("Price", ListSortDirection.Ascending);
                    break;
                case SortingEnum.RANDOM:
                    description = new SortDescription("RandomId", ListSortDirection.Ascending);
                    break;
                case SortingEnum.RELEASED:
                    description = new SortDescription("ReleaseYear", ListSortDirection.Ascending);
                    break;
                case SortingEnum.PURCHASED:
                    description = new SortDescription("PurchaseYear", ListSortDirection.Ascending);
                    break;
                default:                    
                    break;
            }

            _thumbnailView?.SortDescriptions.Add(description);
        }

        /**
         * Is called if the album filter has been set
         */
        private void OnAlbumFilterSet(object sender, EventArgs e)
        {            
            FilterThumbnails(); 
        }

        /**
         * Filters the list of albums in the thumbnail panel
         */
        private void FilterThumbnails()
        {
            if (_thumbnailView != null)
            {
                _thumbnailView.Filter += obj =>
                {
                    bool acceptName = false;
                    bool acceptInterpret = false;
                    AlbumModel album = (AlbumModel)obj;
                    string nameFilter = textboxAlbumFilter.Text.Trim().ToLower();

                    // check if name is accepted
                    if (nameFilter == string.Empty)
                    {
                        acceptName = true;
                    } else
                    {
                        acceptName = album.Name.ToLower().Contains(nameFilter);
                    }

                    // check interpret(s) accepted
                    if (interpretList.SelectedItem == null)
                    {
                        // In this case, no interpret is selected, so we filter for
                        // all interprets currently in the interpretView
                        for (int i=0; i<_interpretView?.Count; i++)
                        {
                            InterpretModel interpret = (InterpretModel)_interpretView.GetItemAt(i);
                            if (album.Interpret == interpret)
                            {
                                acceptInterpret = true;
                                break;
                            }
                        }
                    } else
                    {
                        InterpretModel interpret = (InterpretModel)interpretList.SelectedItem;
                        if (album.Interpret == interpret)
                        {
                            acceptInterpret = true;
                        }
                    }

                    return (acceptInterpret && acceptName);
                };
            }

            SelectFirstAlbumInThumbnailList();
            UpdateStatusLine();
        }

        /**
         * Updates the status line with the album data currently visible in the thumbnail panel
         */
        private void UpdateStatusLine()
        {
            double collectionValue = 0.0;
            int totalLength = 0;

            statusInterprets.Content = _interpretView?.Count;

            if (interpretFilter.Text.Trim() == string.Empty && interpretList.SelectedItem == null)
            {
                statusAlbums.Content = _dataModel.AlbumList.Count;
                statusTracks.Content = CountTracks(false);
                collectionValue = CalcValue(false);
                totalLength = CalcLength(false);

            } else
            {
                statusAlbums.Content = _thumbnailView?.Count;
                statusTracks.Content = CountTracks(true);
                collectionValue = CalcValue(true);
                totalLength = CalcLength(true);
            }

            CultureInfo userCulture = CultureInfo.CurrentCulture;
            string formattedValue = "$" + collectionValue.ToString("N2", userCulture);
            statusValue.Content = formattedValue;

            TimeSpan timeSpan = TimeSpan.FromSeconds(totalLength);
            formattedValue = $"{timeSpan.Days}d {timeSpan.Hours:D2}h {timeSpan.Minutes:D2}m {timeSpan.Seconds:D2}s";
            statusPlayTime.Content = formattedValue;
        }

        /**
         * Returns the number of tracks of all albums in the datamodel or
         * of the thumbnailview (if useViewInsteadOfModel == true)
         */
        private int CountTracks(bool useViewInsteadOfModel)
        {
            List<AlbumModel> albums = CollectVisibleAlbums(useViewInsteadOfModel);
            int trackCount = 0;

            // count tracks
            foreach (AlbumModel album in albums)
            {
                trackCount += album.Songs.Count;
            }

            return trackCount;
        }

        /**
         * Calculates the value of the albums of the datamodel or from the thumbnail
         * view (is useViewInsteadOfModel == true)
         */
        private double CalcValue(bool useViewInsteadOfModel)
        {
            List<AlbumModel> albums = CollectVisibleAlbums(useViewInsteadOfModel);
            double value = 0.0;

            // add prices
            foreach (AlbumModel album in albums)
            {
                value += album.Price;
            }

            return value;
        }

        /**
         * Calculates the length of all albums of the datamodel or from the thumbnail
         * view (if useViewInsteadOfModel == true)
         */
        private int CalcLength(bool useViewInsteadOfModel)
        {
            List<AlbumModel> albums = CollectVisibleAlbums(useViewInsteadOfModel);
            int length = 0;

            // add lengths
            foreach (AlbumModel album in albums)
            {
                length += album.CummulatedLength;
            }

            return length;
        }

        /**
         * Returns a list with the albums of the datamodel or from the thumbnail
         * view (if useViewInsteadOfModel == true)
         */
        private List<AlbumModel> CollectVisibleAlbums(bool useViewInsteadOfModel)
        {
            List<AlbumModel> albums = new();

            if (useViewInsteadOfModel)
            {
                for (int i = 0; i < _thumbnailView?.Count; i++)
                {
                    albums.Add((AlbumModel)_thumbnailView.GetItemAt(i));
                }
            }
            else
            {
                foreach (AlbumModel album in _dataModel.AlbumList)
                {
                    albums.Add(album);
                }
            }
            return albums;
        }

        /**
         * Deletes the selected tracks in the song grid
         */
        private void OnDeleteSelectedTracks(object? sender, EventArgs e)
        {
            if (songTable.SelectedItems.Count > 0)
            {
                MessageBoxResult result = MessageBox.Show(
                    this,
                    "Do you really want to delete the selected songs?",
                    "Delete selected songs",
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) 
                {
                    return;
                }

                ObservableCollection<SongModel> songList = (ObservableCollection<SongModel>) songTable.ItemsSource;
                List<SongModel> deletionList = new();
                foreach (SongModel song in songTable.SelectedItems)
                {
                    deletionList.Add(song);
                }

                foreach(SongModel song in deletionList)
                {
                    songList.Remove(song);
                }
            }
        }

        /**
         * Is called when the user clicks the "delete all tracks" button
         */
        private void OnDeleteAllTracks(object? sender, EventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                this, 
                "Do you really want to delete all songs of this album?",
                "Delete all songs",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result == MessageBoxResult.No) 
            {
                return;
            }

            ObservableCollection<SongModel> songList = (ObservableCollection<SongModel>)songTable.ItemsSource;
            songList.Clear();
        }

        /**
         * Is called if the user clicks the "Import from Discogs" button
         */
        private void OnQueryDiscoGs(object? sender, EventArgs e)
        {
            if (detailPanel.DataContext != null)
            {
                if (_userSettings.DiscogsToken == null || _userSettings.DiscogsToken == string.Empty)
                {
                    _userSettings.DiscogsToken = AskForDiscogsToken();
                    if (_userSettings.DiscogsToken == null)
                    {
                        _userSettings.DiscogsToken = string.Empty;
                        MessageBox.Show(this, "You cannot browse for track lists without a Discogs Token", "Authentication needed", MessageBoxButton.OK, MessageBoxImage.Error);

                        return;
                    }
                }

                string albumName = ((AlbumModel)detailPanel.DataContext).Name;
                string? interpretName = ((AlbumModel)detailPanel.DataContext).Interpret?.Name;
                if (interpretName != null)
                {
                    DiscogsSelectionDialog dlg = new(_userSettings, interpretName, albumName)
                    {
                        Owner = this,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    List<SongModel>? retrievedSongs = dlg.OpenDialog();

                    if (retrievedSongs != null && retrievedSongs.Count > 0)
                    {
                        AlbumModel album = ((AlbumModel)detailPanel.DataContext);
                        album.Songs.Clear();
                        
                        foreach (SongModel song in retrievedSongs)
                        {
                            album.Songs.Add(song);
                        }

                        _dataModel.Save();
                    }
                }
            }
        }

        /**
         * Asks the user for the Discogs token and returns it as a string. Null is returned if the
         * user cancelled the dialog
         */
        private string? AskForDiscogsToken()
        {
            string? discogsToken = null;

            DiscogsTokenDialog dlg = new()
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            discogsToken = dlg.OpenDialog();

            return discogsToken;
        }

        /**
         * Is called if the user wants to lock the song table
         */
        private void OnSongTableLocked(object? sender, RoutedEventArgs e)
        {
            EnableSongTableEditing(false);
            buttonSongDelete.IsEnabled = false;
            buttonDeleteAllSongs.IsEnabled = false;
            buttonQueryDiscogs.IsEnabled = false;
        }

        /**
         * Is called if the user wants to unlock the song table
         */
        private void OnSongTableUnLocked(object?sender, RoutedEventArgs e)
        {
            EnableSongTableEditing(true);
            buttonSongDelete.IsEnabled = true;
            buttonDeleteAllSongs.IsEnabled = true;
            buttonQueryDiscogs.IsEnabled = true;
        }

        /**
         * Enables / disables the editing of the song table
         */
        private void EnableSongTableEditing(bool enable) 
        {
            toggleButtonSongTable.IsChecked = enable;
            songTable.IsReadOnly = !enable;

            if (!enable)
            {
                _dataModel.Save();
            }
        }

        /**
         * Shows the about dialog
         */
        private void OnShowAbout(object? sender, RoutedEventArgs e)
        {
            AboutDialog dialog = new()
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            dialog.ShowDialog();
        }
    }
}
