using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AI_Writing_Assistant.Helpers
{
    public class GlobalKeyboardHook : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        private HookProc _proc = HookCallback;
        private nint _hookID = nint.Zero;
        private static GlobalKeyboardHook _instance;

        public delegate nint HookProc(int nCode, nint wParam, nint lParam);

        public event EventHandler<GlobalKeyboardHookEventArgs> KeyDown;
        public event EventHandler<GlobalKeyboardHookEventArgs> KeyUp;

        public GlobalKeyboardHook()
        {
            _instance = this;
            _hookID = SetHook(_proc);
        }

        private static nint SetHook(HookProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static nint HookCallback(int nCode, nint wParam, nint lParam)
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

                if (wParam == WM_KEYDOWN)
                {
                    _instance?.KeyDown?.Invoke(_instance, args);
                }
                else if (wParam == WM_KEYUP)
                {
                    _instance?.KeyUp?.Invoke(_instance, args);
                }

                if (args.Handled)
                    return 1;
            }

            return CallNextHookEx(_instance._hookID, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(_hookID);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern nint SetWindowsHookEx(int idHook, HookProc lpfn, nint hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(nint hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern nint GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);
    }

    public class GlobalKeyboardHookEventArgs : EventArgs
    {
        public Keys KeyData { get; set; }
        public bool Handled { get; set; }
    }
}
