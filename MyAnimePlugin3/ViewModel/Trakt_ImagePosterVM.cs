using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MyAnimePlugin3.ViewModel
{
	public class Trakt_ImagePosterVM
	{
		public int Trakt_ImagePosterID { get; set; }
		public int Trakt_ShowID { get; set; }
		public int Season { get; set; }
		public string ImageURL { get; set; }
		public int Enabled { get; set; }

		public string FullImagePath
		{
			get
			{
				// typical url
				// http://vicmackey.trakt.tv/images/seasons/3228-1.jpg
				// http://vicmackey.trakt.tv/images/posters/1130.jpg

				if (string.IsNullOrEmpty(ImageURL)) return "";

				int pos = ImageURL.IndexOf(@"images/");
				if (pos <= 0) return "";

				string relativePath = ImageURL.Substring(pos + 7, ImageURL.Length - pos - 7);
				relativePath = relativePath.Replace("/", @"\");

				return Path.Combine(Utils.GetTraktImagePath(), relativePath);
			}
		}

		public string FullThumbnailPath
		{
			get
			{
				return FullImagePath;
			}
		}

		private bool isImageDefault = false;
		public bool IsImageDefault
		{
			get { return isImageDefault; }
			set { isImageDefault = value; }
		}

		public Trakt_ImagePosterVM(JMMServerBinary.Contract_Trakt_ImagePoster contract)
		{
			this.Trakt_ImagePosterID = contract.Trakt_ImagePosterID;
			this.Trakt_ShowID = contract.Trakt_ShowID;
			this.Season = contract.Season;
			this.ImageURL = contract.ImageURL;
			this.Enabled = contract.Enabled;
		}
	}
}
