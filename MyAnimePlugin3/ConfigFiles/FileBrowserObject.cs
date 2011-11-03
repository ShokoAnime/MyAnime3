using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MyAnimePlugin3.ConfigFiles
{
	public class FileBrowserObject
	{
		private bool isTopLevelShare = false;
		public bool IsTopLevelShare
		{
			get { return isTopLevelShare; }
			set { isTopLevelShare = value; }
		}

		private string path = "";
		public string Path
		{
			get { return path; }
			set { path = value; }
		}

		private string displayName = "";
		public string DisplayName
		{
			get { return displayName; }
			set { displayName = value; }
		}

		private bool isFile = false;
		public bool IsFile
		{
			get { return isFile; }
			set { isFile = value; }
		}

		private void SetDisplayName()
		{
			if (File.Exists(path))
			{
				FileInfo fi = new FileInfo(path);
				displayName = fi.Name;
				IsFile = true;
			}
			else if (Directory.Exists(path))
			{
				if (isTopLevelShare)
				{
					displayName = string.Format("[ {0} ]", path);
				}
				else
				{
					DirectoryInfo di = new DirectoryInfo(path);
					displayName = string.Format("[ {0} ]", di.Name);
				}
				IsFile = false;
			}
			else
			{
				displayName = path;
				IsFile = true;
			}
		}

		public FileBrowserObject()
		{
		}

		public FileBrowserObject(bool isTopLevel, string path)
		{
			this.isTopLevelShare = isTopLevel;
			this.path = path;
			SetDisplayName();
		}
	}
}
