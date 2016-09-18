using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MyAnimePlugin3.JMMServerBinary;

namespace MyAnimePlugin3.ViewModel
{
	public class VideoDetailedVM : IVideoInfo
	{
		public int AnimeEpisodeID { get; set; }

		// Import Folder
		public int ImportFolderID { get; set; }
		public string ImportFolderName { get; set; }
		public string ImportFolderLocation { get; set; }

		// CrossRef_File_Episode
		public int Percentage { get; set; }
		public int EpisodeOrder { get; set; }
        //public int CrossRefSource { get; set; }
        // VideoLocal
        public int VideoLocalID { get; set; }
        public string VideoLocal_FileName { get; set; }
        public string VideoLocal_Hash { get; set; }
        public long VideoLocal_FileSize { get; set; }
        public long VideoLocal_ResumePosition { get; set; }
        public DateTime? VideoLocal_WatchedDate { get; set; }
        public int VideoLocal_IsVariation { get; set; }
        //public long VideoLocal_IsWatched { get; set; }
        public string VideoLocal_CRC32 { get; set; }
        public string VideoLocal_MD5 { get; set; }
        public string VideoLocal_SHA1 { get; set; }
        public int VideoLocal_HashSource { get; set; }

        // VideoInfo
        public int VideoInfo_VideoInfoID { get; set; }
        public string VideoInfo_VideoCodec { get; set; }
        public string VideoInfo_VideoBitrate { get; set; }
        public string VideoInfo_VideoBitDepth { get; set; }
        public string VideoInfo_VideoFrameRate { get; set; }
        public string VideoInfo_VideoResolution { get; set; }
        public string VideoInfo_AudioCodec { get; set; }
        public string VideoInfo_AudioBitrate { get; set; }
        public long VideoInfo_Duration { get; set; }

        // AniDB_File
        public int? AniDB_FileID { get; set; }
        public int? AniDB_AnimeID { get; set; }
        public int? AniDB_GroupID { get; set; }
        public string AniDB_File_Source { get; set; }
        public string AniDB_File_AudioCodec { get; set; }
        public string AniDB_File_VideoCodec { get; set; }
        public string AniDB_File_VideoResolution { get; set; }
        public string AniDB_File_FileExtension { get; set; }
        public int? AniDB_File_LengthSeconds { get; set; }
        public string AniDB_File_Description { get; set; }
        public int? AniDB_File_ReleaseDate { get; set; }
        public string AniDB_Anime_GroupName { get; set; }
        public string AniDB_Anime_GroupNameShort { get; set; }
        public int? AniDB_Episode_Rating { get; set; }
        public int? AniDB_Episode_Votes { get; set; }
        public string AniDB_CRC { get; set; }
        public string AniDB_MD5 { get; set; }
        public string AniDB_SHA1 { get; set; }
        public int AniDB_File_FileVersion { get; set; }

        public Media Media { get; set; }

        // Places
        public List<VideoLocal_PlaceVM> Places { get; set; }



        public ReleaseGroupVM ReleaseGroup { get; set; }


        public string VideoResolution
		{
			get
			{
				if (AniDB_File_VideoResolution.Length > 0) return AniDB_File_VideoResolution;
				else return VideoInfo_VideoResolution;
			}
		}
        public bool? IsLocalOrStreaming()
        {
            bool? result;
            if (FileIsAvailable)
                result = false;
            else if (Media?.Parts != null && Media.Parts.Count > 0)
                result = true;
            else
                result = null;
            BaseConfig.MyAnimeLog.Write("Stream or File ? : " + ((result == null) ? "Nothing" : (result == true ? "Streaming" : "File")));
            return result;

        }
        public string VideoCodec
		{
			get
			{
				if (AniDB_File_VideoCodec.Length > 0) return AniDB_File_VideoCodec;
				else return VideoInfo_VideoCodec;
			}
		}

		public string AudioCodec
		{
			get
			{
				if (AniDB_File_AudioCodec.Length > 0) return AniDB_File_AudioCodec;
				else return VideoInfo_AudioCodec;
			}
		}

		// Languages
		public string LanguagesAudio { get; set; }
		public string LanguagesSubtitle { get; set; }


		#region Editable members

		private int crossRefSource = 0;
		public int CrossRefSource
		{
			get { return crossRefSource; }
			set
			{
				crossRefSource = value;
				IsManualAssociation = (crossRefSource != 1);
				IsAutoAssociation = (crossRefSource == 1);
			}
		}

		private bool isManualAssociation = false;
		public bool IsManualAssociation
		{
			get { return isManualAssociation; }
			set
			{
				isManualAssociation = value;
			}
		}

		private bool isAutoAssociation = false;
		public bool IsAutoAssociation
		{
			get { return isAutoAssociation; }
			set
			{
				isAutoAssociation = value;
			}
		}

		private int videoLocal_IsWatched = 0;
		public int VideoLocal_IsWatched
		{
			get { return videoLocal_IsWatched; }
			set
			{
				videoLocal_IsWatched = value;
				Watched = videoLocal_IsWatched == 1;
				Unwatched = videoLocal_IsWatched == 0;
			}
		}

