using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MyAnimePlugin3.ViewModel
{
	public class Trakt_ImageFanartVM
	{
		public int Trakt_ImageFanartID { get; set; }
		public int Trakt_ShowID { get; set; }
		public int Season { get; set; }
		public string ImageURL { get; set; }
		public int Enabled { get; set; }

		public string FullImagePath
		{
			get
			{
				// typical url
				// http://vicmackey.trakt.tv/images/fanart/3228.jpg

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


		public Trakt_ImageFanartVM(JMMServerBinary.Contract_Trakt_ImageFanart contract)
		{
			this.Trakt_ImageFanartID = contract.Trakt_ImageFanartID;
			this.Trakt_ShowID = contract.Trakt_ShowID;
			this.Season = contract.Season;
			this.ImageURL = contract.ImageURL;
			this.Enabled = contract.Enabled;
		}
	}
}
