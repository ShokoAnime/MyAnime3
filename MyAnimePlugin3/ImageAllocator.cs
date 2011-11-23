using System;
using System.Collections.Generic;

using MediaPortal.GUI.Library;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Drawing;

using System.IO;
using MyAnimePlugin3.ViewModel;

//using aclib.Performance;

namespace MyAnimePlugin3
{
    public class ImageAllocator
    {
		public static readonly int PosterWidth = 680;
		public static readonly int PosterHeight = 1000;
		public static readonly int BannerWidth = 758;
		public static readonly int BannerHeight = 140;

		static Random bannerRandom = new Random();
        static String s_sFontName;
        static List<String> s_SeriesImageList = new List<string>();
        static List<String> s_GroupsImageList = new List<string>();
        static List<String> s_OtherPersistentImageList = new List<string>();
        static List<String> s_OtherDiscardableImageList = new List<string>();

		public static Size BannerSize = new Size(1,1);
		public static Size PosterSize = new Size(1,1);

        static ImageAllocator()
        {
			BannerSize = GetBannerSize();
			PosterSize = GetPosterSize();
        }

        #region Helpers
        /// <summary>
        /// Create a banner image of the specified Size, outputting the input text on it
        /// </summary>
        /// <param name="sizeImage">Size of the image to be generated</param>
        /// <param name="label">Text to be output on the image</param>
        /// <returns>a bitmap object</returns>
        private static Bitmap drawSimpleBanner(Size sizeImage, string label)
        {
            Bitmap image = new Bitmap(sizeImage.Width, sizeImage.Height);
            Graphics gph = Graphics.FromImage(image);
            //gph.FillRectangle(new SolidBrush(Color.FromArgb(50, Color.White)), new Rectangle(0, 0, sizeImage.Width, sizeImage.Height));
            GUIFont fontList = GUIFontManager.GetFont(s_sFontName);
            Font font = new Font(fontList.FontName, 36);
            gph.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            gph.DrawString(label, font, new SolidBrush(Color.FromArgb(200, Color.White)), 5, (sizeImage.Height - font.GetHeight()) / 2);
            gph.Dispose();
            return image;
        }

        /// <summary>
        /// Takes an Image sFileName and tries to load it into MP' graphics memory
        /// If the sFileName was already in memory, it will not be reloaded (basically it caches)
        /// </summary>
        /// <param name="sFileName">The sFileName of the image to load, fails silently if it cannot be loaded</param>
        /// <returns>memory identifier</returns>
        public static string buildMemoryImageFromFile(string sFileName, System.Drawing.Size size)
        {
            try
            {
                if (String.IsNullOrEmpty(sFileName) || !System.IO.File.Exists(sFileName)) return string.Empty;
                string ident = buildIdentifier(sFileName);
				if (GUITextureManager.LoadFromMemory(null, ident, 0, size.Width, size.Height) > 0)
				{
					//BaseConfig.MyAnimeLog.Write("buildMemoryImageFromFile: Got from MEMORY: {0}", ident);
					return ident;
				}
				else
				{
					//BaseConfig.MyAnimeLog.Write("buildMemoryImageFromFile: Got from DISK: {0}", ident);
					return buildMemoryImage(LoadImageFastFromFile(sFileName), ident, size, false);
				}
            }
            catch (Exception e)
            {
                BaseConfig.MyAnimeLog.Write("Unable to add to MP's Graphics memory: " + sFileName + " Error: " + e.Message);
                return string.Empty;
            }
        }

        static string buildIdentifier(string name)
        {
            return "[Anime2:" + name + "]";
        }

