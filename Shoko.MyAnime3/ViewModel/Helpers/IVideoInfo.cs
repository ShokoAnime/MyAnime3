namespace Shoko.MyAnime3.ViewModel.Helpers
{
    public interface IVideoInfo
    {
        string FileName { get; }
        string Uri { get; }
        Models.PlexAndKodi.Media Media { get; }
        int VideoLocalID { get; }
        bool? IsLocalOrStreaming();

    }
}
