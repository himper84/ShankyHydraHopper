using System;
using System.Windows.Forms;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

public class WindowFunctions
{
    const int MAXWINDOWTEXTLENGTH = 256;

    private static ArrayList mTitlesList;
    private static Hashtable hWinList;

    private delegate bool EnumDelegate(IntPtr hWnd, int lParam);

    public const int WM_COMMAND = 0x111;
    public const uint WM_KEYDOWN = 0x100;
    public const uint WM_KEYUP = 0x101;
    public const uint WM_LBUTTONDOWN = 0x201;
    public const uint WM_LBUTTONUP = 0x202;
    public const uint WM_CHAR = 0x102;
    public const int MK_LBUTTON = 0x01;
    public const int VK_RETURN = 0x0d;
    public const int VK_ESCAPE = 0x1b;
    public const int VK_TAB = 0x09;
    public const int VK_LEFT = 0x25;
    public const int VK_UP = 0x26;
    public const int VK_RIGHT = 0x27;
    public const int VK_DOWN = 0x28;
    public const int VK_F5 = 0x74;
    public const int VK_F6 = 0x75;
    public const int VK_F7 = 0x76;
    public const int INPUT_MOUSE = 0;
    public const int INPUT_KEYBOARD = 1;
    public const int INPUT_HARDWARE = 2;
    public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
    public const uint KEYEVENTF_KEYUP = 0x0002;
    public const uint KEYEVENTF_UNICODE = 0x0004;
    public const uint KEYEVENTF_SCANCODE = 0x0008;
    public const uint XBUTTON1 = 0x0001;
    public const uint XBUTTON2 = 0x0002;
    public const uint MOUSEEVENTF_MOVE = 0x0001;
    public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    public const uint MOUSEEVENTF_LEFTUP = 0x0004;
    public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    public const uint MOUSEEVENTF_RIGHTUP = 0x0010;
    public const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
    public const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
    public const uint MOUSEEVENTF_XDOWN = 0x0080;
    public const uint MOUSEEVENTF_XUP = 0x0100;
    public const uint MOUSEEVENTF_WHEEL = 0x0800;
    public const uint MOUSEEVENTF_VIRTUALDESK = 0x4000;
    public const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
    public const int WM_SYSCOMMAND = 0x0112;
    public const int SC_CLOSE = 0xF060;
    
    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        int dx;
        int dy;
        uint mouseData;
        uint dwFlags;
        uint time;
        IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HARDWAREINPUT
    {
        uint uMsg;
        ushort wParamL;
        ushort wParamH;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct INPUT
    {
        [FieldOffset(0)]
        public int type;
        [FieldOffset(4)] //*
        public MOUSEINPUT mi;
        [FieldOffset(4)] //*
        public KEYBDINPUT ki;
        [FieldOffset(4)] //*
        public HARDWAREINPUT hi;
    }

    [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
    public static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr GetMenu(HandleRef hWnd);

    [DllImport("user32.dll", EntryPoint = "GetMenuString")]
    public static extern int GetMenuString(int hMenu, int wIDItem, string lpString, int nMaxCount, int wFlag);

    [DllImport("user32.dll")]
    public static extern IntPtr SetFocus(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr GetMessageExtraInfo();

    [DllImport("User32.dll")]
    public static extern uint SendInput(uint numberOfInputs, [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] INPUT[] input, int structSize);

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

    public static int MakeLong(int low, int high)
    {
        return (high << 16) | (low & 0xffff);
    }

    [DllImport("user32.dll")]
    public static extern byte VkKeyScan(char ch);

    [DllImport("user32.dll")]
    public static extern uint MapVirtualKey(uint uCode, uint uMapType);
    
    [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
     ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool _EnumDesktopWindows(IntPtr hDesktop,
    EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

    [DllImport("user32.dll", EntryPoint = "GetWindowText",
     ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int _GetWindowText(IntPtr hWnd,
    StringBuilder lpWindowText, int nMaxCount);

    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    public static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

    [DllImportAttribute("User32.dll")]
    private static extern IntPtr SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public WindowFunctions()
    {
        mTitlesList = new ArrayList();
        hWinList = new Hashtable();
    }

    public IntPtr ToIntPtr(string sPointer)
    {
        if (IntPtr.Size == 8)
        {
            return (IntPtr)Convert.ToInt64(sPointer);
        }
        else
        {
            return (IntPtr)Convert.ToInt32(sPointer);
        }
    }

    private static bool EnumWindowsProc(IntPtr hWnd, int lParam)
    {
        string title = GetWindowText(hWnd);
        if (!mTitlesList.Contains(title))
            mTitlesList.Add(title);
        try
        {
            if(!hWinList.ContainsKey(hWnd))
                hWinList.Add(hWnd, title);
            //else
                //FileFunctions.fConsoleWrite(String.Format("Key already found: {0} {1} as '{2}'", hWnd, title, hWinList[hWnd].ToString()).ToString());
        }
        catch (Exception)
        {
            FileFunctions.fConsoleWrite(String.Format("Couldn't Add {0} {1}", hWnd, title).ToString());
        }
        finally
        {
        }

        return true;
    }

    public string GetWindowTextX(IntPtr hWnd)
    {
        //static?
        StringBuilder title = new StringBuilder(MAXWINDOWTEXTLENGTH);
        int titleLength = _GetWindowText(hWnd, title, title.Capacity + 1);
        title.Length = titleLength;

        return title.ToString();
    }

    public static string GetWindowText(IntPtr hWnd) 
    {
        //static?
        StringBuilder title = new StringBuilder(MAXWINDOWTEXTLENGTH);
        int titleLength = _GetWindowText(hWnd, title, title.Capacity + 1);
        title.Length = titleLength;

        return title.ToString();
    }

    /// <summary>
    /// Returns the caption of all desktop windows.
    /// </summary>
    public static string[] GetDesktopWindowsCaptions()
    {
        mTitlesList = new ArrayList();
        EnumDelegate enumfunc = new EnumDelegate(EnumWindowsProc);
        IntPtr hDesktop = IntPtr.Zero; // current desktop
        bool success = _EnumDesktopWindows(hDesktop, enumfunc, IntPtr.Zero);

        if (success)
        {
            // Copy the result to string array
            string[] titles = new string[mTitlesList.Count];
            mTitlesList.CopyTo(titles);
            return titles;
        }
        else
        {
            // Get the last Win32 error code
            int errorCode = Marshal.GetLastWin32Error();

            string errorMessage = String.Format("EnumDesktopWindows failed with code {0}.", errorCode);
            throw new Exception(errorMessage);
        }
    }


    public void SetWindowToForeground(IntPtr hWnd)
    {
        SetForegroundWindow(hWnd);
    }

    public bool SetWindowPosition(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int wFlags)
    {
        return SetWindowPos(hWnd, hWndInsertAfter, x, y, cx, cy, wFlags)>0;
    }

    public IntPtr GetWindowHandle(string searchTitle, string text = "")
    {
        //mTitlesList = new ArrayList();
        //hWinList = new Hashtable();
        hWinList.Clear();
        mTitlesList.Clear();
        
        //Console.WriteLine("GetWindowHandle" + hWinList.Count.ToString());

        EnumDelegate enumfunc = new EnumDelegate(EnumWindowsProc);
        IntPtr hDesktop = IntPtr.Zero; // current desktop
        bool success = _EnumDesktopWindows(hDesktop, enumfunc, IntPtr.Zero);

        if (success)
        {
            System.Text.RegularExpressions.Regex reTitle = new System.Text.RegularExpressions.Regex(searchTitle);
            
            IDictionaryEnumerator en = hWinList.GetEnumerator();
            while(en.MoveNext())
            {
                Match m = reTitle.Match(en.Value.ToString());
                if (m.Success)
                {
                    return (IntPtr)en.Key;
                }
            }
            return IntPtr.Zero;
        }
        else
        {
            // Get the last Win32 error code
            int errorCode = Marshal.GetLastWin32Error();

            string errorMessage = String.Format(
            "EnumDesktopWindows failed with code {0}.", errorCode);
            throw new Exception(errorMessage);
        }
    }

    public Hashtable WinList(string searchTitle)
    {
        hWinList.Clear();
        mTitlesList.Clear();

        // Console.WriteLine("WinExists String:" + hWinList.Count.ToString());

        EnumDelegate enumfunc = new EnumDelegate(EnumWindowsProc);
        IntPtr hDesktop = IntPtr.Zero; // current desktop
        bool success = _EnumDesktopWindows(hDesktop, enumfunc, IntPtr.Zero);

        Hashtable hshRV = new Hashtable();

        if (success)
        {
            if (hWinList.Count > 0)
            {
                IDictionaryEnumerator en = hWinList.GetEnumerator();
                System.Text.RegularExpressions.Regex reTitle = new System.Text.RegularExpressions.Regex(searchTitle);
                while (en.MoveNext())
                {
                    Match m = reTitle.Match(en.Value.ToString());
                    if (m.Success)
                    {
                        hshRV.Add(en.Key, en.Value);
                    }
                }
            }
            return hshRV;
        }
        else
        {
            // Get the last Win32 error code
            int errorCode = Marshal.GetLastWin32Error();

            string errorMessage = String.Format(
            "EnumDesktopWindows failed with code {0}.", errorCode);
            throw new Exception(errorMessage);
        }
    }

    public IntPtr GUICtrlMenu_GetMenu(IntPtr hWnd)
    {
        HandleRef handleref = new HandleRef(null, hWnd);
        String caption = new String('t', 30);
        return GetMenu(handleref);
    }

    public string GUICtrlMenu_GetItemText(IntPtr hWnd, int iItem)
    {
        string caption = new String('t', 30);
        GetMenuString(hWnd.ToInt32(), iItem, caption, 30, 0);
        return caption;
    }

    public string GetMenuItemText(IntPtr hWnd, int iItem)
    {
        IntPtr hMain = GUICtrlMenu_GetMenu(hWnd);
        return GUICtrlMenu_GetItemText(hMain, iItem);
    }

    public void ControlClick(IntPtr hWnd, string sControlClass, string sControlName, string sMouseButton, int iClicks)
    {
        IntPtr hwndChild = WindowsAPI.FindWindowEx(hWnd, IntPtr.Zero, sControlClass, sControlName);
        WindowsAPI.SendMessage(hwndChild, WindowsAPI.BN_CLICKED, 0, IntPtr.Zero);
    }

    public bool WinExists(string searchTitle)
    {
        //mTitlesList = new ArrayList();
        //hWinList = new Hashtable();
        hWinList.Clear();
        mTitlesList.Clear();

       // Console.WriteLine("WinExists String:" + hWinList.Count.ToString());

        EnumDelegate enumfunc = new EnumDelegate(EnumWindowsProc);
        IntPtr hDesktop = IntPtr.Zero; // current desktop
        bool success = _EnumDesktopWindows(hDesktop, enumfunc, IntPtr.Zero);

        if (success)
        {
            System.Text.RegularExpressions.Regex reTitle = new System.Text.RegularExpressions.Regex(searchTitle);
            
            IDictionaryEnumerator en = hWinList.GetEnumerator();
            while (en.MoveNext())
            {
                Match m = reTitle.Match(en.Value.ToString());
                if (m.Success)
                {
                    return true;
                }
            }
            return false;
        }
        else
        {
            // Get the last Win32 error code
            int errorCode = Marshal.GetLastWin32Error();

            string errorMessage = String.Format(
            "EnumDesktopWindows failed with code {0}.", errorCode);
            throw new Exception(errorMessage);
        }
    }
    public bool WinExists(IntPtr hWnd)
    {
        //mTitlesList = new ArrayList();
        mTitlesList.Clear();
        hWinList.Clear();
       // Console.WriteLine("WinExists" + mTitlesList.Count.ToString());

        EnumDelegate enumfunc = new EnumDelegate(EnumWindowsProc);
        IntPtr hDesktop = IntPtr.Zero; // current desktop
        bool success = _EnumDesktopWindows(hDesktop, enumfunc, IntPtr.Zero);

        if (success)
        {
            IDictionaryEnumerator en = hWinList.GetEnumerator();
            while (en.MoveNext())
            {
                if ((IntPtr)en.Key==hWnd)
                {
                    return true;
                }
            }
            return false;
        }
        else
        {
            // Get the last Win32 error code
            int errorCode = Marshal.GetLastWin32Error();

            string errorMessage = String.Format(
            "EnumDesktopWindows failed with code {0}.", errorCode);
            throw new Exception(errorMessage);
        }
    }

    public bool WinWait(string searchTitle, int seconds)
    {
        DateTime timer = DateTime.Now;
        while (!WinExists(searchTitle) && (DateTime.Now - timer).Seconds < seconds)
        {
            System.Threading.Thread.Sleep(250);
            Application.DoEvents();
        }
        return WinExists(searchTitle);
    }
    public bool WinWait(IntPtr hWin, int seconds)
    {
        DateTime timer = DateTime.Now;
        while (!WinExists(hWin) && (DateTime.Now - timer).Seconds < seconds)
            Application.DoEvents();
        return WinExists(hWin);
    }

    public bool WinWaitClose(string searchTitle, int seconds)
    {
        DateTime timer = DateTime.Now;
        while (WinExists(searchTitle) && (DateTime.Now - timer).Seconds < seconds)
            Application.DoEvents();
        return !WinExists(searchTitle);
    }

    public bool WinWaitClose(IntPtr hWin, int seconds)
    {
        DateTime timer = DateTime.Now;
        while (WinExists(hWin) && (DateTime.Now - timer).Seconds < seconds)
        {
            //Console.WriteLine(WinExists(hWin).ToString());
            Application.DoEvents();
        }
        return !WinExists(hWin);
    }

    public void WinClose(IntPtr hWnd)
    {
        WindowsAPI.SendMessage(hWnd, WindowsAPI.WM_SYSCOMMAND, WindowsAPI.SC_CLOSE, IntPtr.Zero);
    }
    public void WinClose(string sRegEx)
    {
        WinClose(GetWindowHandle(sRegEx));
    }

    public int[] GetWindowPosition(IntPtr hWnd)
    {
        int[] rv = new int[4];
        RECT rect = new RECT();
        if(!GetWindowRect(hWnd, ref rect))
        {
            FileFunctions.fConsoleWrite("Error - uanble to get position");
            rv[0] = -1;
            rv[1] = -1;
            rv[2] = -1;
            rv[3] = -1;
        }
        else
        {
            rv[0] = rect.Left;
            rv[1] = rect.Top;
            rv[2] = rect.Right - rect.Left;
            rv[3] = rect.Bottom - rect.Top;
        }
        return rv;
    }

    public void demo()
    {
        string[] desktopWindowsCaptions = GetDesktopWindowsCaptions();
        foreach (string caption in desktopWindowsCaptions)
        {
            FileFunctions.fConsoleWrite(caption);
        }
    }
}