using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAnimePlugin3.ImageManagement
{
	public class ImageDownloadEventArgs : EventArgs
	{
		public readonly string Status = string.Empty;
		public readonly ImageDownloadRequest Req = null;
		public readonly ImageDownloadEventType EventType;
		public readonly ImageEntityType ImageType;

		public ImageDownloadEventArgs(string status, ImageDownloadRequest req, ImageDownloadEventType eventType)
		{
			Status = status;
			Req = req;
			EventType = eventType;
			ImageType = Req.ImageType;
		}
	}
}
