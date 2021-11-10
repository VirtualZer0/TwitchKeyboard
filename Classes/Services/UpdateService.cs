using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TwitchKeyboard.Classes.Services
{
    public static class UpdateService
    {
        const int curVersion = 3;
        const string updateUrl = "https://raw.githubusercontent.com/VirtualZer0/TwitchKeyboard/master/lastVersion.txt";

        public static bool checkUpdate ()
        {
            using WebClient webClient = new();
            try
            {
                string newVer = webClient.DownloadString(updateUrl);
                if (int.Parse(newVer) > curVersion)
                {
                    return true;
                }
            } catch { }

            return false;

        }
    }
}
