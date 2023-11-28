using System.Windows.Data;
using VinylStudio.model;

namespace VinylStudio.util
{
    public class AdvancedSearchFilter
    {
        public AlbumTypeEnum? AlbumType { get; set; } = null;
        public string? SongName { get; set; } = null;
        public AlbumRatingEnum? AlbumRating { get; set; } = null;
        public string? Location { get; set; } = null;
        public int? ReleaseFrom { get; set; } = null;
        public int? ReleaseTo { get; set; } = null;
        public int? PurchaseFrom { get; set; } = null;
        public int? PurchaseTo { get; set; } = null;
        public int? PriceFrom { get; set; } = null;
        public int? PriceTo { get; set; } = null;
        public GenreModel? Genre { get; set; } = null;

        /**
         * Constructor of this class
         */
        public AdvancedSearchFilter()
        {
        }

        /**
         * Determines if this filter is active (any filter option is set)
         */
        public bool isFilterActive()
        {
            return (AlbumType != null ||
                SongName != null ||
                AlbumRating != null ||
                Location != null ||
                ReleaseFrom != null ||
                ReleaseTo != null ||
                PurchaseFrom != null ||
                PurchaseTo != null ||
                PriceFrom != null ||
                PriceTo != null ||
                Genre != null);
        }

        /**
         * Sets the advanced filter for he thumbnail view
         */
        public void FilterThumbnails(CollectionView thumbnailView)
        {
            if (thumbnailView != null)
            {
                thumbnailView.Filter += obj =>
                {
                    AlbumModel album = (AlbumModel)obj;

                    bool acceptAlbum =
                            AcceptAlbumType(album) &&
                            AcceptSongName(album) &&
                            AcceptRating(album) &&
                            AcceptLocation(album) && 
                            AcceptReleaseYear(album) &&
                            AcceptPurchaseYear(album) &&
                            AcceptPrice(album) &&
                            AcceptGenre(album);

                    return acceptAlbum;
                };
            }
        }

        /**
         * Checks if the genre is accepted
         */
        private bool AcceptGenre(AlbumModel album)
        {
            return (Genre == null || album.Genre == Genre);
        }

        /**
         * Checks if the price is accepted
         */
        private bool AcceptPrice(AlbumModel album)
        {
            bool accept = false;

            if (PriceFrom == null && PriceTo == null)
            {
                accept = true;
            }
            else
            {
                if (PriceFrom == null)
                {
                    if (album.Price <= PriceTo)
                    {
                        accept = true;
                    }
                }
                else if (PriceTo == null)
                {
                    if (album.Price  >= PriceFrom)
                    {
                        accept = true;
                    }
                }
                else
                {
                    if (album.Price >= PriceFrom && album.Price <= PriceTo)
                    {
                        accept = true;
                    }
                }
            }

            return accept;
        }

        /**
         * Checks if the purchase year is accepted
         */
        private bool AcceptPurchaseYear(AlbumModel album)
        {
            bool accept = false;

            if (PurchaseFrom == null && PurchaseTo == null)
            {
                accept = true;
            }
            else
            {
                if (PurchaseFrom == null)
                {
                    if (album.PurchaseYear <= PurchaseTo)
                    {
                        accept = true;
                    }
                }
                else if (PurchaseTo == null)
                {
                    if (album.PurchaseYear >= PurchaseFrom)
                    {
                        accept = true;
                    }
                }
                else
                {
                    if (album.PurchaseYear >= PurchaseFrom && album.PurchaseYear <= PurchaseTo)
                    {
                        accept = true;
                    }
                }
            }

            return accept;
        }

        /**
         * Checks if the release year is accepted
         */
        private bool AcceptReleaseYear(AlbumModel album)
        {
            bool accept = false;

            if (ReleaseFrom == null && ReleaseTo == null)
            {
                accept = true;
            }
            else
            {
                if (ReleaseFrom == null)
                {
                    if (album.ReleaseYear <= ReleaseTo)
                    {
                        accept = true;
                    }
                } 
                else if (ReleaseTo == null)
                {
                    if (album.ReleaseYear >= ReleaseFrom)
                    {
                        accept = true;
                    }
                }
                else
                {
                    if (album.ReleaseYear >= ReleaseFrom && album.ReleaseYear <= ReleaseTo)
                    {
                        accept = true;
                    }
                }
            }

            return accept;
        }

        /**
         * Checks if the album location is accepted
         */
        private bool AcceptLocation(AlbumModel album)
        {
            return (Location == null || album.Location.ToLower().Trim().Contains(Location.ToLower().Trim()));
        }

        /**
         * Checks if the album rating is accepted
         */
        private bool AcceptRating(AlbumModel album)
        {
            return (AlbumRating == null || album.AlbumRating == AlbumRating);
        }

        /**
         * Checks if the album contains a song mathcing the song name filter
         */
        private bool AcceptSongName(AlbumModel album)
        {
            bool accept = false;

            foreach (SongModel song in album.Songs)
            {
                if (SongName == null || song.Name.ToLower().Trim().Contains(SongName.ToLower().Trim()))
                {
                    accept = true;
                }
            }

            return accept;
        }

        /**
         * checks if the album type is accepted
         */
        private bool AcceptAlbumType(AlbumModel album)
        {
            return (AlbumType == null || album.AlbumType == AlbumType);
        }
    }
}
