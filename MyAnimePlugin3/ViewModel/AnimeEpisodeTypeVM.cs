using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAnimePlugin3.ViewModel
{
	public class AnimeEpisodeTypeVM 
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
			EpisodeType = (enEpisodeType)ep.EpisodeType;
			EpisodeTypeDescription = AnimeEpisodeTypeVM.EpisodeTypeTranslated(EpisodeType);
		}


		public static string EpisodeTypeTranslated(enEpisodeType epType)
		{
			switch (epType)
			{
				case enEpisodeType.Credits:
					return "Credits";
				case enEpisodeType.Episode:
					return "Episodes";
				case enEpisodeType.Other:
					return "Other";
				case enEpisodeType.Parody:
					return "Parody";
				case enEpisodeType.Special:
					return "Specials";
				case enEpisodeType.Trailer:
					return "Trailers";
				default:
					return "Other";

			}
		}
	}
}
