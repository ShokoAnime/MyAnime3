using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAnimePlugin3.ViewModel
{
	public class AniDB_Anime_RelationVM
	{
		public int AnimeID { get; set; }
		public int AniDB_Anime_RelationID { get; set; }
		public string RelationType { get; set; }
		public int RelatedAnimeID { get; set; }
		public bool IsSequel { get; set; }
		public bool IsPrequel { get; set; }

		public AniDB_AnimeVM AniDB_Anime { get; set; }
		public AnimeSeriesVM AnimeSeries { get; set; }

		private bool localSeriesExists = false;
		public bool LocalSeriesExists
		{
			get { return localSeriesExists; }
			set
			{
				localSeriesExists = value;
			}
		}

		private bool animeInfoExists = false;
		public bool AnimeInfoExists
		{
			get { return animeInfoExists; }
			set
			{
				animeInfoExists = value;
			}
		}

		private string displayName = "";
		public string DisplayName
		{
			get { return displayName; }
			set
			{
				displayName = value;
			}
		}

		private string posterPath = "";
		public string PosterPath
		{
			get { return posterPath; }
			set
			{
				posterPath = value;
			}
		}

		private int sortPriority = 100;
		public int SortPriority
		{
			get { return sortPriority; }
			set
			{
				sortPriority = value;
			}
		}


		public AniDB_Anime_RelationVM()
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
				AnimeInfoExists = true;
				PosterPath = AniDB_Anime.PosterPath;
			}
			else
			{
				DisplayName = "Data Missing";
				AnimeInfoExists = false;
				PosterPath = "";
			}


			if (AnimeSeries != null)
				LocalSeriesExists = true;
			else
				LocalSeriesExists = false;

		}

		public void Populate(JMMServerBinary.Contract_AniDB_Anime_Relation details)
		{
			this.AniDB_Anime_RelationID = details.AniDB_Anime_RelationID;
			this.AnimeID = details.AnimeID;
			this.RelationType = details.RelationType;
			this.RelatedAnimeID = details.RelatedAnimeID;

			IsPrequel = false;
			IsSequel = false;

			SortPriority = int.MaxValue;
			if (RelationType.Equals("Prequel", StringComparison.InvariantCultureIgnoreCase))
			{
				IsPrequel = true;
				SortPriority = 1;
			}
			if (RelationType.Equals("Sequel", StringComparison.InvariantCultureIgnoreCase))
			{
				IsSequel = true;
				SortPriority = 2;
			}

			PopulateAnime(details.AniDB_Anime);
			PopulateSeries(details.AnimeSeries);
		}
	}
}
