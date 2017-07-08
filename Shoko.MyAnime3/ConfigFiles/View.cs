using System;
using System.Collections.Generic;

namespace Shoko.MyAnime3.ConfigFiles
{
    public class View
    {
        #region Helper objects

        public static int CompareByName(View x, View y)
        {
            return String.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
        }

        public enum eShowType
        {
            ignore = 0,
            show = 1,
            hide = 2
        }

        public static string ShowTypeToString(eShowType showType)
        {
            switch (showType)
            {
                case eShowType.show:
                    return "show";
                case eShowType.hide:
                    return "hide";
                case eShowType.ignore:
                    return "-";
            }

            return "";
        }

        public static eShowType NextShowType(eShowType showType)
        {
            switch (showType)
            {
                case eShowType.show:
                    return eShowType.hide;
                case eShowType.hide:
                    return eShowType.ignore;
                case eShowType.ignore:
                    return eShowType.show;
            }

            return eShowType.ignore;
        }

        public static eShowType GetShowType(string str, List<string> show, List<string> hide)
        {
            if (show.Contains(str))
                return eShowType.show;
            if (hide.Contains(str))
                return eShowType.hide;
            return eShowType.ignore;
        }

        public static void SetShowType(eShowType value, string str, List<string> show, List<string> hide)
        {
            if (value == eShowType.show)
                show.Add(str);
            else if (value == eShowType.hide)
                hide.Add(str);
        }

        public enum eSortType
        {
            Name = 1,
            AniDBRating = 2,
            UserRating = 3,
            AirDate = 4,
            WatchedDate = 5,
            AddedDate = 6
        }

        public enum eLabelStyleGroups
        {
            SystemDefault = 0,
            WatchedUnwatched = 1,
            Unwatched = 2,
            TotalEpisodes = 3
        }

        public enum eLabelStyleEpisodes
        {
            SystemDefault = 0,
            IconsDate = 1,
            IconsOnly = 2
        }

        public enum eDefaultViewType
        {
            All = 0,
            Faves = 1,
            FavesUnwatched = 2,

            //Genre = 3, -> no longer used
            //Year = 4, -> no longer used
            CompleteSeries = 5,
            NewSeason = 6,
            LastWatched = 7,
            MissingEps = 8,
            BluRay = 9,
            DVD = 10
        }

        public class ShowTypeIndexer
        {
            private readonly Dictionary<string, eShowType> _dct;

            public ShowTypeIndexer(Dictionary<string, eShowType> dct)
            {

                _dct = dct;
            }

            public bool? ShowValue(string[] values)
            {
                bool? result = null;
                foreach (KeyValuePair<string, eShowType> kvp in _dct)
                {
                    bool set = false;
                    foreach (string val in values)
                        if (val == kvp.Key)
                        {
                            set = true;
                            break;
                        }

                    bool? show = View.ShowValue(kvp.Value, set);
                    if (show.HasValue)
                        if (show.Value)
                            result = true;
                        else
                            return false;
                }

                return result;
            }

            public void SetValues(string show, string hide)
            {
                _dct.Clear();
                if (!string.IsNullOrEmpty(show))
                    foreach (string val in show.Split(','))
                        this[val] = eShowType.show;

                if (!string.IsNullOrEmpty(hide))
                    foreach (string val in hide.Split(','))
                        this[val] = eShowType.hide;
            }

            public void GetValues(ref string show, ref string hide)
            {
                show = string.Empty;
                hide = string.Empty;
                foreach (KeyValuePair<string, eShowType> kvp in _dct)
                    if (kvp.Value == eShowType.show)
                        show += ',' + kvp.Key;
                    else if (kvp.Value == eShowType.hide)
                        hide += ',' + kvp.Key;
                show = show.Trim(',');
                hide = hide.Trim(',');
            }

            public eShowType this[string index]
            {
                get
                {
                    eShowType showType;
                    if (_dct.TryGetValue(index, out showType))
                        return showType;
                    return eShowType.ignore;
                }
                set
                {
                    if (value == eShowType.ignore)
                        _dct.Remove(index);
                    else
                        _dct[index] = value;
                }
            }

