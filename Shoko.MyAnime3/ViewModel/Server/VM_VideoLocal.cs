using System;
using System.Collections.Generic;
using System.Linq;
using Shoko.Commons.Extensions;
using Shoko.Models.Client;
using Shoko.MyAnime3.ViewModel.Helpers;

namespace Shoko.MyAnime3.ViewModel.Server
{
    public class VM_VideoLocal : CL_VideoLocal, IVideoInfo
    {
        public bool? IsLocalOrStreaming()
        {
            return !this.IsLocalFile();
        }

        public string Uri
        {
            get
            {
                string lpath = this.GetLocalFileSystemFullPath();
                if (!string.IsNullOrEmpty(lpath))
                    return lpath;
                if (Media?.Parts != null && Media.Parts.Count() > 0)
                    return Media.Parts[0].Key;
                return string.Empty;
            }
        }

        public bool FileIsAvailable => !string.IsNullOrEmpty(this.GetLocalFileSystemFullPath());

        public bool FileIsNotAvailable => string.IsNullOrEmpty(this.GetLocalFileSystemFullPath());


        public string FormattedFileSize => Utils.FormatFileSize(FileSize);


        public List<VM_AnimeEpisode_User> GetEpisodes()
        {
            try
            {
                return VM_ShokoServer.Instance.ShokoServices.GetEpisodesForFile(VideoLocalID,
                           VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_AnimeEpisode_User>() ?? new List<VM_AnimeEpisode_User>();
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }

            return new List<VM_AnimeEpisode_User>();
        }

        public string DefaultAudioLanguage
        {
            get
            {
                List<VM_AnimeEpisode_User> eps = GetEpisodes();
                if (eps.Count == 0) return string.Empty;

                return eps[0].DefaultAudioLanguage;
            }
        }

        public string DefaultSubtitleLanguage
        {
            get
            {
                List<VM_AnimeEpisode_User> eps = GetEpisodes();
                if (eps.Count == 0) return string.Empty;

                return eps[0].DefaultSubtitleLanguage;
            }
        }
    }
}