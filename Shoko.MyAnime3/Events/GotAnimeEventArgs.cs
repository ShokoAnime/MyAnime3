using System;

namespace Shoko.MyAnime3.Events
{
    public class GotAnimeEventArgs : EventArgs
    {
        public readonly int AnimeID;

        public GotAnimeEventArgs(int animeID)
        {
            AnimeID = animeID;
        }
    }
}