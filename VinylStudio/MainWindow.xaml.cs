using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using VinylStudio.model;
using VinylStudio.model.legacy;

namespace VinylStudio
{   
    // TODO: thumbnail toolbar button: Add -> Adds a new album and opens an editor dialog
    // TODO: thumbnail toolbar button: Remove (active, when album selected): Removes album after security warning. When no more albums for the artist, asks if artist should also be deleted
    // TODO: thumbnail toolbar button: Sorting combo box: {none, Name, Artist, Random}
    // TODO: thumbnail toolbar button: filtering textbox advanced. Interprets expressions like name=xxx or interpret=xxx and genre=rock
    // TODO: Add Buttons (Vertical) aside of the song table: Lock (Toggle), Add, Remove, Clear, DiscoGS    

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataModel _dataModel;
        private CollectionView? _thumbnailView;
        private CollectionView? _interpretView;

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
        }

        /**
         * Initializes the views of the main window for sorting and filtering
         */
        private void InitViews()
        {
            // We add a collection view in order to sort the interprets all the time
            _interpretView = (CollectionView)CollectionViewSource.GetDefaultView(_dataModel.InterpretList);
            _interpretView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

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
                    Window.GetWindow(this),
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
        private void OnThumbnailClicked(object sender, EventArgs e)
        {   
            if (sender is Image clickedThumbnail)
            {
                if (clickedThumbnail.DataContext is AlbumModel album)
                {
                    detailPanel.DataContext = album;
                    songTable.ItemsSource = album.Songs;
                }
            }
        }

        /**
         * is called if the user has clicked on an interpret in the interpret list
         */
        private void OnInterpretSelected(object sender, SelectionChangedEventArgs e)
        {
            InterpretModel? interpret = (InterpretModel)interpretList.SelectedItem;
            SetInterpretFilter(interpret);

            SelectFirstAlbumInThumbnailList();
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
                var nextControl = Keyboard.FocusedElement as UIElement;
                if (nextControl != null)
                {
                    nextControl.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
            }
        }

        /**
         * Filters all thumbnails by an interpret
         */
        private void SetInterpretFilter(InterpretModel? interpret)
        {
            if (_thumbnailView == null)
            {
                return;
            }

            if (interpret != null)
            {
                _thumbnailView.Filter = obj => ((AlbumModel)obj).Interpret == interpret;
            }
            else
            {
                _thumbnailView.Filter = null;
            }
        }
        /**
         * Filters all thumbnails by a list of interprets
         */
        private void SetInterpretFiler(List<InterpretModel> interprets)
        {
            if (_thumbnailView == null)
            {
                return;
            }

            _thumbnailView.Filter = obj =>
            {
                bool showThumbnail = false;

                foreach (InterpretModel interpret in interprets)
                {
                    AlbumModel album = (AlbumModel)obj;
                    if (album.Interpret == interpret)
                    {
                        showThumbnail = true;
                        break;
                    }
                }

                return showThumbnail;
            };
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
                SetInterpretFiler(interpretList);
            }
            else
            {
                _interpretView.Filter = null;
                _thumbnailView.Filter = null;
            }
        }
    }
}
