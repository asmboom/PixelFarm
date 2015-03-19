//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Timers;
using System.IO;

using MatterHackers.Agg.Image;
using MatterHackers.Agg.RasterizerScanline;
using MatterHackers.Agg.PlatformAbstract;

namespace MatterHackers.Agg.UI
{
    public abstract class WindowsFormsAbstract : Form
    {
        // These dll imports are so that we can set the PROCESS_CALLBACK_FILTER_ENABLED to allow us to get exceptions back into our app during system calls such as on paint.
        //[DllImport("Kernel32.dll", CallingConvention = CallingConvention.Cdecl)]
        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool SetProcessUserModeExceptionPolicy(int dwFlagss);

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool GetProcessUserModeExceptionPolicy(ref int lpFlags);

        protected WidgetForWindowsFormsAbstract aggAppWidget;

        static Form mainForm = null;
        static System.Timers.Timer idleCallBackTimer = null;

        public WindowsFormsAbstract()
        {
            if (idleCallBackTimer == null)
            {
                idleCallBackTimer = new System.Timers.Timer();
                mainForm = this;
                // call up to 100 times a second
                idleCallBackTimer.Interval = 10;
                idleCallBackTimer.Elapsed += CallAppWidgetOnIdle;
                idleCallBackTimer.Start();
            }
        }

        bool hasBeenClosed = false;

        public static void ShowFileInFolder(string fileToShow)
        {
            string argument = "/select, \"" + Path.GetFullPath(fileToShow) + "\"";

            System.Diagnostics.Process.Start("explorer.exe", argument);
        }

        protected void SetUpFormsWindow(AbstractOsMappingWidget app, SystemWindow childSystemWindow)
        {
            aggAppWidget = (WidgetForWindowsFormsAbstract)app;
            this.AllowDrop = true;

            if (File.Exists("application.ico"))
            {
                try
                {
                    this.Icon = new System.Drawing.Icon("application.ico");
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    if (ex.NativeErrorCode != 0)
                    {
                        throw;
                    }
                }
            }
            else if (File.Exists("../MonoBundle/StaticData/application.ico"))
            {
                try
                {
                    this.Icon = new System.Drawing.Icon("../MonoBundle/StaticData/application.ico");
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    if (ex.NativeErrorCode != 0)
                    {
                        throw;
                    }
                }
            }
        }

        List<string> GetDroppedFiles(DragEventArgs drgevent)
        {
            List<string> droppedFiles = new List<string>();
            Array droppedItems = ((IDataObject)drgevent.Data).GetData(DataFormats.FileDrop) as Array;
            if (droppedItems != null)
            {
                foreach (object droppedItem in droppedItems)
                {
                    string fileName = Path.GetFullPath((string)droppedItem);
                    droppedFiles.Add(fileName);
                }
            }

            return droppedFiles;
        }

        Point GetPosForAppWidget(DragEventArgs dragevent)
        {
            Point clientTop = PointToScreen(new Point(0, 0));
            Point appWidgetPos = new Point(dragevent.X - clientTop.X, (int)aggAppWidget.height() - (dragevent.Y - clientTop.Y));

            return appWidgetPos;
        }

        protected override void OnDragEnter(DragEventArgs dragevent)
        {
            List<string> droppedFiles = GetDroppedFiles(dragevent);

            Point appWidgetPos = GetPosForAppWidget(dragevent);
            FileDropEventArgs fileDropEventArgs = new FileDropEventArgs(droppedFiles, appWidgetPos.X, appWidgetPos.Y);
            aggAppWidget.OnDragEnter(fileDropEventArgs);
            if (fileDropEventArgs.AcceptDrop)
            {
                dragevent.Effect = DragDropEffects.Copy;
            }

            base.OnDragEnter(dragevent);
        }

        protected override void OnDragOver(DragEventArgs dragevent)
        {
            List<string> droppedFiles = GetDroppedFiles(dragevent);

            Point appWidgetPos = GetPosForAppWidget(dragevent);
            FileDropEventArgs fileDropEventArgs = new FileDropEventArgs(droppedFiles, appWidgetPos.X, appWidgetPos.Y);
            aggAppWidget.OnDragOver(fileDropEventArgs);
            if (fileDropEventArgs.AcceptDrop)
            {
                dragevent.Effect = DragDropEffects.Copy;
            }
            else
            {
                dragevent.Effect = DragDropEffects.None;
            }

            base.OnDragOver(dragevent);
        }

        protected override void OnDragDrop(DragEventArgs dragevent)
        {
            List<string> droppedFiles = GetDroppedFiles(dragevent);

            Point appWidgetPos = GetPosForAppWidget(dragevent);
            FileDropEventArgs fileDropEventArgs = new FileDropEventArgs(droppedFiles, appWidgetPos.X, appWidgetPos.Y);
            aggAppWidget.OnDragDrop(fileDropEventArgs);

            base.OnDragDrop(dragevent);
        }

