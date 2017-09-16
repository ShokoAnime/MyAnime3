using Shoko.Models.Enums;
using Shoko.MyAnime3.ViewModel.Helpers;
using Shoko.MyAnime3.ViewModel.Server;

namespace Shoko.MyAnime3.ViewModel
{
    public class VM_AnimeEpisodeType : IVM
    {
        public EpisodeType EpisodeType { get; set; }
        public string EpisodeTypeDescription { get; set; }
        public VM_AnimeSeries_User AnimeSeries { get; set; }


        public VM_AnimeEpisodeType()
        {
        }

        public VM_AnimeEpisodeType(VM_AnimeSeries_User series, VM_AnimeEpisode_User ep)
        {
            AnimeSeries = series;
            EpisodeType = (EpisodeType) ep.EpisodeType;
            EpisodeTypeDescription = EpisodeTypeTranslated(EpisodeType);
        }


        public static string EpisodeTypeTranslated(EpisodeType epType)
        {
            switch (epType)
            {
                case EpisodeType.Credits:
                    return Translation.Credits;
                case EpisodeType.Episode:
                    return Translation.Episodes;
                case EpisodeType.Other:
                    return Translation.Other;
                case EpisodeType.Parody:
                    return Translation.Parody;
                case EpisodeType.Special:
                    return Translation.Specials;
                case EpisodeType.Trailer:
                    return Translation.Trailers;
                default:
                    return Translation.Other;
            }
        }
    }
}