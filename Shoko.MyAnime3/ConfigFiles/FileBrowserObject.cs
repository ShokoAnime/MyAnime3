using System.IO;

namespace Shoko.MyAnime3.ConfigFiles
{
    public class FileBrowserObject
    {
        public bool IsTopLevelShare { get; set; }

        public string Path { get; set; } = "";

        public string DisplayName { get; set; } = "";

        public bool IsFile { get; set; }

        private void SetDisplayName()
        {
            if (File.Exists(Path))
            {
                FileInfo fi = new FileInfo(Path);
                DisplayName = fi.Name;
                IsFile = true;
            }
            else if (Directory.Exists(Path))
            {
                if (IsTopLevelShare)
                {
                    DisplayName = $"[ {Path} ]";
                }
                else
                {
                    DirectoryInfo di = new DirectoryInfo(Path);
                    DisplayName = $"[ {di.Name} ]";
                }
                IsFile = false;
            }
            else
            {
                DisplayName = Path;
                IsFile = true;
            }
        }

        public FileBrowserObject()
        {
        }

        public FileBrowserObject(bool isTopLevel, string path)
        {
            IsTopLevelShare = isTopLevel;
            Path = path;
            SetDisplayName();
        }
    }
}