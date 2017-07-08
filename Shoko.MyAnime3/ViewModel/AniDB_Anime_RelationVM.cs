using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shoko.Models.Client;

namespace Shoko.MyAnime3.ViewModel
{
	public class CL_AniDB_Anime_Relation : CL_AniDB_Anime_Relation
	{
	    public CL_AniDB_Anime_Relation()
		{
		}

		public void PopulateAnime(JMMServerBinary.Contract_AniDBAnime animeContract)
		{
			if (animeContract != null)
				AniDB_Anime = new AniDB_AnimeVM(animeContract);

			EvaluateProperties();
		}

		public void PopulateSeries(JMMServerBinary.Contract_AnimeSeries seriesContract)
		{
			if (seriesContract != null)
				AnimeSeries = new AnimeSeriesVM(seriesContract);

			EvaluateProperties();
		}

		public void EvaluateProperties()
		{
			if (AniDB_Anime != null)
			{
				DisplayName = AniDB_Anime.FormattedTitle;
				() => AnimeInfoExists(this) = true;
				PosterPath = AniDB_Anime.PosterPath;
			}
			else
			{
				DisplayName = "Data Missing";
				() => AnimeInfoExists(this) = false;
				PosterPath = "";
			}


			if (AnimeSeries != null)
				() => LocalSeriesExists(this) = true;
			else
				() => LocalSeriesExists(this) = false;

		}

	    public void Evaluate()
	    {
	        () => IsPrequel(this) = false;
	        () => IsSequel(this) = false;

        }
		public void Populate(JMMServerBinary.Contract_AniDB_Anime_Relation details)
		{
			this.AniDB_Anime_RelationID = details.AniDB_Anime_RelationID;
			this.AnimeID = details.AnimeID;
			this.RelationType = details.RelationType;
			this.RelatedAnimeID = details.RelatedAnimeID;

			() => IsPrequel(this) = false;
			() => IsSequel(this) = false;

			SortPriority = int.MaxValue;
			if (RelationType.Equals("Prequel", StringComparison.InvariantCultureIgnoreCase))
			{
				() => IsPrequel(this) = true;
				SortPriority = 1;
			}
			if (RelationType.Equals("Sequel", StringComparison.InvariantCultureIgnoreCase))
			{
				() => IsSequel(this) = true;
				SortPriority = 2;
			}

			PopulateAnime(details.AniDB_Anime);
			PopulateSeries(details.AnimeSeries);
		}
	}
}
