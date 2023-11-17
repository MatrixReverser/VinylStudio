﻿using discogsharp;
using discogsharp.Domain;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VinylStudio
{
    public class DiscogsClient
    {
        private const string TOKEN = "hhyQnFcMGcTYmTGxzbPRzrbmpTDCCKuzYsJFufqW";
        private const int MAX_RELEASE_DOWNLOADS = 10;

        private ObservableCollection<Release> _releaseList = new();

        public ObservableCollection<Release> ReleaseList
        {
            get
            {
                return _releaseList;
            }
        }

        /**
         * Constructor of the class
         */
        public DiscogsClient(string interpret, string album)
        {
            GetAlbums(interpret, album);   
        }        

        /**
         * gets all releases (limited to 10) and stores them in _releaseList
         */
        private async void GetAlbums(string interpret, string album)
        {
            var discogsConnection = DiscogsAuthConnection.WithPersonalAccessToken(TOKEN);
            var databaseService = discogsConnection.CreateDatabaseService();

            var filter = new SearchFilter()
            {
                Artist = interpret,
                ReleaseTitle = album,
                Format = "vinyl"
            };
            var searchResult = await databaseService.SearchAsync(filter);

            // get the concrete releases up to 10 (we don't want reach the limit of discogs)
            int maxItemsToDownload = (searchResult.Items.Count <= 10 ? searchResult.Items.Count : MAX_RELEASE_DOWNLOADS);

            for (int i=0; i<maxItemsToDownload; i++)
            {
                if (searchResult.Items.ElementAt(i) is ReleaseSearchResult)
                {
                    ReleaseSearchResult release = (ReleaseSearchResult)searchResult.Items.ElementAt(i);
                    
                    var concreteRelease = await databaseService.GetReleaseAsync(release.Id);
                    if (concreteRelease != null)
                    {
                        _releaseList.Add(concreteRelease);
                    }
                }
            }
        }
    }
}
