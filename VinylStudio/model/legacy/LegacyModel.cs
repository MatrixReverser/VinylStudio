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
        public string _type;
        public List<Interpret> interpret_list = new();
        public List<Genre> genre_list = new();
        public List<Album> album_list = new();  
    }

    internal class Album
    {
        public string _type;
        public string uid;
        public string title;
        public string interpret;
        public string genre;
        public int rating;
        public int year;
        public int purchase_year;
        public int type;
        public double price;
        public string cover_path;
        public string place;
        public List<Track> track_list = new();
    }

    internal class Track
    {
        public string _type;
        public string uid;
        public string name;
        public int track_number;
        public int length;
        public int side;
    }

    internal class Genre
    {
        public string _type;
        public string uid;
        public string name;
    }

    internal class Interpret
    {
        public string _type;
        public string uid;
        public string name;
        public string country;
    }
}
