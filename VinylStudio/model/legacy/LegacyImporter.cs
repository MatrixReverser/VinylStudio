using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace VinylStudio.model.legacy
{
    /**
     * This class is for importing data from the legacy system (VinylShelf 0.2)
     */
    internal class LegacyImporter
    {
        private readonly string _originFileName;
        private DataModel? _model = null;
        private Dictionary<string, int> _genreDict = new();
        private Dictionary<string, int> _interpretDict = new();
        private Dictionary<string, int> _albumDict = new();
        private int _thumbnailSize = 150;

        /**
         * Constructor of this class
         */
        public LegacyImporter(string originFileName, int thumbnailSize)
        {
            _originFileName = originFileName;
            _thumbnailSize = thumbnailSize;
        }   

        /**
         * Imports data into a new DataModel
         * returns null, if something went terribly wrong, otherwise, the convertet data model
         */
        public DataModel? Import()
        {            
            string json = File.ReadAllText(_originFileName);
            Debug.WriteLine(json);
            LegacyModel? legacyModel = JsonConvert.DeserializeObject<LegacyModel>(json);

            if (legacyModel != null)
            {
                _model = new();
                _genreDict.Clear();
                _interpretDict.Clear();

                ExtractGenres(legacyModel);
                ExtractInterprets(legacyModel);
                ExtractAlbums(legacyModel);
            }

            // delete old database and store new data. Also copy the covers to the new database
            if (_model != null)
            {
                if (Directory.Exists(DataModel.DIR_DATABASE))
                {
                    Directory.Delete(DataModel.DIR_DATABASE, true);
                }

                _model.Save();

                if (legacyModel != null)
                {
                    CopyCovers(legacyModel.album_list);
                }
            }

            return _model;
        }

        /**
         * Copies all album covers from the old location to the new location
         */
        private void CopyCovers(List<Album> albums)
        {
            foreach (Album album in albums)
            {
                string oldName = album.cover_path;
                string newName = DataModel.DIR_THUMBNAIL + "/" + _albumDict[album.uid] + ".jpg";

                using (var originalImage = Image.FromFile(oldName))
                {
                    using (var scaledImage = new Bitmap(400, 400))
                    using (var graphics = Graphics.FromImage(scaledImage))
                    {
                        graphics.DrawImage(originalImage, new Rectangle(0, 0, 400, 400));
                        scaledImage.Save(newName, ImageFormat.Jpeg);
                    }
                }

            }
        }

        /**
         * Extracts all genres from the legacy model and stores it in the DataModel.
         * The mapping from legacy IDs to current IDs is stored in _genreDict
         */
        private void ExtractGenres(LegacyModel legacyModel)
        {
            foreach (Genre legacyGenre in legacyModel.genre_list)
            {
                GenreModel genre = new();
                genre.Name = legacyGenre.name;
                genre.Description = string.Empty;

                _genreDict.Add(legacyGenre.uid, genre.Id);
                _model?.GenreList.Add(genre);
            }
        }

        /**
         * Extracts all interprets from the legacy model and stores it in the DataModel.
         * The mapping from legacy IDs to current IDs is stored in _interpretDict.
         */
        private void ExtractInterprets(LegacyModel legacyModel)
        {
            legacyModel.interpret_list = legacyModel.interpret_list.OrderBy(i => i.name).ToList();

            foreach (Interpret legacyInterpret in legacyModel.interpret_list)
            {
                InterpretModel interpret = new();
                interpret.Name = legacyInterpret.name;
                interpret.Country = legacyInterpret.country;
                // no information for genres in the legacy model. We select genres from the
                // albums later and update the interpret with it.

                _interpretDict.Add(legacyInterpret.uid, interpret.Id);
                _model?.InterpretList.Add(interpret);
            }
        }

        /**
         * Extracts all albums with all songs from the legacy model and stores it in the DataModel.
         * Additionally, the genre of an album is added to the list of genres hold by the interpret.
         * The mapping from legacy IDs to new IDs is stored in _albumDict
         */
        private void ExtractAlbums(LegacyModel legacyModel)
        {
            foreach (Album legacyAlbum in legacyModel.album_list)
            {
                AlbumModel album = new();
                album.ThumbnailSize = _thumbnailSize;
                album.Name = legacyAlbum.title;
                album.Interpret = FindInterpret(legacyAlbum.interpret);
                album.Genre = FindGenre(legacyAlbum.genre);

                album.AlbumType = DecodeAlbumType(legacyAlbum.type);
                album.AlbumRating = DecodeAlbumRating(legacyAlbum.rating);
                album.ReleaseYear = legacyAlbum.year;
                album.PurchaseYear = legacyAlbum.purchase_year;
                album.Price = legacyAlbum.price;
                album.Location = legacyAlbum.place;

                ExtractSongs(album, legacyAlbum);

                _albumDict.Add(legacyAlbum.uid, album.Id);
                _model?.AlbumList.Add(album);
            }
        }

        /**
         * Transfers all songs from the legacy album to the new model album
         */
        private void ExtractSongs(AlbumModel album, Album legacyAlbum)
        {
            foreach (Track legacySong in legacyAlbum.track_list)
            {
                SongModel song = new();
                song.Name = legacySong.name;
                song.Side = DecodeAlbumSide(legacySong.side);
                song.Length = legacySong.length;
                song.Index = legacySong.track_number;

                album.Songs.Add(song);
            }
        }

        /**
         * Decodes the legacy side of the album and returns the corresponding new model side
         */
        private AlbumSideEnum DecodeAlbumSide(int legacySide)
        {
            switch (legacySide)
            {
                case 1:
                    return AlbumSideEnum.A;
                case 2:
                    return AlbumSideEnum.B;
                case 3:
                    return AlbumSideEnum.C;
                case 4:
                    return AlbumSideEnum.D;
                default:
                    return AlbumSideEnum.A;
            }
        }

        /**
         * Decodes the legacy album rating and return the corresponding new model rating
         */
        private AlbumRatingEnum DecodeAlbumRating(int legacyRating)
        {
            switch (legacyRating)
            {
                case 1:
                    return AlbumRatingEnum.VERY_GOOD;
                case 2:
                    return AlbumRatingEnum.GOOD;
                case 3:
                    return AlbumRatingEnum.SATISFYING;
                case 4:
                    return AlbumRatingEnum.SUFFICIENT;
                case 5:
                    return AlbumRatingEnum.POOR;
                case 6:
                    return AlbumRatingEnum.INSUFFICIENT;
                default:
                    return AlbumRatingEnum.SATISFYING;
            }
        }

        /**
         * Decodes the legacy album type and returns the corresponding new model type
         */
        private AlbumTypeEnum DecodeAlbumType(int legacyType)
        {
            switch (legacyType)
            {                
                case 2:
                    return AlbumTypeEnum.SINGLE;
                case 3:
                    return AlbumTypeEnum.MAXI;
                case 4:
                    return AlbumTypeEnum.EP;
                default:
                    return AlbumTypeEnum.LP;
            }
        }

        /** 
         * Finds a new model interpret by the ID of the legacy interpret
         */
        private InterpretModel? FindInterpret(string uid)
        {
            int id = _interpretDict[uid];
            InterpretModel? interpret = null;

            if (_model != null) 
            {
                foreach (InterpretModel cmp in _model.InterpretList)
                {
                    if (cmp.Id == id)
                    {
                        interpret = cmp;
                        break;
                    }
                }
            }
            if (interpret == null)
            {
                Debug.WriteLine("Warning: Interpret with ID " + uid + " not in the list");
            }

            return interpret;
        }

        /** 
         * Finds a new model genre by the ID of the legacy genre
         */
        private GenreModel? FindGenre(string uid)
        {
            int id = _genreDict[uid];
            GenreModel? genre = null;

            if (_model != null)
            {
                foreach (GenreModel cmp in _model.GenreList)
                {
                    if (cmp.Id == id) 
                    {
                        genre = cmp;
                        break;
                    }
                }
            }
            if (genre == null)
            {
                Debug.WriteLine("Warning: Genre with ID " + uid + " not in the list");
            }

            return genre;
        }
    }
}
