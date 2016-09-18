using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyAnimePlugin3.JMMServerBinary;

namespace MyAnimePlugin3.ViewModel
{
    public interface IVideoInfo
    {
        string FileName { get; }
        string Uri { get; }
        Media Media { get; }
        int VideoLocalID { get; }
        bool? IsLocalOrStreaming();

    }
}
