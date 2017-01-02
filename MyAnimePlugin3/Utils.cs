using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using MediaPortal.GUI.Library;

using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using MediaPortal.Dialogs;
using MediaPortal.Configuration;
using MyAnimePlugin3.ViewModel;
using BinaryNorthwest;
using MyAnimePlugin3.Windows;

namespace MyAnimePlugin3
{
	public class Utils
	{
		/// <summary>
		/// Compute Levenshtein distance --- http://www.merriampark.com/ldcsharp.htm
		/// </summary>
		/// <param name="s"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		public static int LevenshteinDistance(string s, string t)
		{
			int n = s.Length; //length of s
			int m = t.Length; //length of t

			int[,] d = new int[n + 1, m + 1]; // matrix

			int cost; // cost

			// Step 1
			if (n == 0) return m;
			if (m == 0) return n;

			// Step 2
			for (int i = 0; i <= n; d[i, 0] = i++) ;
			for (int j = 0; j <= m; d[0, j] = j++) ;

			// Step 3
			for (int i = 1; i <= n; i++)
			{
				//Step 4
				for (int j = 1; j <= m; j++)
				{
					// Step 5
					cost = (t.Substring(j - 1, 1) == s.Substring(i - 1, 1) ? 0 : 1);

					// Step 6
					d[i, j] = System.Math.Min(System.Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
							  d[i - 1, j - 1] + cost);
				}
			}

			// Step 7
			return d[n, m];
		}
        public static int InverseLevenshteinDistance(string s, string t)
        {
            int n = s.Length; //length of s
            int m = t.Length; //length of t

            int[,] d = new int[n + 1, m + 1]; // matrix

            int cost; // cost

            // Step 1
            if (n == 0) return m;
            if (m == 0) return n;

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 0; j <= m; d[0, j] = j++) ;

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    cost = (t.Substring((m-j), 1) == s.Substring((n-i), 1) ? 0 : 1);

