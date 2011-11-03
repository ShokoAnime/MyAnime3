using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using MediaPortal.Profile;
using System.IO;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MyAnimePlugin3.ViewModel;

namespace MyAnimePlugin3
{
	public class Logos
	{
		static string pathfortmpfile = MainWindow.settings.ThumbsFolder;
		static string tmpFile = @"tmpLogos.png";

		static string mediaFolder
		{
			get
			{
				
				//string skinFolder = Config.GetFolder(Config.Dir.Skin);
				string skinFolder = GUIGraphicsContext.Skin;
				skinFolder = skinFolder.Trim().TrimEnd('\\');
				skinFolder += @"\Media\Logos\";
				return skinFolder;
			}
		}

		/*
		static string LogoSourceBluRay = mediaFolder + @"BLURAY.png";
		static string LogoSourceDVD = mediaFolder + @"DVD.png";

		static string LogoCodecX264 = mediaFolder + @"AVC.png";
		static string LogoCodecXVid = mediaFolder + @"XVID.png";
		static string LogoCodecDivx = mediaFolder + @"DIVX.png";
		static string LogoCodecMpeg2 = mediaFolder + @"MP2V.png";
		static string LogoCodecAVC = mediaFolder + @"AVC.png";

		static string LogoRes720 = mediaFolder + @"720p.png";
		static string LogoRes1080 = mediaFolder + @"1080p.png";

		static string LogoDim16x9 = mediaFolder + @"WIDESCREEN.png";
		static string LogoDim4x3 = mediaFolder + @"FULLSCREEN.png";
		*/

		public static string buildLogoImage(AnimeEpisodeVM ep)
		{
			List<string> logosForBuilding = new List<string>();

			string bestSource = "";
			int bestWidth = 0;
			int bestHeight = 0;
			VideoDetailedVM bestFile = null;

			// blu-ray > dvd
			// dvd > hdtv
			// hdtv > dtv
			// dtv > tv
			// tv > vhs


			foreach (VideoDetailedVM vid in ep.FilesForEpisode)
			{
				//BaseConfig.MyAnimeLog.Write(vid.ToString());


				// get video width
				int videoWidth = vid.GetVideoWidth();
				int videoHeight = vid.GetVideoHeight();

				if (bestFile == null)
				{
					bestFile = vid;
					bestSource = vid.AniDB_File_Source;
					bestWidth = videoWidth;
					bestHeight = videoHeight;
				}
				else
				{
					// get best source
					if ((GetVideoSourceRanking(vid.AniDB_File_Source) > GetVideoSourceRanking(bestSource)) ||
						(GetVideoSourceRanking(vid.AniDB_File_Source) == GetVideoSourceRanking(bestSource) && videoWidth > bestWidth))
					{
						bestFile = vid;
						bestSource = vid.AniDB_File_Source;
						bestWidth = videoWidth;
						bestHeight = videoHeight;
					}
				}
			}

			if (bestFile == null) return "";

			//MyAnimeLog.Write("Width: {0}, height: {1}", bestWidth, bestHeight);

			if (bestSource.ToUpper().Contains("BLU") && File.Exists(SkinSettings.LogoSourceBluray)) logosForBuilding.Add(SkinSettings.LogoSourceBluray);
			if (bestSource.ToUpper().Contains("DVD") && File.Exists(SkinSettings.LogoSourceDVD)) logosForBuilding.Add(SkinSettings.LogoSourceDVD);

			if (bestWidth >= 1280 && bestWidth < 1920 && File.Exists(SkinSettings.LogoRes720)) logosForBuilding.Add(SkinSettings.LogoRes720);
			if (bestWidth >= 1920 && File.Exists(SkinSettings.LogoRes1080)) logosForBuilding.Add(SkinSettings.LogoRes1080);

			if (bestFile.VideoCodec.ToUpper().Contains("H264") && File.Exists(SkinSettings.LogoCodecAVC))
                logosForBuilding.Add(SkinSettings.LogoCodecAVC);
			if (bestFile.VideoCodec.ToUpper().Contains("DIVX") && File.Exists(SkinSettings.LogoCodecDivx))
                logosForBuilding.Add(SkinSettings.LogoCodecDivx);
			if (bestFile.VideoCodec.ToUpper().Contains("XVID") && File.Exists(SkinSettings.LogoCodecDivx))
                logosForBuilding.Add(SkinSettings.LogoCodecDivx);
            
		    if (bestWidth > 0 && bestHeight > 0)
			{
				double dim = (double)bestWidth / (double)bestHeight;
				if (dim > (double)1.4)
				{
					if (File.Exists(SkinSettings.LogoWidescreen))
						logosForBuilding.Add(SkinSettings.LogoWidescreen);
				}
				else
				{
					if (File.Exists(SkinSettings.LogoFullscreen))
						logosForBuilding.Add(SkinSettings.LogoFullscreen);
				}
			}
			
			return buildLogoImage(logosForBuilding, 800, 50);
		}

