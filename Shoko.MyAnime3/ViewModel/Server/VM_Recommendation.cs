using System;
using MediaPortal.GUI.Library;
using Shoko.Models.Client;

namespace Shoko.MyAnime3.ViewModel.Server
{
	public class VM_Recommendation : CL_Recommendation
	{
	    public new VM_AniDB_Anime Recommended_AniDB_Anime => (VM_AniDB_Anime) base.Recommended_AniDB_Anime;
	    public new VM_AnimeSeries_User Recommended_AnimeSeries => (VM_AnimeSeries_User) base.Recommended_AnimeSeries;
	    public new VM_AniDB_Anime BasedOn_AniDB_Anime => (VM_AniDB_Anime)base.BasedOn_AniDB_Anime;
        public new VM_AnimeSeries_User BasedOn_AnimeSeries => (VM_AnimeSeries_User)base.BasedOn_AnimeSeries;

        public string Recommended_DisplayName
	    {
	        get
	        {
	            if (Recommended_AniDB_Anime != null)
	                return Recommended_AniDB_Anime.FormattedTitle;
	            return Translation.DataMissing;
            }
	    }

	    public string Recommended_Description
	    {
	        get
	        {
	            if (Recommended_AniDB_Anime != null)
	                return Recommended_AniDB_Anime.Description;
                return Translation.OverviewNotAvailable;
            }
	    }

	    public bool Recommended_LocalSeriesExists => (Recommended_AnimeSeries != null);
        public bool Recommended_AnimeInfoExists => (Recommended_AniDB_Anime != null);

	    public string Recommended_ApprovalRating => $"{Utils.FormatPercentage(RecommendedApproval)}";

	    public string Recommended_PosterPath
        {
	        get
	        {
	            if (Recommended_AniDB_Anime != null)
	                return Recommended_AniDB_Anime.PosterPath;
	            return GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_blankchar.png";
            }

	    }

	    public string BasedOn_DisplayName => BasedOn_AniDB_Anime?.FormattedTitle;

	    public string BasedOn_VoteValueFormatted => String.Format("{0:0.0}", BasedOnVoteValue / 100);

	    public string BasedOn_PosterPath => BasedOn_AniDB_Anime?.PosterPath;
	}
}
