using System;

namespace Shoko.MyAnime3.Events
{
    /// <summary>
    /// This event occurs when all the related anime for an anime have beedn download on the server
    /// </summary>
    public class GotAnimeForRelatedEventArgs : EventArgs
    {
        public readonly int AnimeID;

        public GotAnimeForRelatedEventArgs(int animeID)
        {
            AnimeID = animeID;
        }
    }
}