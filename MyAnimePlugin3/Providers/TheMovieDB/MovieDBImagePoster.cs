using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace MyAnimePlugin3.Providers.TheMovieDB
{
	//TODO???
	/*
    public class MovieDBImagePoster : Schema.MovieDB_ImagePoster
    {
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("MovieDB_ImagePoster:: Id: " + Id);
            sb.Append(" | MovieID: " + MovieID.ToString());
            sb.Append(" | Url: " + Url);
            sb.Append(" | Enabled: " + Enabled.ToString());

            return sb.ToString();
        }

        public bool Init(int movID, string url, string posterID)
        {
            try
            {
                this.MovieID = movID;
				this.Url = url;
				this.Id = posterID;

                //BaseConfig.MyAnimeLog.Write("urL: {0} - iStart: {1}, iEnd: {2}, posterID: {3}", url, iStart, iEnd, posterID);

                return true;

            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write("MovieDB_ImagePoster.Init error: {0}", ex);
                return false;
            }
        }

    }*/
}
