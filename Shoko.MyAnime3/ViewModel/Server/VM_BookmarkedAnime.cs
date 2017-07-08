using System;
using System.ComponentModel;
using Shoko.Models.Client;

namespace Shoko.MyAnime3.ViewModel.Server
{
    public class VM_BookmarkedAnime : CL_BookmarkedAnime, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                var args = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, args);
            }
        }

        public new VM_AniDB_Anime Anime
        {
            get { return (VM_AniDB_Anime) base.Anime; }
            set
            {
                base.Anime = value;
                NotifyPropertyChanged("AniDB_Anime");
            }
        }

        public new int AnimeID
        {
            get { return base.AnimeID; }
            set
            {
                base.AnimeID = value;
                NotifyPropertyChanged("AnimeID");
            }
        }


        public new int Priority
        {
            get { return base.Priority; }
            set
            {
                base.Priority = value;
                NotifyPropertyChanged("Priority");
            }
        }


        public new string Notes
        {
            get { return base.Notes; }
            set
            {
                base.Notes = value;
                NotifyPropertyChanged("Notes");
            }
        }


        public new int Downloading
        {
            get { return base.Downloading; }
            set
            {
                base.Downloading = value;
                NotifyPropertyChanged("Downloading");
                NotifyPropertyChanged("DownloadingBool");
                NotifyPropertyChanged("NotDownloadingBool");
            }
        }

        public bool DownloadingBool => Downloading == 1;

        public bool NotDownloadingBool => Downloading != 1;


        public void Populate(VM_BookmarkedAnime contract)
        {
            BookmarkedAnimeID = contract.BookmarkedAnimeID;
            AnimeID = contract.AnimeID;
            Priority = contract.Priority;
            Notes = contract.Notes;
            Downloading = contract.Downloading;
            Anime = contract.Anime;
        }

        public bool Save()
        {
            CL_Response<CL_BookmarkedAnime> resp = VM_ShokoServer.Instance.ShokoServices.SaveBookmarkedAnime(this);

            if (!string.IsNullOrEmpty(resp.ErrorMessage))
            {
                Utils.DialogMsg("Error", resp.ErrorMessage);
                return false;
            }
            Populate((VM_BookmarkedAnime) resp.Result);

            return true;
        }
    }
}