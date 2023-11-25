using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VinylStudio.model
{
    /**
     * Represents an interpret
     */
    public class InterpretModel : AbstractObjectModel
    {
        private string _country = "<country>";
        [JsonProperty("genre_ids")]
        private List<int> _genreIds = new List<int>();
        private ObservableCollection<GenreModel> _genreModels = new ObservableCollection<GenreModel>();

        public string Country
        {
            get
            {
                return _country;
            }
            set
            {
                if (value != _country)
                {
                    _country = value;
                    OnPropertyChanged(nameof(Country));
                }
            }
        }

        [JsonIgnore]
        public ObservableCollection<GenreModel> GenreModels
        {
            get
            {
                return _genreModels;
            }
        }

        /**
         * Constructor of this class
         */
        public InterpretModel() : base()
        {
            _genreModels.CollectionChanged += OnGenreListChanged;
        }

        /**
         * Is called if the list of genre models has changed
         */
        private void OnGenreListChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (GenreModel model in e.OldItems)
                {
                    int id = model.Id;
                    _genreIds.Remove(id);
                }
            }

            if (e.NewItems != null)
            {
                foreach (GenreModel model in e.NewItems)
                {
                    int id = model.Id;
                    _genreIds.Add(id);
                }
            }

            OnPropertyChanged(nameof(GenreModels));
        }

        /**
         * Initializes the list of genre models after this object was loaded from a json file
         */
        public void initReferences(ObservableCollection<GenreModel>? genreList)
        {
            _genreModels.Clear();
            _genreModels.CollectionChanged -= OnGenreListChanged;

            if (genreList != null)
            {
                foreach (int id in _genreIds)
                {
                    foreach (GenreModel model in genreList)
                    {
                        if (model.Id == id)
                        {
                            _genreModels.Add(model);
                            break;
                        }
                    }
                }
            }

            _genreModels.CollectionChanged += OnGenreListChanged;
        }
    }
}
