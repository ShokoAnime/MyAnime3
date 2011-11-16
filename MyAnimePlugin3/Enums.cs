using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAnimePlugin3
{
	public enum GroupFilterConditionType
	{
		CompletedSeries = 1,
		MissingEpisodes = 2,
		HasUnwatchedEpisodes = 3,
		AllEpisodesWatched = 4,
		UserVoted = 5,
		Category = 6,
		AirDate = 7,
		Studio = 8,
		AssignedTvDBInfo = 9,
		ReleaseGroup = 11,
		AnimeType = 12,
		VideoQuality = 13,
		Favourite = 14,
		AnimeGroup = 15,
		AniDBRating = 16,
		UserRating = 17,
		SeriesCreatedDate = 18,
		EpisodeAddedDate = 19,
		EpisodeWatchedDate = 20,
		FinishedAiring = 21,
		MissingEpisodesCollecting = 22,
		AudioLanguage = 23,
		SubtitleLanguage = 24,
		AssignedTvDBOrMovieDBInfo = 25,
		AssignedMovieDBInfo = 26
	}

	public enum GroupFilterOperator
	{
		Include = 1,
		Exclude = 2,
		GreaterThan = 3,
		LessThan = 4,
		Equals = 5,
		NotEquals = 6,
		In = 7,
		NotIn = 8,
		LastXDays = 9,
		InAllEpisodes = 10,
		NotInAllEpisodes = 11
	}

	public enum GroupFilterSorting
	{
		SeriesAddedDate = 1,
		EpisodeAddedDate = 2,
		EpisodeAirDate = 3,
		EpisodeWatchedDate = 4,
		GroupName = 5,
		Year = 6,
		SeriesCount = 7,
		UnwatchedEpisodeCount = 8,
		MissingEpisodeCount = 9,
		UserRating = 10,
		AniDBRating = 11,
		SortName = 12
	}

	public enum GroupFilterSortDirection
	{
		Asc = 1,
		Desc = 2
	}

	public enum GroupFilterBaseCondition
	{
		Include = 1,
		Exclude = 2
	}

	public enum ImageEntityType
	{
		AniDB_Cover = 1, // use AnimeID
		AniDB_Character = 2, // use CharID
		AniDB_Creator = 3, // use CreatorID
		TvDB_Banner = 4, // use TvDB Banner ID
		TvDB_Cover = 5, // use TvDB Cover ID
		TvDB_Episode = 6, // use TvDB Episode ID
		TvDB_FanArt = 7, // use TvDB FanArt ID
		MovieDB_FanArt = 8,
		MovieDB_Poster = 9,
		Trakt_Poster = 10,
		Trakt_Fanart = 11,
		Trakt_Episode = 12
	}

	public enum ImageDownloadEventType
	{
		Started = 1,
		Complete = 2
	}

	public enum enAnimeType
	{
		Movie = 0,
		OVA = 1,
		TVSeries = 2,
		TVSpecial = 3,
		Web = 4,
		Other = 5
	}

	public enum enEpisodeType
	{
		Episode = 1,
		Credits = 2,
		Special = 3,
		Trailer = 4,
		Parody = 5,
		Other = 6
	}

	public enum ImageSizeType
	{
		Poster = 1,
		Fanart = 2,
		WideBanner = 3
	}

	public enum VoteType
	{
		AnimePermanent = 1,
		AnimeTemporary = 2
	}
}
