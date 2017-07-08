using System;
using System.IO;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using Action = MediaPortal.GUI.Library.Action;

namespace Shoko.MyAnime3.Windows
{
    public class RatingDialog : GUIDialogWindow
    {
        public static int GetWindowID
        { get { return Constants.WindowIDs.RATINGDIALOG; } }

        public override int GetID
        { get { return Constants.WindowIDs.RATINGDIALOG; } }

        public int GetWindowId()
        { return Constants.WindowIDs.RATINGDIALOG; }

        /*private enum TenStarDescription {
			Abysmal = 1,
			Terrible,
			Bad,
			Poor,
			Mediocre,
			Fair,
			Good,
			Great,
			Superb,
			Perfect
		}

		private enum FiveStarDescription {			
			Terrible = 1,						
			Mediocre,			
			Good,			
			Superb,
			Perfect
		}*/

        public enum StarDisplay
        {
            FIVE_STARS = 5,
            TEN_STARS = 10
        }

        [SkinControl(6)] protected GUILabelControl lblText = null;
        [SkinControl(7)] protected GUILabelControl lblRating = null;
        [SkinControl(100)] protected GUICheckMarkControl btnStar1 = null;
        [SkinControl(101)] protected GUICheckMarkControl btnStar2 = null;
        [SkinControl(102)] protected GUICheckMarkControl btnStar3 = null;
        [SkinControl(103)] protected GUICheckMarkControl btnStar4 = null;
        [SkinControl(104)] protected GUICheckMarkControl btnStar5 = null;
        [SkinControl(105)] protected GUICheckMarkControl btnStar6 = null;
        [SkinControl(106)] protected GUICheckMarkControl btnStar7 = null;
        [SkinControl(107)] protected GUICheckMarkControl btnStar8 = null;
        [SkinControl(108)] protected GUICheckMarkControl btnStar9 = null;
        [SkinControl(109)] protected GUICheckMarkControl btnStar10 = null;

        public string Text
        {
            get { return lblText.Label; }

            set { lblText.Label = value; }
        }

        public StarDisplay DisplayStars
        {
            get { return _displayStars; }
            set { _displayStars = value; }
        }

        public StarDisplay _displayStars = StarDisplay.TEN_STARS;

        public int Rating { get; set; }
        public bool IsSubmitted { get; set; }

        /// <summary>
        /// MediaPortal will set #currentmodule with GetModuleName()
        /// </summary>
        /// <returns>Localized Window Name</returns>
        public override string GetModuleName()
        {
            return "Ratings dialog";
        }

        public override void Reset()
        {
            base.Reset();

            SetHeading("");
            SetLine(1, "");
            SetLine(2, "");
            SetLine(3, "");
            SetLine(4, "");
        }

        public override void DoModal(int ParentID)
        {
            LoadSkin();
            AllocResources();
            InitControls();
            UpdateStarVisibility();

            base.DoModal(ParentID);
        }

        public override bool Init()
        {
            return Load(GUIGraphicsContext.Skin + @"\TVSeries.RatingDialog.xml");
        }

        public override void OnAction(Action action)
        {
            switch (action.wID)
            {
                case Action.ActionType.REMOTE_1:
                    Rating = 1;
                    UpdateRating();
                    break;
                case Action.ActionType.REMOTE_2:
                    Rating = 2;
                    UpdateRating();
                    break;
                case Action.ActionType.REMOTE_3:
                    Rating = 3;
                    UpdateRating();
                    break;
                case Action.ActionType.REMOTE_4:
                    Rating = 4;
                    UpdateRating();
                    break;
                case Action.ActionType.REMOTE_5:
                    Rating = 5;
                    UpdateRating();
                    break;
                case Action.ActionType.REMOTE_6:
                    if (DisplayStars == StarDisplay.FIVE_STARS) break;
                    Rating = 6;
                    UpdateRating();
                    break;
                case Action.ActionType.REMOTE_7:
                    if (DisplayStars == StarDisplay.FIVE_STARS) break;
                    Rating = 7;
                    UpdateRating();
                    break;
                case Action.ActionType.REMOTE_8:
                    if (DisplayStars == StarDisplay.FIVE_STARS) break;
                    Rating = 8;
                    UpdateRating();
                    break;
                case Action.ActionType.REMOTE_9:
                    if (DisplayStars == StarDisplay.FIVE_STARS) break;
                    Rating = 9;
                    UpdateRating();
                    break;
                case Action.ActionType.REMOTE_0:
                    if (DisplayStars == StarDisplay.FIVE_STARS) break;
                    Rating = 10;
                    UpdateRating();
                    break;
                case Action.ActionType.ACTION_SELECT_ITEM:
                    IsSubmitted = true;
                    PageDestroy();
                    return;
                case Action.ActionType.ACTION_PREVIOUS_MENU:
                case Action.ActionType.ACTION_CLOSE_DIALOG:
                case Action.ActionType.ACTION_CONTEXT_MENU:
                    IsSubmitted = false;
                    PageDestroy();
                    return;
            }

            base.OnAction(action);
        }

