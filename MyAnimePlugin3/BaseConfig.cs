using System;
using System.Collections.Generic;
using System.Text;

namespace MyAnimePlugin3
{
    public static class BaseConfig
    {
        private static AnimePluginSettings settingsLocal = null;
        private static ILog log = null;
        public static AnimePluginSettings Settings
        {
            get
            {
                if ((Utils.IsRunningFromConfig()) || (MainWindow.settings == null))
                {
                    if (settingsLocal == null)
                        settingsLocal = new AnimePluginSettings();
                    return settingsLocal;
                }
                return MainWindow.settings;
            }
        }
        public static ILog MyAnimeLog
        {
            get
            {
                if (log==null)
                    log=new MyAnimeLog();
                return log;
            }
            set
            {

                log = value;
            }
        }
    }
}
