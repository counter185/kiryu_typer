using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace kiryu
{
    public partial class Form1 : Form
    {

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public enum State { 
            Idle,H0,H1
        }

        public readonly Bitmap[] idleBitmaps = new Bitmap[]{
            kiryu.Properties.Resources.idle__000000,
            kiryu.Properties.Resources.idle__000001,
            kiryu.Properties.Resources.idle__000002,
            kiryu.Properties.Resources.idle__000003,
            kiryu.Properties.Resources.idle__000004,
            kiryu.Properties.Resources.idle__000005,
            kiryu.Properties.Resources.idle__000006,
            kiryu.Properties.Resources.idle__000007,
            kiryu.Properties.Resources.idle__000008,
            kiryu.Properties.Resources.idle__000009,
            kiryu.Properties.Resources.idle__000010,
            kiryu.Properties.Resources.idle__000011,
            kiryu.Properties.Resources.idle__000012,
            kiryu.Properties.Resources.idle__000013,
            kiryu.Properties.Resources.idle__000014,
            kiryu.Properties.Resources.idle__000015,
            kiryu.Properties.Resources.idle__000016,
            kiryu.Properties.Resources.idle__000017,
            kiryu.Properties.Resources.idle__000018,
            kiryu.Properties.Resources.idle__000019,
            kiryu.Properties.Resources.idle__000020,
            kiryu.Properties.Resources.idle__000021,
            kiryu.Properties.Resources.idle__000022,
            kiryu.Properties.Resources.idle__000023,
            kiryu.Properties.Resources.idle__000024,
            kiryu.Properties.Resources.idle__000025,
            kiryu.Properties.Resources.idle__000026,
            kiryu.Properties.Resources.idle__000027,
            kiryu.Properties.Resources.idle__000028,
            kiryu.Properties.Resources.idle__000029,
            kiryu.Properties.Resources.idle__000030,
            kiryu.Properties.Resources.idle__000031,
            kiryu.Properties.Resources.idle__000032,
            kiryu.Properties.Resources.idle__000033,
            kiryu.Properties.Resources.idle__000034,
        };

        public readonly Bitmap[] h0Bitmaps = new Bitmap[] {
            //kiryu.Properties.Resources.h0_fr_000000,
            //kiryu.Properties.Resources.h0_fr_000001,
            kiryu.Properties.Resources.h0_fr_000002,
            kiryu.Properties.Resources.h0_fr_000003,
            kiryu.Properties.Resources.h0_fr_000004,
            kiryu.Properties.Resources.h0_fr_000005,
            kiryu.Properties.Resources.h0_fr_000006,
            kiryu.Properties.Resources.h0_fr_000007,
            kiryu.Properties.Resources.h0_fr_000008,
        };

        public readonly Bitmap[] h1Bitmaps = new Bitmap[] {
            //kiryu.Properties.Resources.h1_fr_000000,
            //kiryu.Properties.Resources.h1_fr_000001,
            kiryu.Properties.Resources.h1_fr_000002,
            kiryu.Properties.Resources.h1_fr_000003,
            kiryu.Properties.Resources.h1_fr_000004,
            kiryu.Properties.Resources.h1_fr_000005,
            kiryu.Properties.Resources.h1_fr_000006,
        };

        public const byte idleMax = 35, h0Max = 7, h1Max = 5;

        public static byte currentIdleTick = 0, h0Tick = 0, h1Tick = 0;
        public static bool idleReversing = false;
        public static State currentState = State.Idle;
        public static int lastKey = -1;
        public static bool lastStateH0 = true;

        public Form1()
        {
            _hookID = SetHook(_proc);

            InitializeComponent();

            timer1.Tick += delegate
            {
                //Console.WriteLine("Tick");
                switch (currentState) {
                    case State.Idle:
                        kiryuMain.Image = idleBitmaps[currentIdleTick];
                        if (idleReversing)
                        {
                            currentIdleTick--;
                            if (currentIdleTick == 0xff)
                            {
                                currentIdleTick = 0;
                                idleReversing = false;
                            }
                        }
                        else
                        {
                            currentIdleTick++;
                            if (currentIdleTick == idleMax)
                            {
                                currentIdleTick--;
                                idleReversing = true;
                            }
                        }
                        break;
                    case State.H0:
                        kiryuMain.Image = h0Bitmaps[h0Tick];
                        h0Tick++;
                        if (h0Tick == h0Max) {
                            h0Tick = 0;
                            currentState = State.Idle;
                        }
                        break;
                    case State.H1:
                        kiryuMain.Image = h1Bitmaps[h1Tick];
                        h1Tick++;
                        if (h1Tick == h1Max)
                        {
                            h1Tick = 0;
                            currentState = State.Idle;
                        }
                        break;
                }
                
            };
        }

        protected override void OnClosed(EventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
            base.OnClosed(e);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (vkCode != lastKey)
                {
                    if (lastStateH0)
                    {
                        lastStateH0 = false;
                        currentState = State.H1;
                        h1Tick = 0;
                    }
                    else
                    {
                        lastStateH0 = true;
                        currentState = State.H0;
                        h0Tick = 0;
                    }
                }
                else {
                    if (lastStateH0)
                    {
                        lastStateH0 = true;
                        currentState = State.H0;
                        h0Tick = 0;
                    }
                    else
                    {
                        lastStateH0 = false;
                        currentState = State.H1;
                        h1Tick = 0;
                    }
                }
                currentIdleTick = 0;
                idleReversing = false;
                lastKey = vkCode;
                //Console.WriteLine((Keys)vkCode);
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
    }
}