            public void Reset()
            {
                _dct.Clear();
            }
        }

        #endregion

        #region Name

        public string Name { get; set; }

        public string DisplayName
        {
            get { return Name + (isEdited ? "*" : ""); }
        }

        #endregion

        public bool isEdited { get; set; }

        #region Construction

        public View()
        {
            ShowTypeGenre = new ShowTypeIndexer(_dctGenre);
            ShowTypeYear = new ShowTypeIndexer(_dctYear);
            ShowTypeAudioLanguages = new ShowTypeIndexer(_dctAudioLang);
            ShowTypeSubtitleLanguages = new ShowTypeIndexer(_dctSubtitleLang);
            ShowTypeAnimeTypes = new ShowTypeIndexer(_dctAnimeType);
        }

        public View(View view) : this()
        {
            //copy all members (deep copy)
            Name = view.Name;
            isEdited = view.isEdited;
            _showTypeCompleted = view._showTypeCompleted;
            _showTypeAdultContent = view._showTypeAdultContent;
            _showTypeMissingEpisodes = view._showTypeMissingEpisodes;
            _showTypeBluRay = view._showTypeBluRay;
            _showTypeDVD = view._showTypeDVD;
            _showTypeFavorite = view._showTypeFavorite;
            _showTypeNewEps = view._showTypeNewEps;
            _showTypeNewSeason = view._showTypeNewSeason;
            _showTypeRecentlyWatched = view._showTypeRecentlyWatched;
            _showTypeWatched = view._showTypeWatched;
            _showByDefault = view._showByDefault;
            SortType = view.SortType;
            LabelStyleEpisodes = view.LabelStyleEpisodes;
            LabelStyleGroups = view.LabelStyleGroups;
            foreach (KeyValuePair<string, eShowType> y in view._dctGenre)
                _dctGenre.Add(y.Key, y.Value);
            foreach (KeyValuePair<string, eShowType> y in view._dctYear)
                _dctYear.Add(y.Key, y.Value);
            foreach (KeyValuePair<string, eShowType> y in view._dctAudioLang)
                _dctAudioLang.Add(y.Key, y.Value);
            foreach (KeyValuePair<string, eShowType> y in view._dctSubtitleLang)
                _dctSubtitleLang.Add(y.Key, y.Value);
            foreach (KeyValuePair<string, eShowType> y in view._dctAnimeType)
                _dctAnimeType.Add(y.Key, y.Value);
        }

        public View(eDefaultViewType viewType) : this()
        {
            switch (viewType)
            {
                case eDefaultViewType.All:
                    Name = "All";
                    break;
                case eDefaultViewType.Faves:
                    Name = "Favorites";
                    ShowTypeFavorite = eShowType.show;
                    break;
                case eDefaultViewType.FavesUnwatched:
                    Name = "Unwatched Favorites";
                    ShowTypeFavorite = eShowType.show;
                    ShowTypeWatched = eShowType.hide;
                    break;
                case eDefaultViewType.CompleteSeries:
                    Name = "Complete Series";
                    ShowTypeCompleted = eShowType.show;
                    break;
                case eDefaultViewType.NewSeason:
                    Name = "New Season";
                    ShowTypeNewSeason = eShowType.show;
                    break;
                case eDefaultViewType.LastWatched:
                    Name = "Last Watched";
                    ShowTypeRecentlyWatched = eShowType.show;
                    ShowTypeWatched = eShowType.hide;
                    SortType = eSortType.WatchedDate;
                    break;
                case eDefaultViewType.MissingEps:
                    Name = "Missing Episodes";
                    ShowTypeMissingEpisodes = eShowType.show;
                    break;
                case eDefaultViewType.BluRay:
                    Name = "Blu-Ray";
                    ShowTypeBluRay = eShowType.show;
                    break;
                case eDefaultViewType.DVD:
                    Name = "DVD";
                    ShowTypeDVD = eShowType.show;
                    break;
            }
        }