        /// <summary>
        /// Takes an Image and tries to load it into MP' graphics memory
        /// If the sFileName was already in memory, it will not be reloaded (basically it caches)
        /// </summary>
        /// <param name="image">The System.Drawing.Bitmap to be loaded</param>
        /// <param name="identifier">A unique identifier for the image so it can be retrieved later on</param>
        /// <returns>memory identifier</returns>
        public static string buildMemoryImage(Image image, string identifier, System.Drawing.Size size, bool buildIdentifier)
        {
            string name = buildIdentifier ? ImageAllocator.buildIdentifier(identifier) : identifier;
            try
            {
                // we don't have to try first, if name already exists mp will not do anything with the image
                if (size.Height > 0 && (size.Height != image.Size.Height || size.Width != image.Size.Width)) //resize
                {
                    image = Resize(image, size);
                }
                //PerfWatcher.GetNamedWatch("add to TextureManager").Start();
                GUITextureManager.LoadFromMemory(image, name, 0, size.Width, size.Height);
                //PerfWatcher.GetNamedWatch("add to TextureManager").Stop();
            }
            catch (Exception)
            {
                //MPTVSeriesLog.Write("Unable to add to MP's Graphics memory: " + identifier);
                return string.Empty;
            }
            return name;
        }

        public static string ExtractFullName(string identifier)
        {
            String RegExp = @"\[Anime2:(.*)\]";
            Regex Engine = new Regex(RegExp, RegexOptions.IgnoreCase);
            Match match = Engine.Match(identifier);
            if (match.Success)
                return match.Groups[1].Value;
            else
                return identifier;
        }

        public static void Flush(List<String> toFlush)
        {
            foreach (String sTextureName in toFlush)
            {
                Flush(sTextureName);
            }
            toFlush.Clear();
        }

        public static void Flush(string sTextureName)
        {
            GUITextureManager.ReleaseTexture(sTextureName);
        }
        #endregion

        /// <summary>
        /// Set the font name to be used to create dummy banners
        /// </summary>
        /// <param name="sFontName">Size of the image to be generated</param>
        /// <returns>nothing</returns>
        public static void SetFontName(String sFontName)
        {
            s_sFontName = sFontName;
        }

		public static Size GetBannerSize()
		{
			double dwid = (double)BannerWidth * (double)MainWindow.settings.BannerSizePct / (double)100;
			int wid = System.Convert.ToInt32(dwid);

			double dhght = (double)BannerHeight * (double)MainWindow.settings.BannerSizePct / (double)100;
			int hght = System.Convert.ToInt32(dhght);

			BaseConfig.MyAnimeLog.Write("BannerSize {0} : {1} ", wid, hght);

			return new Size(wid, hght);
		}

		public static Size GetPosterSize()
		{
			double dwid = (double)PosterWidth * (double)MainWindow.settings.PosterSizePct / (double)100;
			int wid = System.Convert.ToInt32(dwid);

			double dhght = (double)PosterHeight * (double)MainWindow.settings.PosterSizePct / (double)100;
			int hght = System.Convert.ToInt32(dhght);

			BaseConfig.MyAnimeLog.Write("PosterSize {0} : {1} ", wid, hght);

			return new Size(wid, hght);
		}

		public static String GetGroupImage(AnimeGroupVM grp, GUIFacadeControl.Layout viewMode)
		{
			string imgFileName = "";
			Size sz = PosterSize;

			switch (viewMode)
			{
				case GUIFacadeControl.Layout.LargeIcons:
					imgFileName = GetWideBannerAsFileName(grp); sz = BannerSize; break;
				case GUIFacadeControl.Layout.List:
				case GUIFacadeControl.Layout.AlbumView:
				case GUIFacadeControl.Layout.Filmstrip:
				case GUIFacadeControl.Layout.CoverFlow:
					imgFileName = GetPosterAsFileName(grp); sz = PosterSize; break;
			}

			//BaseConfig.MyAnimeLog.Write("GetGroupBanner::viewMode: {0} : {1} : {2}", viewMode, imgFileName, grp);

			if (string.IsNullOrEmpty(imgFileName))
			{
				string ident = "series_" + grp.GroupName;
				string sTextureName = buildMemoryImage(drawSimpleBanner(sz, grp.GroupName), ident, sz, true);

				if (sTextureName.Length > 0 && !s_SeriesImageList.Contains(sTextureName)) s_SeriesImageList.Add(sTextureName);
				return sTextureName;
			}
			else
			{
				string sTextureName = "";
				if (imgFileName.Length > 0 && System.IO.File.Exists(imgFileName))
				{
					sTextureName = buildMemoryImageFromFile(imgFileName, sz);
				}
				return sTextureName;
			}

		}

