using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AI_Writing_Assistant
{
    public class GlobalKeyboardHook : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        private HookProc _proc = HookCallback;
        private IntPtr _hookID = IntPtr.Zero;
        private static GlobalKeyboardHook _instance;

        public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        public event EventHandler<GlobalKeyboardHookEventArgs> KeyDown;
        public event EventHandler<GlobalKeyboardHookEventArgs> KeyUp;

        public GlobalKeyboardHook()
        {
            _instance = this;
            _hookID = SetHook(_proc);
        }

        private static IntPtr SetHook(HookProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var vkCode = Marshal.ReadInt32(lParam);
                var key = (Keys)vkCode;

                var modifiers = Keys.None;
                if ((GetAsyncKeyState(Keys.ControlKey) & 0x8000) != 0)
                    modifiers |= Keys.Control;
                if ((GetAsyncKeyState(Keys.ShiftKey) & 0x8000) != 0)
                    modifiers |= Keys.Shift;
                if ((GetAsyncKeyState(Keys.Menu) & 0x8000) != 0)
                    modifiers |= Keys.Alt;

                var keyData = key | modifiers;
                var args = new GlobalKeyboardHookEventArgs { KeyData = keyData };

                if (wParam == (IntPtr)WM_KEYDOWN)
                {
                    _instance?.KeyDown?.Invoke(_instance, args);
                }
                else if (wParam == (IntPtr)WM_KEYUP)
                {
                    _instance?.KeyUp?.Invoke(_instance, args);
                }

                if (args.Handled)
                    return (IntPtr)1;
            }

            return CallNextHookEx(_instance._hookID, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(_hookID);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);
    }

    public class GlobalKeyboardHookEventArgs : EventArgs
    {
        public Keys KeyData { get; set; }
        public bool Handled { get; set; }
    }
}
