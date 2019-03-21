using System.Timers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using Gwen;
using Gwen.Controls;
using linerider.Tools;
using linerider.Utils;
using linerider.IO;
using OpenTK.Graphics;
using linerider;

namespace CallFunction
{
    public class MinTimer
    {
        public static void Timer()
        {
            // timer to call MyMethod() every minutes 
            System.Timers.Timer timer = new System.Timers.Timer(TimeSpan.FromMinutes(0.25).TotalMilliseconds);
            timer.AutoReset = true;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(TimeElapsed);
            timer.Start();
        }

        public static void TimeElapsed(object sender, ElapsedEventArgs e)
        {
            Settings.MinutesElapsed = Settings.MinutesElapsed + (float)0.25;
            Settings.Save();
        }
    }
}