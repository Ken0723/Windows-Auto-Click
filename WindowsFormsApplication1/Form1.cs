using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Diagnostics;

namespace WindowsFormsApplication1
{

    public partial class Form1 : Form
    {
        [DllImport("User32")]
        public static extern void mouse_event(
            int dwFlags,
            int dx,
            int dy,
            int dwData,
            int dwExtraInfo
        );

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out MousePoint lpMousePoint);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern bool BlockInput(bool block);

        [DllImport("user32.dll")]
        private static extern int FindWindow(string strClassName, string strWindowName);

        [DllImport("User32.dll")]
        public static extern int FindWindowEx(int hwndParent, int hwndChildAfter, string strClassName, string strWindowName);

        Boolean isReadRunning = false;
        Boolean isautoClick1Running = false;
        Boolean isGettingXY = false;
        Thread autoThread1, ky, gettingXY;

        private System.Int32 iHandle;
        private Process[] currentProc;

        const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        const int MOUSEEVENTF_MOVE = 0x0001;
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        const int MOUSEEVENTF_WHEEL = 0x0800;
        const int MOUSEEVENTF_XDOWN = 0x0080;
        const int MOUSEEVENTF_XUP = 0x1000;
        const int MOUSEEVENTF_HWHEEL = 0x01000;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            btnStop.Enabled = false;
            btnStart.Enabled = false;
            getRunningApplication();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            readKeyboard();
            lsbAsyncTaskVal.Items.Add("Starting...");
        }

        private void readKeyboard()
        {
            isReadRunning = true;
            //lsbAsyncTaskVal.Items.Clear();
            if (ky == null)
            {
                ky = new Thread(Keyboardd);
                ky.SetApartmentState(ApartmentState.STA);
                CheckForIllegalCrossThreadCalls = false;
                isReadRunning = true;
                ky.Start();
                lsbAsyncTaskVal.Items.Add("Completed");
            }
        }

        private void getRunningApplication()
        {
            Process[] processes = Process.GetProcesses();
            foreach (Process p in processes)
            {
                if (!String.IsNullOrEmpty(p.MainWindowTitle))
                {
                    lstApplication.Items.Add(p.MainWindowTitle);
                }
            }
        }   

        void Keyboardd()
        {
            while (isReadRunning)
            {
                Thread.Sleep(40);

                if ((Keyboard.GetKeyStates(Key.F2) & KeyStates.Down) > 0)
                {
                    if (autoThread1 == null)
                    {
                        lsbAsyncTaskVal.Items.Add("Auto Clicking...");
                        autoThread1 = new Thread(autoClick1);
                        autoThread1.SetApartmentState(ApartmentState.STA);
                        CheckForIllegalCrossThreadCalls = false;
                        isautoClick1Running = true;
                        autoThread1.Start();

                        btnStop.Enabled = true;
                        continue;
                    }

                    if (autoThread1.IsAlive)
                    {
                        MousePoint currentMousePoint;
                        var gotPoint = GetCursorPos(out currentMousePoint);

                        lsbAsyncTaskVal.Items.Add("Auto Clicking....");
                        autoThread1 = new Thread(autoClick1);
                        autoThread1.SetApartmentState(ApartmentState.STA);
                        CheckForIllegalCrossThreadCalls = false;
                        isautoClick1Running = true;
                        autoThread1.Start();

                        btnStop.Enabled = true;
                        continue;
                    }
                }

                if ((Keyboard.GetKeyStates(Key.F3) & KeyStates.Down) > 0)
                {
                    if (autoThread1 == null)
                    {
                        continue;
                    }
                    if (autoThread1.IsAlive)
                    {
                        ky = null;
                        autoThread1 = null;
                        lsbAsyncTaskVal.Items.Add("Stop!");
                        isautoClick1Running = false;
                        btnStop.Enabled = false;
                        BlockInput(false);
                    }
                }

            }
        }

        void autoClick1()
        {
            while (isautoClick1Running)
            {
                Thread.Sleep(50);
                SetCursorPos(int.Parse(lblXVal.Text.ToString()), int.Parse(lblYVal.Text.ToString()));
                //BlockInput(true);
                mouse_event(MOUSEEVENTF_LEFTDOWN, int.Parse(lblXVal.Text.ToString()), int.Parse(lblYVal.Text.ToString()), 0, 0);
            }
        }

        void getXY()
        {
            while (isGettingXY)
            {
                Thread.Sleep(50);
                if ((Keyboard.GetKeyStates(Key.F4) & KeyStates.Down) > 0)
                {
                    MousePoint currentMousePoint;
                    var gotPoint = GetCursorPos(out currentMousePoint);

                    int x, y;
                    x = currentMousePoint.X;
                    y = currentMousePoint.Y;
                    lblXVal.Text = x.ToString();
                    lblYVal.Text = y.ToString();

                    btnStart.Enabled = true;
                    btnStop.Enabled = true;
                    btnStart.Enabled = true;
                    isGettingXY = false;
                    gettingXY = null;
                    btnGetXY.Enabled = true;
                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                }
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            ky = null;
            autoThread1 = null;
            isReadRunning = false;
            isautoClick1Running = false;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MousePoint
        {
            public int X;
            public int Y;

            public MousePoint(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        private void btnGetXY_Click(object sender, EventArgs e)
        {
            if (gettingXY == null)
            {
                btnStart.Enabled = false;
                btnStop.Enabled = false;
                gettingXY = new Thread(getXY);
                gettingXY.SetApartmentState(ApartmentState.STA);
                CheckForIllegalCrossThreadCalls = false;
                isGettingXY = true;
                gettingXY.Start();
                btnGetXY.Enabled = false;
            }
        }

        private void lstApplication_SelectedChanged(object sender, EventArgs e)
        {
            String appName = lstApplication.SelectedItem.ToString();
            iHandle = FindWindow("", appName);
       } 

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            lstApplication.Items.Clear();
            getRunningApplication();
        }
    }
}
