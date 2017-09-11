using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using System.Diagnostics;
using IOConnect;
using Microsoft.Win32;

namespace IOInputWin
{
    public partial class Form1 : Form
    {
        private static Connector connector;
        private static Guid guid = Guid.NewGuid();
        private static IKeyboardMouseEvents hook;
        private static Stopwatch idleWatch = new Stopwatch();
        private static int screenWidth;
        private static int screenHeight;
        private static int threshold = 75;
        private static Point lastPosition;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            connector = new Connector("pwc.downstreamlabs.com");
            lastPosition = Cursor.Position;
            determineScreensize();
            hook = Hook.GlobalEvents();
            hook.MouseMoveExt += Hook_MouseMoveExt;
            hook.MouseUp += Hook_MouseUp;
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            determineScreensize();
        }

        private void Hook_MouseUp(object sender, MouseEventArgs e)
        {
            var newPosition = e.Location;
            
            var clickEvent = new MouseClickEvent();
            clickEvent.uid = guid.ToString();
            clickEvent.x = Math.Round(((float)newPosition.X / screenWidth), 4);
            clickEvent.y = Math.Round(((float)newPosition.Y / screenHeight), 4);
            connector.DispatchMessage(clickEvent);
        }

        private void determineScreensize()
        {
            screenWidth = SystemInformation.VirtualScreen.Width;
            screenHeight = SystemInformation.VirtualScreen.Height;
            if(screenHeight > 1200)
            {
                threshold = 75;
            } else
            {
                threshold = 50;
            }
        }

        private void Hook_MouseMoveExt(object sender, MouseEventExtArgs e)
        {
            var newPosition = e.Location;
            var pX = newPosition.X - lastPosition.X;
            var pY = newPosition.Y - lastPosition.Y;
            var d = Math.Sqrt((pX * pX) * (pY * pY));
            if(d > threshold)
            {
                var moveEvent = new MouseMoveEvent();
                moveEvent.uid = guid.ToString();
                moveEvent.elapsed = (idleWatch.Elapsed.TotalSeconds > 30) ? idleWatch.Elapsed.TotalSeconds : 0;
                moveEvent.x = Math.Round(((float)newPosition.X / screenWidth), 4);
                moveEvent.y = Math.Round(((float)newPosition.Y / screenHeight), 4);
                idleWatch.Stop();
                idleWatch = Stopwatch.StartNew();
                lastPosition = newPosition;
                connector.DispatchMessage(moveEvent);
            }
        }
    }
}
