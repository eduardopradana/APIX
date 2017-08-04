using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace alert
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            Timer timer = new Timer();
            timer.Visible = false;
            timer.Hide();
            Application.Run(timer);
//            Application.Run(new Timer());
        }     

    }
}