        void CallAppWidgetOnIdle(object sender, ElapsedEventArgs e)
        {
            if (aggAppWidget != null 
                && !hasBeenClosed)
            {
                if (InvokeRequired)
                {
                    // you are calling this from another thread and should not be
                    //throw new Exception("You are calling this from another thread and should not be.");
                    Invoke(new Action(DoCallAppWidgetOnIdle));
                }
                else
                {
                    DoCallAppWidgetOnIdle();
                }
            }
        }

        void DoCallAppWidgetOnIdle()
        {
            IdleCount++;
            UiThread.DoRunAllPending();
        }

        protected override void WndProc(ref Message m)
        {
            if (InvokeRequired)
            {
                // you are calling this from another thread and should not be
                throw new Exception("You are calling this from another thread and should not be.");
            }
            base.WndProc(ref m);
        }

        public bool ShowingSystemDialog = false;
        protected override void OnPaint(PaintEventArgs paintEventArgs)
        {
            base.OnPaint(paintEventArgs);

            if (ShowingSystemDialog)
            {
                // We do this because calling Invalidate within an OnPaint message will cause our
                // SaveDialog to not show its 'overwrite' dialog if needed.  
                // We use the Invalidate to cause a continuous pump of the OnPaint message to call our OnIdle.
                // We could figure another solution but it must be very carful to ensure we don't break SaveDialog
                return;
            }

            Rectangle rect = paintEventArgs.ClipRectangle;
            if (ClientSize.Width > 0 && ClientSize.Height > 0)
            {
                DrawCount++;
                aggAppWidget.OnDraw(aggAppWidget.NewGraphics2D());

                CopyBackBufferToScreen(paintEventArgs.Graphics);
            }

            OnPaintCount++;
            // use this to debug that windows are drawing and updating.
            //Text = string.Format("Draw {0}, Idle {1}, OnPaint {2}", DrawCount, IdleCount, OnPaintCount);
        }

        int DrawCount = 0;
        int IdleCount = 0;
        int OnPaintCount;

        public abstract void CopyBackBufferToScreen(Graphics displayGraphics);

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // don't call this so that windows will not erase the background.
            //base.OnPaintBackground(e);
        }

        protected override void OnActivated(EventArgs e)
        {
            // focus the first child of the forms window (should be the system window)
            if (aggAppWidget != null
                && aggAppWidget.Children.Count > 0
                && aggAppWidget.Children[0] != null)
            {
                aggAppWidget.Children[0].Focus();
            }
            base.OnActivated(e);
        }

        protected override void OnResize(EventArgs e)
        {
            aggAppWidget.LocalBounds = new RectangleDouble(0, 0, ClientSize.Width, ClientSize.Height);
            aggAppWidget.Invalidate();

            base.OnResize(e);
        }

        bool waitingForIdleTimerToStop = false;
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Call on closing and check if we can close (a "do you want to save" might cancel the close. :).
            bool CancelClose;
            aggAppWidget.OnClosing(out CancelClose);

            if (CancelClose)
            {
                e.Cancel = true;
            }
            else
            {
                if (!hasBeenClosed)
                {
                    hasBeenClosed = true;
                    aggAppWidget.Close();
                }

                if (this == mainForm && !waitingForIdleTimerToStop)
                {
                    waitingForIdleTimerToStop = true;
                    idleCallBackTimer.Stop();
                    idleCallBackTimer.Elapsed -= CallAppWidgetOnIdle;
                    e.Cancel = true;
                    // We just need to wait for this event to end so we can re-enter the idle loop with the time stoped
                    // If we close with the idle loop timer not stoped we throw and exception.
                    System.Windows.Forms.Timer delayedCloseTimer = new System.Windows.Forms.Timer();
                    delayedCloseTimer.Tick += DoDelayedClose;
                    delayedCloseTimer.Start();
                }
            }

            base.OnClosing(e);
        }

        void DoDelayedClose(object sender, EventArgs e)
        {
            ((System.Windows.Forms.Timer)sender).Stop();
            this.Close();
        }

        internal virtual void RequestInvalidate(Rectangle windowsRectToInvalidate)
        {
            // In mono this can throw an invalid exception sometimes. So we catch it to prevent a crash.
            // http://lists.ximian.com/pipermail/mono-bugs/2007-September/061540.html
            try
            {
                Invalidate(windowsRectToInvalidate);

                Rectangle allRectToInvalidate = new Rectangle(0, 0, (int)Width, (int)Height);
                Invalidate(allRectToInvalidate);
            }
            catch (Exception)
            {
            }
        }

        internal virtual void RequestClose()
        {
            if (!hasBeenClosed)
            {
                Close();
            }
        }
    }
}
