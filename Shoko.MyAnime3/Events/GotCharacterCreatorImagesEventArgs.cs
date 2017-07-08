using System;

namespace Shoko.MyAnime3.Events
{
    public class GotCharacterCreatorImagesEventArgs : EventArgs
    {
        public readonly int AnimeID;

        public GotCharacterCreatorImagesEventArgs(int animeID)
        {
            AnimeID = animeID;
        }
    }
}