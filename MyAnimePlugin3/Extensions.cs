using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using Action = System.Action;

namespace MyAnimePlugin3
{
    public static class Extensions
    {
        public const string BaseProperties = "#Anime3";

        public static bool InitSkin<T>(this GUIWindow window, string skinfile)
        {
            string xmlSkin = Path.Combine(GUIGraphicsContext.Skin, skinfile);
            bool res = window.Load(xmlSkin);
            foreach (T p in Enum.GetValues(typeof(T)))
            {
                string name = p.ToString();
                if (!name.StartsWith("Fanart_"))
                    window.ClearGUIProperty(name);
            }
            return res;
        }

        public static string GetPropertyName(this GUIWindow window, string which, bool isInternalMediaportal = false)
        {
            string PropertyName = string.Format("{0}.{1}", BaseProperties, which.Replace("_", ".").Replace("ñ", "_"));

            if (isInternalMediaportal)
            {
                PropertyName = string.Format("#{0}", which.Replace("_", ".").Replace("ñ", "_"));
            }

            return PropertyName;
        }

        public static void SetGUIProperty(this GUIWindow window, string which, string value, bool isInternalMediaportal = false)
        {
            if (string.IsNullOrEmpty(value))
                value = " ";
            GUIPropertyManager.SetProperty(window.GetPropertyName(which, isInternalMediaportal), value);
        }


        public static void ClearGUIProperty(this GUIWindow window, string which)
        {
            window.SetGUIProperty(which, " ");
        }

