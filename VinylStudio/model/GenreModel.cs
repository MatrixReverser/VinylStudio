using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VinylStudio.model
{
    /**
     * Represents a genre
     */
    public class GenreModel : AbstractObjectModel
    {
        private string _description = string.Empty;

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                if (value != _description)
                {
                    _description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        /**
         * Constructor of this class
         */
        public GenreModel() : base ()
        {
        }

        
    }
}
