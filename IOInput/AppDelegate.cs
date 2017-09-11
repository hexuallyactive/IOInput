using System;
using System.Diagnostics;

using AppKit;
using CoreGraphics;
using Foundation;

using IOConnect;

namespace IOInput
{
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        private static NSMenu menu;
        private static NSStatusItem menuItem;
        private static Connector connector;
        static Guid guid = Guid.NewGuid();
        private static Stopwatch idleWatch = new Stopwatch();
        private static nfloat screenWidth;
        private static nfloat screenHeight;
        private static int threshold = 50;
        private static Boolean isIdle = false;
        private static CGPoint lastPosition;
        private static NSObject notificationToken;

        public AppDelegate()
        {
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            // Insert code here to initialize your application

            menu = new NSMenu();
            menuItem = NSStatusBar.SystemStatusBar.CreateStatusItem(30);
            menuItem.Image = NSImage.ImageNamed("status_bar_icon");
            menuItem.AlternateImage = NSImage.ImageNamed("status_bar_icon");
            menuItem.HighlightMode = true;
            var quitItem = new NSMenuItem("Quit", (a, b) => Shutdown());
            menu.AddItem(quitItem);
            menuItem.Menu = menu;
            //connector = new Connector("pwc.downstreamlabs.com");
            connector = new Connector("input.io");
            lastPosition = NSEvent.CurrentMouseLocation;

            notificationToken = NSNotificationCenter.DefaultCenter.AddObserver(NSApplication.DidChangeScreenParametersNotification, (obj) => {
                Console.WriteLine("screen param changed, redoing screen metrics");
                determineScreensize();
            });

            determineScreensize();

            NSEvent.AddGlobalMonitorForEventsMatchingMask(NSEventMask.MouseMoved | NSEventMask.LeftMouseDragged | NSEventMask.RightMouseDragged, (e) =>
            {
                var newPosition = e.LocationInWindow;
                var pX = newPosition.X - lastPosition.X;
                var pY = newPosition.Y - lastPosition.Y;

                var d = Math.Sqrt((pX * pX) + (pY * pY));

                if (d > threshold)
                {
                    var mouseEvent = new MouseMoveEvent();
                    mouseEvent.uid = guid.ToString();
                    mouseEvent.elapsed = (idleWatch.Elapsed.TotalSeconds > 30) ? idleWatch.Elapsed.TotalSeconds : 0;
                    mouseEvent.x = Math.Round((newPosition.X / screenWidth), 4);
                    mouseEvent.y = Math.Round(((screenHeight - newPosition.Y) / screenHeight), 4);
                    idleWatch.Stop();
                    idleWatch = Stopwatch.StartNew();
                    lastPosition = e.LocationInWindow;
                    connector.DispatchMessage(mouseEvent);
                }
            });

            NSEvent.AddGlobalMonitorForEventsMatchingMask(NSEventMask.LeftMouseUp | NSEventMask.RightMouseUp, (e) =>
            {
                var newPosition = e.LocationInWindow;
                var clickEvent = new MouseClickEvent();
                clickEvent.uid = guid.ToString();
                clickEvent.x = Math.Round((newPosition.X / screenWidth), 4);
                clickEvent.y = Math.Round(((screenHeight - newPosition.Y) / screenHeight), 4);
                connector.DispatchMessage(clickEvent);
            });

        }

        private void Shutdown()
        {
            NSApplication.SharedApplication.Terminate(this);
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }

        private void determineScreensize()
        {
            var width = 0f;
            foreach (var screen in NSScreen.Screens)
            {
                width = width + (float)screen.VisibleFrame.Width;
            }
            screenWidth = width;
            screenHeight = NSScreen.MainScreen.Frame.Height;
            if (screenHeight > 1200)
            {
                threshold = 75;
            }
            else
            {
                threshold = 50;
            }
            Console.WriteLine("dims: " + screenWidth + "x" + screenHeight);
            Console.WriteLine("threshold: " + threshold);
        }

    }
}
