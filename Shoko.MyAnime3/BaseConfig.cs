using System;
using System.Collections.Generic;
using System.Text;
using Shoko.MyAnime3.ConfigFiles;

namespace Shoko.MyAnime3
{
    public static class BaseConfig
    {
        private static ILog log = null;
        public static AnimePluginSettings Settings => AnimePluginSettings.Instance;

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
