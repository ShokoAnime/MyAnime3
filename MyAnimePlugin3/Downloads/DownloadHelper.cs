
using System.Collections.Generic;

using MediaPortal.GUI.Library;
using MyAnimePlugin3.DataHelpers;
using MyAnimePlugin3.ViewModel;
using BinaryNorthwest;

namespace MyAnimePlugin3.Downloads
{
	public class DownloadHelper
	{
		public static void SearchEpisode(AnimeEpisodeVM ep)
		{
			MainWindow.currentDownloadSearch = new DownloadSearchCriteria(DownloadSearchType.Episode, ep);
			GUIWindowManager.ActivateWindow(Constants.WindowIDs.DOWNLOADS);
			return;
		}

		public static void SearchAnime(AniDB_AnimeVM anime)
		{
			MainWindow.currentDownloadSearch = new DownloadSearchCriteria(DownloadSearchType.Series, anime);
			GUIWindowManager.ActivateWindow(Constants.WindowIDs.DOWNLOADS);
			return;
		}

		public static List<TorrentLink> SearchTorrents(DownloadSearchCriteria search)
		{
			List<string> parms = search.GetParms();
			List<TorrentLink> links = new List<TorrentLink>();
			

			TorrentsAnimeSuki suki = new TorrentsAnimeSuki();
			TorrentsBakaUpdates bakau = new TorrentsBakaUpdates();

			List<string> episodeGroupParms = new List<string>();

			if (BaseConfig.Settings.TorrentPreferOwnGroups)
			{
				// lets do something special for episodes
				if (search.SearchType == DownloadSearchType.Episode)
				{
					AnimeEpisodeVM ep = search.SearchParameter as AnimeEpisodeVM;

					AnimeSeriesVM series = JMMServerHelper.GetSeries(ep.AnimeSeriesID);
					if (series != null && series.AniDB_Anime != null)
					{

						List<GroupVideoQualityVM> videoQualityRecords = new List<GroupVideoQualityVM>();
						List<JMMServerBinary.Contract_GroupVideoQuality> summ = JMMServerVM.Instance.clientBinaryHTTP.GetGroupVideoQualitySummary(series.AniDB_Anime.AnimeID);
						foreach (JMMServerBinary.Contract_GroupVideoQuality contract in summ)
						{
							GroupVideoQualityVM vidQual = new GroupVideoQualityVM(contract);
							videoQualityRecords.Add(vidQual);
						}

						// apply sorting
						if (videoQualityRecords.Count > 0)
						{
							List<SortPropOrFieldAndDirection> sortlist = new List<SortPropOrFieldAndDirection>();
							sortlist.Add(new SortPropOrFieldAndDirection("FileCountNormal", true, SortType.eInteger));
							videoQualityRecords = Sorting.MultiSort<GroupVideoQualityVM>(videoQualityRecords, sortlist);
						}

						//only use the first 2
						int i = 0;
						foreach (GroupVideoQualityVM gvq in videoQualityRecords)
						{
							if (i == 2) break;
							if (!episodeGroupParms.Contains(gvq.GroupNameShort))
							{
								episodeGroupParms.Add(gvq.GroupNameShort);
								i++;
							}
						}
					}
				}
			}

            foreach (string src in BaseConfig.Settings.TorrentSources)
			{
				if (src == MyAnimePlugin3.Constants.TorrentSourceNames.Nyaa)
				{
					TorrentsNyaa nyaa = new TorrentsNyaa();
					List<TorrentLink> ttLinks = null;
					Dictionary<string, TorrentLink> dictLinks = new Dictionary<string, TorrentLink>();

					foreach (string grp in episodeGroupParms)
					{
						List<string> tempParms = new List<string>();
						foreach (string parmTemp in parms)
							tempParms.Add(parmTemp);
						tempParms.Insert(0, grp);
						ttLinks = nyaa.GetTorrents(tempParms);

						BaseConfig.MyAnimeLog.Write("Searching for: " + search.ToString() + "(" + grp + ")");

						// only use the first 20
						int x = 0;
						foreach (TorrentLink link in ttLinks)
						{
							if (x == 20) break;
							dictLinks[link.TorrentDownloadLink] = link;
							BaseConfig.MyAnimeLog.Write("Adding link: " + link.ToString());
						}
					}

					BaseConfig.MyAnimeLog.Write("Searching for: " + search.ToString());
					ttLinks = nyaa.GetTorrents(parms);
					foreach (TorrentLink link in ttLinks)
					{
						dictLinks[link.TorrentDownloadLink] = link;
						BaseConfig.MyAnimeLog.Write("Adding link: " + link.ToString());
					}

					links.AddRange(dictLinks.Values);
				}

				if (src == MyAnimePlugin3.Constants.TorrentSourceNames.AnimeSuki)
				{
					List<TorrentLink> sukiLinks = suki.GetTorrents(parms);
					links.AddRange(sukiLinks);
				}

				if (src == MyAnimePlugin3.Constants.TorrentSourceNames.TT)
				{
					TorrentsTokyoToshokan tt = new TorrentsTokyoToshokan();
					List<TorrentLink> ttLinks = null;
					Dictionary<string, TorrentLink> dictLinks = new Dictionary<string, TorrentLink>();

					foreach (string grp in episodeGroupParms)
					{
						List<string> tempParms = new List<string>();
						foreach (string parmTemp in parms)
							tempParms.Add(parmTemp);
						tempParms.Insert(0, grp);
						ttLinks = tt.GetTorrents(tempParms);

						BaseConfig.MyAnimeLog.Write("Searching for: " + search.ToString() + "(" + grp + ")");

						// only use the first 20
						int x = 0;
						foreach (TorrentLink link in ttLinks)
						{
							if (x == 20) break;
							dictLinks[link.TorrentDownloadLink] = link;
							BaseConfig.MyAnimeLog.Write("Adding link: " + link.ToString());
						}
					}

					BaseConfig.MyAnimeLog.Write("Searching for: " + search.ToString());
					ttLinks = tt.GetTorrents(parms);
					foreach (TorrentLink link in ttLinks)
					{
						dictLinks[link.TorrentDownloadLink] = link;
						BaseConfig.MyAnimeLog.Write("Adding link: " + link.ToString());
					}

					links.AddRange(dictLinks.Values);
				}

				if (src == MyAnimePlugin3.Constants.TorrentSourceNames.BakaUpdates)
				{
					List<TorrentLink> bakauLinks = bakau.GetTorrents(parms);
					links.AddRange(bakauLinks);
				}
			}

			

			return links;
		}

