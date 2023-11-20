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
                "Create New Album",
                "Create New Album",
                typeof(CustomCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.A, ModifierKeys.Control)
                }
            );

        /**
         * Command for deleting the currently selected album
         */
        public static readonly RoutedUICommand DeleteAlbum = new RoutedUICommand
            (
                "Delete Selected Album",
                "Delete Selected Album",
                typeof(CustomCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.D, ModifierKeys.Control)
                }
            );
    }
}
