using System;
using System.IO;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using Action = MediaPortal.GUI.Library.Action;

namespace MyAnimePlugin3.Windows
{
    public class RatingDialog : GUIDialogWindow
    {
        [SkinControlAttribute(7)]
        protected GUILabelControl lblRating = null;
        [SkinControlAttribute(100)]
        protected GUICheckMarkControl btnStar1 = null;
        [SkinControlAttribute(101)]
        protected GUICheckMarkControl btnStar2 = null;
        [SkinControlAttribute(102)]
        protected GUICheckMarkControl btnStar3 = null;
        [SkinControlAttribute(103)]
        protected GUICheckMarkControl btnStar4 = null;
        [SkinControlAttribute(104)]
        protected GUICheckMarkControl btnStar5 = null;
        [SkinControlAttribute(105)]
        protected GUICheckMarkControl btnStar6 = null;
        [SkinControlAttribute(106)]
        protected GUICheckMarkControl btnStar7 = null;
        [SkinControlAttribute(107)]
        protected GUICheckMarkControl btnStar8 = null;
        [SkinControlAttribute(108)]
        protected GUICheckMarkControl btnStar9 = null;
        [SkinControlAttribute(109)]
        protected GUICheckMarkControl btnStar10 = null;
        [SkinControlAttribute(110)]
        protected GUICheckMarkControl btnStar11 = null;
        [SkinControlAttribute(111)]
        protected GUICheckMarkControl btnStar12 = null;
        [SkinControlAttribute(112)]
        protected GUICheckMarkControl btnStar13 = null;
        [SkinControlAttribute(113)]
        protected GUICheckMarkControl btnStar14 = null;
        [SkinControlAttribute(114)]
        protected GUICheckMarkControl btnStar15 = null;
        [SkinControlAttribute(115)]
        protected GUICheckMarkControl btnStar16 = null;
        [SkinControlAttribute(116)]
        protected GUICheckMarkControl btnStar17 = null;
        [SkinControlAttribute(117)]
        protected GUICheckMarkControl btnStar18 = null;
        [SkinControlAttribute(118)]
        protected GUICheckMarkControl btnStar19 = null;
        [SkinControlAttribute(119)]
        protected GUICheckMarkControl btnStar20 = null;

        private GUICheckMarkControl[] buttons;

        public bool IsSubmitted { get; set; }


        private int InternalRating;
    
        public decimal Rating
        {
            get
            {
                decimal v = InternalRating;
                v /= 2;
                return v;
            }
            set
            {
                InternalRating = (int)Math.Round(value*2);
            }
        }


        public static int GetWindowID
        { get { return Constants.WindowIDs.RATINGDIALOG; } }

        public override int GetID
        { get { return Constants.WindowIDs.RATINGDIALOG; } }

        public int GetWindowId()
        { return Constants.WindowIDs.RATINGDIALOG; }




        public override void Reset()
        {
            base.Reset();
            SetHeading(string.Empty);
            SetLine(1, string.Empty);
            SetLine(2, string.Empty);
            SetLine(3, string.Empty);
            SetLine(4, string.Empty);
        }

        public override void DoModal(int ParentID)
        {
                LoadSkin();
                AllocResources();
                InitControls();
                base.DoModal(ParentID);
        }

        public override bool Init()
        {
            string xmlSkin = Path.Combine(GUIGraphicsContext.Skin,"Anime3_Rating.xml");
            bool res=Load(xmlSkin);

            return res;
        }



