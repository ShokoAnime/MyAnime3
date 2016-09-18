using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyAnimePlugin3.JMMServerBinary;

namespace MyAnimePlugin3.ViewModel
{
    public class VideoLocal_PlaceVM
    {

        public int VideoLocal_Place_ID { get; set; }
        public int VideoLocalID { get; set; }
        public string FilePath { get; set; }
        public int ImportFolderID { get; set; }
        public int ImportFolderType { get; set; }

        public ImportFolderVM ImportFolder { get; set; }

        public string LocalFileSystemFullPath
        {
            get
            {
                if (ImportFolder.CloudID.HasValue)
                    return string.Empty;

                BaseConfig.MyAnimeLog.Write("Checking Config for Possible file: "+ ImportFolder.ImportFolderLocation+" / "+FilePath);
                if (BaseConfig.Settings.ImportFolderMappings.ContainsKey(ImportFolderID))
                    return Path.Combine(BaseConfig.Settings.ImportFolderMappings[ImportFolderID], FilePath);
                try
                {
                    if (File.Exists(Path.Combine(ImportFolder.ImportFolderLocation, FilePath)))
                        return Path.Combine(ImportFolder.ImportFolderLocation, FilePath);
                }
                catch (Exception)
                {
                    //ignored
                }
                return string.Empty;
            }
        }

        public string FullPath => Path.Combine(ImportFolder.ImportFolderLocation, FilePath);

        public int CompareTo(VideoLocal_PlaceVM obj)
        {
            return String.Compare(FullPath, obj.FullPath, StringComparison.Ordinal);
        }
        public string FileName => Path.GetFileName(FilePath);

        public string FileDirectory => Path.GetDirectoryName(FullPath);



        public VideoLocal_PlaceVM(Contract_VideoLocal_Place contract)
        {
            this.VideoLocal_Place_ID = contract.VideoLocal_Place_ID;
            this.VideoLocalID = contract.VideoLocalID;
            this.FilePath = contract.FilePath;
            this.ImportFolderID = contract.ImportFolderID;
            this.ImportFolderType = contract.ImportFolderType;
            ImportFolder = new ImportFolderVM(contract.ImportFolder);

        }

    }
}
