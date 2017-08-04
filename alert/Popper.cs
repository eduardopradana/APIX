using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Collections;

namespace alert
{
    public partial class Popper : Form
    {
        //https://stackoverflow.com/questions/8046560/how-to-stop-flickering-c-sharp-winforms
        //BUAT FLICKERING
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont,
            IntPtr pdv, [System.Runtime.InteropServices.In] ref uint pcFonts);

        private PrivateFontCollection fonts = new PrivateFontCollection();
        Random rnd = new Random();
        string name;
        
        Font myFont, myFont2;
        List<string> quotes = new List<string>();
        List<string> characters = new List<string>();
       
        public Popper()
        {
            InitializeComponent();    
            //generateChart();
            idlePeriod.Text = "       You have been idle for " + "00:00:00";
            quotes.Add("\"The way to get started \n  is to quit talking and begin doing.\"");
            quotes.Add("\"Your Future is created \n  by what you do today.\"");
            quotes.Add("\"Productivity is less about what you do with your time,\n   and more about how you run your mind.\"");
            quotes.Add("\"Time is non-refundable, \n  use it with intention.\"");
            quotes.Add("\"The one thing you can control is your effort.\"");
            quotes.Add("\"It's not about being the best. \n   it's about being better than you were yesterday.\"");
            quotes.Add("\"Start where you are, \n use what you have, do what you can.\"");
            quotes.Add("\"Productivity is never an accident. \n  it is always the result of a commitment.\"");

            characters.Add("shekun");
            characters.Add("davidmagson");
            //characters.Add("davidmagson");
            //characters.Add("davidmagson");
            //characters.Add("davidmagson");

            int style = NativeWinAPI.GetWindowLong(this.Handle, NativeWinAPI.GWL_EXSTYLE);
            style |= NativeWinAPI.WS_EX_COMPOSITED;
            NativeWinAPI.SetWindowLong(this.Handle, NativeWinAPI.GWL_EXSTYLE, style);

            byte[] fontData = Properties.Resources.OPTIMA;
            IntPtr fontPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(fontData.Length);
            System.Runtime.InteropServices.Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
            uint dummy = 0;
            fonts.AddMemoryFont(fontPtr, Properties.Resources.OPTIMA.Length);
            AddFontMemResourceEx(fontPtr, (uint)Properties.Resources.OPTIMA.Length, IntPtr.Zero, ref dummy);
            System.Runtime.InteropServices.Marshal.FreeCoTaskMem(fontPtr);

            myFont = new Font(fonts.Families[0], 20.0F);
        }

        private void generateChart()
        {
            chart1.ChartAreas["ChartArea1"].AxisX.MajorGrid.Enabled = false;
            chart1.ChartAreas["ChartArea1"].AxisY.MajorGrid.Enabled = false;
            chart1.ChartAreas[0].AxisY.LabelStyle.Enabled = false;
            chart1.BackColor = Color.Transparent;
            chart1.Series[0].Points.AddXY("Monday", 2);
            chart1.Series[0].Points.AddXY("Tuesday", 3);
            chart1.Series[0].Points.AddXY("Wednesday", 4);
            chart1.Series[0].Points.AddXY("Thursday", 8);
            chart1.Series[0].Points.AddXY("Friday", 5);
            //based on 5 min/hour -> 7.3 hour
            //based on 10 min/hour -> 6.6 hour
            //based on 15 min/hour -> 6 hour
            //based on 20 min/hour -> 5.3 hour
            
        }
        private void button2_Click(object sender, EventArgs e)
        {
            MainController.CollapsePoppers();            
        }
        
        private void Popper_Load(object sender, EventArgs e)
        {
            timer1.Start();
            timer1.Interval = 1000;

            idlePeriod.Font = myFont2;
            idlePeriod.Image = Properties.Resources.timer;
            idlePeriod.ImageAlign = ContentAlignment.MiddleLeft;

            panel1.Left = panel2.Left + 20;
            panel1.Top = panel2.Top - panel1.Height;
            panel1.BackColor = System.Drawing.ColorTranslator.FromHtml("#27586f");

            quoteLabel.Font = myFont;
            quoteLabel.Text = quotes[rnd.Next(quotes.Count)];
            panel2.Width = quoteLabel.Width + 50;
            panel2.Height = quoteLabel.Height + 50;
            panel2.BackColor = System.Drawing.ColorTranslator.FromHtml("#34323e");

            button2.Font = myFont2;
            button2.Left = panel2.Right - button2.Width - 20;
            button2.Top = panel2.Bottom;
            button2.BackColor = System.Drawing.ColorTranslator.FromHtml("#e17126");
            button2.TabStop = false;
            button2.Image = Properties.Resources.tick16;
            button2.TextImageRelation = TextImageRelation.ImageBeforeText;
            button2.FlatStyle = FlatStyle.Flat;
            button2.FlatAppearance.BorderSize = 0;

            chartButton1.BackColor = System.Drawing.ColorTranslator.FromHtml("#e17126");
            chartButton1.TabStop = false;
            chartButton1.Image = Properties.Resources.chart36;
            chartButton1.TextImageRelation = TextImageRelation.ImageBeforeText;
            chartButton1.FlatStyle = FlatStyle.Flat;
            chartButton1.FlatAppearance.BorderSize = 0;
            chartButton1.Height = 50;
            chartButton1.Width = 50;
            chartButton1.Left = 70;

            configButton.BackColor = System.Drawing.ColorTranslator.FromHtml("#e17126");
            configButton.TabStop = false;
            configButton.Image = Properties.Resources.gear36;
            configButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            configButton.FlatStyle = FlatStyle.Flat;
            configButton.FlatAppearance.BorderSize = 0;
            configButton.Height = 50;
            configButton.Width = 50;
            configButton.Left = 70;

            this.BackgroundImage = (Image)Properties.Resources.ResourceManager.GetObject(characters[rnd.Next(characters.Count)]);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan t = TimeSpan.FromSeconds(MainController.GetLastIdleTime());
            idlePeriod.Text = "       You have been idle for " + t.ToString(@"hh\:mm\:ss");
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

        private void chart1_Click(object sender, EventArgs e)
        {

        }
    }

    public class RoundButton : Button
    {
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            GraphicsPath grPath = new GraphicsPath();
            grPath.AddEllipse(0, 0, ClientSize.Width, ClientSize.Height);
            this.Region = new System.Drawing.Region(grPath);
            base.OnPaint(e);
        }
    }
}
