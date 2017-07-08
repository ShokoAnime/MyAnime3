using System;
using System.Linq;
using Shoko.Commons.Extensions;
using Shoko.Models.Client;
using Shoko.Models.Server;
using Shoko.MyAnime3.ViewModel.Helpers;

namespace Shoko.MyAnime3.ViewModel.Server
{
    public class VM_VideoDetailed : CL_VideoDetailed, IVideoInfo
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

        public bool IsManualAssociation => CrossRefSource != 1;
        public bool IsAutoAssociation => CrossRefSource == 1;
        public bool Ignored => VideoLocal_IsIgnored == 1;
        public bool Watched => VideoLocal_IsWatched == 1;
        public bool Unwatched => VideoLocal_IsWatched != 1;


        public string FileName => VideoLocal_FileName;


        public bool FileIsAvailable => this.IsLocalFile();

        public bool FileIsNotAvailable => !this.IsLocalFile();


        public string VideoInfoSummary => $"{this.GetVideoResolution()} ({this.GetVideoCodec()}) - {this.GetAudioCodec()}";

        public string FormattedFileSize => Utils.FormatFileSize(VideoLocal_FileSize);


        public bool HasReleaseGroup => ReleaseGroup != null;

        public string ReleaseGroupName
        {
            get
            {
                if (ReleaseGroup != null)
                    return ReleaseGroup.GroupName;
                return string.Empty;
            }
        }


        public bool HasAniDBFile => AniDB_FileID.HasValue;


        public FileFfdshowPreset FileFfdshowPreset => VM_ShokoServer.Instance.ShokoServices.GetFFDPreset(VideoLocalID);

        public string FileSelectionDisplay
        {
            get
            {
                try
                {
                    string ret = BaseConfig.Settings.fileSelectionDisplayFormat;

                    ret = ret.Replace(Constants.FileSelectionDisplayString.AudioCodec, this.GetAudioCodec());
                    ret = ret.Replace(Constants.FileSelectionDisplayString.FileCodec, this.GetVideoCodec());
                    ret = ret.Replace(Constants.FileSelectionDisplayString.FileRes, this.GetVideoResolution());
                    ret = ret.Replace(Constants.FileSelectionDisplayString.VideoBitDepth, VideoInfo_VideoBitDepth);

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
                retString += this.GetVideoResolution();
                retString += " (" + this.GetVideoCodec() + ")";

                return retString;
            }
        }
    }
}