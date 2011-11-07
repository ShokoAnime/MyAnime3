using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.GUI.Library;

namespace MyAnimePlugin3.ViewModel
{
	public class RecommendationVM
	{
		public int RecommendedAnimeID { get; set; }
		public int BasedOnAnimeID { get; set; }
		public double Score { get; set; }
		public int BasedOnVoteValue { get; set; }
		public double RecommendedApproval { get; set; }

		public AniDB_AnimeVM Recommended_AniDB_Anime { get; set; }
		public AnimeSeriesVM Recommended_AnimeSeries { get; set; }

		public AniDB_AnimeVM BasedOn_AniDB_Anime { get; set; }
		public AnimeSeriesVM BasedOn_AnimeSeries { get; set; }

		public string Recommended_DisplayName { get; set; }
		public string Recommended_Description { get; set; }
		public string Recommended_AniDB_SiteURL { get; set; }
		public bool Recommended_LocalSeriesExists { get; set; }
		public bool Recommended_AnimeInfoExists { get; set; }
		public string Recommended_ApprovalRating { get; set; }
		public string Recommended_PosterPath { get; set; }

		public string BasedOn_DisplayName { get; set; }
		public string BasedOn_VoteValueFormatted { get; set; }
		public string BasedOn_AniDB_SiteURL { get; set; }
		public string BasedOn_PosterPath { get; set; }

		public void Populate(JMMServerBinary.Contract_Recommendation details)
		{
			this.RecommendedAnimeID = details.RecommendedAnimeID;
			this.BasedOnAnimeID = details.BasedOnAnimeID;
			this.Score = details.Score;
			this.BasedOnVoteValue = details.BasedOnVoteValue;
			this.RecommendedApproval = details.RecommendedApproval;

			Recommended_ApprovalRating = string.Format("{0}", Utils.FormatPercentage(RecommendedApproval));
			BasedOn_VoteValueFormatted = String.Format("{0:0.0}", (double)BasedOnVoteValue / (double)100);

			Recommended_AniDB_SiteURL = string.Format(Constants.URLS.AniDB_Series, RecommendedAnimeID);
			BasedOn_AniDB_SiteURL = string.Format(Constants.URLS.AniDB_Series, BasedOnAnimeID);

			PopulateRecommendedAnime(details.Recommended_AniDB_Anime);
			PopulateRecommendedSeries(details.Recommended_AnimeSeries);

			PopulateBasedOnAnime(details.BasedOn_AniDB_Anime);
			PopulateBasedOnSeries(details.BasedOn_AnimeSeries);
		}

		public void PopulateRecommendedAnime(JMMServerBinary.Contract_AniDBAnime animeContract)
		{
			if (animeContract != null)
				Recommended_AniDB_Anime = new AniDB_AnimeVM(animeContract);

			EvaluateProperties();
		}

		public void PopulateBasedOnAnime(JMMServerBinary.Contract_AniDBAnime animeContract)
		{
			if (animeContract != null)
				BasedOn_AniDB_Anime = new AniDB_AnimeVM(animeContract);

			EvaluateProperties();
		}

		public void PopulateRecommendedSeries(JMMServerBinary.Contract_AnimeSeries seriesContract)
		{
			if (seriesContract != null)
				Recommended_AnimeSeries = new AnimeSeriesVM(seriesContract);

			EvaluateProperties();
		}

		public void PopulateBasedOnSeries(JMMServerBinary.Contract_AnimeSeries seriesContract)
		{
			if (seriesContract != null)
				BasedOn_AnimeSeries = new AnimeSeriesVM(seriesContract);

			EvaluateProperties();
		}

		public void EvaluateProperties()
		{
			if (Recommended_AniDB_Anime != null)
			{
				Recommended_DisplayName = Recommended_AniDB_Anime.FormattedTitle;
				Recommended_AnimeInfoExists = true;
				Recommended_PosterPath = Recommended_AniDB_Anime.PosterPath;
				Recommended_Description = Recommended_AniDB_Anime.Description;
			}
			else
			{
				Recommended_DisplayName = "Data Missing";
				Recommended_AnimeInfoExists = false;
				Recommended_PosterPath = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_blankchar.png";
				Recommended_Description = "Overview not available";
			}

			if (BasedOn_AniDB_Anime != null)
			{
				BasedOn_DisplayName = BasedOn_AniDB_Anime.FormattedTitle;
				BasedOn_PosterPath = BasedOn_AniDB_Anime.PosterPath;
			}

			if (Recommended_AnimeSeries != null)
				Recommended_LocalSeriesExists = true;
			else
				Recommended_LocalSeriesExists = false;

		}
	}
}