		public static string GetWideBannerAsFileName(AnimeGroupVM grp)
		{
			List<string> allBanners = new List<string>();

			if (grp.DefaultAnimeSeriesID.HasValue)
			{
				AnimeSeriesVM ser = grp.DefaultSeries;
				if (ser != null)
				{
					AniDB_AnimeVM anime = ser.AniDB_Anime;

					string fileName = GetWideBannerAsFileName(anime);
					if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
						allBanners.Add(fileName);
				}
			}
			else
			{
				// get all the series for this group
				foreach (AnimeSeriesVM ser in grp.AllSeries)
				{
					AniDB_AnimeVM anime = ser.AniDB_Anime;

					string fileName = GetWideBannerAsFileName(anime);
					if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
						allBanners.Add(fileName);
				}
			}

			if (allBanners.Count == 0) return "";

			return allBanners[bannerRandom.Next(0, allBanners.Count - 1)];
		}

		public static string GetWideBannerAsFileName(AniDB_AnimeVM anime)
		{
			// check for user options
			List<string> banners = new List<string>();
			foreach (TvDB_ImageWideBannerVM wdban in anime.AniDB_AnimeCrossRefs.TvDBImageWideBanners)
			{
				if (wdban.IsImageDefault && File.Exists(wdban.FullImagePath)) return wdban.FullImagePath;

				if (File.Exists(wdban.FullImagePath)) banners.Add(wdban.FullImagePath);
			}

			if (banners.Count == 0) return "";

			return banners[bannerRandom.Next(0, banners.Count - 1)];

		}

		public static string GetPosterAsFileName(AnimeGroupVM grp)
		{
			List<string> allPosters = new List<string>();
			string fileName = "";

			if (grp.DefaultAnimeSeriesID.HasValue)
			{
				AnimeSeriesVM ser = grp.DefaultSeries;
				if (ser != null)
				{
					AniDB_AnimeVM anime = ser.AniDB_Anime;
					fileName = GetPosterAsFileName(anime);
					if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
						allPosters.Add(fileName);
				}
			}
			else
			{
				// get all the series for this group
				foreach (AnimeSeriesVM ser in grp.AllSeries)
				{
					AniDB_AnimeVM anime = ser.AniDB_Anime;

					fileName = GetPosterAsFileName(anime);
					if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
						allPosters.Add(fileName);
				}
			}

			if (allPosters.Count > 0)
				fileName = allPosters[bannerRandom.Next(0, allPosters.Count - 1)];

			return fileName;
		}

		public static string GetPosterAsFileName(AniDB_AnimeVM anime)
		{
			// check for user options
			string fileName = "";
			// check if user has specied a poster to always be used
			fileName = anime.DefaultPosterPath;

			if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
			{
				string msg = string.Format("POSTER MISSING: {0} ({1}) - {2}", anime.MainTitle, anime.AnimeID, fileName);
				return fileName;
			}
			
			// if we can't find the specified poster, we will try and use a random one

			List<string> allPosters = new List<string>();

			foreach (PosterContainer pstr in anime.AniDB_AnimeCrossRefs.AllPosters)
			{
				if (!pstr.IsImageEnabled) continue;
				if (!File.Exists(pstr.FullImagePath)) continue;

				allPosters.Add(pstr.FullImagePath);
			}

			if (allPosters.Count > 0)
				fileName = allPosters[bannerRandom.Next(0, allPosters.Count - 1)];

			return fileName;
		}

		public static String GetGroupImageAsFileName(AnimeGroupVM grp, GUIFacadeControl.Layout viewMode)
		{
			string imgFileName = "";

			DateTime start = DateTime.Now;

			switch (viewMode)
			{
				case GUIFacadeControl.Layout.LargeIcons:
					imgFileName = GetWideBannerAsFileName(grp); break;
				case GUIFacadeControl.Layout.List:
				case GUIFacadeControl.Layout.AlbumView:
				case GUIFacadeControl.Layout.Filmstrip:
				case GUIFacadeControl.Layout.CoverFlow:
					imgFileName = GetPosterAsFileName(grp); break;
			}

			TimeSpan ts = DateTime.Now - start;
			BaseConfig.MyAnimeLog.Write("GetGroupImageAsFileName::: {0} in {1}ms", grp.GroupName, ts.TotalMilliseconds);

			return imgFileName;

		}

