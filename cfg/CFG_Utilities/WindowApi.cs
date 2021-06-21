using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace CFG_Utilities
{
    public static class WindowApi
    {

        #region window api functions

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr i);
        delegate bool EnumWindowProc(IntPtr hWnd, IntPtr parameter);

        // Get a handle to an application window.
        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool SetWindowText(IntPtr hWnd, string txt);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);
        const int WM_COMMAND = 0x0111;
        const int BM_CLICK = 0x00F5;
        const int WM_SETTEXT = 0x000C;
        const int WM_KEYDOWN = 0x0100;
        const int VK_RETURN = 0x0D;
        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);
        [DllImport("USER32.DLL")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx", CharSet = CharSet.Auto)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll")]
        static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

        #endregion

        private static List<IntPtr> GetChildWindows(IntPtr parent)
        {
            List<IntPtr> result = new List<IntPtr>();
            GCHandle listHandle = GCHandle.Alloc(result);
            try
            {
                EnumChildWindows(parent, (hWnd, parameter) =>
                {
                    GCHandle gch = GCHandle.FromIntPtr(parameter);
                    List<IntPtr> list = gch.Target as List<IntPtr>;
                    if (list == null)
                    {
                        throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
                    }
                    list.Add(hWnd);
                        //  You can modify this to check to see if you want to cancel the operation, then return a null here
                        return true;
                }, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result;
        }

        private static string GetText(IntPtr hWnd)
        {
            // Allocate correct string length first
            int length = GetWindowTextLength(hWnd);
            StringBuilder sb = new StringBuilder(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        public static void SetTextByControlId(IntPtr hParent, int controlId, string str)
        {
            var hWnd = GetDlgItem(hParent, controlId);
            SetWindowText(hWnd, str);
        }

        public static string GetTextByControlId(IntPtr hParent, int controlId)
        {
            var controlHandle = GetDlgItem(hParent, controlId);
            return GetText(controlHandle);
        }

        public static void ClickButtonOnWindow(string windowTitle, string buttonCaption)
        {
            var windowHandle = FindWindow(default(string), windowTitle);
            if (windowHandle == IntPtr.Zero)
            {
                throw new Exception(string.Format("Window {0} not found!", windowTitle));
            }
            var lstChilds = GetChildWindows(windowHandle);
            foreach (var hWndButton in lstChilds)
            {
                if (GetText(hWndButton).Equals(buttonCaption))
                {
                    //send BN_CLICKED message
                    SendMessage(hWndButton, BM_CLICK, 0, IntPtr.Zero);
                    return;
                }
            }
            throw new Exception(string.Format("Button {0} not found in the window {1}", buttonCaption, windowTitle));
        }
    }

    public class WindowWrapper : System.Windows.Forms.IWin32Window
    {
        public WindowWrapper(IntPtr handle)
        {
            _hwnd = handle;
        }

        public IntPtr Handle
        {
            get { return _hwnd; }
        }

        private IntPtr _hwnd;
    }
}
