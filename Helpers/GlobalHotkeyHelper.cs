using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace LucidNotes.Helpers
{
    public static class GlobalHotkeyHelper
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public const int WM_HOTKEY = 0x0312;

        public static void RegisterHotKey(HwndSource source, int id, ModifierKeys modifiers, Key key)
        {
            if (source == null) return;

            uint mod = (uint)modifiers;
            uint vk = (uint)KeyInterop.VirtualKeyFromKey(key);

            RegisterHotKey(source.Handle, id, mod, vk);
        }

        public static void UnregisterHotKey(HwndSource source, int id)
        {
            if (source == null) return;
            UnregisterHotKey(source.Handle, id);
        }
    }
}
