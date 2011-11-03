using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

namespace MyAnimePlugin3
{
	public static class SkinSettings
	{
		private static string logoSourceBluray = "";
		public static string LogoSourceBluray
		{
			get { return SkinSettings.logoSourceBluray; }
			set { SkinSettings.logoSourceBluray = value; }
		}

		private static string logoSourceDVD = "";
		public static string LogoSourceDVD
		{
			get { return SkinSettings.logoSourceDVD; }
			set { SkinSettings.logoSourceDVD = value; }
		}

		private static string logoCodecXVid = "";
		public static string LogoCodecXVid
		{
			get { return SkinSettings.logoCodecXVid; }
			set { SkinSettings.logoCodecXVid = value; }
		}

		private static string logoCodecDivx = "";
		public static string LogoCodecDivx
		{
			get { return SkinSettings.logoCodecDivx; }
			set { SkinSettings.logoCodecDivx = value; }
		}

		private static string logoCodecMpeg2 = "";
		public static string LogoCodecMpeg2
		{
			get { return SkinSettings.logoCodecMpeg2; }
			set { SkinSettings.logoCodecMpeg2 = value; }
		}

		private static string logoCodecAVC = "";
		public static string LogoCodecAVC
		{
			get { return SkinSettings.logoCodecAVC; }
			set { SkinSettings.logoCodecAVC = value; }
		}

		private static string logoRes720 = "";
		public static string LogoRes720
		{
			get { return SkinSettings.logoRes720; }
			set { SkinSettings.logoRes720 = value; }
		}

		private static string logoRes1080 = "";
		public static string LogoRes1080
		{
			get { return SkinSettings.logoRes1080; }
			set { SkinSettings.logoRes1080 = value; }
		}

		private static string logoWidescreen = "";
		public static string LogoWidescreen
		{
			get { return SkinSettings.logoWidescreen; }
			set { SkinSettings.logoWidescreen = value; }
		}

		private static string logoFullscreen = "";
		public static string LogoFullscreen
		{
			get { return SkinSettings.logoFullscreen; }
			set { SkinSettings.logoFullscreen = value; }
		}

		public static void Load()
		{
			string skinSettings = GUIGraphicsContext.Skin + "\\Anime3_SkinSettings.xml";

			BaseConfig.MyAnimeLog.Write(skinSettings);

			// Check if File Exist
			if (!System.IO.File.Exists(skinSettings))
				return;

			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load(skinSettings);
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error: Cannot Load skin settings xml file: {0} --- {1}", skinSettings, ex.ToString());
				return;
			}

			// Read and Import Skin Settings into database
			GetLogos(doc);
		}


		private static void GetLogos(XmlDocument doc)
		{
			XmlNode node = null;
			XmlNode innerNode = null;

			node = doc.DocumentElement.SelectSingleNode("/settings/logos");
			if (node != null)
			{
				BaseConfig.MyAnimeLog.Write("Loading Logos");

				innerNode = node.SelectSingleNode("bluray");
				if (innerNode != null) logoSourceBluray = GUIGraphicsContext.Skin + @"\" + innerNode.InnerText.Trim();

				innerNode = node.SelectSingleNode("dvd");
				if (innerNode != null) logoSourceDVD = GUIGraphicsContext.Skin + @"\" + innerNode.InnerText.Trim();

				innerNode = node.SelectSingleNode("avc");
				if (innerNode != null) logoCodecAVC = GUIGraphicsContext.Skin + @"\" + innerNode.InnerText.Trim();

				innerNode = node.SelectSingleNode("divx");
				if (innerNode != null) logoCodecDivx = GUIGraphicsContext.Skin + @"\" + innerNode.InnerText.Trim();

				innerNode = node.SelectSingleNode("xvid");
				if (innerNode != null) logoCodecXVid = GUIGraphicsContext.Skin + @"\" + innerNode.InnerText.Trim();

				innerNode = node.SelectSingleNode("mpeg2");
				if (innerNode != null) logoCodecMpeg2 = GUIGraphicsContext.Skin + @"\" + innerNode.InnerText.Trim();

				innerNode = node.SelectSingleNode("res720p");
				if (innerNode != null) logoRes720 = GUIGraphicsContext.Skin + @"\" + innerNode.InnerText.Trim();

				innerNode = node.SelectSingleNode("res1080p");
				if (innerNode != null) logoRes1080 = GUIGraphicsContext.Skin + @"\" + innerNode.InnerText.Trim();

				innerNode = node.SelectSingleNode("widescreen");
				if (innerNode != null) logoWidescreen = GUIGraphicsContext.Skin + @"\" + innerNode.InnerText.Trim();

				innerNode = node.SelectSingleNode("fullscreen");
				if (innerNode != null) logoFullscreen = GUIGraphicsContext.Skin + @"\" + innerNode.InnerText.Trim();
			}
		}
	}
}
