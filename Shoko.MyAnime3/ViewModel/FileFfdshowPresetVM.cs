﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shoko.Models.Server;

namespace Shoko.MyAnime3.ViewModel
{
	public class FileFfdshowPreset 
	{
		public int? FileFfdshowPresetID { get; set; }
		public string Hash { get; set; }
		public long FileSize { get; set; }
		public string Preset { get; set; }

		public FileFfdshowPreset()
		{
		}

		public FileFfdshowPreset(JMMServerBinary.Contract_FileFfdshowPreset contract)
		{
			this.FileFfdshowPresetID = contract.FileFfdshowPresetID;
			this.Hash = contract.Hash;
			this.FileSize = contract.FileSize;
			this.Preset = contract.Preset;
			
		}

		public JMMServerBinary.Contract_FileFfdshowPreset ToContract()
		{
			JMMServerBinary.Contract_FileFfdshowPreset contract = new JMMServerBinary.Contract_FileFfdshowPreset();
			contract.FileFfdshowPresetID = this.FileFfdshowPresetID;
			contract.Hash = this.Hash;
			contract.FileSize = this.FileSize;
			contract.Preset = this.Preset;
			return contract;
		}

		public override string ToString()
		{
			return string.Format("{0} - {1}", Hash, Preset);
		}
	}
}
