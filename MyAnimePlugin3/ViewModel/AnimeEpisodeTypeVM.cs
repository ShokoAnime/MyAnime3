using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAnimePlugin3.ViewModel
{
    public class AnimeEpisodeTypeVM : IVM
    {
        public enEpisodeType EpisodeType { get; set; }
        public string EpisodeTypeDescription { get; set; }
        public AnimeSeriesVM AnimeSeries { get; set; }




        public AnimeEpisodeTypeVM()
        {

        }

        public AnimeEpisodeTypeVM(AnimeSeriesVM series, AnimeEpisodeVM ep)
        {
            AnimeSeries = series;
            EpisodeType = (enEpisodeType) ep.EpisodeType;
            EpisodeTypeDescription = AnimeEpisodeTypeVM.EpisodeTypeTranslated(EpisodeType);
        }


        public static string EpisodeTypeTranslated(enEpisodeType epType)
        {
            switch (epType)
            {
                case enEpisodeType.Credits:
                    return Translation.Credits;
                case enEpisodeType.Episode:
                    return Translation.Episodes;
                case enEpisodeType.Other:
                    return Translation.Other;
                case enEpisodeType.Parody:
                    return Translation.Parody;
                case enEpisodeType.Special:
                    return Translation.Specials;
                case enEpisodeType.Trailer:
                    return Translation.Trailers;
                default:
                    return Translation.Other;

            }
        }
    }
}
