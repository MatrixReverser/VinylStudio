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

        /**
         * Constructor of this class
         */
        public InterpretModel() : base()
        {            
        }
    }
}