        public override void OnAction(Action action)
        {
            switch (action.wID) {
                case Action.ActionType.REMOTE_1:
                    InternalRating = 2;
                    UpdateRating();
                    break;
                case Action.ActionType.REMOTE_2:
                    InternalRating = 4;
                    UpdateRating();
                    break;
                case Action.ActionType.REMOTE_3:
                    InternalRating = 6;
                    UpdateRating();
                    break;
                case Action.ActionType.REMOTE_4:
                    InternalRating = 8;
                    UpdateRating();
                    break;
                case Action.ActionType.REMOTE_5:
                    InternalRating = 10;
                    UpdateRating();
                    break;
                case Action.ActionType.REMOTE_6:
                    InternalRating = 12;
                    UpdateRating();
                    break;
                case Action.ActionType.REMOTE_7:
                    InternalRating = 14;
                    UpdateRating();
                    break;
                case Action.ActionType.REMOTE_8:
                    InternalRating = 16;
                    UpdateRating();
                    break;
                case Action.ActionType.REMOTE_9:
                    InternalRating = 18;
                    UpdateRating();
                    break;
                case Action.ActionType.REMOTE_0:
                    InternalRating = 20;
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

        protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
        {
            base.OnClicked(controlId, control, actionType);
            for (int x = 0; x <= 19; x++)
            {
                if (control==buttons[x])
                {
                    InternalRating = x + 1;
                    PageDestroy();
                    return;
                }
            }
        }

        public void MayInitButtons()
        {
            BaseConfig.MyAnimeLog.Write("Message Init Rating");

            if (buttons == null)
            {
                buttons = new[] { btnStar1, btnStar2, btnStar3, btnStar4, btnStar5, btnStar6, btnStar7, btnStar8, btnStar9, btnStar10, btnStar11, btnStar12, btnStar13, btnStar14, btnStar15, btnStar16, btnStar17, btnStar18, btnStar19, btnStar20 };
                if (btnStar1 == null)
                    BaseConfig.MyAnimeLog.Write("BtnStar1==null");
                if (btnStar2 == null)
                    BaseConfig.MyAnimeLog.Write("BtnStar2==null");
                if (btnStar3 == null)
                    BaseConfig.MyAnimeLog.Write("BtnStar3==null");
                if (btnStar4 == null)
                    BaseConfig.MyAnimeLog.Write("BtnStar4==null");
                if (btnStar5 == null)
                    BaseConfig.MyAnimeLog.Write("BtnStar5==null");                
            }
        }
        public override bool OnMessage(GUIMessage message) {
            switch (message.Message) {
                case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
                    base.OnMessage(message);
                    MayInitButtons();
                    IsSubmitted = false;
                    UpdateRating();
                    return true;

                case GUIMessage.MessageType.GUI_MSG_SETFOCUS:
                    if (message.TargetControlId < 100 || message.TargetControlId > 119)
                        break;
                    MayInitButtons();
                    InternalRating = message.TargetControlId - 99;
                    UpdateRating();
                    break;
            }
            return base.OnMessage(message);
        }

        private void UpdateRating()
        {
            for (int i = 0; i < 20; i++)
            {
                BaseConfig.MyAnimeLog.Write("Button "+i);
                if (buttons[i]==null)
                    BaseConfig.MyAnimeLog.Write("Button");
                buttons[i].Label = string.Empty;
                buttons[i].Selected = (InternalRating >= i + 1);
            }
            BaseConfig.MyAnimeLog.Write("Rating: " + InternalRating);
            buttons[InternalRating - 1].Focus = true;
            if (lblRating != null)
            {                   
                lblRating.Label = string.Format("({0}) {1} / {2}", GetRatingDescription(), Rating.ToString(Globals.Culture), 10);
            }
        }

        public void SetHeading(string HeadingLine) {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, 1, 0, 0, null);
            msg.Label = HeadingLine;
            OnMessage(msg);
        }

        public void SetLine(int LineNr, string Line) {
            if (LineNr < 1) return;
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, 1 + LineNr, 0, 0, null);
            msg.Label = Line;
            if ((msg.Label == string.Empty) || (msg.Label == "")) msg.Label = "  ";
            OnMessage(msg);
        }
                
      

        private string GetRatingDescription()
        {

                string description = string.Empty;

                switch (InternalRating) {
                    case 1:
                    case 2:
                        description = Translation.RateOne;
                        break;
                    case 3:
                    case 4:
                        description = Translation.RateTwo;
                        break;
                    case 5:
                    case 6:
                        description = Translation.RateThree;
                        break;
                    case 7:
                    case 8:
                        description = Translation.RateFour;
                        break;
                    case 9:
                    case 10:
                        description = Translation.RateFive;
                        break;
                    case 11:
                    case 12:
                        description = Translation.RateSix;
                        break;
                    case 13:
                    case 14:
                        description = Translation.RateSeven;
                        break;
                    case 15:
                    case 16:
                        description = Translation.RateEight;
                        break;
                    case 17:
                    case 18:
                        description = Translation.RateNine;
                        break;
                    case 19:
                    case 20:
                        description = Translation.RateTen;
                        break;
                }
                return description;
        }

    }
}

