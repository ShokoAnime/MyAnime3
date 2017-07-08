using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Shoko.MyAnime3.ViewModel
{
	public class ImportFolder 
	{
		public int? ImportFolderID { get; set; }
        public int ImportFolderType { get; set; }

        public string ImportFolderName { get; set; }
		public string ImportFolderLocation { get; set; }
		public int IsDropSource { get; set; }
		public int IsDropDestination { get; set; }
		public string LocalPathTemp { get; set; }
		public int IsWatched { get; set; }
        public int? CloudID { get; set; }


	    public ImportFolder()
		{
		}



		public ImportFolder(JMMServerBinary.Contract_ImportFolder contract)
		{
			// read only members
			this.ImportFolderID = contract.ImportFolderID;
			this.ImportFolderName = contract.ImportFolderName;
			this.ImportFolderLocation = contract.ImportFolderLocation;
			this.IsDropSource = contract.IsDropSource;
			this.IsDropDestination = contract.IsDropDestination;
			this.IsWatched = contract.IsWatched;
            this.CloudID = contract.CloudID;
            this.ImportFolderType = contract.ImportFolderType;
        }

		public JMMServerBinary.Contract_ImportFolder ToContract()
		{
			JMMServerBinary.Contract_ImportFolder contract = new JMMServerBinary.Contract_ImportFolder();
			contract.ImportFolderID = this.ImportFolderID;
			contract.ImportFolderName = this.ImportFolderName;
			contract.ImportFolderLocation = this.ImportFolderLocation;
			contract.IsDropSource = this.IsDropSource;
			contract.IsDropDestination = this.IsDropDestination;
			contract.IsWatched = this.IsWatched;
            contract.ImportFolderType = this.ImportFolderType;
            contract.CloudID = this.CloudID;
            return contract;
		}



	}
}
