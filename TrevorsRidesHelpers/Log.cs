using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if ANDROID
using Android.Util;
#endif

namespace TrevorsRidesHelpers
{
    public class Log
    {
        public static void Debug(string tag, string message) 
        {
#if ANDROID
            Android.Util.Log.Debug(tag, message);
#endif
        }

        public static void Debug(string message)
        {
            Debug("Maui", message);
        }
    }
}
