using Shoko.Models.Enums;
using Shoko.MyAnime3.ViewModel.Helpers;
using Shoko.MyAnime3.ViewModel.Server;

namespace Shoko.MyAnime3.ViewModel
{
    public class VM_AnimeEpisodeType : IVM
    {
        public enEpisodeType EpisodeType { get; set; }
        public string EpisodeTypeDescription { get; set; }
        public VM_AnimeSeries_User AnimeSeries { get; set; }


        public VM_AnimeEpisodeType()
        {
        }

        public VM_AnimeEpisodeType(VM_AnimeSeries_User series, VM_AnimeEpisode_User ep)
        {
            AnimeSeries = series;
            EpisodeType = (enEpisodeType) ep.EpisodeType;
            EpisodeTypeDescription = EpisodeTypeTranslated(EpisodeType);
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