                    // Step 6
                    d[i, j] = System.Math.Min(System.Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                              d[i - 1, j - 1] + cost);
                }
            }

            // Step 7
            return d[n, m];
        }

		// Function to display parent function
		public static string GetParentMethodName()
		{
			StackTrace stackTrace = new StackTrace();
			StackFrame stackFrame = stackTrace.GetFrame(2);
			MethodBase methodBase = stackFrame.GetMethod();
			return methodBase.Name;
		}

		public static string GetApplicationVersion(Assembly a)
		{
			AssemblyName an = a.GetName();
			return an.Version.ToString();
		}
        public static void GetLatestVersionAsync()
        {
            if (DateTime.Now > MainWindow.NextVersionCheck)
            {
                MainWindow.NextVersionCheck = DateTime.Now.AddMinutes(30);
                Thread th = new Thread(GetLatestVersionThread);
                th.Start(null);
            }
        }
        private static void GetLatestVersionThread(object obj)
        {
            string xml = DownloadWebPage(Constants.MediaPortalUpdateXml);
            if (!string.IsNullOrEmpty(xml))
            {
                Match m = Constants.UpdateXmlVersion.Match(xml);
                if (m.Success)
                {
                    MainWindow.LastestVersion = m.Groups["Major"] + "." + m.Groups["Minor"] + "." + m.Groups["Build"] + "." + m.Groups["Revision"];
                    return;
                }

            }
            MainWindow.LastestVersion = string.Empty;
        }

        public static Regex UpdateXmlVersion = new Regex("<ExtensionCollection.*?<GeneralInfo>.*?<Name>MyAnime</Name>.*?<Version>.*?<Major>(?<Major>.*?)</Major>.*?<Minor>(?<Minor>.*?)</Minor>.*?<Build>(?<Build>.*?)</Build>.*?<Revision>(?<Revision>.*?)</Revision>", RegexOptions.Singleline);

        public static void CheckRequiredFiles(ILog log)
		{
			try
			{
				Assembly a = Assembly.Load("Core");
				AssemblyName an = a.GetName();
				log.Write("Core: {0}", an.Version.ToString());

				Assembly a3 = Assembly.Load("MediaPortal");
				AssemblyName an3 = a3.GetName();
				log.Write("MediaPortal: {0}", an3.Version.ToString());

				Assembly a2 = Assembly.Load("ICSharpCode.SharpZipLib");
				AssemblyName an2 = a2.GetName();
				log.Write("ICSharpCode.SharpZipLib: {0} (0.85.5.452 or better recommended)", an2.Version.ToString());

				

				string mediaInfoVersion = "DLL Not found";
				try
				{
					if (File.Exists(Config.GetFolder(Config.Dir.Base) + @"\MediaInfo.dll"))
					{
						FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Config.GetFolder(Config.Dir.Base) + @"\MediaInfo.dll");
						mediaInfoVersion = string.Format("{0}.{1}.{2}.{3}", fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart);
					}
					else if (File.Exists(Config.GetFolder(Config.Dir.Plugins) + @"\windows\MediaInfo.dll"))
					{
						FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Config.GetFolder(Config.Dir.Plugins) + @"\windows\MediaInfo.dll");
						mediaInfoVersion = string.Format("{0}.{1}.{2}.{3}", fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart);
					}

				}
				catch { }

				log.Write("MediaInfo: {0} (0.7.20.0 or better recommended)", mediaInfoVersion);

				log.Write("Base Folder: {0}", Config.GetFolder(Config.Dir.Base));
				log.Write("Skin Folder: {0}", Config.GetFolder(Config.Dir.Skin));
				log.Write("Plugins Folder: {0}", Config.GetFolder(Config.Dir.Plugins));
			}
			catch (Exception ex)
			{
				log.Write("CheckRequiredFiles: {0}", ex.ToString());
			}
		}

		public static bool IsRunningFromConfig()
		{
		    return false;
		}


		public static string DownloadWebPage(string url)
		{
			return DownloadWebPage(url, null, false);
		}

        public static string DownloadWebPage(string url, string cookieHeader, bool setUserAgent)
        {
            try
            {
                //BaseConfig.MyAnimeLog.Write("DownloadWebPage called by: {0} - {1}", GetParentMethodName(), url);

                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);
                webReq.Timeout = 10000; // 10 seconds
                webReq.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                webReq.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                if (!string.IsNullOrEmpty(cookieHeader))
                    webReq.Headers.Add("Cookie", cookieHeader);
                if (setUserAgent)
                    webReq.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";

                HttpWebResponse webResponse = (HttpWebResponse)webReq.GetResponse();

                Stream responseStream = webResponse.GetResponseStream();
                Encoding encoding = null;
                if (!String.IsNullOrEmpty(webResponse.CharacterSet))
                    encoding = Encoding.GetEncoding(webResponse.CharacterSet);
                if (encoding == null)
                    encoding = Encoding.Default;
                string output = string.Empty;
                if (responseStream != null)
                {
                    StreamReader reader = new StreamReader(responseStream, encoding);
                    output = reader.ReadToEnd();
                    //BaseConfig.MyAnimeLog.Write("DownloadWebPage: {0}", output);
                    responseStream.Close();
                }
                webResponse.Close();
                return output;
            }
            catch (Exception ex)
            {
                string msg = "---------- ERROR IN DOWNLOAD WEB PAGE ---------" + Environment.NewLine +
                    url + Environment.NewLine +
                    ex + Environment.NewLine + "------------------------------------";
                BaseConfig.MyAnimeLog.Write(msg);

                // if the error is a 404 error it may mean that there is a bad series association
                // so lets log it to the web cache so we can investigate
                if (ex.ToString().Contains("(404) Not Found"))
                {
                }

                return string.Empty;
            }
        }

        public static Stream DownloadWebBinary(string url)
        {
            try
            {
                HttpWebResponse response;
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);
                // Note: some network proxies require the useragent string to be set or they will deny the http request
                // this is true for instance for EVERY thailand internet connection (also needs to be set for banners/episodethumbs and any other http request we send)
                webReq.UserAgent = "Anime2MP";
                webReq.Timeout = 20000; // 20 seconds
                response = (HttpWebResponse)webReq.GetResponse();
                return response.GetResponseStream();
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
                return null;
            }
        }

        public static string GetAniDBDate(int secs)
		{
			if (secs == 0) return "";

			DateTime thisDate = new DateTime(1970, 1, 1, 0, 0, 0);
			thisDate = thisDate.AddSeconds(secs);
			return thisDate.ToString("dd MMM yyyy", Globals.Culture);
		}

		public static string GetAniDBDateWithShortYear(int secs)
		{
			if (secs == 0) return "";

			DateTime thisDate = new DateTime(1970, 1, 1, 0, 0, 0);
			thisDate = thisDate.AddSeconds(secs);
			return thisDate.ToString("dd MMM yy", Globals.Culture);
		}

	    public static DateTime GetAniDBDateAsDate(int secs)
		{
			if (secs == 0) return DateTime.Now;

			DateTime thisDate = new DateTime(1970, 1, 1, 0, 0, 0);
			thisDate = thisDate.AddSeconds(secs);
			return thisDate;
		}
        public static string AniDBDate(DateTime date)
        {
            TimeSpan sp = date.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
            return ((long)sp.TotalSeconds).ToString();


        }

	    public static void PromptToRateSeriesOnCompletion(AnimeSeriesVM ser)
	    {
	        try
	        {
	            if (!BaseConfig.Settings.DisplayRatingDialogOnCompletion) return;

	            // if the user doesn't have all the episodes return
	            if (ser.MissingEpisodeCount > 0) return;

	            // if we have no anidb info return
	            if (ser.AniDB_Anime == null || ser.AniDB_ID == 0)
	                return;

	            // only prompt the user if the series has finished airing
	            // and the user has watched all the episodes
	            if (!ser.AniDB_Anime.FinishedAiring || ser.UnwatchedEpisodeCount > 0) return;

	            // don't prompt if the user has already rated this
	            AniDB_VoteVM UserVote = ser.AniDB_Anime.UserVote;
	            if (UserVote != null) return;

	            decimal rating = Utils.PromptAniDBRating(ser.AniDB_Anime.FormattedTitle);
	            if (rating > 0)
	            {
	                JMMServerVM.Instance.clientBinaryHTTP.VoteAnime(ser.AniDB_ID, rating, (int) VoteType.AnimePermanent);
	            }

	        }
	        catch (Exception ex)
	        {
	            BaseConfig.MyAnimeLog.Write(ex.ToString());
	        }
	    }

        public static void PromptToRateSeriesMaually(AnimeSeriesVM ser)
        {
            try
            {
                // if we have no anidb info return
                if (ser.AniDB_Anime == null || ser.AniDB_ID == 0)
                    return;

                decimal rating = Utils.PromptAniDBRating(ser.AniDB_Anime.FormattedTitle);
                if (rating > 0)
                {
                    JMMServerVM.Instance.clientBinaryHTTP.VoteAnime(ser.AniDB_ID, rating, (int)VoteType.AnimePermanent);
                    MainWindow.StaticSetGUIProperty(MainWindow.GuiProperty.SeriesGroup_MyRating, $"{rating}");
                }

            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
        }

        public static decimal PromptAniDBRating(string title)
        {
            RatingDialog ratingDlg = (RatingDialog)GUIWindowManager.GetWindow(Constants.WindowIDs.RATINGDIALOG);
            ratingDlg.Reset();
            ratingDlg.SetHeading(string.IsNullOrEmpty(title) ? Translation.UserRating : Translation.UserRating + " - " + title);
            ratingDlg.Rating = 7;
            ratingDlg.DoModal(ratingDlg.GetID);
            if (ratingDlg.IsSubmitted)
            {
                return ratingDlg.Rating;
            }
            return 0;
        }
        

        private static string[] escapes = { "SOURCE", "TAKEN", "FROM", "HTTP", "ANN", "ANIMENFO", "ANIDB", "ANIMESUKI" };

        public static string ReparseDescription(string description)
        {
			if (description == null || description.Length == 0) return "";

            string val = description;
            val = val.Replace("<br />", Environment.NewLine).Replace("<br/>", Environment.NewLine).Replace("<i>", "").
                    Replace("</i>", "").Replace("<b>", "").Replace("</b>", "").Replace("[i]", "").Replace("[/i]", "").
                    Replace("[b]", "").Replace("[/b]", "");
            val = val.Replace("<BR />", Environment.NewLine).Replace("<BR/>", Environment.NewLine).Replace("<I>", "").Replace("</I>", "").Replace("<B>", "").Replace("</B>", "").Replace("[I]", "").Replace("[/I]", "").
                    Replace("[B]", "").Replace("[/B]", "");

            string vup = val.ToUpper();
            while ((vup.Contains("[URL")) || (vup.Contains("[/URL]")))
            {
                int a = vup.IndexOf("[URL");
                if (a >= 0)
                {
                    int b = vup.IndexOf("]", a + 1);
                    if (b >= 0)
                    {
                        val = val.Substring(0, a) + val.Substring(b + 1);
                        vup = val.ToUpper();
                    }
                }
                a = vup.IndexOf("[/URL]");
                if (a >= 0)
                {
                    val = val.Substring(0, a) + val.Substring(a + 6);
                    vup = val.ToUpper();
                }
            }
            while (vup.Contains("HTTP:"))
            {
                int a = vup.IndexOf("HTTP:");
                if (a >= 0)
                {
                    int b = vup.IndexOf(" ", a + 1);
                    if (b >= 0)
                    {
                        if (vup[b + 1] == '[')
                        {
                            int c = vup.IndexOf("]", b + 1);
                            val = val.Substring(0, a) + " " + val.Substring(b + 2, c - b - 2) + val.Substring(c + 1);
                        }
                        else
                        {
                            val = val.Substring(0, a) + val.Substring(b);
                        }
                        vup = val.ToUpper();
                    }
                    else
                    {
                        break;
                    }
                }
            }
            int d = -1;
            do
            {
                if (d + 1 >= vup.Length)
                    break;
                d = vup.IndexOf("[", d + 1);
                if (d != -1)
                {
                    int b = vup.IndexOf("]", d + 1);
                    if (b != -1)
                    {
                        string cont = vup.Substring(d, b - d);
                        bool dome = false;
                        foreach (string s in escapes)
                        {
                            if (cont.Contains(s))
                            {
                                dome = true;
                                break;
                            }
                        }
                        if (dome)
                        {
                            val = val.Substring(0, d) + val.Substring(b + 1);
                            vup = val.ToUpper();
                        }
                    }
                }
            } while (d != -1);
            d = -1;
            do
            {
                if (d + 1 >= vup.Length)
                    break;

                d = vup.IndexOf("(", d + 1);
                if (d != -1)
                {
                    int b = vup.IndexOf(")", d + 1);
                    if (b != -1)
                    {
                        string cont = vup.Substring(d, b - d);
                        bool dome = false;
                        foreach (string s in escapes)
                        {
                            if (cont.Contains(s))
                            {
                                dome = true;
                                break;
                            }
                        }
                        if (dome)
                        {
                            val = val.Substring(0, d) + val.Substring(b + 1);
                            vup = val.ToUpper();
                        }
                    }
                }
            } while (d != -1);
            d = vup.IndexOf("SOURCE:");
            if (d == -1)
                d = vup.IndexOf("SOURCE :");
            if (d > 0)
            {
                val = val.Substring(0, d);
            }
            return val.Trim();
        }

        public static string FormatSecondsToDisplayTime(int secs)
        {
            TimeSpan t = TimeSpan.FromSeconds(secs);

            if (t.Hours > 0)
                return string.Format("{0}:{1}:{2}", t.Hours, t.Minutes.ToString().PadLeft(2, '0'), t.Seconds.ToString().PadLeft(2, '0'));
            else
                return string.Format("{0}:{1}", t.Minutes, t.Seconds.ToString().PadLeft(2, '0'));
        }

        public static string FormatAniDBRating(double rat)
        {
            // the episode ratings from UDP are out of 1000, while the HTTP AP gives it out of 10
		    //rat /= 100;

            return String.Format("{0:0.00}", rat);
            
        }

		public static string FormatPercentage(double val)
		{
			return String.Format("{0:0.0}%", val);

		}

		public static string FormatFileSize(long bytes)
		{
			double mb = (bytes / 1024f) / 1024f;

			return mb.ToString("##.# MB");
		}

		public static int? ProcessNullableInt(string sint)
		{
			if (string.IsNullOrEmpty(sint))
				return null;
			else
				return int.Parse(sint);
		}

        public static string RemoveInvalidFolderNameCharacters(string folderName)
        {
            string ret = folderName.Replace(@"*", "");
            ret = ret.Replace(@"|", "");
            ret = ret.Replace(@"\", "");
            ret = ret.Replace(@"/", "");
            ret = ret.Replace(@":", "");
            ret = ret.Replace(((Char)34).ToString(Globals.Culture), ""); // double quote
            ret = ret.Replace(@">", "");
            ret = ret.Replace(@"<", "");
            ret = ret.Replace(@"?", "");

            return ret;
        }

        public static string GetSortName(string name)
        {
            string newName = name;
            if (newName.ToLower().StartsWith("the "))
                newName = newName.Remove(0, 4);
            if (newName.ToLower().StartsWith("a "))
                newName = newName.Remove(0, 2);

            return newName;
        }

        public static string GetOSInfo()
		{
			//Get Operating system information.
			OperatingSystem os = Environment.OSVersion;
			//Get version information about the os.
			Version vs = os.Version;

			//Variable to hold our return value
			string operatingSystem = "";

			if (os.Platform == PlatformID.Win32Windows)
			{
				//This is a pre-NT version of Windows
				switch (vs.Minor)
				{
					case 0:
						operatingSystem = "95";
						break;
					case 10:
						if (vs.Revision.ToString() == "2222A")
							operatingSystem = "98SE";
						else
							operatingSystem = "98";
						break;
					case 90:
						operatingSystem = "Me";
						break;
					default:
						break;
				}
			}
			else if (os.Platform == PlatformID.Win32NT)
			{
				switch (vs.Major)
				{
					case 3:
						operatingSystem = "NT 3.51";
						break;
					case 4:
						operatingSystem = "NT 4.0";
						break;
					case 5:
						if (vs.Minor == 0)
							operatingSystem = "2000";
						else
							operatingSystem = "XP";
						break;
					case 6:
						if (vs.Minor == 0)
							operatingSystem = "Vista";
						else if (vs.Minor== 1)
							operatingSystem = "7";
                        else if (vs.Minor == 2)
                            operatingSystem = "8";
                        else
                            operatingSystem = "8.1";
						break;
                    case 10:
                        operatingSystem = "10";
				        break;
                    default:
						break;
				}
			}
			//Make sure we actually got something in our OS check
			//We don't want to just return " Service Pack 2" or " 32-bit"
			//That information is useless without the OS version.
			if (operatingSystem != "")
			{
				//Got something.  Let's prepend "Windows" and get more info.
				operatingSystem = "Windows " + operatingSystem;
				//See if there's a service pack installed.
				if (os.ServicePack != "")
				{
					//Append it to the OS name.  i.e. "Windows XP Service Pack 3"
					operatingSystem += " " + os.ServicePack;
				}
				//Append the OS architecture.  i.e. "Windows XP Service Pack 3 32-bit"
				operatingSystem += " " + getOSArchitecture() + "-bit";
			}
			//Return the information we've gathered.
			return operatingSystem;
		}

		public static int getOSArchitecture()
		{
			//easiest way: Just check the Size property of IntPtr.
			return IntPtr.Size * 8;
		}

		#region PrettyFilesize
		[DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
		static extern long StrFormatByteSize(long fileSize,
		[MarshalAs(UnmanagedType.LPTStr)] StringBuilder buffer, int bufferSize);

		public static string FormatByteSize(long fileSize)
		{
			StringBuilder sbBuffer = new StringBuilder(20);
			StrFormatByteSize(fileSize, sbBuffer, 20);
			return sbBuffer.ToString();
		}
		#endregion


		public static void DialogMsg(string title, string msg)
		{
			GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
			if (null == dlgOK)
				return;

			dlgOK.Reset();
			dlgOK.SetHeading(title);

			string[] lines = msg.Split('\n');
			BaseConfig.MyAnimeLog.Write("lines: " + lines.Length.ToString());

			if (lines.Length == 1)
				dlgOK.SetLine(1, lines[0]);
			if (lines.Length == 2)
				dlgOK.SetLine(2, lines[1]);
			if (lines.Length == 3)
				dlgOK.SetLine(2, lines[2]);

			dlgOK.DoModal(GUIWindowManager.ActiveWindow);
		}

		public static bool DialogConfirm(string msg)
		{
			GUIDialogYesNo dlg = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
			if (dlg == null)
				return false;

			dlg.Reset();
			dlg.SetHeading(Translation.Confirmation);
			dlg.SetLine(2, msg);
			dlg.DoModal(GUIWindowManager.ActiveWindow);

			return dlg.IsConfirmed;
		}

		public static bool DialogText(ref string text, int windowID)
		{
			return DialogText(ref text, false, windowID);
		}

		public static bool DialogText(ref string text, bool password, int windowID)
		{
			VirtualKeyboard keyBoard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
			if (keyBoard == null)
				return false;

			keyBoard.Reset();
			keyBoard.IsSearchKeyboard = true;
			keyBoard.Text = text;
			keyBoard.Password = password;
			keyBoard.DoModal(windowID);

			if (keyBoard.IsConfirmed)
			{
				text = keyBoard.Text;
				return true;
			}

			return false;
		}

		public static bool DialogLanguage(ref String language, bool allowNone)
		{
			//get list of languages (sorted by name)
			List<string> lstLanguages = Utils.GetAllAudioSubtitleLanguages();

			//show the selection dialog
			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return false;

			dlg.Reset();
            dlg.SetHeading(Translation.SelectDefaultLanguage);

            dlg.Add("< " + Translation.UseSystemDefault + " >");
            dlg.Add("< " + Translation.UseFileDefault + " >");
            if (allowNone)
                dlg.Add("< " + Translation.None + " >");
            int index = allowNone ? 2 : 1;
			foreach (string lang in lstLanguages)
			{
				dlg.Add(lang);
				index++;
				if (lang.Equals(language, StringComparison.OrdinalIgnoreCase))
					dlg.SelectedLabel = index;
			}

			dlg.DoModal(GUIWindowManager.ActiveWindow);

			if (dlg.SelectedLabel < 0)
				return false;

			if (dlg.SelectedLabel == 0)
				language = string.Empty;
            else if (dlg.SelectedLabel == 1)
                language = "<" + Translation.File + ">";
            else if (allowNone && dlg.SelectedLabel == 2)
                language = "<" + Translation.NoneL + ">";
            else
				language = dlg.SelectedLabelText;

			return true;
		}

		public static bool DialogSelectSeries(ref AnimeSeriesVM ser, List<AnimeSeriesVM> seriesList)
		{
			//show the selection dialog
			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return false;

			dlg.Reset();
            dlg.SetHeading(Translation.SelectSeries);

            int index = 0;
			foreach (AnimeSeriesVM serTemp in seriesList)
			{
				dlg.Add(serTemp.SeriesName);
				index++;
			}

			dlg.DoModal(GUIWindowManager.ActiveWindow);

			if (dlg.SelectedLabel < 0)
				return false;
			ser = seriesList[dlg.SelectedLabel];

			return true;
		}

		public static bool DialogSelectGFQuickSort(ref string sortType, ref GroupFilterSortDirection sortDirection, string previousMenu)
		{
			//show the selection dialog
			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return false;

			List<string> sortTypes = GroupFilterHelper.GetQuickSortTypes();

			while (true)
			{
				dlg.Reset();

                dlg.SetHeading(Translation.QuickSort);

                dlg.Add("<<< " + previousMenu);

                string menu = string.Format("{1} ({0}) >>>", sortDirection == GroupFilterSortDirection.Asc ? Translation.Asc : Translation.Desc, Translation.SortDirection);
                
				dlg.Add(menu);

				int index = 0;
				foreach (string srt in sortTypes)
				{
					dlg.Add(srt);
					index++;
				}

				dlg.DoModal(GUIWindowManager.ActiveWindow);
				int selection = dlg.SelectedLabel;

				if (selection <= 0)
					return true;

				if (selection == 1)
				{
                    DialogSelectGFQuickSortDirection(ref sortDirection, Translation.QuickSort);
                    // display quick sort again
                }
				else
				{
					sortType = sortTypes[selection - 2];
					return false;
				}
			}

		}

		public static bool DialogSelectGFQuickSortDirection(ref GroupFilterSortDirection sortDirection, string previousMenu)
		{
			//show the selection dialog
			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return false;

			dlg.Reset();
            dlg.SetHeading(Translation.SortDirection);

            dlg.Add("<<< " + previousMenu);
            dlg.Add(Translation.Ascending);
            dlg.Add(Translation.Descending);

            dlg.DoModal(GUIWindowManager.ActiveWindow);
			int selection = dlg.SelectedLabel;

			if (selection <= 0)
				return true;

			if (selection == 1) sortDirection = GroupFilterSortDirection.Asc;
			if (selection == 2) sortDirection = GroupFilterSortDirection.Desc;

			return true;
		}
		

		public static string GetBaseImagesPath()
		{
			//string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			AnimePluginSettings settings = new AnimePluginSettings();
			string filePath = Path.Combine(settings.ThumbsFolder, "Anime3");

            // If user has custom thumbs folder do not add additional folder to path to allows for shared server thumb path
            if (settings.HasCustomThumbsFolder)
            {
                filePath = settings.ThumbsFolder;
            }

            if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static string GetBaseAniDBImagesPath()
		{
			string filePath = Path.Combine(GetBaseImagesPath(), "AniDB");

			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static string GetBaseAniDBCharacterImagesPath()
		{
			string filePath = Path.Combine(GetBaseImagesPath(), "AniDB_Char");

			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static string GetBaseAniDBCreatorImagesPath()
		{
			string filePath = Path.Combine(GetBaseImagesPath(), "AniDB_Creator");

			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static string GetImagesTempFolder()
		{
			string filePath = Path.Combine(GetBaseImagesPath(), "_Temp_");

			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static string GetAniDBImagePath(int animeID)
		{
			string subFolder = "";
			string sid = animeID.ToString();
			if (sid.Length == 1)
				subFolder = sid;
			else
				subFolder = sid.Substring(0, 2);

			string filePath = Path.Combine(GetBaseAniDBImagesPath(), subFolder);

			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static string GetAniDBCharacterImagePath(int charID)
		{
			string subFolder = "";
			string sid = charID.ToString();
			if (sid.Length == 1)
				subFolder = sid;
			else
				subFolder = sid.Substring(0, 2);

			string filePath = Path.Combine(GetBaseAniDBCharacterImagesPath(), subFolder);

			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static string GetAniDBCreatorImagePath(int creatorID)
		{
			string subFolder = "";
			string sid = creatorID.ToString();
			if (sid.Length == 1)
				subFolder = sid;
			else
				subFolder = sid.Substring(0, 2);

			string filePath = Path.Combine(GetBaseAniDBCreatorImagesPath(), subFolder);

			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static string GetBaseTraktImagesPath()
		{
			string filePath = Path.Combine(GetBaseImagesPath(), "Trakt");

			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static string GetTraktImagePath()
		{
			string filePath = GetBaseTraktImagesPath();

			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static string GetBaseTvDBImagesPath()
		{
			string filePath = Path.Combine(GetBaseImagesPath(), "TvDB");

			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static string GetTvDBImagePath()
		{
			string filePath = GetBaseTvDBImagesPath();

			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static string GetBaseMovieDBImagesPath()
		{
			string filePath = Path.Combine(GetBaseImagesPath(), "MovieDB");

			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static string GetMovieDBImagePath()
		{
			string filePath = GetBaseMovieDBImagesPath();

			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			return filePath;
		}

		public static List<string> GetAllAudioSubtitleLanguages()
		{
			List<string> lstPrefLanguages = new List<string>();
			List<string> lstLanguages = new List<string>();
			foreach (System.Globalization.CultureInfo cultureInformation in System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.NeutralCultures))
			{
				if (cultureInformation.ThreeLetterISOLanguageName.Equals("eng", StringComparison.InvariantCultureIgnoreCase) ||
					cultureInformation.ThreeLetterISOLanguageName.Equals("jpn", StringComparison.InvariantCultureIgnoreCase))
				{
					lstPrefLanguages.Add(cultureInformation.EnglishName);
				}
				else
				{
					lstLanguages.Add(cultureInformation.EnglishName);
				}
			}

			lstLanguages.Sort(StringComparer.OrdinalIgnoreCase);

			// add back preferred languages
			foreach (string lan in lstPrefLanguages)
				lstLanguages.Insert(0, lan);

			return lstLanguages;
		}

		public static string PromptSelectTag(string title)
		{
			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return "";

			List<string> allTags = new List<string>();
			allTags = new List<string>(JMMServerVM.Instance.clientBinaryHTTP.GetAllTagNames());
			allTags.Sort();

			dlg.Reset();
			if (string.IsNullOrEmpty(title))
                dlg.SetHeading(Translation.SelectTag);
            else
				dlg.SetHeading(title);

			foreach (string tag in allTags)
				dlg.Add(tag);

			dlg.DoModal(GUIWindowManager.ActiveWindow);

			if (dlg.SelectedId > 0)
				return allTags[dlg.SelectedId - 1];
			else
				return "";

		}
	}
}
