using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using VinylStudio.model;
using VinylStudio.model.legacy;

namespace VinylStudio
{    
    // TODO: Fill Status Line
    // TODO: Add Buttons (Vertical) aside of the song table: Lock (Toggle), Add, Remove, Clear, DiscoGS    
    // Menu entries for organizing Interprets and Genres (shows table with orphan elements and possibility to add, remove interpret / genre with all albums)

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
                MessageBoxResult result = AskForSavingChanges();

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        _dataModel.Save();
                        break;
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
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
                MessageBoxResult result = AskForSavingChanges();

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        _dataModel.Save();
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                }
            }

            // we don't want to ask the user again, when the window is closing
            // therefore we remove the event handler
            this.Closing -= OnWindowClosing; 
            Close();
        }

        /**
         * Asks the user if changes should be saved
         */
        private MessageBoxResult AskForSavingChanges()
        {
            MessageBoxResult result = MessageBox.Show(
                    this,
                    "There are unsaved changes. Do you want to save it to the database?",
                    "Unsaved Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);
            return result;
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
            AlbumEditDialog dlg = new(_dataModel, album)
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
            }
        }

        /**
         * Is called if the user clicks on the AddAlbum button
         */
        private void OnAddAlbum(object sender, EventArgs e)
        {
            AlbumEditDialog dlg = new(_dataModel)
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
                MessageBoxResult result = MessageBox.Show(this, message, "Delete album", MessageBoxButton.YesNo, MessageBoxImage.Question);

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
        }
    }
}