        public bool EqualsView(View v)
        {
            if (!ContentEquals(v))
                return false;

            if (LabelStyleEpisodes != v.LabelStyleEpisodes
                || LabelStyleGroups != v.LabelStyleGroups
                || SortType != v.SortType)
                return false;

            return true;
        }

        public bool ContentEquals(object obj)
        {
            View view = obj as View;
            if (view == null)
                return false;

            if (_showTypeCompleted != view._showTypeCompleted
                || _showTypeNewEps != view._showTypeNewEps
                || _showTypeNewSeason != view._showTypeNewSeason
                || _showTypeWatched != view._showTypeWatched
                || _showTypeFavorite != view._showTypeFavorite
                || _showTypeAdultContent != view._showTypeAdultContent
                || _showTypeMissingEpisodes != view._showTypeMissingEpisodes
                || _showTypeBluRay != view._showTypeBluRay
                || _showTypeDVD != view._showTypeDVD
                || _showTypeRecentlyWatched != view._showTypeRecentlyWatched)
                return false;

            if (_dctGenre.Count != view._dctGenre.Count
                || _dctYear.Count != view._dctYear.Count
                || _dctAudioLang.Count != view._dctAudioLang.Count
                || _dctSubtitleLang.Count != view._dctSubtitleLang.Count
                || _dctAnimeType.Count != view._dctAnimeType.Count)
                return false;

            foreach (KeyValuePair<string, eShowType> kvp in _dctGenre)
            {
                if (!view._dctGenre.ContainsKey(kvp.Key))
                    return false;
                if (kvp.Value != view._dctGenre[kvp.Key])
                    return false;
            }

            foreach (KeyValuePair<string, eShowType> kvp in _dctYear)
            {
                if (!view._dctYear.ContainsKey(kvp.Key))
                    return false;
                if (kvp.Value != view._dctYear[kvp.Key])
                    return false;
            }

            foreach (KeyValuePair<string, eShowType> kvp in _dctAudioLang)
            {
                if (!view._dctAudioLang.ContainsKey(kvp.Key))
                    return false;
                if (kvp.Value != view._dctAudioLang[kvp.Key])
                    return false;
            }

            foreach (KeyValuePair<string, eShowType> kvp in _dctSubtitleLang)
            {
                if (!view._dctSubtitleLang.ContainsKey(kvp.Key))
                    return false;
                if (kvp.Value != view._dctSubtitleLang[kvp.Key])
                    return false;
            }

            foreach (KeyValuePair<string, eShowType> kvp in _dctAnimeType)
            {
                if (!view._dctAnimeType.ContainsKey(kvp.Key))
                    return false;
                if (kvp.Value != view._dctAnimeType[kvp.Key])
                    return false;
            }

            return true;
        }

        #endregion

        #region Content

        #region Show Types

        eShowType _showTypeCompleted = eShowType.ignore;

        public eShowType ShowTypeCompleted
        {
            get { return _showTypeCompleted; }
            set
            {
                _showTypeCompleted = value;
                UpdateDefault(_showTypeCompleted);
            }
        }

        eShowType _showTypeNewEps = eShowType.ignore;

        public eShowType ShowTypeNewEps
        {
            get { return _showTypeNewEps; }
            set
            {
                _showTypeNewEps = value;
                UpdateDefault(_showTypeNewEps);
            }
        }

        eShowType _showTypeNewSeason = eShowType.ignore;

        public eShowType ShowTypeNewSeason
        {
            get { return _showTypeNewSeason; }
            set
            {
                _showTypeNewSeason = value;
                UpdateDefault(_showTypeNewSeason);
            }
        }

        eShowType _showTypeWatched = eShowType.ignore;

        public eShowType ShowTypeWatched
        {
            get { return _showTypeWatched; }
            set
            {
                _showTypeWatched = value;
                UpdateDefault(_showTypeWatched);
            }
        }

        eShowType _showTypeFavorite = eShowType.ignore;

        public eShowType ShowTypeFavorite
        {
            get { return _showTypeFavorite; }
            set
            {
                _showTypeFavorite = value;
                UpdateDefault(_showTypeFavorite);
            }
        }

        eShowType _showTypeRecentlyWatched = eShowType.ignore;

