using discogsharp;
using discogsharp.Domain;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Diagnostics;
using VinylStudio.ui;

namespace VinylStudio.util
{
    public class DiscogsClient
    {
        private const int MAX_RELEASE_DOWNLOADS = 10;

        private ObservableCollection<Release> _releaseList = new();
        private ObservableCollection<string> _releaseNameList = new();
        private readonly string _token;

        public ObservableCollection<Release> ReleaseList
        {
            get
            {
                return _releaseList;
            }
        }

        public ObservableCollection<string> ReleaseNameList
        {
            get
            {
                return _releaseNameList;
            }
        }

        /**
         * Constructor of the class
         */
        public DiscogsClient(string token, string interpret, string album)
        {
            _token = token;
            GetAlbums(interpret, album);           
        }

        /**
         * gets all releases (limited to 10) and stores them in _releaseList
         */
        private async void GetAlbums(string interpret, string album)
        {
            var discogsConnection = DiscogsAuthConnection.WithPersonalAccessToken(_token);
            var databaseService = discogsConnection.CreateDatabaseService();
            
            var filter = new SearchFilter()
            {
                Artist = interpret,
                ReleaseTitle = album,
                Format = "vinyl"
            };

            try
            {
                var searchResult = await databaseService.SearchAsync(filter);

                // get the concrete releases up to 10 (we don't want reach the limit of discogs)
                int maxItemsToDownload = searchResult.Items.Count <= 10 ? searchResult.Items.Count : MAX_RELEASE_DOWNLOADS;

                for (int i = 0; i < maxItemsToDownload; i++)
                {
                    if (searchResult.Items.ElementAt(i) is ReleaseSearchResult)
                    {
                        ReleaseSearchResult release = (ReleaseSearchResult)searchResult.Items.ElementAt(i);
                        
                        var concreteRelease = await databaseService.GetReleaseAsync(release.Id);

                        if (concreteRelease != null)
                        {
                            _releaseList.Add(concreteRelease);
                            _releaseNameList.Add(extractReleaseName(concreteRelease));
                        }                        
                    }
                }
            } catch (Exception ex)
            {                
                VinylException exception = new VinylException(VinylExceptionType.DISCOGS_EXCEPTION, "Failed to get search results from Discogs", ex);
                ErrorDialog dialog = new ErrorDialog(exception);
                dialog.ShowDialog();
            }
        }

        /**
         * Extracts a human readable form of the given release
         */
        private string extractReleaseName(Release release)
        {
            string name = string.Empty;

            name += $"{release.Title} ({release.Year}): {release.Country}";

            return name;
        }
    }
}