        protected override void OnClicked(int controlId, GUIControl control,
            MediaPortal.GUI.Library.Action.ActionType actionType)
        {
            base.OnClicked(controlId, control, actionType);
            if (control == btnStar1)
            {
                Rating = 1;
                IsSubmitted = true;
                PageDestroy();
                return;
            }
            else if (control == btnStar2)
            {
                Rating = 2;
                IsSubmitted = true;
                PageDestroy();
                return;
            }
            else if (control == btnStar3)
            {
                Rating = 3;
                IsSubmitted = true;
                PageDestroy();
                return;
            }
            else if (control == btnStar4)
            {
                Rating = 4;
                IsSubmitted = true;
                PageDestroy();
                return;
            }
            else if (control == btnStar5)
            {
                Rating = 5;
                IsSubmitted = true;
                PageDestroy();
                return;
            }
            else if (control == btnStar6)
            {
                Rating = 6;
                IsSubmitted = true;
                PageDestroy();
                return;
            }
            else if (control == btnStar7)
            {
                Rating = 7;
                IsSubmitted = true;
                PageDestroy();
                return;
            }
            else if (control == btnStar8)
            {
                Rating = 8;
                IsSubmitted = true;
                PageDestroy();
                return;
            }
            else if (control == btnStar9)
            {
                Rating = 9;
                IsSubmitted = true;
                PageDestroy();
                return;
            }
            else if (control == btnStar10)
            {
                Rating = 10;
                IsSubmitted = true;
                PageDestroy();
                return;
            }
        }

        public override bool OnMessage(GUIMessage message)
        {
            switch (message.Message)
            {
                case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
                    base.OnMessage(message);
                    IsSubmitted = false;
                    UpdateRating();
                    return true;

                case GUIMessage.MessageType.GUI_MSG_SETFOCUS:
                    if (message.TargetControlId < 100 || message.TargetControlId > (100 + (int) DisplayStars))
                        break;

                    Rating = message.TargetControlId - 99;
                    UpdateRating();
                    break;
            }
            return base.OnMessage(message);
        }

        private void UpdateRating()
        {
            GUICheckMarkControl[] btnStars;
            if (DisplayStars == StarDisplay.FIVE_STARS)
            {
                btnStars = new GUICheckMarkControl[5]
                {
                    btnStar1, btnStar2, btnStar3, btnStar4, btnStar5
                };
            }
            else
            {
                btnStars = new GUICheckMarkControl[10]
                {
                    btnStar1, btnStar2, btnStar3, btnStar4, btnStar5,
                    btnStar6, btnStar7, btnStar8, btnStar9, btnStar10
                };
            }

            for (int i = 0; i < (int) DisplayStars; i++)
            {
                btnStars[i].Label = string.Empty;
                btnStars[i].Selected = (Rating >= i + 1);
            }
            btnStars[Rating - 1].Focus = true;

            // Display Rating Description
            if (lblRating != null)
            {
                lblRating.Label = string.Format("({0}) {1} / {2}", GetRatingDescription(), Rating.ToString(),
                    (int) DisplayStars);
            }
        }

        public void SetHeading(string HeadingLine)
        {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, 1, 0, 0, null);
            msg.Label = HeadingLine;
            OnMessage(msg);
        }

        public void SetLine(int LineNr, string Line)
        {
            if (LineNr < 1) return;
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, 1 + LineNr, 0, 0, null);
            msg.Label = Line;
            if ((msg.Label == string.Empty) || (msg.Label == "")) msg.Label = "  ";
            OnMessage(msg);
        }

        private void UpdateStarVisibility()
        {

            // Check skin supports 10 stars, if not fallback to 5 stars
            if (btnStar10 == null && DisplayStars == StarDisplay.TEN_STARS)
                DisplayStars = StarDisplay.FIVE_STARS;

            // Hide star controls 6-10
            if (DisplayStars == StarDisplay.FIVE_STARS)
            {
                if (btnStar6 != null) btnStar6.Visible = false;
                if (btnStar7 != null) btnStar7.Visible = false;
                if (btnStar8 != null) btnStar8.Visible = false;
                if (btnStar9 != null) btnStar9.Visible = false;
                if (btnStar10 != null) btnStar10.Visible = false;
            }
        }

        private string GetRatingDescription()
        {

            string description = string.Empty;

            if (DisplayStars == StarDisplay.FIVE_STARS)
            {
                switch (Rating)
                {
                    case 1:
                        description = Translation.RateOne;
                        break;
                    case 2:
                        description = Translation.RateTwo;
                        break;
                    case 3:
                        description = Translation.RateThree;
                        break;
                    case 4:
                        description = Translation.RateFour;
                        break;
                    case 5:
                        description = Translation.RateFive;
                        break;
                }
            }
            else
            {
                switch (Rating)
                {
                    case 1:
                        description = Translation.RateOne;
                        break;
                    case 2:
                        description = Translation.RateTwo;
                        break;
                    case 3:
                        description = Translation.RateThree;
                        break;
                    case 4:
                        description = Translation.RateFour;
                        break;
                    case 5:
                        description = Translation.RateFive;
                        break;
                    case 6:
                        description = Translation.RateSix;
                        break;
                    case 7:
                        description = Translation.RateSeven;
                        break;
                    case 8:
                        description = Translation.RateEight;
                        break;
                    case 9:
                        description = Translation.RateNine;
                        break;
                    case 10:
                        description = Translation.RateTen;
                        break;
                }
            }
            return description;
        }
    }
}

