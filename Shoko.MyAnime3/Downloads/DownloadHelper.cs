using MediaPortal.GUI.Library;
using Shoko.MyAnime3.ViewModel.Server;

namespace Shoko.MyAnime3.Downloads
{
    public class DownloadHelper
    {
        public static void SearchEpisode(VM_AnimeEpisode_User ep)
        {
            MainWindow.currentDownloadSearch = new DownloadSearchCriteria(DownloadSearchType.Episode, ep);
            GUIWindowManager.ActivateWindow(Constants.WindowIDs.DOWNLOADS);
        }

        public static void SearchAnime(VM_AniDB_Anime anime)
        {
            MainWindow.currentDownloadSearch = new DownloadSearchCriteria(DownloadSearchType.Series, anime);
            GUIWindowManager.ActivateWindow(Constants.WindowIDs.DOWNLOADS);
        }
    }
}