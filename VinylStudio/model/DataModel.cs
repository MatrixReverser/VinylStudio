using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using VinylStudio.util;

namespace VinylStudio.model
{
    /**
     * This class is the root of the data model. It contains a list of all interprets,
     * genres and all albums contained in the database.
     */
    internal class DataModel
    {
        public static readonly string DIR_DATABASE = "database";
        public static readonly string DIR_THUMBNAIL = "database/thumbnails";
        private static readonly string FILE_GENRE_LIST = "genre.json";
        private static readonly string FILE_INTERPRET_LIST = "interpret.json";
        private static readonly string FILE_ALBUM_LIST = "album.json";

        private ObservableCollection<GenreModel> _genreList = new();
        private ObservableCollection<InterpretModel> _interpretList = new();
        private ObservableCollection<AlbumModel> _albumList = new();
        
        public ObservableCollection<GenreModel> GenreList
        {
            get
            {
                return _genreList;
            }
        }

        public ObservableCollection<InterpretModel> InterpretList
        {
            get
            {
                return _interpretList;
            }
        }

        public ObservableCollection<AlbumModel> AlbumList
        {
            get
            {
                return _albumList;
            }
        }

        public bool IsModified { get; private set; }

        /**
         * Constructor of the class
         */
        public DataModel()
        {
            CheckDatabaseExistence();
            _genreList.CollectionChanged += OnCollectionChanged;
            _interpretList.CollectionChanged += OnCollectionChanged;
            _albumList.CollectionChanged += OnCollectionChanged;
        }

        /**
         * Is called if any of the collections in this class has been changed
         */
        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (AbstractObjectModel obj in e.OldItems) 
                {
                    obj.ModelModified -= OnModelModified;
                }
            }

            if (e.NewItems != null)
            {
                foreach (AbstractObjectModel obj in e.NewItems)
                {
                    obj.ModelModified += OnModelModified;
                }
            }