        public eShowType ShowTypeRecentlyWatched
        {
            get { return _showTypeRecentlyWatched; }
            set
            {
                _showTypeRecentlyWatched = value;
                UpdateDefault(_showTypeRecentlyWatched);
            }
        }

        eShowType _showTypeAdultContent = eShowType.ignore;

        public eShowType ShowTypeAdultContent
        {
            get { return _showTypeAdultContent; }
            set
            {
                _showTypeAdultContent = value;
                UpdateDefault(_showTypeAdultContent);
            }
        }

        eShowType _showTypeMissingEpisodes = eShowType.ignore;

        public eShowType ShowTypeMissingEpisodes
        {
            get { return _showTypeMissingEpisodes; }
            set
            {
                _showTypeMissingEpisodes = value;
                UpdateDefault(_showTypeMissingEpisodes);
            }
        }

        eShowType _showTypeBluRay = eShowType.ignore;

        public eShowType ShowTypeBluRay
        {
            get { return _showTypeBluRay; }
            set
            {
                _showTypeBluRay = value;
                UpdateDefault(_showTypeBluRay);
            }
        }

        eShowType _showTypeDVD = eShowType.ignore;

        public eShowType ShowTypeDVD
        {
            get { return _showTypeDVD; }
            set
            {
                _showTypeDVD = value;
                UpdateDefault(_showTypeDVD);
            }
        }

        readonly Dictionary<string, eShowType> _dctGenre = new Dictionary<string, eShowType>();

        public ShowTypeIndexer ShowTypeGenre { get; }

        public bool ShowAllGenres
        {
            get { return _dctGenre.Count == 0; }
        }

        public void SetGenres(string show, string hide)
        {
            ShowTypeGenre.SetValues(show, hide);
        }

        public void GetGenres(ref string show, ref string hide)
        {
            ShowTypeGenre.GetValues(ref show, ref hide);
        }

        readonly Dictionary<string, eShowType> _dctYear = new Dictionary<string, eShowType>();
        public ShowTypeIndexer ShowTypeYear { get; }

        readonly Dictionary<string, eShowType> _dctAudioLang = new Dictionary<string, eShowType>();
        public ShowTypeIndexer ShowTypeAudioLanguages { get; }

        readonly Dictionary<string, eShowType> _dctSubtitleLang = new Dictionary<string, eShowType>();
        public ShowTypeIndexer ShowTypeSubtitleLanguages { get; }

        readonly Dictionary<string, eShowType> _dctAnimeType = new Dictionary<string, eShowType>();
        public ShowTypeIndexer ShowTypeAnimeTypes { get; }

        public bool ShowAllYears
        {
            get { return _dctYear.Count == 0; }
        }

        public bool ShowAllAudioLanguages
        {
            get { return _dctAudioLang.Count == 0; }
        }

        public bool ShowAllSubtitleLanguages
        {
            get { return _dctSubtitleLang.Count == 0; }
        }

        public bool ShowAllAnimeTypes
        {
            get { return _dctAnimeType.Count == 0; }
        }

        public void SetYears(string show, string hide)
        {
            ShowTypeYear.SetValues(show, hide);
        }

        public void GetYears(ref string show, ref string hide)
        {
            ShowTypeYear.GetValues(ref show, ref hide);
        }

        public void SetAudioLanguages(string show, string hide)
        {
            ShowTypeAudioLanguages.SetValues(show, hide);
        }

        public void GetAudioLanguages(ref string show, ref string hide)
        {
            ShowTypeAudioLanguages.GetValues(ref show, ref hide);
        }

        public void SetSubtitleLanguages(string show, string hide)
        {
            ShowTypeSubtitleLanguages.SetValues(show, hide);
        }

        public void GetSubtitleLanguages(ref string show, ref string hide)
        {
            ShowTypeSubtitleLanguages.GetValues(ref show, ref hide);
        }

        public void SetAnimeTypes(string show, string hide)
        {
            ShowTypeAnimeTypes.SetValues(show, hide);
        }

