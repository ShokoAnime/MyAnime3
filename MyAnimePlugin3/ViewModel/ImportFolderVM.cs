using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MyAnimePlugin3.ViewModel
{
	public class ImportFolderVM
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


        public string LocalPath
		{
			get
			{
				if (ImportFolderID.HasValue)
				{
					if (BaseConfig.Settings.ImportFolderMappings.ContainsKey(ImportFolderID.Value))
						return BaseConfig.Settings.ImportFolderMappings[ImportFolderID.Value];
					else
						return ImportFolderLocation;
				}
				else
					return LocalPathTemp;

			}
		}

		public bool LocalPathIsValid
		{
			get
			{
				try
				{
					if (string.IsNullOrEmpty(LocalPath)) return false;

					return Directory.Exists(LocalPath);
				}
				catch { }

				return false;
			}
		}
        public bool IsCloud => CloudID.HasValue;
        public bool IsNotCloud => !CloudID.HasValue;

        public bool FolderIsDropSource
		{
			get
			{
				return IsDropSource == 1;
			}
		}

		public bool FolderIsDropDestination
		{
			get
			{
				return IsDropDestination == 1;
			}
		}

		public bool FolderIsWatched
		{
			get
			{
				return IsWatched == 1;
			}
		}

		public string Description
		{
			get
			{
				string desc = ImportFolderLocation;
				if (FolderIsDropSource)
					desc += " (Drop Source)";

				if (FolderIsDropDestination)
					desc += " (Drop Destination)";
			    if (CloudID.HasValue)
			        desc += " *** CLOUD FOLDER ***";
				else if (!LocalPathIsValid)
					desc += " *** LOCAL PATH INVALID ***";

				return desc;
			}
		}

		public ImportFolderVM()
		{
		}



		public ImportFolderVM(JMMServerBinary.Contract_ImportFolder contract)
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
