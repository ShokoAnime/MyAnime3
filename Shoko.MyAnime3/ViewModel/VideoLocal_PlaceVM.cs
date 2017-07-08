using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shoko.Models.Client;
using Shoko.MyAnime3.JMMServerBinary;

namespace Shoko.MyAnime3.ViewModel
{
    public class CL_VideoLocal_Place : CL_VideoLocal_Place
    {

        public int VideoLocal_Place_ID { get; set; }
        public int VideoLocalID { get; set; }
        public string FilePath { get; set; }
        public int ImportFolderID { get; set; }
        public int ImportFolderType { get; set; }

        public ImportFolder ImportFolder { get; set; }


        public CL_VideoLocal_Place(Contract_VideoLocal_Place contract)
        {
            this.VideoLocal_Place_ID = contract.VideoLocal_Place_ID;
            this.VideoLocalID = contract.VideoLocalID;
            this.FilePath = contract.FilePath;
            this.ImportFolderID = contract.ImportFolderID;
            this.ImportFolderType = contract.ImportFolderType;
            ImportFolder = new ImportFolder(contract.ImportFolder);

        }

    }
}