		static int GetVideoSourceRanking(string source)
		{
			if (source.ToUpper().Contains("BLU")) return 100;
			if (source.ToUpper().Contains("DVD")) return 99;
			if (source.ToUpper().Contains("HDTV")) return 98;
			if (source.ToUpper().Contains("DTV")) return 97;
			if (source.ToUpper().Trim() == "TV") return 96;
			if (source.ToUpper().Contains("VHS")) return 95;

			return 0;
		}

		public static string buildLogoImage(List<string> logosForBuilding, int imgWidth, int imgHeight)
		{
			try
			{
				if (logosForBuilding.Count == 1) return logosForBuilding[0];
				else if (logosForBuilding.Count > 1)
				{
					tmpFile = string.Empty;
					foreach (string logo in logosForBuilding)
						tmpFile += System.IO.Path.GetFileNameWithoutExtension(logo);
					tmpFile = Path.Combine(pathfortmpfile, @"\anime3_" + tmpFile + ".png");

					Bitmap b = new Bitmap(imgWidth, imgHeight);
					Image img = b;
					Graphics g = Graphics.FromImage(img);
					appendLogos(logosForBuilding, ref g, imgHeight, imgWidth);
				
					return ImageAllocator.GetOtherImage(b, tmpFile, new Size(), true); // don't resize in allocator
				}
				else return string.Empty;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("The Logo Building Engine generated an error: " + ex.ToString());
				return string.Empty;
			}
		}

		static void appendLogos(List<string> logosForBuilding, ref Graphics g, int totalHeight, int totalWidth)
		{
			int noImgs = logosForBuilding.Count;
			List<Image> imgs = new List<Image>();
			List<Size> imgSizes = new List<Size>();
			int spacer = 5;
			int checkWidth = 0;
			// step one: get all sizes (not all logos are obviously square) and scale them to fit vertically
			Image single = null;
			float scale = 0, totalHeightf = (float)totalHeight;
			Size tmp = default(Size);
			int x_pos = 0;
			for (int i = 0; i < logosForBuilding.Count; i++)
			{
				try
				{
					single = ImageAllocator.LoadImageFastFromFile(logosForBuilding[i]);
				}
				catch (Exception)
				{
					BaseConfig.MyAnimeLog.Write("Could not load Image file... " + logosForBuilding[i]);
					return;
				}
				scale = totalHeightf / (float)single.Size.Height;
				tmp = new Size((int)(single.Width * scale), (int)(single.Height * scale));
				checkWidth += tmp.Width;
				imgSizes.Add(tmp);
				imgs.Add(single);
			}
			// step two: check if we are too big horizontally and if so scale again
			checkWidth += imgSizes.Count * spacer;
			if (checkWidth > totalWidth)
			{
				scale = (float)checkWidth / (float)totalWidth;
				for (int i = 0; i < imgSizes.Count; i++)
				{
					imgSizes[i] = new Size((int)(imgSizes[i].Width / scale), (int)(imgSizes[i].Height / scale));
				}
			}
			// step three: finally draw them
			for (int i = 0; i < imgs.Count; i++)
			{
				g.DrawImage(imgs[i], x_pos, totalHeight - imgSizes[i].Height, imgSizes[i].Width, imgSizes[i].Height);
				x_pos += imgSizes[i].Width + spacer;
			}
		}
	}
}
