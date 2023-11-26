using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VinylStudio.util
{
    public class UserSettings
    {
        private const string FILENAME = "userSettings.json";

        public int AppWidth = 1280;
        public int AppHeight = 800;
        public int XPosition = -1;
        public int YPosition = -1;
        public double ThumbnailContainerHeight = -1;
        public int ThumbnailSize = 150;

        /**
         * Constructor of this class
         */
        public UserSettings() 
        {
            string? json;

            if (File.Exists(FILENAME))
            {
                try
                {
                    json = File.ReadAllText(FILENAME);
                    JsonConvert.PopulateObject(json, this);
                }
                catch (Exception ex)
                {
                    throw new VinylException(VinylExceptionType.DATABASE_EXCEPTION,
                        "Failed to load user settings from filesystem",
                        ex);
                }
            }
        }

        /**
         * Saves the settings to disk
         */
        public void Save()
        {
            JsonSerializerSettings settings = new() { Formatting = Formatting.Indented };

            try
            {
                if (File.Exists(FILENAME))
                {
                    File.Delete(FILENAME);
                }

                string json = JsonConvert.SerializeObject(this, settings);
                File.WriteAllText(FILENAME, json);
            } catch (Exception ex)
            {
                throw new VinylException(VinylExceptionType.DATABASE_EXCEPTION,
                    "Failed to save user settings to filesystem",
                    ex);
            }
        }
    }
}
