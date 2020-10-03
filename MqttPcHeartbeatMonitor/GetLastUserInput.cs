using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MqttPcHeartbeatMonitor
{
    public class GetLastUserInput
    {
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }
        
        private static LASTINPUTINFO _lastInputInfo;

        static GetLastUserInput()
        {
            _lastInputInfo = new LASTINPUTINFO();
            _lastInputInfo.cbSize = (uint)Marshal.SizeOf(_lastInputInfo);
        }

        [DllImport("User32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        public static uint GetIdleTickCount()
        {
            return ((uint)Environment.TickCount - GetLastInputTime());
        }

        public static uint GetLastInputTime()
        {
            if (!GetLastInputInfo(ref _lastInputInfo))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return _lastInputInfo.dwTime;
        }
        public static bool IsIdle()
        {
            var idleTime = GetIdleTickCount();

            if (idleTime >= (60 * 5))
            {
                return true;
            }

            return false;
        }
    }
}
