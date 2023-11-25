using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace VinylStudio.ui
{
    public class UIInterpretModel
    {
        public bool IsUsed { get; set; }
        public InterpretModel Model { get; set; }

        public UIInterpretModel()
        {
            IsUsed = false;
            Model = new InterpretModel();
        }
    }

    public partial class InterpretManagementDialog : Window
    {
        private ObservableCollection<UIInterpretModel> _data = new();
        private CollectionView _interpretView;
        private DataModel _mainModel;

        /**
         * Constructor of this class
         */
        public InterpretManagementDialog(DataModel mainModel)
        {
            InitializeComponent();

            _mainModel = mainModel;
            InitializeDataModel(mainModel);

            _data.CollectionChanged += (sender, e) => {
                if (e.OldItems != null)
                {
                    foreach (UIInterpretModel obj in e.OldItems)
                    {
                        _mainModel.InterpretList.Remove(obj.Model);
                    }
                }
                if (e.NewItems != null)
                {
                    foreach (UIInterpretModel obj in e.NewItems)
                    {
                        _mainModel.InterpretList.Add(obj.Model);
                    }
                }
            };
        }

        /**
         * Initializes the data model for this dialog.
         */
        private void InitializeDataModel(DataModel mainModel)
        {
            ObservableCollection<InterpretModel> interpretList = mainModel.InterpretList;

            foreach (InterpretModel model in interpretList)
            {
                UIInterpretModel record = new()
                {
                    Model = model,
                    IsUsed = _mainModel.IsInterpretUsed(model)
                };
                _data.Add(record);
            }

            // we want the interprets to be sorted by name
            _interpretView = (CollectionView)CollectionViewSource.GetDefaultView(_data);
            _interpretView.SortDescriptions.Add(new SortDescription("Model.Name", ListSortDirection.Ascending));


            interpretTable.ItemsSource = _data;
        }

        /**
         * Is called if the user clicked the OK button
         */
        private void OnOkClicked(object sender, EventArgs e)
        {
            Close();
        }

        /**
         * Is called if the user wants to delete the unused interprets
         */
        private void OnDeleteUnused(object sender, EventArgs e)
        {
            List<UIInterpretModel> deletionList = GetUnusedInterprets();
            string message = "Do you really want to delete the following interprets: \n";

            if (deletionList.Count > 0)
            {
                DeleteInterprets(deletionList, message);
            }
        }

        /**
         * Deletes all interprets in the deletion list after a security question
         */
        private void DeleteInterprets(List<UIInterpretModel> deletionList, string message)
        {
            foreach (UIInterpretModel uiGenre in deletionList)
            {
                message += uiGenre.Model.Name + "\n";
            }

            VinylMessageBox msgBox = new VinylMessageBox(this, "Delete Unused Interprets", message, VinylMessageBoxType.QUESTION, VinylMessageBoxButtons.YES_NO);
            VinylMessageBoxResult answer = msgBox.OpenDialog();

            if (answer == VinylMessageBoxResult.NO)
            {
                return;
            }

            // delete simply from the UI list
            foreach (UIInterpretModel interpret in deletionList)
            {
                _data.Remove(interpret);
            }
        }

        /**
         * Returns a list with all UIInterpretModels that are not in use
         */
        private List<UIInterpretModel> GetUnusedInterprets()
        {
            List<UIInterpretModel> result = new();

            foreach (UIInterpretModel model in _data)
            {
                if (!model.IsUsed)
                {
                    result.Add(model);
                }
            }

            return result;
        }

        /**
         * Is called if the user wants to delete the selected interprets
         */
        private void OnDeleteSelected(object sender, EventArgs e)
        {
            List<UIInterpretModel> deletionList = new();

            foreach (var uiInterpret in interpretTable.SelectedItems)
            {
                if (uiInterpret is UIInterpretModel)
                {
                    if (!((UIInterpretModel)uiInterpret).IsUsed)
                    {
                        deletionList.Add((UIInterpretModel)uiInterpret);
                    }
                }
            }

            if (deletionList.Count > 0)
            {
                DeleteInterprets(deletionList, "Do you really want to delete the selected (unused) interprets:\n");
            }
        }
    }
}
