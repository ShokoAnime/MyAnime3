﻿using System;

namespace Shoko.MyAnime3.Events
{
    public class ServerStatusEventArgs : EventArgs
    {
        public bool IsBanned { get; set; }
        public string BanReason { get; set; }

        public int HasherQueueCount { get; set; }
        public string HasherQueueState { get; set; }
        public bool HasherQueueRunning { get; set; }

        public int GeneralQueueCount { get; set; }
        public string GeneralQueueState { get; set; }
        public bool GeneralQueueRunning { get; set; }

        public int ImagesQueueCount { get; set; }
        public string ImagesQueueState { get; set; }
        public bool ImagesQueueRunning { get; set; }
    }
}