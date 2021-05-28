using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace messageBoxOnStartup
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
            CanHandleSessionChangeEvent = true;
            CanHandlePowerEvent = true;
            CanPauseAndContinue = true;
            CanShutdown = true;
            CanStop = true;
            AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            WriteToFile("Service is started at " + DateTime.Now);
            SessionChangeDescription sschange = new SessionChangeDescription();
            OnSessionChange(sschange);
        }

        protected override void OnStop()
        {
        }
        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            Thread.Sleep(3000);
            try
            {
                base.OnSessionChange(changeDescription);
                if ((changeDescription.Reason == SessionChangeReason.SessionUnlock) || (changeDescription.Reason == SessionChangeReason.SessionLogon))
                {
                    WriteToFile("User Logged in");
                    ShowMessageBox();
                }
                else
                {
                    WriteToFile("User not log");
                }
            }
            catch (Exception exp)
            {
                WriteToFile(exp.ToString());
            }
        }
        public void ShowMessageBox()
        {
            bool result = false;
            String title = "WARNING";
            int tlen = title.Length;
            string msg = "18520545";
            int mlen = msg.Length;
            int resp = 0;
            result = WTSSendMessage(WTS_CURRENT_SERVER_HANDLE, 4, title, tlen, msg, mlen, 0, 0, out resp, true);
            WriteToFile(result.ToString());
            int err = Marshal.GetLastWin32Error();
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int WTSGetActiveConsoleSessionID();

        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern bool WTSSendMessage(
                IntPtr hServer,
                [MarshalAs(UnmanagedType.I4)] int SessionId,
                String pTitle,
                [MarshalAs(UnmanagedType.U4)] int TitleLength,
                String pMessage,
                [MarshalAs(UnmanagedType.U4)] int MessageLength,
                [MarshalAs(UnmanagedType.U4)] int Style,
                [MarshalAs(UnmanagedType.U4)] int Timeout,
                [MarshalAs(UnmanagedType.U4)] out int pResponse,
                bool bWait);
        public static IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;
        public static int WTS_CURRENT_SESSION = 4;
        public void WriteToFile(string message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(message);
                }
            }
        }
    }
}
