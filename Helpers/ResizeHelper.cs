using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace GlassNotes.Helpers;

/// <summary>
/// Enables full edge/corner resizing for transparent WPF windows.
/// AllowsTransparency=True disables the OS hit-test on the window frame, so we hook
/// WM_NCHITTEST and return the correct HT* values ourselves.
/// All comparisons are done in screen pixels to be DPI-safe.
/// </summary>
public static class ResizeHelper
{
    private const int WM_NCHITTEST = 0x0084;
    private const int HTCLIENT = 1;
    private const int HTLEFT = 10;
    private const int HTRIGHT = 11;
    private const int HTTOP = 12;
    private const int HTTOPLEFT = 13;
    private const int HTTOPRIGHT = 14;
    private const int HTBOTTOM = 15;
    private const int HTBOTTOMLEFT = 16;
    private const int HTBOTTOMRIGHT = 17;

    // Resize border in physical pixels (≈8 logical px at 96 dpi)
    private const int BorderPx = 8;

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    /// <summary>Call from Window.SourceInitialized.</summary>
    public static void Attach(Window window)
    {
        var helper = new WindowInteropHelper(window);
        var source = HwndSource.FromHwnd(helper.Handle);
        source?.AddHook((IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) =>
        {
            if (msg != WM_NCHITTEST) return IntPtr.Zero;

            // lParam encodes cursor position in screen pixels
            int x = unchecked((short)(lParam.ToInt32() & 0xFFFF));
            int y = unchecked((short)((lParam.ToInt32() >> 16) & 0xFFFF));

            // Get window rect in screen pixels
            if (!GetWindowRect(hwnd, out RECT r)) return IntPtr.Zero;

            bool onLeft = x < r.Left + BorderPx;
            bool onRight = x > r.Right - BorderPx;
            bool onTop = y < r.Top + BorderPx;
            bool onBottom = y > r.Bottom - BorderPx;

            int hit = HTCLIENT;

            if (onTop && onLeft) hit = HTTOPLEFT;
            else if (onTop && onRight) hit = HTTOPRIGHT;
            else if (onBottom && onLeft) hit = HTBOTTOMLEFT;
            else if (onBottom && onRight) hit = HTBOTTOMRIGHT;
            else if (onTop) hit = HTTOP;
            else if (onBottom) hit = HTBOTTOM;
            else if (onLeft) hit = HTLEFT;
            else if (onRight) hit = HTRIGHT;

            if (hit != HTCLIENT)
            {
                handled = true;
                return new IntPtr(hit);
            }

            return IntPtr.Zero;
        });
    }
}