            OnModelModified();
        }        

        /**
         * Checks if the database directories exists. If not, new directories will be created
         */
        private static void CheckDatabaseExistence()
        {
            if (!Directory.Exists(DIR_DATABASE))
            {
                Directory.CreateDirectory(DIR_DATABASE);                                
            }

            if (!Directory.Exists(DIR_THUMBNAIL))
            {
                Directory.CreateDirectory(DIR_THUMBNAIL);
            }
        }

        /**
         * Saves all data in this model to JSON files
         * @throws VinylException if an error has occured
         */
        public void Save()
        {
            CheckDatabaseExistence();

            string genreFileName = DIR_DATABASE + "/" + FILE_GENRE_LIST;
            string interpretFileName = DIR_DATABASE + "/" + FILE_INTERPRET_LIST;
            string albumFileName = DIR_DATABASE + "/" + FILE_ALBUM_LIST;

            JsonSerializerSettings settings = new() { Formatting = Formatting.Indented };

            try
            {
                // save genres to json
                if (File.Exists(genreFileName))
                {
                    File.Delete(genreFileName);
                }
                string json = JsonConvert.SerializeObject(GenreList, settings);
                File.WriteAllText(genreFileName, json);

                // save interprets to json
                if (File.Exists(interpretFileName))
                {
                    File.Delete(interpretFileName);
                }
                json = JsonConvert.SerializeObject(InterpretList, settings);
                File.WriteAllText(interpretFileName, json);

                // save albums to json
                if (File.Exists(albumFileName))
                {
                    File.Delete(albumFileName);
                }
                json = JsonConvert.SerializeObject(AlbumList, settings);
                File.WriteAllText (albumFileName, json);
            } catch (Exception ex)
            {
                throw new VinylException(VinylExceptionType.DATABASE_EXCEPTION,
                    "Failed to save changes to database",
                    ex);
            }

            IsModified = false;
        }

        /**
         * Loads all data from the JSON files into the lists of this class
         */
        public void Load()
        {
            string genreFileName = DIR_DATABASE + "/" + FILE_GENRE_LIST;
            string interpretFileName = DIR_DATABASE + "/" + FILE_INTERPRET_LIST;
            string albumFileName = DIR_DATABASE + "/" + FILE_ALBUM_LIST;
            string? jsonGenre;
            string? jsonInterpret;
            string? jsonAlbum;

            // skip loading if files are not present. We do this silent and do not throw an exception
            if (!File.Exists(genreFileName) ||
                !File.Exists(interpretFileName) ||
                !File.Exists(albumFileName))
            {
                return;
            }

            try
            {
                jsonGenre = File.ReadAllText(genreFileName);
                jsonInterpret = File.ReadAllText(interpretFileName);
                jsonAlbum = File.ReadAllText(albumFileName);
            } catch (Exception ex)
            {
                throw new VinylException(VinylExceptionType.DATABASE_EXCEPTION,
                    "Failed to load data from filesystem",
                    ex);
            }

            try
            {
                _genreList.CollectionChanged -= OnCollectionChanged;
                _interpretList.CollectionChanged -= OnCollectionChanged;
                _albumList.CollectionChanged -= OnCollectionChanged;

                ObservableCollection<GenreModel>? resultGenreList = JsonConvert.DeserializeObject<ObservableCollection<GenreModel>>(jsonGenre);
                _genreList = resultGenreList ?? new ObservableCollection<GenreModel>();

                ObservableCollection<InterpretModel>? resultInterpretList = JsonConvert.DeserializeObject<ObservableCollection<InterpretModel>>(jsonInterpret);
                _interpretList = resultInterpretList ?? new ObservableCollection<InterpretModel>();

                ObservableCollection<AlbumModel>? resultAlbumList = JsonConvert.DeserializeObject<ObservableCollection<AlbumModel>>(jsonAlbum);
                _albumList = resultAlbumList ?? new ObservableCollection<AlbumModel>();
            } catch (Exception ex)
            {
                throw new VinylException(VinylExceptionType.DATABASE_EXCEPTION,
                    "The database seems to be corrupt",
                    ex);
            } finally
            {
                _genreList.CollectionChanged += OnCollectionChanged;
                _interpretList.CollectionChanged += OnCollectionChanged;
                _albumList.CollectionChanged += OnCollectionChanged;
            }

            InitReferences();
            InitListeners();
        }
                
        /**
         * Adds listeners to all objects that has been loaded from json
         */
        private void InitListeners()
        {
            if (_genreList != null)
            {
                foreach (GenreModel genre in _genreList)
                {
                    genre.ModelModified += OnModelModified;
                }
            }

            if (_interpretList != null)
            {
                foreach (InterpretModel interpret in _interpretList)
                {
                    interpret.ModelModified += OnModelModified;
                }
            }

            if (_albumList != null)
            {
                foreach (AlbumModel album in _albumList)
                {
                    album.ModelModified += OnModelModified;
                    album.InitListeners();
                }
            }
        }

        /**
         * is called if any object in the model has been modified (including adding / removing objects from lists)
         */
        private void OnModelModified()
        {
            IsModified = true;
        }

        /**
         * Initializes all references in the objects after loading from json
         */
        private void InitReferences() { 
            if (_interpretList != null)
            {
                foreach (InterpretModel interpretModel in _interpretList)
                {
                    interpretModel.initReferences(_genreList);
                }
            }

            if (_albumList != null)
            {
                foreach (AlbumModel albumModel in _albumList)
                {
                    albumModel.initReferences(_interpretList, _genreList);
                }
            }
        }

        /**
         * Deletes all objects from this data model
         */
        public void Clear()
        {
            if (_albumList != null)
            {
                foreach (AbstractObjectModel obj in _albumList)
                {
                    obj.ModelModified -= OnModelModified;
                }
                _albumList.Clear();
            }

            if (_interpretList != null)
            {
                foreach (AbstractObjectModel obj in _interpretList)
                {
                    obj.ModelModified -= OnModelModified;
                }
                _interpretList.Clear(); 
            }

            if (_genreList != null)
            {
                foreach (AbstractObjectModel obj in _genreList)
                {
                    obj.ModelModified -= OnModelModified;
                }
                _genreList.Clear();
            }
        }
    }
}
