using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Runtime.Serialization;

namespace MyAnimePlugin3.Downloads
{
	[DataContract]
	public class Torrent
	{
		private const int StatusStarted = 1;
		private const int StatusChecking = 2;
		private const int StatusStartAfterCheck = 4;
		private const int StatusChecked = 8;
		private const int StatusError = 16;
		private const int StatusPaused = 32;
		private const int StatusQueued = 64;
		private const int StatusLoaded = 128;

		[DataMember]
		public string Hash { get; set;}

		[DataMember]
		public int Status { get; set;}

		[DataMember]
		public string Name { get; set;}

		/// <summary>
		/// in bytes
		/// </summary>
		[DataMember]
		public long Size { get; set;}

		/// <summary>
		/// integer in per mils
		/// </summary>
		[DataMember]
		public long PercentProgress { get; set;}

		public string PercentProgressFormatted
		{
			get
			{
				double pro = (double)PercentProgress / (double)10;
				return String.Format("{0:0.0}%", pro);
			}
		}

		/// <summary>
		/// integer in bytes
		/// </summary>
		[DataMember]
		public long Downloaded { get; set;}

		/// <summary>
		/// integer in bytes
		/// </summary>
		[DataMember]
		public long Uploaded { get; set;}


		/// <summary>
		/// integer in per mils
		/// </summary>
		[DataMember]
		public long Ratio { get; set;}

		/// <summary>
		/// integer in bytes per second
		/// </summary>
		[DataMember]
		public long UploadSpeed { get; set;}

		/// <summary>
		/// integer in bytes per second
		/// </summary>
		[DataMember]
		public long DownloadSpeed { get; set;}

		/// <summary>
		/// integer in seconds
		/// </summary>
		[DataMember]
		public long ETA { get; set;}

		[DataMember]
		public string Label { get; set;}

		[DataMember]
		public long PeersConnected { get; set;}

		[DataMember]
		public long PeersInSwarm { get; set;}

		[DataMember]
		public long SeedsConnected { get; set;}

		[DataMember]
		public long SeedsInSwarm { get; set;}

		/// <summary>
		/// integer in 1/65535ths
		/// </summary>
		[DataMember]
		public long Availability { get; set;}

		[DataMember]
		public long TorrentQueueOrder { get; set;}

		/// <summary>
		/// integer in bytes
		/// </summary>
		[DataMember]
		public long Remaining { get; set;}

		public Torrent()
		{
		}

		public Torrent(object[] row)
		{
			this.Hash = row[0].ToString();
			this.Status = int.Parse(row[1].ToString());
			this.Name = row[2].ToString();
            this.Size = long.Parse(row[3].ToString());
			this.PercentProgress = long.Parse(row[4].ToString());
			this.Downloaded = long.Parse(row[5].ToString());
			this.Uploaded = long.Parse(row[6].ToString());
			this.Ratio = long.Parse(row[7].ToString());
			this.UploadSpeed = long.Parse(row[8].ToString());
			this.DownloadSpeed = long.Parse(row[9].ToString());
			this.ETA = long.Parse(row[10].ToString());
			this.Label = row[11].ToString();
			this.PeersConnected = long.Parse(row[12].ToString());
			this.PeersInSwarm = long.Parse(row[13].ToString());
			this.SeedsConnected = long.Parse(row[14].ToString());
			this.SeedsInSwarm = long.Parse(row[15].ToString());
			this.Availability = long.Parse(row[16].ToString());
			this.TorrentQueueOrder = long.Parse(row[17].ToString());
			this.Remaining = long.Parse(row[18].ToString());
		}

		public override string ToString()
		{
			return string.Format("Torrent: {0} - {1} - {2}", Name, PercentProgressFormatted, Status);
		}

		public string SizeFormatted
		{
			get
			{
				return Utils.FormatByteSize(Size);
			}
		}

		public string DownloadSpeedFormatted
		{
			get
			{
				return Utils.FormatByteSize((long)DownloadSpeed) + "/sec";
			}
		}

		public string UploadSpeedFormatted
		{
			get
			{
				return Utils.FormatByteSize((long)UploadSpeed) + "/sec";
			}
		}

		public string DownloadedFormatted
		{
			get
			{
				return Utils.FormatByteSize((long)Downloaded);
			}
		}

		public string UploadedFormatted
		{
			get
			{
				return Utils.FormatByteSize((long)Uploaded);
			}
		}

		public string RatioFormatted
		{
			get
			{
				double temp = (double)Ratio / (double)1000;
				return String.Format("{0:0.000}", temp);
			}
		}

		public bool IsDownloading
		{
			get
			{
				if (Remaining == 0)
					return false;

				if (Remaining > 0 && Status == 136)
					return false;

				int paused = Status & StatusPaused;
				if (paused > 0)
					return false;

				return true;
			}
		}

		public string StatusFormatted
		{
			get
			{
				if (Status == 136 && Remaining == 0)
					return "Finished";

				if (Status == 136 && Remaining > 0)
					return "Stopped";

				if (Status == 201 && Remaining == 0)
					return "Seeding";

				if (Status == 201 && Remaining > 0)
					return "";

				int paused = Status & StatusPaused;
				if (paused > 0)
					return "Paused";

				return "";
			}
		}

		public string ListDisplay
		{
			get
			{
				if (StatusFormatted.Length > 0)
					return string.Format("{0}: {1} - {2}", StatusFormatted.ToUpper(), Name, PercentProgressFormatted);
				else
					return string.Format("{0} - {1}", Name, PercentProgressFormatted);
			}
		}

	}
}
