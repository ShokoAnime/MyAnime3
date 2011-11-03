using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace MyAnimePlugin3.Providers.TheMovieDB
{
	//TODO???
	/*
    public class MovieDBImageFanart : Schema.MovieDB_ImageFanart
    {
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("MovieDB_ImageFanart:: Id: " + Id);
            sb.Append(" | MovieID: " + MovieID.ToString());
            sb.Append(" | Url: " + Url);
            sb.Append(" | Enabled: " + Enabled.ToString());
            sb.Append(" | ImageSize: " + ImageSize.ToString());

            return sb.ToString();
        }

		public bool Init(int movID, string url, string fanartID, int imagesize)
        {
            try
            {
                this.MovieID = movID;
				this.Url = url;
				this.Id = fanartID;
				this.ImageSize = imagesize;

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