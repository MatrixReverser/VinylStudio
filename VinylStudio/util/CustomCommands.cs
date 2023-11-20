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
                "Create New _Album",
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
                "_Delete Selected Album",
                "Delete Selected Album",
                typeof(CustomCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.D, ModifierKeys.Control)
                }
            );

        /**
         * Command for toggling the track list locking
         */
        public static readonly RoutedUICommand ToggleTracklistLock = new RoutedUICommand
            (
                "Lock/Unlock _Tracklist",
                "Lock/Unlock Tracklist",
                typeof(CustomCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.T, ModifierKeys.Control)
                }
            );

        /**
         * Command for setting the focus on the thumbnail filter
         */
        public static readonly RoutedUICommand FilterThumbnails = new RoutedUICommand
            (
                "Filter Thumb_nails",
                "Filter Thumbnails",
                typeof(CustomCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.T, ModifierKeys.Control | ModifierKeys.Alt)
                }
            );

        /**
         * Command for setting the focus on the interprets filter
         */
        public static readonly RoutedUICommand FilterInterprets = new RoutedUICommand
            (
                "Filter _Interprets",
                "Filter Interprets",
                typeof(CustomCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.I, ModifierKeys.Control | ModifierKeys.Alt)
                }
            );
    }
}
