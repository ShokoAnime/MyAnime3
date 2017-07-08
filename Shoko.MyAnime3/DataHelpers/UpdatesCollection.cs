using System.Collections.Generic;

namespace Shoko.MyAnime3.DataHelpers
{
    public class UpdatesCollection
    {
        protected string rawAnimeIDs = "";

        public string RawAnimeIDs
        {
            get { return rawAnimeIDs; }
            set { rawAnimeIDs = value; }
        }

        protected long updateCount;

        public long UpdateCount
        {
            get { return updateCount; }
            set { updateCount = value; }
        }

        public List<int> AnimeIDs
        {
            get
            {
                List<int> ids = new List<int>();
                string[] sids = rawAnimeIDs.Split('|');
                foreach (string sid in sids)
                {
                    int id;
                    if (int.TryParse(sid, out id)) ids.Add(id);
                }

                return ids;
            }
        }


        // default constructor
    }
}