		public static String GetSeriesImage(AnimeSeriesVM ser, GUIFacadeControl.Layout viewMode)
		{
			string imgFileName = "";
			Size sz = PosterSize;

			switch (viewMode)
			{
				case GUIFacadeControl.Layout.LargeIcons:
					imgFileName = GetWideBannerAsFileName(ser.AniDB_Anime); sz = BannerSize; break;
				case GUIFacadeControl.Layout.List:
				case GUIFacadeControl.Layout.AlbumView:
				case GUIFacadeControl.Layout.Filmstrip:
				case GUIFacadeControl.Layout.CoverFlow:
					imgFileName = GetPosterAsFileName(ser.AniDB_Anime); sz = PosterSize; break;
			}

			//BaseConfig.MyAnimeLog.Write("GetSeriesBannerAsFileName::viewMode: {0} : {1} : {2}", viewMode, imgFileName, ser);

			if (imgFileName.Length == 0)
			{
				string ident = "series_" + ser.SeriesName;
				string sTextureName = buildMemoryImage(drawSimpleBanner(sz, ser.SeriesName), ident, sz, true);

				if (sTextureName.Length > 0 && !s_SeriesImageList.Contains(sTextureName)) s_SeriesImageList.Add(sTextureName);
				return sTextureName;
			}
			else
			{
				string sTextureName = "";
				if (imgFileName.Length > 0 && System.IO.File.Exists(imgFileName))
				{
					sTextureName = buildMemoryImageFromFile(imgFileName, sz);
				}
				return sTextureName;
			}

		}

		public static String GetSeriesImageAsFileName(AnimeSeriesVM ser, GUIFacadeControl.Layout viewMode)
		{
			string imgFileName = "";

			switch (viewMode)
			{
				case GUIFacadeControl.Layout.LargeIcons:
					imgFileName = GetWideBannerAsFileName(ser.AniDB_Anime); break;
				case GUIFacadeControl.Layout.List:
				case GUIFacadeControl.Layout.AlbumView:
				case GUIFacadeControl.Layout.Filmstrip:
				case GUIFacadeControl.Layout.CoverFlow:
					imgFileName = GetPosterAsFileName(ser.AniDB_Anime); break;
			}

			//BaseConfig.MyAnimeLog.Write("GetSeriesBannerAsFileName::viewMode: {0} : {1} : {2}", viewMode, imgFileName, ser);

			return imgFileName;

		}

		public static String GetAnimeImageAsFileName(AniDB_AnimeVM anime, GUIFacadeControl.Layout viewMode)
		{
			string imgFileName = "";

			switch (viewMode)
			{
				case GUIFacadeControl.Layout.LargeIcons:
					imgFileName = GetWideBannerAsFileName(anime); break;
				case GUIFacadeControl.Layout.List:
				case GUIFacadeControl.Layout.AlbumView:
				case GUIFacadeControl.Layout.Filmstrip:
				case GUIFacadeControl.Layout.CoverFlow:
					imgFileName = GetPosterAsFileName(anime); break;
			}

			//BaseConfig.MyAnimeLog.Write("GetSeriesBannerAsFileName::viewMode: {0} : {1} : {2}", viewMode, imgFileName, ser);

			return imgFileName;

		}

		public static String GetEpisodeImageAsFileName(AnimeEpisodeVM ep)
		{
			string imgFileName = ep.EpisodeImageLocation;
			BaseConfig.MyAnimeLog.Write("GetEpisodeImageAsFileName:: {0} : {1}", imgFileName, ep);

			return imgFileName;

		}

      
        public static String GetOtherImage(string sFileName, System.Drawing.Size size, bool bPersistent)
        {
            return GetOtherImage(null, sFileName, size, bPersistent);
        }

