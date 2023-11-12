using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * This class contains the legacy data model from the python app (VinylShelf v0.2)
 * It is used for migrating old data to the data model of this app
 */
namespace VinylStudio.model.legacy
{
    internal class LegacyModel
    {
        public string _type = string.Empty;
        public List<Interpret> interpret_list = new();
        public List<Genre> genre_list = new();
        public List<Album> album_list = new();  
    }

    internal class Album
    {
        public string _type = string.Empty;
        public string uid = string.Empty;
        public string title = string.Empty;
        public string interpret = string.Empty;
        public string genre = string.Empty;
        public int rating;
        public int year;
        public int purchase_year;
        public int type;
        public double price;
        public string cover_path = string.Empty;
        public string place = string.Empty;
        public List<Track> track_list = new();
    }

    internal class Track
    {
        public string _type = string.Empty;
        public string uid = string.Empty;
        public string name = string.Empty;
        public int track_number;
        public int length;
        public int side;
    }

    internal class Genre
    {
        public string _type = string.Empty;
        public string uid = string.Empty;
        public string name = string.Empty;
    }

    internal class Interpret
    {
        public string _type = string.Empty;
        public string uid = string.Empty;
        public string name = string.Empty;
        public string country = string.Empty;
    }
}
