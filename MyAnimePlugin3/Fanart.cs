using System;
using System.Collections.Generic;
using System.Text;
using MyAnimePlugin3.ViewModel;
using System.IO;

namespace MyAnimePlugin3
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

		private List<string> GetFanartFilenames(AniDB_AnimeVM anime)
		{
			List<string> allFanart = new List<string>();

			// check if user has specied a fanart to always be used
			if (anime.DefaultFanart != null)
			{
				if (!string.IsNullOrEmpty(anime.DefaultFanart.FullImagePath) && File.Exists(anime.DefaultFanart.FullImagePath))
				{
					allFanart.Add(anime.DefaultFanart.FullImagePath);
					return allFanart;
				}
			}

			
			foreach (FanartContainer fanart in anime.AniDB_AnimeCrossRefs.AllFanarts)
			{
				if (!fanart.IsImageEnabled) continue;
				if (!File.Exists(fanart.FullImagePath)) continue;

				allFanart.Add(fanart.FullImagePath);
			}


			return allFanart;
		}

		public Fanart(object fanartObject)
		{
			this.fileName = "";
			List<string> allFanarts = new List<string>();

			// check for a default fanart
			if (fanartObject.GetType() == typeof(AnimeGroupVM))
			{
				AnimeGroupVM grp = fanartObject as AnimeGroupVM;

				if (grp.DefaultAnimeSeriesID.HasValue)
				{
					AnimeSeriesVM ser = grp.DefaultSeries;
					if (ser != null)
					{
						AniDB_AnimeVM anime = ser.AniDB_Anime;
						allFanarts.AddRange(GetFanartFilenames(anime));
					}
				}
				else
				{
					// get all the series for this group
					foreach (AnimeSeriesVM ser in grp.AllSeries)
					{
						AniDB_AnimeVM anime = ser.AniDB_Anime;
						allFanarts.AddRange(GetFanartFilenames(anime));
					}
				}
			}
			else if (fanartObject.GetType() == typeof(AnimeSeriesVM))
			{
				AnimeSeriesVM ser = fanartObject as AnimeSeriesVM;
				AniDB_AnimeVM anime = ser.AniDB_Anime;
				allFanarts.AddRange(GetFanartFilenames(anime));
			}
			else if (fanartObject.GetType() == typeof(AniDB_AnimeVM))
			{
				AniDB_AnimeVM anime = fanartObject as AniDB_AnimeVM;
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
