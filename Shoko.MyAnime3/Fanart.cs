using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Shoko.MyAnime3.ViewModel;
using Shoko.MyAnime3.ViewModel.Server;

namespace Shoko.MyAnime3
{
	public class Fanart
	{
		static Random fanartRandom = new Random();

		private string fileName = "";
		public string FileName
		{
			get { return fileName; }
			set { fileName = value; }
		}

		private string colors = "";

		public Fanart()
		{
		}

		public Fanart(string fileName, string colors)
		{
			this.fileName = fileName;
			this.colors = colors;
		}

		private List<string> GetFanartFilenames(VM_AniDB_Anime anime)
		{
			List<string> allFanart = new List<string>();

			// check if user has specied a fanart to always be used
			if (anime.DefaultImageFanart != null)
			{
				if (!string.IsNullOrEmpty(anime.DefaultImageFanart.FullImagePath) && File.Exists(anime.DefaultImageFanart.FullImagePath))
				{
					allFanart.Add(anime.DefaultImageFanart.FullImagePath);
				    BaseConfig.MyAnimeLog.Write("Default Fanart: " +anime.DefaultImageFanart.FullImagePath);

                    return allFanart;
				}
			}

			//if (anime.AniDB_AnimeCrossRefs != nul
			foreach (FanartContainer fanart in anime.AniDB_AnimeCrossRefs.AllFanarts)
			{
				if (!fanart.IsImageEnabled) continue;
				//if (!File.Exists(fanart.FullImagePath)) continue;

				allFanart.Add(fanart.FullImagePath);
			}


			return allFanart;
		}

		public Fanart(object fanartObject)
		{
			this.fileName = "";
			List<string> allFanarts = new List<string>();

			// check for a default fanart
			if (fanartObject.GetType() == typeof(VM_AnimeGroup_User))
			{
				VM_AnimeGroup_User grp = fanartObject as VM_AnimeGroup_User;

				if (grp.DefaultAnimeSeriesID.HasValue)
				{
					VM_AnimeSeries_User ser = grp.DefaultSeries;
					if (ser != null)
					{
						VM_AniDB_Anime anime = ser.Anime;
						allFanarts.AddRange(GetFanartFilenames(anime));
					}
				}
				else
				{
					// get all the series for this group
					foreach (VM_AnimeSeries_User ser in grp.AllSeries)
					{
						VM_AniDB_Anime anime = ser.Anime;
						allFanarts.AddRange(GetFanartFilenames(anime));
					}
				}
			}
			else if (fanartObject.GetType() == typeof(VM_AnimeSeries_User))
			{
				VM_AnimeSeries_User ser = fanartObject as VM_AnimeSeries_User;
				VM_AniDB_Anime anime = ser.Anime;
				allFanarts.AddRange(GetFanartFilenames(anime));
			}
			else if (fanartObject.GetType() == typeof(VM_AniDB_Anime))
			{
				VM_AniDB_Anime anime = fanartObject as VM_AniDB_Anime;
				allFanarts.AddRange(GetFanartFilenames(anime));
			}

			string randomFanart = "";
			if (allFanarts.Count > 0)
			{
				randomFanart = allFanarts[fanartRandom.Next(0, allFanarts.Count)];
			}

			if (!String.IsNullOrEmpty(randomFanart))
				fileName = randomFanart;
			
		}

		public bool HasColorInfo
		{
			get
			{
				return !String.IsNullOrEmpty(this.colors);
			}
		}

		public System.Drawing.Color GetColor(int which)
		{
			if (HasColorInfo && which <= 3 && which > 0)
			{
				string[] split = this.colors.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
				if (split.Length != 3) return default(System.Drawing.Color);
				string[] rgbValues = split[--which].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				return System.Drawing.Color.FromArgb(100, Int32.Parse(rgbValues[0]), Int32.Parse(rgbValues[1]), Int32.Parse(rgbValues[2]));
			}
			else return default(System.Drawing.Color);
		}

		public System.Drawing.Color[] Colors
		{
			get
			{
				if (HasColorInfo)
				{
					System.Drawing.Color[] colors = new System.Drawing.Color[3];
					for (int i = 0; i < 3; )
						colors[i] = this.GetColor(++i);
					return colors;
				}
				else return null;
			}
		}

		public static string RGBColorToHex(System.Drawing.Color color)
		{
			// without alpha
			return String.Format("{0:x}", color.R) +
				   String.Format("{0:x}", color.G) +
				   String.Format("{0:x}", color.B);
		}
	}
}
