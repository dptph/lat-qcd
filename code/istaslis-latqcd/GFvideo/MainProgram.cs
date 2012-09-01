using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using OpenCLNet;

namespace GFVideo
{
    static class MainProgram
    {
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var isAuto = args.Length>0 && args[0] == "-run";
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            Application.Run(new Form1(isAuto));
        }
    }
}
