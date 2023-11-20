using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VinylStudio.util
{
    public static class CustomCommands
    {
        /**
         * Command for adding a new album
         */
        public static readonly RoutedUICommand NewAlbum = new RoutedUICommand
            (
                "New Album",
                "New Album",
                typeof(CustomCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.A, ModifierKeys.Control)
                }
            );

    }
}