        public static String GetOtherImage(Image i, string sFileName, System.Drawing.Size size, bool bPersistent)
        {
            String sTextureName;
            if (i != null) sTextureName = buildMemoryImage(i, sFileName, size, true);
            else sTextureName = buildMemoryImageFromFile(sFileName, size);
            if (bPersistent)
            {
                if (!s_OtherPersistentImageList.Contains(sTextureName))
                    s_OtherPersistentImageList.Add(sTextureName);
            }
            else if (!s_OtherDiscardableImageList.Contains(sTextureName))
                s_OtherDiscardableImageList.Add(sTextureName);
            return sTextureName;
        }

        public static void FlushAll()
        {
            FlushOthers(true);
            FlushGroups();
            FlushSeries();
        }
        public static void FlushSeries()
        {
            Flush(s_SeriesImageList);
        }
        public static void FlushGroups()
        {
            Flush(s_GroupsImageList);
        }
        public static void FlushOthers(bool bFlushPersistents)
        {
            Flush(s_OtherDiscardableImageList);
            if (bFlushPersistents)
                Flush(s_OtherPersistentImageList);
        }

        #region FastBitmapLoading From File
        [DllImport("gdiplus.dll", CharSet = CharSet.Unicode)]
        private static extern int GdipLoadImageFromFile(string filename, out IntPtr image);
        private static Type imageType = typeof(System.Drawing.Bitmap);

        /// <summary>
        /// Loads an Image from a File by invoking GDI Plus instead of using build-in .NET methods, or falls back to Image.FromFile
        /// Can perform up to 10x faster
        /// </summary>
        /// <param name="filename">The filename to load</param>
        /// <returns>A .NET Image object</returns>
        public static Image LoadImageFastFromFile(string filename)
        {
            //PerfWatcher.GetNamedWatch("Img Loading").Start();
            IntPtr image = IntPtr.Zero;
            Image i = null;
            try
            {
                // We are not using ICM at all, fudge that, this should be FAAAAAST!
                if (GdipLoadImageFromFile(filename, out image) != 0)
                {
                    //MPTVSeriesLog.Write("ImageLoadFast threw an error");
                    i = Image.FromFile(filename);
                }
                else i = (Image)imageType.InvokeMember("FromGDIplus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { image });

            }
            catch (Exception e)
            {
                // this probably means the image is bad
                //PerfWatcher.GetNamedWatch("Img Loading").Stop();
                BaseConfig.MyAnimeLog.Write("ImageLoading threw an error: " + filename + " - " + e.Message);
                return null;
            }
            //PerfWatcher.GetNamedWatch("Img Loading").Stop();
            return i;
        }
        #endregion

        public static Bitmap Resize(Image img, Size size)
        {
            Bitmap bmp = null;

            // this should be tons faster by using simple nearestneighbour (about 3x in my testapp)
            // but for some reason when run in here its about 2x slower than a simple new Bitmap()
            // ???????

            //if (Size.Height % 16 != 0)
            //    Size.Height += 16 - Size.Height % 16;
            //if (Size.Width % 16 != 0)
            //    Size.Width += 16 - Size.Width % 16;
            //MPTVSeriesLog.Write(Size.Width + "x" + Size.Height);

            //PerfWatcher.GetNamedWatch("NN ImgScaling").Start();
            ////create a new Bitmap the Size of the new image
            //bmp = new Bitmap(Size.Width, Size.Height);
            ////create a new graphic from the Bitmap
            //Graphics graphic = Graphics.FromImage((Image)bmp);
            //graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            //graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            //graphic.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.AssumeLinear;
            ////draw the newly resized image
            //PerfWatcher.GetNamedWatch("NN ImgScaling draw").Start();
            //graphic.DrawImage(img, 0, 0, Size.Width, Size.Height);
            //PerfWatcher.GetNamedWatch("NN ImgScaling draw").Stop();
            ////dispose and free up the resources
            //graphic.Dispose();

            //PerfWatcher.GetNamedWatch("NN ImgScaling").Stop();
            //aclib.Performance.PerfWatcher.GetNamedWatch("ImgScaling").Start();
            bmp = new Bitmap(img, size);
            //bmp = (Bitmap)img.GetThumbnailImage(FileSize.Width, FileSize.Height, null, IntPtr.Zero);
            //bmp = (Bitmap)img;
            //aclib.Performance.PerfWatcher.GetNamedWatch("ImgScaling").Stop();
            return bmp;
        }
    }
}
