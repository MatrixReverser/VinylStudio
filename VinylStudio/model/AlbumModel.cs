using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace VinylStudio.model
{
    public enum AlbumTypeEnum
    {
        LP,
        EP,
        MAXI,
        SINGLE,
        DOUBLE_LP,
        LIVE_ALBUM
    }

    public enum AlbumRatingEnum
    {
        VERY_GOOD,
        GOOD,
        SATISFYING,
        SUFFICIENT,
        POOR,
        INSUFFICIENT
    }

    public static class AlbumTypeTranslator
    {
        public static string GetEnumDescription(AlbumTypeEnum albumType)
        {
            switch (albumType)
            {
                case AlbumTypeEnum.LP: return "LP";
                case AlbumTypeEnum.EP: return "EP";
                case AlbumTypeEnum.MAXI: return "Maxi Single";
                case AlbumTypeEnum.SINGLE: return "Single";
                case AlbumTypeEnum.DOUBLE_LP: return "Double LP";
                case AlbumTypeEnum.LIVE_ALBUM: return "Live Album";
                default: return "<unknown>";
            }
        }

        public static AlbumTypeEnum GetEnumValue(string translation)
        {
            switch (translation)
            {
                case "LP": return AlbumTypeEnum.LP;
                case "EP": return AlbumTypeEnum.EP;
                case "Maxi Single": return AlbumTypeEnum.MAXI;
                case "Single": return AlbumTypeEnum.SINGLE;
                case "Double LP": return AlbumTypeEnum.DOUBLE_LP;
                case "Live Album": return AlbumTypeEnum.LIVE_ALBUM;
                default: return AlbumTypeEnum.LP;
            }
        }
    }

    public static class AlbumRatingTranslator
    {
        public static string GetEnumDescription(AlbumRatingEnum albumRating)
        {
            switch (albumRating)
            {
                case AlbumRatingEnum.VERY_GOOD: return "Very Good";
                case AlbumRatingEnum.GOOD: return "Good";
                case AlbumRatingEnum.SATISFYING: return "Satisfying";
                case AlbumRatingEnum.SUFFICIENT: return "Sufficient";
                case AlbumRatingEnum.POOR: return "Poor";
                case AlbumRatingEnum.INSUFFICIENT: return "Insufficient";
                default: return "<unknown>";
            }
        }

        public static AlbumRatingEnum GetEnumValue(string translation)
        {
            switch (translation)
            {
                case "Very Good": return AlbumRatingEnum.VERY_GOOD;
                case "Good": return AlbumRatingEnum.GOOD;
                case "Satisfying": return AlbumRatingEnum.SATISFYING;
                case "Sufficient": return AlbumRatingEnum.SUFFICIENT;
                case "Poor": return AlbumRatingEnum.POOR;
                case "Insufficient": return AlbumRatingEnum.INSUFFICIENT;
                default: return AlbumRatingEnum.GOOD;
            }
        }
    }

    /**
     * Represents an album
     */
    internal class AlbumModel : AbstractObjectModel
    {
        [JsonProperty("interpret_id")]
        private int? _interpretId = null;
        private InterpretModel? _interpret = null;
        [JsonProperty("genre_id")]
        private int? _genreId = null;
        private GenreModel? _genre = null;
        private AlbumTypeEnum _albumType = AlbumTypeEnum.LP;
        private AlbumRatingEnum _albumRating = AlbumRatingEnum.GOOD;
        private int _releaseYear = 0;
        private int _purchaseYear = 0;
        private double _price = 0.0;
        private string _location = string.Empty;
        [JsonProperty("song_list")]
        private ObservableCollection<SongModel> _songs = new();
        private BitmapImage? _image;
        
        [JsonIgnore]
        public InterpretModel? Interpret 
        { 
            get
            {
                return _interpret; 
            }
            set
            {
                if (value != _interpret)
                {
                    _interpret = value;
                    _interpretId = (_interpret?.Id);
                    OnPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public GenreModel? Genre
        {
            get
            {
                return _genre;
            }
            set
            {
                if (value != _genre)
                {
                    _genre = value;
                    _genreId = (_genre?.Id);
                    OnPropertyChanged();
                }
            }
        }

        public AlbumTypeEnum AlbumType
        {
            get
            {
                return _albumType;
            }
            set
            {
                if (value != _albumType)
                {
                    _albumType = value;
                    OnPropertyChanged();
                }
            }
        }

        public string AlbumTypeTranslation
        {
            get
            {                
                return AlbumTypeTranslator.GetEnumDescription(AlbumType);
            }
        }

        public string CummulatedLength
        {
            get
            {
                int length = 0;
                foreach (SongModel song in Songs)
                {
                    length += song.Length;
                }
                TimeSpan timeSpan = TimeSpan.FromSeconds(length);
                return timeSpan.ToString();
            }
        }

        public AlbumRatingEnum AlbumRating
        {
            get
            {
                return _albumRating;    
            }
            set
            {
                if (value != _albumRating)
                {
                    _albumRating = value;
                    OnPropertyChanged();
                }
            }
        }

        public int ReleaseYear
        {
            get
            {
                return _releaseYear;
            }
            set
            {
                if (value != _releaseYear)
                {
                    _releaseYear = value;
                    OnPropertyChanged();
                }
            }
        }

        public int PurchaseYear
        {
            get
            {
                return _purchaseYear;
            }
            set
            {
                if (value != _purchaseYear)
                {
                    _purchaseYear = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Price
        {
            get
            {
                return _price;
            }
            set
            {
                if (value != _price)
                {
                    _price = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public string PriceString
        {
            get
            {
                return Price.ToString();
            }
        }

        public string Location
        {
            get
            {
                return _location;
            }
            set
            {
                if (value != _location)
                {
                    _location = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public ObservableCollection<SongModel> Songs
        {
            get
            {
                return _songs;
            }
        }

        [JsonIgnore]
        public string ImagePath
        {
            get
            {
                Assembly? currentAssembly = Assembly.GetEntryAssembly();
                string? appPath = currentAssembly == null ? "" : System.IO.Path.GetDirectoryName(currentAssembly.Location);
                string filename = appPath + "/" + DataModel.DIR_THUMBNAIL + "/" + Id + ".jpg";

                return filename;
            }
        }

        [JsonIgnore]
        public BitmapImage? ImageSource
        {
            get
            {
                string filename = ImagePath;

                // This method of loading the image blocks the file on the file system. Not good if we want to 
                // to a new migration while the current database is loaded
                //_image = new(new Uri(filename));

                // found this solution. After loading the image the following way, it is not
                // blocked in the file system and files can be deleted by the LegacyImporter
                _image = new BitmapImage();
                var stream = File.OpenRead(filename);
                _image.BeginInit();
                _image.CacheOption = BitmapCacheOption.OnLoad;
                _image.StreamSource = stream;
                _image.EndInit();
                stream.Close();
                stream.Dispose();

                return _image;
            }
        }

        /**
         * Constructor of this class
         */
        public AlbumModel() : base()
        {
            _songs.CollectionChanged += OnSongListChanged;
        }

        /**
         * Is called if the list of songs has been changed
         */
        private void OnSongListChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (SongModel song in e.OldItems)
                {
                    song.ModelModified -= OnSongModified;
                }
            }

            if (e.NewItems != null)
            {
                foreach (SongModel song in e.NewItems)
                {
                    song.ModelModified += OnSongModified;
                }
            }

            OnPropertyChanged();
        }

        /**
         * Is called if a song of this album has been modified
         */
        private void OnSongModified()
        {
            OnPropertyChanged();
        }

        /**
         * Updates the references for the IDs - set by Newtonsoft after loading
         */
        public void InitReferences(ObservableCollection<InterpretModel>? interpretRefs, ObservableCollection<GenreModel>? genreRefs)
        {
            // update interpret
            _interpret = null;
            if (interpretRefs != null)
            {
                foreach (InterpretModel interpret in interpretRefs)
                {
                    if (interpret.Id == _interpretId)
                    {
                        _interpret = interpret;
                        break;
                    }
                }
            }

            // update genre
            _genre = null;
            if (genreRefs != null)
            {
                foreach (GenreModel genre in genreRefs)
                {
                    if (genre.Id == _genreId)
                    {
                        _genre = genre;
                        break;
                    }
                }
            }
        }

        /**
         * Initialzes the listeners after loading this album from json
         */
        public void InitListeners()
        {
            foreach (SongModel song in _songs)
            {
                song.ModelModified += OnSongModified;
            }
        }
    }
}
