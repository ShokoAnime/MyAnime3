using System;
using System.Collections.Generic;
using System.Text;

namespace MyAnimePlugin3.Downloads
{
	public class TorrentFileList
	{
		private int _build;
		public int build
		{
			get { return _build; }
			set { _build = value; }
		}

		private object[] _files;
		public object[] files
		{
			get { return _files; }
			set { _files = value; }
		}
	}
}
