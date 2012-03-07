using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MyAnimePlugin3.ViewModel
{
	public class AniDB_CharacterVM
	{
		public int AniDB_CharacterID { get; set; }
		public int CharID { get; set; }
		public string PicName { get; set; }
		public string CreatorListRaw { get; set; }
		public string CharName { get; set; }
		public string CharKanjiName { get; set; }
		public string CharDescription { get; set; }

		// from AniDB_Anime_Character
		public string CharType { get; set; }

		public AniDB_SeiyuuVM Creator { get; set; }
		public AniDB_AnimeVM Anime { get; set; }

		public override string ToString()
		{
			return string.Format("CHAR: {0} - {1} ({2})", CharID, CharName, PosterPath);
		}

		public string PosterPath
		{
			get
			{
				if (string.IsNullOrEmpty(PicName)) return "";

				return Path.Combine(Utils.GetAniDBCharacterImagePath(CharID), PicName);
			}
		}

		public AniDB_CharacterVM(JMMServerBinary.Contract_AniDB_Character details)
		{
			this.AniDB_CharacterID = details.AniDB_CharacterID;
			this.CharID = details.CharID;
			this.PicName = details.PicName;
			this.CreatorListRaw = details.CreatorListRaw;
			this.CharName = details.CharName;
			this.CharKanjiName = details.CharKanjiName;
			this.CharDescription = details.CharDescription;

			this.CharType = details.CharType;

			if (details.Seiyuu != null)
				this.Creator = new AniDB_SeiyuuVM(details.Seiyuu);

			if (details.Anime != null)
				this.Anime = new AniDB_AnimeVM(details.Anime);
			
		}
	}
}