        public static void Alert(this GUIWindow window, string title, params string[] lines)
        {
            GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
            if (null != dlgOk)
            {
                dlgOk.SetHeading(title);
                for (int x = 0; x < lines.Length; x++)
                    dlgOk.SetLine(x + 1, lines[x]);
                dlgOk.DoModal(GUIWindowManager.ActiveWindow);
            }
        }
        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> source, int seed)
        {
            Random rnd = new Random(seed);
            return source.OrderBy(item => rnd.Next());
        }
    }

    public interface IMenuItem
    {
        string Caption { get; set; }
    }
    public class MenuItem : IMenuItem
    {
        public Func<ContextMenuAction> Function { get; set; }
        public string Caption { get; set; }
        public MenuItem(string caption, Func<ContextMenuAction> function)
        {
            Caption = caption;
            Function = function;
        }
    }
    public class BoolMenuItem : IMenuItem
    {
        public Func<bool> Function { get; set; }
        public string Caption { get; set; }
        public BoolMenuItem(string caption, Func<bool> function)
        {
            Caption = caption;
            Function = function;
        }
    }
    public class VoidMenuItem : IMenuItem
    {
        public Action Function { get; set; }
        public string Caption { get; set; }
        public VoidMenuItem(string caption, Action function)
        {
            Caption = caption;
            Function = function;
        }
    }

    public enum ContextMenuAction
    {
        Exit,
        Continue,
        PreviousMenu
    }

    public class ContextMenu : List<IMenuItem>
    {
        public string Title { get; set; }
        public string PreviousMenuPrefix { get; set; }
        public int SelectedLabel { get; set; }
        public bool PreviousMenu { get; internal set; }

        public ContextMenu(string title, string previousmenu = null)
        {
            SelectedLabel = -1;
            PreviousMenuPrefix = " <<<";
            Title = title;
            if (!string.IsNullOrEmpty(previousmenu))
            {
                PreviousMenu = true;
                Add(PreviousMenuPrefix + previousmenu, () => ContextMenuAction.PreviousMenu);
            }
        }

        public void Add(string caption, Func<ContextMenuAction> function, bool selected = false)
        {
            if (selected)
                SelectedLabel = Count;
            Add(new MenuItem(caption, function));
        }
        public void AddAction(string caption, Action function, bool selected = false)
        {
            if (selected)
                SelectedLabel = Count;
            Add(new VoidMenuItem(caption, function));
        }

        public ContextMenuAction Show()
        {
            GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            if (dlg == null)
            {
                return PreviousMenu ? ContextMenuAction.PreviousMenu : ContextMenuAction.Exit;
            }
            do
            {
                dlg.Reset();
                if (!string.IsNullOrEmpty(Title))
                    dlg.SetHeading(Title);
                foreach (IMenuItem m in this)
                    dlg.Add(m.Caption);
                if (SelectedLabel != -1)
                    dlg.SelectedLabel = SelectedLabel;
                dlg.DoModal(GUIWindowManager.ActiveWindow);
                if ((dlg.SelectedLabel >= 0) && (dlg.SelectedLabel < Count))
                {
                    IMenuItem obj = this[dlg.SelectedLabel];
                    if (obj is MenuItem)
                    {
                        ContextMenuAction act = ((MenuItem)obj).Function();
                        if (act != ContextMenuAction.Continue)
                        {
                            if (act == ContextMenuAction.PreviousMenu)
                                return ContextMenuAction.Continue;
                            return act;
                        }
                    }
                    else if (obj is VoidMenuItem)
                    {
                        ((VoidMenuItem)obj).Function();
                        return ContextMenuAction.Exit;
                    }
                }
                else
                {
                    return PreviousMenu ? ContextMenuAction.Continue : ContextMenuAction.Exit;
                }
            } while (true);
        }
    }


    public class MainMenu : List<object>
    {
        public void AddFunc(GUIButtonControl button, Func<bool> function)
        {
            if (button != null)
                Add(new ButtonHandler(button, function));
        }
        public void AddFunc<T>(GUIButtonControl button, Func<T, bool> function)
        {
            if (button != null)
                Add(new ButtonHandler<T>(button, function));
        }
        public void AddFunc<T, TS>(GUIButtonControl button, Func<T, TS, bool> function)
        {
            if (button != null)
                Add(new ButtonHandler<T, TS>(button, function));
        }
        public void Add(GUIButtonControl button, Action function)
        {
            if (button != null)
                Add(new VoidButtonHandler(button, function));
        }
        public void Add<T>(GUIButtonControl button, Action<T> function)
        {
            if (button != null)
                Add(new VoidButtonHandler<T>(button, function));
        }
        public void Add<T, TS>(GUIButtonControl button, Action<T, TS> function)
        {
            if (button != null)
                Add(new VoidButtonHandler<T, TS>(button, function));
        }
        public void AddContext(GUIButtonControl button, Func<ContextMenuAction> function)
        {
            if (button != null)
                Add(new ContextButtonHandler(button, function));
        }
        public void AddContext<T>(GUIButtonControl button, Func<T, ContextMenuAction> function)
        {
            if (button != null)
                Add(new ContextButtonHandler<T>(button, function));
        }
        public void AddContext<T, TS>(GUIButtonControl button, Func<T, TS, ContextMenuAction> function)
        {
            if (button != null)
                Add(new ContextButtonHandler<T, TS>(button, function));
        }
        public bool Check<T>(GUIControl control, T param1)
        {
            return Check(control, param1, param1);
        }
        public bool Check(GUIControl control)
        {
            return Check<string, string>(control, null, null);
        }
        public bool Check<T, TS>(GUIControl control, T param1, TS param2)
        {
            foreach (object h in this)
            {
                if (h is ButtonHandler)
                {
                    if (((ButtonHandler)h).Check(control))
                        return true;
                }
                else if (h is ButtonHandler<T>)
                {
                    if (((ButtonHandler<T>)h).Check(control, param1))
                        return true;
                }
                else if (h is ButtonHandler<T, TS>)
                {
                    if (((ButtonHandler<T, TS>)h).Check(control, param1, param2))
                        return true;
                }
                else if (h is ContextButtonHandler)
                {
                    if (((ContextButtonHandler)h).Check(control))
                        return true;
                }
                else if (h is ContextButtonHandler<T>)
                {
                    if (((ContextButtonHandler<T>)h).Check(control, param1))
                        return true;
                }
                else if (h is ContextButtonHandler<T, TS>)
                {
                    if (((ContextButtonHandler<T, TS>)h).Check(control, param1, param2))
                        return true;
                }
                else if (h is VoidButtonHandler)
                {
                    if (((VoidButtonHandler)h).Check(control))
                        return true;
                }
                else if (h is VoidButtonHandler<T>)
                {
                    if (((VoidButtonHandler<T>)h).Check(control, param1))
                        return true;
                }
                else if (h is VoidButtonHandler<T, TS>)
                {
                    if (((VoidButtonHandler<T, TS>)h).Check(control, param1, param2))
                        return true;
                }
            }
            if (MA3WindowManager.HandleWindowChangeButton(control))
                return true;

            return false;
        }
    }
    public class VoidButtonHandler
    {
        public Action Function { get; set; }
        public GUIButtonControl Button { get; set; }

        public VoidButtonHandler(GUIButtonControl button, Action function)
        {
            Button = button;
            Function = function;
        }

        public bool Check(GUIControl control)
        {
            if ((Button != null) && (control == Button))
            {
                Button.IsFocused = false;
                Function();
                return true;
            }
            return false;
        }

    }

    public class VoidButtonHandler<T>
    {
        public Action<T> Function { get; set; }
        public GUIButtonControl Button { get; set; }

        public VoidButtonHandler(GUIButtonControl button, Action<T> function)
        {
            Button = button;
            Function = function;
        }

        public bool Check(GUIControl control, T param1)
        {
            if ((Button != null) && (control == Button))
            {
                Button.IsFocused = false;
                Function(param1);
                return true;
            }
            return false;
        }
    }
    public class VoidButtonHandler<T, TS>
    {
        public Action<T, TS> Function { get; set; }
        public GUIButtonControl Button { get; set; }

        public VoidButtonHandler(GUIButtonControl button, Action<T, TS> function)
        {
            Button = button;
            Function = function;
        }

        public bool Check(GUIControl control, T param1, TS param2)
        {
            if ((Button != null) && (control == Button))
            {
                Button.IsFocused = false;
                Function(param1, param2);
                return true;
            }
            return false;
        }
    }
    public class ButtonHandler
    {
        public Func<bool> Function { get; set; }
        public GUIButtonControl Button { get; set; }

        public ButtonHandler(GUIButtonControl button, Func<bool> function)
        {
            Button = button;
            Function = function;
        }

        public bool Check(GUIControl control)
        {
            if ((Button != null) && (control == Button))
            {
                Button.IsFocused = false;
                return Function();
            }
            return false;
        }

    }

    public class ButtonHandler<T>
    {
        public Func<T, bool> Function { get; set; }
        public GUIButtonControl Button { get; set; }

        public ButtonHandler(GUIButtonControl button, Func<T, bool> function)
        {
            Button = button;
            Function = function;
        }

        public bool Check(GUIControl control, T param1)
        {
            if ((Button != null) && (control == Button))
            {
                Button.IsFocused = false;
                return Function(param1);
            }
            return false;
        }
    }
    public class ButtonHandler<T, TS>
    {
        public Func<T, TS, bool> Function { get; set; }
        public GUIButtonControl Button { get; set; }

        public ButtonHandler(GUIButtonControl button, Func<T, TS, bool> function)
        {
            Button = button;
            Function = function;
        }

        public bool Check(GUIControl control, T param1, TS param2)
        {
            if ((Button != null) && (control == Button))
            {
                Button.IsFocused = false;
                return Function(param1, param2);
            }
            return false;
        }
    }
    public class ContextButtonHandler
    {
        public Func<ContextMenuAction> Function { get; set; }
        public GUIButtonControl Button { get; set; }

        public ContextButtonHandler(GUIButtonControl button, Func<ContextMenuAction> function)
        {
            Button = button;
            Function = function;
        }

        public bool Check(GUIControl control)
        {
            if ((Button != null) && (control == Button))
            {
                Button.IsFocused = false;
                ContextMenuAction ret = Function();
                return (ret != ContextMenuAction.Continue);
            }
            return false;
        }

    }

    public class ContextButtonHandler<T>
    {
        public Func<T, ContextMenuAction> Function { get; set; }
        public GUIButtonControl Button { get; set; }

        public ContextButtonHandler(GUIButtonControl button, Func<T, ContextMenuAction> function)
        {
            Button = button;
            Function = function;
        }

        public bool Check(GUIControl control, T param1)
        {
            if ((Button != null) && (control == Button))
            {
                Button.IsFocused = false;
                ContextMenuAction ret = Function(param1);
                return (ret != ContextMenuAction.Continue);
            }
            return false;
        }
    }
    public class ContextButtonHandler<T, TS>
    {
        public Func<T, TS, ContextMenuAction> Function { get; set; }
        public GUIButtonControl Button { get; set; }

        public ContextButtonHandler(GUIButtonControl button, Func<T, TS, ContextMenuAction> function)
        {
            Button = button;
            Function = function;
        }

        public bool Check(GUIControl control, T param1, TS param2)
        {
            if ((Button != null) && (control == Button))
            {
                Button.IsFocused = false;
                ContextMenuAction ret = Function(param1, param2);
                return (ret != ContextMenuAction.Continue);
            }
            return false;
        }
    }
}

