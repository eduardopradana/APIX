using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Media;
using System.Configuration;
using System.Diagnostics;
using Microsoft.Lync.Model;

namespace alert
{
    public partial class Timer : Form
    {
        public Timer()
        {
            InitializeComponent();

            int style = NativeWinAPI.GetWindowLong(this.Handle, NativeWinAPI.GWL_EXSTYLE);
            style |= NativeWinAPI.WS_EX_COMPOSITED;
            NativeWinAPI.SetWindowLong(this.Handle, NativeWinAPI.GWL_EXSTYLE, style);

        }
        
        private string lyncUserName= "there"; //default value if not getting the lync user name
        private bool allowshowdisplay = true;
        private int ticks = 0;
        const int cRem = 10; //seconds
        private int idleTimeSpec;
        private int miniIdleTimeSpec;
        private double timer3ticks = 0;
       
        private bool alertON;
        private bool miniAlertON;
        
        SoundPlayer sound = new SoundPlayer(Properties.Resources.thunderwav);

        
        private void Form1_Load(object sender, EventArgs e)
        {            
            idleTimeSpec = cRem*1000;
            miniIdleTimeSpec = cRem * 1000/2;

            timer1.Interval = 1000; //single timer tick is every real 1 second
            timer1.Start();
            var contextMenu1 = new ContextMenu();
            contextMenu1.MenuItems.Add("Exit", MainController.ExitApplication);
            MainController.statusBarIcon.ContextMenu = contextMenu1;
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return "you can't lick your nose tips"; //originally return null which is a trouble
        }

        public bool isUserAvailable()
        {
            LyncClient lyncClient;
            try
            {
                lyncClient = LyncClient.GetClient();
            }
            catch (Exception e)
            {
                return true; //if can't get client we won't check the Lync activity
            }

            lyncUserName = (lyncClient.Self.Contact.GetContactInformation(ContactInformationType.DisplayName)).ToString(); //get the current user name     
            
            var activity = lyncClient.Self.Contact.GetContactInformation(ContactInformationType.ActivityId);

            switch(activity.ToString())
            {
                case "on-the-phone": return false;
                case "in-a-meeting": return false;
                case "in-a-conference": return false;
                case "in-presentation": return false;
                default: return true;
            }

        }

        public bool isBrowsersOpeningNonWorkSite()
        {            
            bool chromeMatch = false;
            bool firefoxMatch = false;
            bool ieMatch = false;
            string activeWindowTitle = GetActiveWindowTitle();
            var nonWorkSites = new List<string>(ConfigurationManager.AppSettings["title"].Split(new char[] { ';' }));
            //EventLog.WriteEntry("title window" + activeWindowTitle, activeWindowTitle);
            if (activeWindowTitle.Contains("Chrome"))
            {
                /*var chromeTitle = from p1 in Process.GetProcessesByName("chrome")
                                  where p1.MainWindowTitle != ""
                                  select p1.MainWindowTitle;
                chromeMatch = nonWorkSites.Any(s => (String.Join("", chromeTitle).ToLower().Contains(s)));*/
                chromeMatch = nonWorkSites.Any(s => (activeWindowTitle.ToLower().Contains(s)));
                if (chromeMatch) return true;
            }

            if (activeWindowTitle.Contains("Mozilla Firefox"))
            {
                firefoxMatch = nonWorkSites.Any(s => (activeWindowTitle.ToLower().Contains(s)));
                if (firefoxMatch) return true;
            }

            if (activeWindowTitle.Contains("Microsoft Internet Explorer"))
            {
                ieMatch = nonWorkSites.Any(s => (activeWindowTitle.ToLower().Contains(s)));
                if (ieMatch) return true;
            }

            return false;
        }

