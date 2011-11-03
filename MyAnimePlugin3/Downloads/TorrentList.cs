using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace MyAnimePlugin3.Downloads
{
	[DataContract]
	public class TorrentList
	{
		[DataMember]
		public int build { get; set;}

		[DataMember]
		public List<object> label { get; set; }
		
		[DataMember]
		public List<object[]> torrents { get; set; }

		[DataMember]
		public int torrentc { get; set; }

		public List<Torrent> TorrentObjects { get; set; }


		//torrents
		//torrentc
	}
}
