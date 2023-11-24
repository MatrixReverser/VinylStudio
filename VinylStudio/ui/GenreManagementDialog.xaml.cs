using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using VinylStudio.model;

namespace VinylStudio.ui
{
    public class UIGenreModel
    {
        public bool IsUsed { get; set; }
        public GenreModel Model { get; set; }

        public UIGenreModel() 
        {
            IsUsed = false;
            Model = new GenreModel();   
        }
    }

    /// <summary>
    /// Interaktionslogik für GenreManagementDialog.xaml
    /// </summary>
    public partial class GenreManagementDialog : Window
    {
        private ObservableCollection<UIGenreModel> _data = new();
        private DataModel _mainModel;

        internal GenreManagementDialog(DataModel mainModel)
        {
            InitializeComponent();

            _mainModel = mainModel;
            InitializeDataModel(mainModel);            

            _data.CollectionChanged += (sender, e) => {
                if (e.OldItems != null)
                {
                    foreach (UIGenreModel obj in e.OldItems)
                    {
                        _mainModel.GenreList.Remove(obj.Model);
                    }
                }
                if (e.NewItems != null)
                {
                    foreach (UIGenreModel obj in e.NewItems)
                    {
                        _mainModel.GenreList.Add(obj.Model);
                    }
                }
            };
        }

        /**
         * Initializes the data model for this dialog.
         */
        private void InitializeDataModel(DataModel mainModel)
        {
            ObservableCollection<GenreModel> genreList = mainModel.GenreList;

            foreach (GenreModel model in genreList)
            {
                UIGenreModel record = new()
                {
                    Model = model,
                    IsUsed = _mainModel.IsGenreUsed(model)
                };
                _data.Add(record);
            }

            genreTable.ItemsSource = _data;
        }

        /**
         * Is called if the user clicked the OK button
         */
        private void OnOkClicked(object sender, EventArgs e)
        {
            Close();
        }

        /**
         * Is called if the user wants to delete the unused genres
         */
        private void OnDeleteUnused(object sender, EventArgs e)
        {
            List<UIGenreModel> deletionList = GetUnusedGenres();
            string message = "Do you really want to delete the following genres: \n";

            if (deletionList.Count > 0)
            {
                DeleteGenres(deletionList, message);
            }
        }

        /**
         * Deletes all genres in the deletion list after a security question
         */
        private void DeleteGenres(List<UIGenreModel> deletionList, string message)
        {
            foreach (UIGenreModel uiGenre in deletionList)
            {
                message += uiGenre.Model.Name + "\n";
            }

            VinylMessageBox msgBox = new VinylMessageBox(this, "Delete Unused Genres", message, VinylMessageBoxType.QUESTION, VinylMessageBoxButtons.YES_NO);
            VinylMessageBoxResult answer = msgBox.OpenDialog();

            if (answer == VinylMessageBoxResult.NO)
            {
                return;
            }

            // delete simply from the UI list
            foreach (UIGenreModel genre in deletionList)
            {
                _data.Remove(genre);
            }
        }

        /**
         * Returns a list with all UIGenreModels that are not in use
         */
        private List<UIGenreModel> GetUnusedGenres()
        {
            List<UIGenreModel> result = new();

            foreach (UIGenreModel model in _data)
            {
                if (!model.IsUsed)
                {
                    result.Add(model);
                }
            }

            return result;
        }

        /**
         * Is called if the user wants to delete the selected album
         */
        private void OnDeleteSelected(object sender, EventArgs e) 
        {
            List<UIGenreModel> deletionList = new();

            foreach (var uiGenre in genreTable.SelectedItems)
            {
                if (uiGenre is UIGenreModel)
                {
                    if (!((UIGenreModel)uiGenre).IsUsed)
                    {
                        deletionList.Add((UIGenreModel)uiGenre);
                    }
                }
            }
            
            if (deletionList.Count > 0)
            {
                DeleteGenres(deletionList, "Do you really want to delete the selected (unused) genres:\n");
            }
        }
    }
}
