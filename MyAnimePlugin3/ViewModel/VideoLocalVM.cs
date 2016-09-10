using System;
using System.Collections.Generic;
using System.Linq;
using MyAnimePlugin3.JMMServerBinary;

namespace MyAnimePlugin3.ViewModel
{
	public class VideoLocalVM : IVideoInfo
	{
		public int VideoLocalID { get; set; }
		public string FileName { get; set; }
		public int ImportFolderID { get; set; }
		public string Hash { get; set; }
		public string CRC32 { get; set; }
		public string MD5 { get; set; }
		public string SHA1 { get; set; }
		public int HashSource { get; set; }
		public long FileSize { get; set; }
		public int IsWatched { get; set; }
		public int IsIgnored { get; set; }
        public int IsVariation { get; set; }

        public List<VideoLocal_PlaceVM> Places { get; set; }
        public DateTime? WatchedDate { get; set; }
        public long ResumePosition { get; set; }
		public DateTime DateTimeUpdated { get; set; }
        public Media Media { get; set; }
		public ImportFolderVM ImportFolder { get; set; }
        public long Duration { get; set; }

        public bool? IsLocalOrStreaming()
        {
            if (FileIsAvailable)
                return false;
            if (Media?.Parts != null && Media.Parts.Count > 0)
                return true;
            return null;
        }
        public bool FileIsAvailable => !string.IsNullOrEmpty(LocalFileSystemFullPath);


        public string FileDirectory
        {
            get { return string.Join(",", Places.Select(a => a.FileDirectory)); }
        }

        public string LocalFileSystemFullPath
        {
            get
            {
                VideoLocal_PlaceVM b = Places?.FirstOrDefault(a => !string.IsNullOrEmpty(a.LocalFileSystemFullPath));
                if (b == null)
                    return string.Empty;
                return b.LocalFileSystemFullPath;
            }
        }



		public string FormattedFileSize => Utils.FormatFileSize(FileSize);

	    public VideoLocalVM()
		{
		}

		public VideoLocalVM(JMMServerBinary.Contract_VideoLocal contract)
		{
            this.CRC32 = contract.CRC32;
            this.DateTimeUpdated = contract.DateTimeUpdated;
            this.FileName = contract.FileName;
            this.FileSize = contract.FileSize;
            this.Hash = contract.Hash;
            this.HashSource = contract.HashSource;
            this.IsWatched = contract.IsWatched;
            this.IsIgnored = contract.IsIgnored;
            this.IsVariation = contract.IsVariation;
            this.ResumePosition = contract.ResumePosition;
            this.MD5 = contract.MD5;
            this.SHA1 = contract.SHA1;
            this.VideoLocalID = contract.VideoLocalID;
            this.WatchedDate = contract.WatchedDate;
            this.Media = contract.Media;
            this.Duration = contract.Duration;
            this.Places = contract.Places.Select(a => new VideoLocal_PlaceVM(a)).ToList();
        }

		public List<AnimeEpisodeVM> GetEpisodes()
		{
			List<AnimeEpisodeVM> eps = new List<AnimeEpisodeVM>();

			try
			{
				List<JMMServerBinary.Contract_AnimeEpisode> epContracts = JMMServerVM.Instance.clientBinaryHTTP.GetEpisodesForFile(this.VideoLocalID,
					JMMServerVM.Instance.CurrentUser.JMMUserID);
				foreach (JMMServerBinary.Contract_AnimeEpisode epcontract in epContracts)
				{
					AnimeEpisodeVM ep = new AnimeEpisodeVM(epcontract);
					eps.Add(ep);
				}
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}

			return eps;
		}

		public string DefaultAudioLanguage
		{
			get
			{
				List<AnimeEpisodeVM> eps = GetEpisodes();
				if (eps.Count == 0) return string.Empty;

				return eps[0].DefaultAudioLanguage;
			}
		}

		public string DefaultSubtitleLanguage
		{
			get
			{
				List<AnimeEpisodeVM> eps = GetEpisodes();
				if (eps.Count == 0) return string.Empty;

				return eps[0].DefaultSubtitleLanguage;
			}
		}
	}
}