        public void GetAnimeTypes(ref string show, ref string hide)
        {
            ShowTypeAnimeTypes.GetValues(ref show, ref hide);
        }

        #endregion

        #region Show Values

        protected bool _showByDefault = true;

        public void UpdateDefault(eShowType showType)
        {
            if (showType == eShowType.show)
            {
                _showByDefault = false;
                return;
            }

            _showByDefault = _showTypeCompleted != eShowType.show
                             && _showTypeNewEps != eShowType.show
                             && _showTypeNewSeason != eShowType.show
                             && _showTypeWatched != eShowType.show
                             && _showTypeFavorite != eShowType.show
                             && _showTypeAdultContent != eShowType.show
                             && _showTypeMissingEpisodes != eShowType.show
                             && _showTypeRecentlyWatched != eShowType.show
                             && _showTypeBluRay != eShowType.show
                             && _showTypeDVD != eShowType.show
                             && !ItemIsShown(_dctGenre)
                             && !ItemIsShown(_dctYear)
                             && !ItemIsShown(_dctAudioLang)
                             && !ItemIsShown(_dctSubtitleLang)
                             && !ItemIsShown(_dctAnimeType);
        }

        private bool ItemIsShown(Dictionary<string, eShowType> dic)
        {
            foreach (KeyValuePair<string, eShowType> kvp in dic)
                if (kvp.Value == eShowType.show)
                    return true;
            return false;
        }

        public bool ShowByDefault()
        {
            return _showByDefault;
        }

        public static bool? ShowValue(eShowType showType, bool bSet)
        {
            switch (showType)
            {
                case eShowType.show:
                    return bSet ? true : (bool?) null;
                case eShowType.hide:
                    return bSet ? false : (bool?) null;
                case eShowType.ignore:
                default:
                    return null;
            }
        }

        public bool? ShowCompleted(bool isCompleted)
        {
            return ShowValue(_showTypeCompleted, isCompleted);
        }

        public bool? ShowNewEps(bool isNewEps)
        {
            return ShowValue(_showTypeNewEps, isNewEps);
        }

        public bool? ShowNewSeason(bool isNewSeason)
        {
            return ShowValue(_showTypeNewSeason, isNewSeason);
        }

        public bool? ShowWatched(bool isWatched)
        {
            return ShowValue(_showTypeWatched, isWatched);
        }

        public bool? ShowFavorite(bool isFavorite)
        {
            return ShowValue(_showTypeFavorite, isFavorite);
        }

        public bool? ShowRecentlyWatched(bool isRecentlyWatched)
        {
            return ShowValue(_showTypeRecentlyWatched, isRecentlyWatched);
        }

        public bool? ShowAdultContent(bool isAdultContent)
        {
            return ShowValue(_showTypeAdultContent, isAdultContent);
        }

        public bool? ShowMissingEpisodes(bool isMissingEpisodes)
        {
            return ShowValue(_showTypeMissingEpisodes, isMissingEpisodes);
        }

        public bool? ShowBluRay(bool isBluRay)
        {
            return ShowValue(_showTypeBluRay, isBluRay);
        }

        public bool? ShowDVD(bool isDVD)
        {
            return ShowValue(_showTypeDVD, isDVD);
        }

        public bool? ShowGenres(string[] genres)
        {
            return ShowTypeGenre.ShowValue(genres);
        }

        public bool? ShowYears(string[] years)
        {
            return ShowTypeYear.ShowValue(years);
        }

        public bool? ShowAudioLanguages(string[] languages)
        {
            return ShowTypeAudioLanguages.ShowValue(languages);
        }

        public bool? ShowSubtitleLanguages(string[] languages)
        {
            return ShowTypeAudioLanguages.ShowValue(languages);
        }

        #endregion

        #endregion

        #region Sorting

        public eSortType SortType { get; set; } = eSortType.Name;

        #endregion

        #region Label Styles

        public eLabelStyleGroups LabelStyleGroups { get; set; } = eLabelStyleGroups.SystemDefault;

        public eLabelStyleEpisodes LabelStyleEpisodes { get; set; } = eLabelStyleEpisodes.SystemDefault;

        #endregion
    }
}