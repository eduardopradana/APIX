using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Configuration;

namespace alert
{
    class MainController
    {
        public static DateTime lastIdleTime;
        public static DateTime StartLunchBreak = new DateTime(1, 1, 1, 12, 0, 0);
        public static DateTime EndLunchBreak = new DateTime(1, 1, 1, 13, 0, 0);
        public static DateTime StartWorkHour = new DateTime(1, 1, 1, 0, 0, 0);
        public static DateTime EndWorkHour = new DateTime(1, 1, 1, 23, 59, 0);

        public static NotifyIcon statusBarIcon = new NotifyIcon()
        {
            Text = "Anti Procrastinating Impulse X",
            Icon = Properties.Resources.logoM,
            Visible = true
        };
        

        public static int GetLastIdleTime()
        {
            var totalSeconds = (DateTime.Now - lastIdleTime).TotalSeconds;
            return (int)Math.Round(totalSeconds);            
        }

        public static void ExitApplication(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public static void PopAlert(string name)
        {
            for (int i = 0; i < Screen.AllScreens.Count(); i++)
            {
                if (Screen.AllScreens[i] != null)
                {
                    //https://stackoverflow.com/questions/12813752/avoid-flickering-in-windows-forms
                    var popper = new Popper();
                    popper.Name = name;
                    popper.FormBorderStyle = FormBorderStyle.None;
                    popper.Bounds = Screen.PrimaryScreen.Bounds;
                    popper.Location = Screen.AllScreens[i].WorkingArea.Location;
                    popper.TopMost = true;
                    popper.Show();                    
                }
            }
        }

        public static void CollapsePoppers()
        {
            List<Form> openForms = new List<Form>();

            foreach (Form f in Application.OpenForms)
                openForms.Add(f);

            foreach (Form f in openForms)
            {
                if (f.Name != "Timer")
                {
                    f.Dispose();
                    f.Close();
                }
            }
            //masih membiingungkan karena kalo semua di close
            //Timer timerForm = new Timer();
            //timerForm.Activate();
        }
    }
}
