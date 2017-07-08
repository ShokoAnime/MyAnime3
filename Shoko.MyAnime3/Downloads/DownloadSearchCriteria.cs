using System.Collections.Generic;
using Shoko.MyAnime3.ViewModel.Helpers;
using Shoko.MyAnime3.ViewModel.Server;

namespace Shoko.MyAnime3.Downloads
{
    public class DownloadSearchCriteria
    {
        public DownloadSearchType SearchType { get; set; }

        public object SearchParameter { get; set; }

        public DownloadSearchCriteria(DownloadSearchType sType, object parm)
        {
            SearchType = sType;
            SearchParameter = parm;
        }

        public List<string> GetParms()
        {
            List<string> parms = new List<string>();

            if (SearchType == DownloadSearchType.Episode)
            {
                VM_AnimeEpisode_User ep = (VM_AnimeEpisode_User) SearchParameter;

                VM_AnimeSeries_User series = ShokoServerHelper.GetSeries(ep.AnimeSeriesID);
                if (series == null) return parms;

                VM_AniDB_Anime anime = series.Anime;
                if (anime == null) return parms;

                // only use the first 2 words of the anime's title
                string[] titles = anime.MainTitle.Split(' ');
                int i = 0;
                foreach (string s in titles)
                {
                    i++;
                    parms.Add(s.Trim());
                    if (i == 2) break;
                }

                parms.Add(ep.EpisodeNumber.ToString().PadLeft(2, '0'));
            }

            if (SearchType == DownloadSearchType.Series)
            {
                VM_AniDB_Anime anime = (VM_AniDB_Anime) SearchParameter;

                // only use the first 2 words of the anime's title
                string[] titles = anime.MainTitle.Split(' ');
                int i = 0;
                foreach (string s in titles)
                {
                    i++;
                    parms.Add(s.Trim());
                    if (i == 2) break;
                }
            }

            if (SearchType == DownloadSearchType.Manual)
            {
                string[] titles = SearchParameter.ToString().Split(' ');
                foreach (string s in titles)
                    parms.Add(s.Trim());
            }

            return parms;
        }

        public override string ToString()
        {
            string ret = "";

            switch (SearchType)
            {
                case DownloadSearchType.Episode:
                    ret = Translation.Episode;
                    break;
                case DownloadSearchType.Manual:
                    ret = Translation.Manual;
                    break;
                case DownloadSearchType.Series:
                    ret = Translation.Anime;
                    break;
            }

            ret += ": ";

            int i = 0;
            List<string> parms = GetParms();
            foreach (string parm in parms)
            {
                i++;
                ret += parm;
                if (i < parms.Count) ret += " + ";
            }

            return ret;
        }
    }
}