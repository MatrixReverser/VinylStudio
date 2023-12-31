﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VinylStudio.model
{
    /**
     * An abstract class which acts as a base class for all model classes (except DataModel.cs).
     * Provides a name and an ID for each object and a static counter to generate unique IDs.
     */
    public abstract class AbstractObjectModel : INotifyPropertyChanged
    {
        public delegate void ModelModifiedEventHandler();
        public event ModelModifiedEventHandler? ModelModified;
        public event PropertyChangedEventHandler? PropertyChanged;

        private static int idCounter = 0;
        protected int _id = -1;
        protected string _name = string.Empty;

        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                if (_id >= idCounter)
                {
                    idCounter = _id + 1;
                }
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        /**
         * Constructor of this class. 
         */
        protected AbstractObjectModel()
        {            
            Id = idCounter++;
        }

        /**
         * Is called if any property of this object has changed (except of the ID)
         * If dismissModelModifiedEvent is true, then NO ModelModifiedEvent is fired but
         * only the PropertyChange event is genereated
         */
        protected void OnPropertyChanged(string propertyName, bool dismissModelModifiedEvent = false)
        {
            if (!dismissModelModifiedEvent)
            {
                ModelModified?.Invoke();
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /**
         * Returns the name as the string representation of all objects derived from this class
         */
        public override string ToString()
        {
            return Name;
        }
    }
}
