﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;

namespace MPExtended.Services.MediaAccessService.Interfaces.TVShow
{
    public class WebTVShowBasic : ITitleSortable, IDateAddedSortable, IYearSortable, IGenreSortable, ICategorySortable
    {
        public WebTVShowBasic()
        {
            DateAdded = new DateTime(1970, 1, 1);
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public bool IsProtected { get; set; }
        public DateTime DateAdded { get; set; }
        public int Year { get; set; }
        public int EpisodeCount { get; set; }
        public int UnwatchedEpisodeCount { get; set; }
        public IList<string> BannerPaths { get; set; }
        public IList<string> UserDefinedCategories { get; set; }
        public IList<string> Genres { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}