        public bool isNonOfficeChattingOpened()
        {
            string activeWindowTitle = GetActiveWindowTitle();
            var nonOfficeChatting = new List<string>(ConfigurationManager.AppSettings["chatprogram"].Split(new char[] { ';' }));
            
            if(nonOfficeChatting.Any(s => (activeWindowTitle.ToLower().Contains(s))))
            {
                return true;
            }
            else
            {
                return false;
            }            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if ((DateTime.Now.TimeOfDay >= MainController.StartWorkHour.TimeOfDay && DateTime.Now.TimeOfDay <= MainController.StartLunchBreak.TimeOfDay) || (DateTime.Now.TimeOfDay <= MainController.EndWorkHour.TimeOfDay && DateTime.Now.TimeOfDay >= MainController.EndLunchBreak.TimeOfDay))
            {
                if (isUserAvailable())
                {
                    var idleTime = Win32.GetIdleTime();
                    bool IsNonWorkSiteOpened = isBrowsersOpeningNonWorkSite();
                    bool isChattingOpened = isNonOfficeChattingOpened();

                    //EventLog.WriteEntry("non work" + IsNonWorkSiteOpened.ToString(), IsNonWorkSiteOpened.ToString());

                    if (IsNonWorkSiteOpened || isChattingOpened || (idleTime > miniIdleTimeSpec && !alertON))
                    {
                        MainController.lastIdleTime = DateTime.Now.AddSeconds(-1 * miniIdleTimeSpec / 1000);
                        if (idleTime > miniIdleTimeSpec && !alertON)
                        {
                            if (!miniAlertON) MiniAlert();
                        }

                        if (IsNonWorkSiteOpened || isChattingOpened)
                        {
                            timer3ticks = idleTime / 1000;
                            timer3.Interval = 1000; //single timer tick is every real 1 second
                            timer3.Start();
                        }
                        else
                        {
                            timer3.Stop();
                        }
                        if ((idleTime > idleTimeSpec && !alertON))
                        {

                            //need to put another timer here for time wasted in non working sites
                            MainController.lastIdleTime = DateTime.Now.AddSeconds(-1 * idleTimeSpec / 1000);
                            Alert(lyncUserName);
                            PlaySound();
                            timer2.Interval = 1000; //single timer tick is every real 1 second
                            timer2.Start();
                        }
                    }
                }
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            ticks++;
            if (ticks == 60)
            {
                Alert(lyncUserName);
                PlaySound();
                ticks = 0;
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            timer3ticks++;
            if (timer3ticks > idleTimeSpec && !alertON)
            {
                Alert(lyncUserName);
                PlaySound();
                timer3ticks = 0;
                timer3.Stop();
            }
        }

        private void Alert(string name)
        {
            alertON = true;
            MainController.PopAlert(name);
        }

        private void MiniAlert()
        {
            miniAlertON = true;
            
            //notifyIcon1.Icon = SystemIcons.Exclamation;
            TimeSpan t = TimeSpan.FromSeconds(MainController.GetLastIdleTime());
            MainController.statusBarIcon.ShowBalloonTip(5000, "Hey, Psst...", "You've been idle/not productive for " + t.ToString(@"hh\:mm\:ss") + ". Get back to work.", ToolTipIcon.Warning);
            //Minimum and maximum timeout values are enforced by the operating system and are typically 10 and 30 seconds, respectively, however this can vary depending on the operating system. 
        }
                
        public void DeAlert()
        {
            MainController.lastIdleTime = DateTime.Now;
            timer1.Stop();
            timer2.Stop();
            miniAlertON = false;
            alertON = false;            
            timer1.Start();
        }

        private void PlaySound()
        {
            sound.Play();            
        }
        
        private void notifyIcon1_BalloonTipClosed(object sender, EventArgs e)
        {
            //miniAlertON = false; 
            notifyIcon1.Visible = false;
        }

        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            //miniAlertON = false;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            notifyIcon1.Dispose();
        }

        #region CORE
        internal struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        // Basically this class get return values when user are not idle or idle
        public class Win32
        {
            [DllImport("User32.dll")]
            public static extern bool LockWorkStation();

            [DllImport("User32.dll")]
            private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

            [DllImport("Kernel32.dll")]
            private static extern uint GetLastError();

            public static uint GetIdleTime()
            {
                LASTINPUTINFO lastInPut = new LASTINPUTINFO();
                lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
                GetLastInputInfo(ref lastInPut);

                return ((uint)Environment.TickCount - lastInPut.dwTime);
            }

            public static long GetTickCount()
            {
                return Environment.TickCount;
            }

            public static long GetLastInputTime()
            {
                LASTINPUTINFO lastInPut = new LASTINPUTINFO();
                lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
                if (!GetLastInputInfo(ref lastInPut))
                {
                    throw new Exception(GetLastError().ToString());
                }

                return lastInPut.dwTime;
            }

        }
#endregion

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(allowshowdisplay ? value : allowshowdisplay);
        }

        internal static class NativeWinAPI
        {
            internal static readonly int GWL_EXSTYLE = -20;
            internal static readonly int WS_EX_COMPOSITED = 0x02000000;

            [DllImport("user32")]
            internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

            [DllImport("user32")]
            internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        }

        

        //no email send out functionality because this is not a whistleblower app, 
        //this is a motivational app that helps employee to be even more productive kinda train themselves to be a better people.

        //todo
        //make it startup
                
        //to be a smart app, it needs to check for meeting application presence and even better lync status because lync connects to outlook
    }
}