		private int videoLocal_IsIgnored = 0;
		public int VideoLocal_IsIgnored
		{
			get { return videoLocal_IsIgnored; }
			set
			{
				videoLocal_IsIgnored = value;
				Ignored = videoLocal_IsIgnored == 1;
			}
		}

		private bool ignored = false;
		public bool Ignored
		{
			get { return ignored; }
			set
			{
				ignored = value;
			}
		}

		private bool watched = false;
		public bool Watched
		{
			get { return watched; }
			set
			{
				watched = value;
			}
		}

		private bool unwatched = false;
		public bool Unwatched
		{
			get { return unwatched; }
			set
			{
				unwatched = value;
			}
		}

        #endregion

        public string FileName => VideoLocal_FileName;

	    public string LocalFileSystemFullPath => FullPath;

	    public string FullPath
        {
            get
            {
                VideoLocal_PlaceVM b = Places?.FirstOrDefault(a => a.ImportFolderType == 0 && !string.IsNullOrEmpty(a.LocalFileSystemFullPath));
                if (b == null)
                    return string.Empty;
                return b.LocalFileSystemFullPath;
            }
        }
        public bool FileIsAvailable
        {
            get
            {
                if (!string.IsNullOrEmpty(LocalFileSystemFullPath))
                    return File.Exists(LocalFileSystemFullPath);
                return false;

            }
        }

        public bool FileIsNotAvailable
        {
            get
            {
                if (string.IsNullOrEmpty(LocalFileSystemFullPath))
                    return false;
                return !File.Exists(LocalFileSystemFullPath);
            }
        }


        public string VideoInfoSummary
		{
			get
			{
				return string.Format("{0} ({1}) - {2}", VideoResolution, VideoCodec, AudioCodec);
			}
		}

		public string FormattedFileSize
		{
			get
			{
				return Utils.FormatFileSize(VideoLocal_FileSize);
			}
		}

		#region Video Properties

		public bool IsBluRay
		{
			get
			{
				return AniDB_File_Source.ToUpper().Contains("BLU");
			}
		}

		public bool IsDVD
		{
			get
			{
				return AniDB_File_Source.ToUpper().Contains("DVD");
			}
		}

		public bool IsHD
		{
			get
			{
				return (GetVideoWidth() >= 1280 && GetVideoWidth() < 1920);
			}
		}

		public bool IsFullHD
		{
			get
			{
				return (GetVideoWidth() >= 1920);
			}
		}

		public int GetVideoWidth()
		{
			int videoWidth = 0;
			if (AniDB_File_VideoResolution.Trim().Length > 0)
			{
				string[] dimensions = AniDB_File_VideoResolution.Split('x');
				if (dimensions.Length > 0) int.TryParse(dimensions[0], out videoWidth);
			}
			return videoWidth;
		}

		public int GetVideoHeight()
		{
			int videoHeight = 0;
			if (AniDB_File_VideoResolution.Trim().Length > 0)
			{
				string[] dimensions = AniDB_File_VideoResolution.Split('x');
				if (dimensions.Length > 1) int.TryParse(dimensions[1], out videoHeight);
			}
			return videoHeight;
		}

		#endregion

		public bool HasReleaseGroup
		{
			get
			{
				return ReleaseGroup != null;
			}
		}

		public string ReleaseGroupName
		{
			get
			{
				if (ReleaseGroup != null)
					return ReleaseGroup.GroupName;
				else
					return "";
			}
		}

		public string ReleaseGroupAniDBURL
		{
			get
			{
				if (ReleaseGroup != null)
					return string.Format(Constants.URLS.AniDB_ReleaseGroup, ReleaseGroup.GroupID);
				else
					return "";
			}
		}

		public bool HasAniDBFile
		{
			get
			{
				return AniDB_FileID.HasValue;
			}
		}

		public string AniDB_SiteURL
		{
			get
			{
				if (AniDB_FileID.HasValue)
					return string.Format(Constants.URLS.AniDB_File, AniDB_FileID.Value);
				else
					return "";

			}
		}

		public VideoDetailedVM()
		{
			ReleaseGroup = null;
		}

