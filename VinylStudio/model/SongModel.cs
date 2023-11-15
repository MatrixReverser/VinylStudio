using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VinylStudio.model
{
    public enum AlbumSideEnum
    {
        A, B, C, D
    }

    /**
     * Represents a song of an album. 
     */
    internal class SongModel : AbstractObjectModel
    {
        private AlbumSideEnum _side = AlbumSideEnum.A;
        private int _length = 0;
        private int _index = 0;

        public AlbumSideEnum Side
        {
            get
            {
                return _side;
            }
            set
            {
                if (value != _side)
                {
                    _side = value;
                    OnPropertyChanged(nameof(Side));
                }
            }
        }

        public int Length
        {
            get
            {
                return _length;  
            }
            set
            {
                if (value != _length)
                {
                    _length = value;
                    OnPropertyChanged(nameof(Length));
                }
            }
        }

        public string LengthInMinutes
        {
            get
            {
                TimeSpan time = TimeSpan.FromSeconds(Length);
                string length = time.ToString(@"m\:ss");                
                return length;
            }
            set
            {
                try
                {
                    TimeSpan timeSpan = TimeSpan.ParseExact(value, "m':'ss", null);
                    int seconds = (int)timeSpan.TotalSeconds;
                    if (seconds != Length)
                    {
                        Length = seconds;
                    }
                } catch (FormatException)
                {
                    Debug.WriteLine("Uhh.");
                }
            }
        }

        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                if (value != _index)
                {
                    _index = value;
                    OnPropertyChanged(nameof(Index));
                }
            }
        }

        /**
         * Constructor of this class
         */
        public SongModel() : base()
        {
        }
    }
}
