using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Av.Utils;

namespace NowPlaying
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            #region " Logging configuration "
            Log4cs.Dir = Common.GetPath() + "Logs\\";
            Log4cs.FileName = "winfrom_{0}.log";
            Log4cs.OutputToConsole = false;
            #endregion

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