		public void Populate(JMMServerBinary.Contract_VideoDetailed contract)
		{
            ReleaseGroup = null;

            this.AnimeEpisodeID = contract.AnimeEpisodeID;

            this.Places = contract.Places.Select(a => new VideoLocal_PlaceVM(a)).ToList();
            this.Percentage = contract.Percentage;
            this.EpisodeOrder = contract.EpisodeOrder;
            this.CrossRefSource = contract.CrossRefSource;

            this.VideoLocalID = contract.VideoLocalID;
            this.VideoLocal_FileName = contract.VideoLocal_FileName;
            this.VideoLocal_ResumePosition = contract.VideoLocal_ResumePosition;

            this.VideoLocal_Hash = contract.VideoLocal_Hash;
            this.VideoLocal_FileSize = contract.VideoLocal_FileSize;
            this.VideoLocal_IsWatched = contract.VideoLocal_IsWatched;
            this.VideoLocal_WatchedDate = contract.VideoLocal_WatchedDate;

            this.VideoLocal_IsIgnored = contract.VideoLocal_IsIgnored;
            this.VideoLocal_IsVariation = contract.VideoLocal_IsVariation;
            this.VideoLocal_MD5 = contract.VideoLocal_MD5;
            this.VideoLocal_SHA1 = contract.VideoLocal_SHA1;
            this.VideoLocal_CRC32 = contract.VideoLocal_CRC32;
            this.VideoLocal_HashSource = contract.VideoLocal_HashSource;


            this.VideoInfo_VideoCodec = contract.VideoInfo_VideoCodec;
            this.VideoInfo_VideoBitrate = contract.VideoInfo_VideoBitrate;
            this.VideoInfo_VideoBitDepth = contract.VideoInfo_VideoBitDepth;
            this.VideoInfo_VideoFrameRate = contract.VideoInfo_VideoFrameRate;
            this.VideoInfo_VideoResolution = contract.VideoInfo_VideoResolution;
            this.VideoInfo_AudioCodec = contract.VideoInfo_AudioCodec;
            this.VideoInfo_AudioBitrate = contract.VideoInfo_AudioBitrate;
            this.VideoInfo_Duration = contract.VideoInfo_Duration;

            this.AniDB_Anime_GroupName = contract.AniDB_Anime_GroupName;
            this.AniDB_Anime_GroupNameShort = contract.AniDB_Anime_GroupNameShort;
            this.AniDB_AnimeID = contract.AniDB_AnimeID;
            this.AniDB_CRC = contract.AniDB_CRC;
            this.AniDB_Episode_Rating = contract.AniDB_Episode_Rating;
            this.AniDB_Episode_Votes = contract.AniDB_Episode_Votes;
            this.AniDB_File_AudioCodec = contract.AniDB_File_AudioCodec;
            this.AniDB_File_Description = contract.AniDB_File_Description;
            this.AniDB_File_FileExtension = contract.AniDB_File_FileExtension;
            this.AniDB_File_LengthSeconds = contract.AniDB_File_LengthSeconds;
            this.AniDB_File_ReleaseDate = contract.AniDB_File_ReleaseDate;
            this.AniDB_File_Source = contract.AniDB_File_Source;
            this.AniDB_File_VideoCodec = contract.AniDB_File_VideoCodec;
            this.AniDB_File_VideoResolution = contract.AniDB_File_VideoResolution;
            this.AniDB_FileID = contract.AniDB_FileID;
            this.AniDB_GroupID = contract.AniDB_GroupID;
            this.AniDB_MD5 = contract.AniDB_MD5;
            this.AniDB_SHA1 = contract.AniDB_SHA1;
            this.AniDB_File_FileVersion = contract.AniDB_File_FileVersion;

            this.LanguagesAudio = contract.LanguagesAudio;
            this.LanguagesSubtitle = contract.LanguagesSubtitle;
            this.Media = contract.Media;
            if (contract.ReleaseGroup != null)
                this.ReleaseGroup = new ReleaseGroupVM(contract.ReleaseGroup);

        }

        public VideoDetailedVM(JMMServerBinary.Contract_VideoDetailed contract)
		{
			Populate(contract);
			
		}

		public FileFfdshowPresetVM FileFfdshowPreset
		{
			get
			{
				JMMServerBinary.Contract_FileFfdshowPreset contract = JMMServerVM.Instance.clientBinaryHTTP.GetFFDPreset(this.VideoLocalID);
				if (contract == null) return null;

				return new FileFfdshowPresetVM(contract);
			}
		}

		public string FileSelectionDisplay
		{
			get
			{
				try
				{
					string ret = BaseConfig.Settings.fileSelectionDisplayFormat;

					ret = ret.Replace(Constants.FileSelectionDisplayString.AudioCodec, this.AudioCodec);
					ret = ret.Replace(Constants.FileSelectionDisplayString.FileCodec, this.VideoCodec);
					ret = ret.Replace(Constants.FileSelectionDisplayString.FileRes, this.VideoResolution);
					ret = ret.Replace(Constants.FileSelectionDisplayString.VideoBitDepth, this.VideoInfo_VideoBitDepth);

					ret = ret.Replace(Constants.FileSelectionDisplayString.FileSource, AniDB_File_Source.Trim());
					ret = ret.Replace(Constants.FileSelectionDisplayString.Group, AniDB_Anime_GroupName.Trim());
					ret = ret.Replace(Constants.FileSelectionDisplayString.GroupShort, AniDB_Anime_GroupNameShort.Trim());

					return ret;
				}
				catch (Exception ex)
				{
					BaseConfig.MyAnimeLog.Write("Error in FileSelectionDisplay: {0}", ex);
					return "";
				}
			}
		}

		public string MediaInfoDisplay
		{
			get
			{
				string retString = "";
				retString += this.VideoResolution;
				retString += " (" + this.VideoCodec + ")";

				return retString;
			}

		}
	}
}
