using System;

namespace Shoko.MyAnime3.Events
{
    public class GotCharacterImagesEventArgs : EventArgs
    {
        public readonly int AniDB_SeiyuuID;

        public GotCharacterImagesEventArgs(int aniDB_SeiyuuID)
        {
            AniDB_SeiyuuID = aniDB_SeiyuuID;
        }
    }
}