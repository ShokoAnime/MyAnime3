using System;
using Shoko.Models.Enums;

namespace Shoko.MyAnime3.ImageManagement
{
    public class ImageDownloadEventArgs : EventArgs
    {
        public readonly string Status;
        public readonly ImageDownloadRequest Req;
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