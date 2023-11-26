using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
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
            return albumType switch
            {
                AlbumTypeEnum.LP => "LP",
                AlbumTypeEnum.EP => "EP",
                AlbumTypeEnum.MAXI => "Maxi Single",
                AlbumTypeEnum.SINGLE => "Single",
                AlbumTypeEnum.DOUBLE_LP => "Double LP",
                AlbumTypeEnum.LIVE_ALBUM => "Live Album",
                _ => "<unknown>",
            };
        }

        public static AlbumTypeEnum GetEnumValue(string translation)
        {
            return translation switch
            {
                "LP" => AlbumTypeEnum.LP,
                "EP" => AlbumTypeEnum.EP,
                "Maxi Single" => AlbumTypeEnum.MAXI,
                "Single" => AlbumTypeEnum.SINGLE,
                "Double LP" => AlbumTypeEnum.DOUBLE_LP,
                "Live Album" => AlbumTypeEnum.LIVE_ALBUM,
                _ => AlbumTypeEnum.LP,
            };
        }
    }

    public static class AlbumRatingTranslator
    {
        public static string GetEnumDescription(AlbumRatingEnum albumRating)
        {
            return albumRating switch
            {
                AlbumRatingEnum.VERY_GOOD => "Very Good",
                AlbumRatingEnum.GOOD => "Good",
                AlbumRatingEnum.SATISFYING => "Satisfying",
                AlbumRatingEnum.SUFFICIENT => "Sufficient",
                AlbumRatingEnum.POOR => "Poor",
                AlbumRatingEnum.INSUFFICIENT => "Insufficient",
                _ => "<unknown>",
            };
        }

        public static AlbumRatingEnum GetEnumValue(string translation)
        {
            return translation switch
            {
                "Very Good" => AlbumRatingEnum.VERY_GOOD,
                "Good" => AlbumRatingEnum.GOOD,
                "Satisfying" => AlbumRatingEnum.SATISFYING,
                "Sufficient" => AlbumRatingEnum.SUFFICIENT,
                "Poor" => AlbumRatingEnum.POOR,
                "Insufficient" => AlbumRatingEnum.INSUFFICIENT,
                _ => AlbumRatingEnum.GOOD,
            };
        }
    }

    /**
     * Represents an album
     */
    public class AlbumModel : AbstractObjectModel
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
        public int _thumbnailSize = 150;

        [JsonIgnore]        
        public int ThumbnailSize 
        {
            get 
            {
                return _thumbnailSize;
            } 
            set
            {
                _thumbnailSize = value;
                OnPropertyChanged(nameof(ThumbnailSize), true);
            } 
        }

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
                    OnPropertyChanged(nameof(Interpret));
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
                    OnPropertyChanged(nameof(Genre));
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
                    OnPropertyChanged(nameof(AlbumType));
                    OnPropertyChanged(nameof(AlbumTypeTranslation));
                }
            }
        }

        [JsonIgnore]
        public string AlbumTypeTranslation
        {
            get
            {                
                return AlbumTypeTranslator.GetEnumDescription(AlbumType);
            }
        }

        [JsonIgnore]
        public string CummulatedLengthString
        {
            get
            {
                int length = CummulatedLength;
                TimeSpan timeSpan = TimeSpan.FromSeconds(length);
                return timeSpan.ToString();
            }
        }

        [JsonIgnore]
        public int CummulatedLength
        {
            get
            {
                int length = 0;
                foreach (SongModel song in Songs)
                {
                    length += song.Length;
                }
                return length;
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
                    OnPropertyChanged(nameof(AlbumRating));
                    OnPropertyChanged(nameof(AlbumRatingTranslator));
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
                    OnPropertyChanged(nameof(ReleaseYear));
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
                    OnPropertyChanged(nameof(PurchaseYear));
                }
            }
        }

        [JsonIgnore]
        public long RandomId
        {
            get
            {
                return new Random().Next();
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
                    OnPropertyChanged(nameof(Price));
                    OnPropertyChanged(nameof(PriceString));
                }
            }
        }

        [JsonIgnore]
        public string PriceString
        {
            get
            {
                CultureInfo userCulture = CultureInfo.CurrentCulture;
                string formattedString = Price.ToString("C", userCulture);
                return formattedString;
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
                    OnPropertyChanged(nameof(Location));
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
            
            // Setting default thumbnail size
            ThumbnailSize = 150; 
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

            OnPropertyChanged(nameof(Songs));
        }

        /**
         * Is called if a song of this album has been modified
         */
        private void OnSongModified()
        {
            OnPropertyChanged(nameof(Songs));
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

        /**
         * Bad hack for informing the UI that the underlying image has changed.
         * Actually, only the image on the file system has changed, but not its
         * path. But we have to tell the UI that the path has changed in order to
         * force a reload of the new image
         */
        public void FireImageChangedEvents()
        {
            OnPropertyChanged(nameof(ImagePath));
            OnPropertyChanged(nameof(ImageSource));
        }
    }
}