		public static List<TorrentLink> BrowseTorrents(TorrentSource source)
		{
			List<TorrentLink> links = new List<TorrentLink>();

			if (source == TorrentSource.Nyaa)
			{
				TorrentsNyaa nyaa = new TorrentsNyaa();
				List<TorrentLink> ttLinks = nyaa.BrowseTorrents();
				links.AddRange(ttLinks);
			}

			if (source == TorrentSource.TokyoToshokan)
			{
				TorrentsTokyoToshokan tt = new TorrentsTokyoToshokan();
				List<TorrentLink> ttLinks = tt.BrowseTorrents();
				links.AddRange(ttLinks);
			}

			if (source == TorrentSource.AnimeSuki)
			{
				TorrentsAnimeSuki suki = new TorrentsAnimeSuki();
				List<TorrentLink> sukiLinks = suki.BrowseTorrents();
				links.AddRange(sukiLinks);
			}

			if (source == TorrentSource.BakaUpdates)
			{
				TorrentsBakaUpdates bakau = new TorrentsBakaUpdates();
				List<TorrentLink> bakauLinks = bakau.BrowseTorrents();
				links.AddRange(bakauLinks);
			}

			return links;
		}

		public static string FixNyaaTorrentLink(string url)
		{
			// on some trackers the user will post the torrent page instead of the 
			// direct torrent link
			return url.Replace("page=torrentinfo", "page=download");
		}

		public static string GetTorrentSourceDescription(TorrentSource source)
		{
			switch (source)
			{
				case TorrentSource.AnimeSuki: return MyAnimePlugin3.Constants.TorrentSourceNames.AnimeSuki;
				case TorrentSource.BakaBT: return MyAnimePlugin3.Constants.TorrentSourceNames.BakaBT;
				case TorrentSource.BakaUpdates: return MyAnimePlugin3.Constants.TorrentSourceNames.BakaUpdates;
				case TorrentSource.Nyaa: return MyAnimePlugin3.Constants.TorrentSourceNames.Nyaa;
				case TorrentSource.TokyoToshokan: return MyAnimePlugin3.Constants.TorrentSourceNames.TT;
			}

			return "Undefined";
		}

		public static TorrentSource GetTorrentSourceEnum(string src)
		{
			if (src == MyAnimePlugin3.Constants.TorrentSourceNames.AnimeSuki) return TorrentSource.AnimeSuki;
			if (src == MyAnimePlugin3.Constants.TorrentSourceNames.BakaBT) return TorrentSource.BakaBT;
			if (src == MyAnimePlugin3.Constants.TorrentSourceNames.BakaUpdates) return TorrentSource.BakaUpdates;
			if (src == MyAnimePlugin3.Constants.TorrentSourceNames.Nyaa) return TorrentSource.Nyaa;
			if (src == MyAnimePlugin3.Constants.TorrentSourceNames.TT) return TorrentSource.TokyoToshokan;

			return TorrentSource.TokyoToshokan;
		}
	}

	public enum TorrentSource
	{
		TokyoToshokan = 1,
		BakaBT = 2,
		Nyaa = 3,
		AnimeSuki = 4,
		BakaUpdates = 5
	}


	
}
