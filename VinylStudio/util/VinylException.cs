using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VinylStudio.util
{
    internal enum VinylExceptionType
    {
        DATABASE_EXCEPTION = 0
    }

    /**
     * An exception that occured inside the application and should be displayed to the user
     */
    internal class VinylException : Exception
    {
        private readonly string[] title = new string[] 
        {
            "Database Error"
        };

        public VinylExceptionType Type { get; private set; }

        public VinylException(VinylExceptionType type, string? message, Exception? innerException) : base(message, innerException)
        {
            Type = type;
        }

        public string GetTitle()
        {
            return title[(int)Type];
        }
    }
}
