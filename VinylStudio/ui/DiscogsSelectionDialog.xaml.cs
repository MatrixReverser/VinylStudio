using discogsharp.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VinylStudio.model;
using VinylStudio.util;

namespace VinylStudio
{
    /// <summary>
    /// Interaktionslogik für DiscogsSelectionDialog.xaml
    /// </summary>
    public partial class DiscogsSelectionDialog : Window
    {
        private DiscogsClient _discogsClient;
        private List<SongModel>? _retrievedSongs = null;

        public DiscogsSelectionDialog(UserSettings userSettings, string interpret, string album) : base()
        {
            InitializeComponent();

            _discogsClient = new DiscogsClient(userSettings.DiscogsToken, interpret, album);
            releaseTable.ItemsSource = _discogsClient.ReleaseList;
        }

        /**
         * Is called when the muse moved over a track cell in the data grid.
         * A tooltip with all tracks and durations is shown
         */
        private void OnShowTrackTooltip(object sender, MouseEventArgs e)
        {
            DataGridRow? row = FindVisualParent<DataGridRow>(e.OriginalSource as DependencyObject);
            
            if (row != null)
            {
                string tooltipText = GetTooltipTextForRow(row.Item as Release); 

                ToolTip tooltip = new ToolTip();
                tooltip.Content = tooltipText;

                row.ToolTip = tooltip;
            }
        }

        /**
         * Find the parent of the value the mouse is hovering
         */
        private T? FindVisualParent<T>(DependencyObject? obj) where T : DependencyObject
        {
            while (obj != null)
            {
                if (obj is T parent)
                    return parent;

                obj = VisualTreeHelper.GetParent(obj);
            }

            return null;
        }

        /**
         * gets the string with tracks to be shown as a tooltip
         */
        private string GetTooltipTextForRow(Release? release)
        {
            if (release == null || release.TrackList.Count == 0)
            {
                return "no track data available";
            }

            int trackCount = release.TrackList.Count;
            string tooltip = "TRACKLIST\n---------\n";

            for (int i=0; i<trackCount; i++)
            {
                Track track = release.TrackList[i];
                string duration = (track.Duration == string.Empty ? "[?:??]" : track.Duration);

                //tooltip += $"{i,2}: ";
                tooltip += $"{track.Position}: ";
                tooltip += $"{duration}: ";
                tooltip += $"{track.Title}\n";

            }

            return tooltip;
        }

        /**
         * Shows this dialog. If the user chose a release, a list with the songs is returned,
         * otherwise null is returned.
         */
        internal List<SongModel>? OpenDialog()
        {
            ShowDialog();

            return _retrievedSongs;
        }

        /**
         * Is called if the selection in the release table has changed
         */
        private void OnReleaseSelected(object sender, SelectionChangedEventArgs e)
        {
            buttonOk.IsEnabled = (releaseTable.SelectedItem != null);
        }

        /**
         * Called if the user wants to use the selected release from discogs
         */
        private void OnUseSelectedReleaseClicked(object sender, EventArgs e)
        {
            Release? release = releaseTable.SelectedItem as Release;

            if (release != null)
            {
                _retrievedSongs = new();
                
                for (int i=0; i < release.TrackList.Count; i++)
                {
                    Track track = release.TrackList[i];
                    SongModel song = new();

                    song.Name = track.Title;
                    song.Length = ConvertTimeStringToSeconds(track.Duration);
                    song.Side = ConvertPositionToSide(track.Position);
                    song.Index = ConvertPositionToIndex(track.Position);

                    _retrievedSongs.Add(song);
                }
            }

            Close();    
        }

        /**
         * Called to cancel this dialog
         */
        private void OnCancelClicked(object sender, EventArgs e) 
        {
            Close();   
        }

        /**
         * Extracts the index of a vinyl album from the discogs position and returns it
         */
        private static int ConvertPositionToIndex(string position)
        {
            try
            {
                string strIndex = position.Substring(1);
                if (int.TryParse(strIndex, out int index))
                {
                    return index;
                }
            }
            catch (Exception) { }

            return 0;
        }

        /**
         * Extracts the side of a vinyl album from the discogs position and returns it
         */
        private static AlbumSideEnum ConvertPositionToSide(string position)
        {
            AlbumSideEnum side = AlbumSideEnum.A;

            if (position.ToLower().StartsWith("b")) 
            {
                side = AlbumSideEnum.B;
            } else if (position.ToLower().StartsWith("c"))
            {
                side = AlbumSideEnum.C;
            } else if (position.ToLower().StartsWith("d"))
            {
                side = AlbumSideEnum.D;
            }

            return side;
        }

        /**
         * Converts a discogs time string to seconds
         */
        private static int ConvertTimeStringToSeconds(string timeString)
        {
            TimeSpan timeSpan;

            if (timeString.IndexOf(':') == 1)
            {
                timeString = "0" + timeString;
            }

            if (TimeSpan.TryParseExact(timeString, "mm':'ss", null, out timeSpan))
            {
                return (int)timeSpan.TotalSeconds;
            }
            else
            {
                return 0;
            }
        }
    }
}
