using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;

public class WindowsAPI
{
    private static IntPtr handle;

    #region Window state constants
    public const int SW_HIDE = 0;
    public const int SW_SHOWNORMAL = 1;
    public const int SW_SHOWMINIMIZED = 2;
    public const int SW_SHOWMAXIMIZED = 3;
    public const int SW_SHOWNOACTIVATE = 4;
    public const int SW_RESTORE = 9;
    public const int SW_SHOWDEFAULT = 10;
    #endregion

    #region Menu Constants
    public const uint MF_BYCOMMAND = 0x00000000;
    public const uint MF_BYPOSITION = 0x00000400;
    public const UInt32 MIM_MAXHEIGHT = 0x00000001;
    public const UInt32 MIM_BACKGROUND = 0x00000002;
    public const UInt32 MIM_HELPID = 0x00000004;
    public const UInt32 MIM_MENUDATA = 0x00000008;
    public const UInt32 MIM_STYLE = 0x00000010;
    public const UInt32 MIM_APPLYTOSUBMENUS = 0x80000000;
    #endregion
    
    #region SendMessage Constants
    public const int WM_SYSCOMMAND = 0x0112;
    public const int WM_COMMAND = 0x111;
    public const int SC_CLOSE = 0xF060;
    public const int MK_LBUTTON = 0x01;
    public const int WM_CLOSE = 16;
    public const int BN_CLICKED = 245;
    public const int HWND_BROADCAST = 0xffff;
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

    public const int WM_LBUTTONDOWN = 0x0201;
    public const int WM_LBUTTONUP = 0x0202;
    public const int WM_LBUTTONDBLCLK = 0x0203;
    public const int WM_RBUTTONDOWN = 0x0204;
    public const int WM_RBUTTONUP = 0x0205;
    public const int WM_RBUTTONDBLCLK = 0x0206;
    public const int WM_MOUSEMOVE = 0x0200;

    public const int WM_KEYDOWN = 0x100;
    public const int WM_KEYUP = 0x0101;
    public const int WM_CHAR = 0x0102;

    #endregion SendMessage Constants

    #region Virtual Keys Constants

    public const int VK_0 = 0x30;
    public const int VK_1 = 0x31;
    public const int VK_2 = 0x32;
    public const int VK_3 = 0x33;
    public const int VK_4 = 0x34;
    public const int VK_5 = 0x35;
    public const int VK_6 = 0x36;
    public const int VK_7 = 0x37;
    public const int VK_8 = 0x38;
    public const int VK_9 = 0x39;
    public const int VK_A = 0x41;
    public const int VK_B = 0x42;
    public const int VK_C = 0x43;
    public const int VK_D = 0x44;
    public const int VK_E = 0x45;
    public const int VK_F = 0x46;
    public const int VK_G = 0x47;
    public const int VK_H = 0x48;
    public const int VK_I = 0x49;
    public const int VK_J = 0x4A;
    public const int VK_K = 0x4B;
    public const int VK_L = 0x4C;
    public const int VK_M = 0x4D;
    public const int VK_N = 0x4E;
    public const int VK_O = 0x4F;
    public const int VK_P = 0x50;
    public const int VK_Q = 0x51;
    public const int VK_R = 0x52;
    public const int VK_S = 0x53;
    public const int VK_T = 0x54;
    public const int VK_U = 0x55;
    public const int VK_V = 0x56;
    public const int VK_W = 0x57;
    public const int VK_X = 0x58;
    public const int VK_Y = 0x59;
    public const int VK_Z = 0x5A;

    public const int VK_BACK = 0x08;
    public const int VK_TAB = 0x09;
    public const int VK_CLEAR = 0x0C;
    public const int VK_RETURN = 0x0D;
    public const int VK_SHIFT = 0x10;
    public const int VK_CONTROL = 0x11;
    public const int VK_MENU = 0x12;
    public const int VK_ALT = 0x12;
    public const int VK_PAUSE = 0x13;
    public const int VK_CAPITAL = 0x14;
    public const int VK_KANA = 0x15;
    public const int VK_HANGEUL = 0x15;
    public const int VK_HANGUL = 0x15;
    public const int VK_JUNJA = 0x17;
    public const int VK_FINAL = 0x18;
    public const int VK_HANJA = 0x19;
    public const int VK_KANJI = 0x19;
    public const int VK_ESCAPE = 0x1B;
    public const int VK_CONVERT = 0x1C;
    public const int VK_NONCONVERT = 0x1D;
    public const int VK_ACCEPT = 0x1E;
    public const int VK_MODECHANGE = 0x1F;
    public const int VK_SPACE = 0x20;
    public const int VK_PRIOR = 0x21;
    public const int VK_NEXT = 0x22;
    public const int VK_END = 0x23;
    public const int VK_HOME = 0x24;
    public const int VK_LEFT = 0x25;
    public const int VK_UP = 0x26;
    public const int VK_RIGHT = 0x27;
    public const int VK_DOWN = 0x28;
    public const int VK_SELECT = 0x29;
    public const int VK_PRINT = 0x2A;
    public const int VK_EXECUTE = 0x2B;
    public const int VK_SNAPSHOT = 0x2C;
    public const int VK_INSERT = 0x2D;
    public const int VK_DELETE = 0x2E;
    public const int VK_HELP = 0x2F;
    public const int VK_LWIN = 0x5B;
    public const int VK_RWIN = 0x5C;
    public const int VK_APPS = 0x5D;
    public const int VK_SLEEP = 0x5F;
    public const int VK_NUMPAD0 = 0x60;
    public const int VK_NUMPAD1 = 0x61;
    public const int VK_NUMPAD2 = 0x62;
    public const int VK_NUMPAD3 = 0x63;
    public const int VK_NUMPAD4 = 0x64;
    public const int VK_NUMPAD5 = 0x65;
    public const int VK_NUMPAD6 = 0x66;
    public const int VK_NUMPAD7 = 0x67;
    public const int VK_NUMPAD8 = 0x68;
    public const int VK_NUMPAD9 = 0x69;
    public const int VK_MULTIPLY = 0x6A;
    public const int VK_ADD = 0x6B;
    public const int VK_SEPARATOR = 0x6C;
    public const int VK_SUBTRACT = 0x6D;
    public const int VK_DECIMAL = 0x6E;
    public const int VK_DIVIDE = 0x6F;
    public const int VK_F1 = 0x70;
    public const int VK_F2 = 0x71;
    public const int VK_F3 = 0x72;
    public const int VK_F4 = 0x73;
    public const int VK_F5 = 0x74;
    public const int VK_F6 = 0x75;
    public const int VK_F7 = 0x76;
    public const int VK_F8 = 0x77;
    public const int VK_F9 = 0x78;
    public const int VK_F10 = 0x79;
    public const int VK_F11 = 0x7A;
    public const int VK_F12 = 0x7B;
    public const int VK_F13 = 0x7C;
    public const int VK_F14 = 0x7D;
    public const int VK_F15 = 0x7E;
    public const int VK_F16 = 0x7F;
    public const int VK_F17 = 0x80;
    public const int VK_F18 = 0x81;
    public const int VK_F19 = 0x82;
    public const int VK_F20 = 0x83;
    public const int VK_F21 = 0x84;
    public const int VK_F22 = 0x85;
    public const int VK_F23 = 0x86;
    public const int VK_F24 = 0x87;
    public const int VK_NUMLOCK = 0x90;
    public const int VK_SCROLL = 0x91;
    
    #endregion Virtual Keys Constants

    [DllImport("user32.dll")]
    public static extern
        bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")]
    public static extern
        bool IsIconic(IntPtr hWnd);
    [DllImport("user32.dll")]
    private static extern
        bool IsZoomed(IntPtr hWnd);
    [DllImport("user32.dll")]
    private static extern
        IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]
    private static extern
        IntPtr GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);
    [DllImport("user32.dll")]
    private static extern
        IntPtr AttachThreadInput(IntPtr idAttach, IntPtr idAttachTo, int fAttach);
    [DllImport("user32.dll")]
    public static extern
        bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int SendMessage(int hWnd, int msg, int wParam, IntPtr lParam);
    [DllImport("user32.dll", CharSet = CharSet.Auto)] // used for button-down & button-up
    private static extern int PostMessage(int hWnd, int msg, int wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)] // used for button-down & button-up
    private static extern int PostMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern IntPtr GetMenu(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern int GetMenuItemCount(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern int GetMenuItemID(IntPtr hWnd, int nPos);

    [DllImport("user32.dll")]
    public static extern int GetMenuString(IntPtr hMenu, uint uIDItem,
       [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder lpString, int nMaxCount, uint uFlag);

    [DllImport("user32.dll")]
    static extern bool GetMenuItemInfo(IntPtr hMenu, uint uItem, bool fByPosition,
       ref MENUITEMINFO lpmii);

    [DllImport("user32.dll")]
    public static extern IntPtr GetSubMenu(IntPtr hWnd, int nPos);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    public static extern uint GetCurrentThreadId();

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern uint RegisterWindowMessage(string lpString);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool ReadProcessMemory(
      IntPtr hProcess,
      IntPtr lpBaseAddress,
      [Out()] byte[] lpBuffer,
      int dwSize,
      out int lpNumberOfBytesRead
     );

    public static void SwitchWindow(IntPtr windowHandle)
    {
        if (GetForegroundWindow() == windowHandle)
            return;

        IntPtr foregroundWindowHandle = GetForegroundWindow();
        uint currentThreadId = GetCurrentThreadId();
        uint temp;
        uint foregroundThreadId = GetWindowThreadProcessId(foregroundWindowHandle, out temp);
        AttachThreadInput(currentThreadId, foregroundThreadId, true);
        SetForegroundWindow(windowHandle);
        AttachThreadInput(currentThreadId, foregroundThreadId, false);

        while (GetForegroundWindow() != windowHandle)
        {
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

    [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
    public static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

    [DllImport("user32.dll")]
    public static extern byte VkKeyScan(char ch);

    [DllImport("user32.dll")]
    public static extern uint MapVirtualKey(uint uCode, uint uMapType);

    public static IntPtr FindWindow(string name)
    {
        Process[] procs = Process.GetProcesses();

        foreach (Process proc in procs)
        {
            if (proc.MainWindowTitle == name)
            {
                return proc.MainWindowHandle;
            }
        }

        return IntPtr.Zero;
    }

    [DllImport("user32.dll")]
    public static extern IntPtr SetFocus(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    public static int MakeLong(int low, int high)
    {
        return (high << 16) | (low & 0xffff);
    }

    [DllImport("User32.dll")]
    public static extern uint SendInput(uint numberOfInputs, [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] INPUT[] input, int structSize);

    [DllImport("user32.dll")]
    public static extern IntPtr GetMessageExtraInfo();


    public static void PressKey(char ch, bool press)
    {
        byte vk = WindowsAPI.VkKeyScan(ch);
        ushort scanCode = (ushort)WindowsAPI.MapVirtualKey(vk, 0);

        if (press)
            KeyDown(scanCode);
        else
            KeyUp(scanCode);
    }

    public static void KeyDown(ushort scanCode)
    {
        INPUT[] inputs = new INPUT[1];
        inputs[0].type = WindowsAPI.INPUT_KEYBOARD;
        inputs[0].ki.dwFlags = 0;
        inputs[0].ki.wScan = (ushort)(scanCode & 0xff);

        uint intReturn = WindowsAPI.SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
        if (intReturn != 1)
        {
            throw new Exception("Could not send key: " + scanCode);
        }
    }

    public static void KeyUp(ushort scanCode)
    {
        INPUT[] inputs = new INPUT[1];
        inputs[0].type = WindowsAPI.INPUT_KEYBOARD;
        inputs[0].ki.wScan = scanCode;
        inputs[0].ki.dwFlags = WindowsAPI.KEYEVENTF_KEYUP;
        uint intReturn = WindowsAPI.SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
        if (intReturn != 1)
        {
            throw new Exception("Could not send key: " + scanCode);
        }
    }
	public static void TakeOver(IntPtr passedHandle)
	{
        handle = passedHandle;
	}

    public static void SetFocus() 
	{
		SetForegroundWindow(handle);
	}

	public static void SendLeftButtonDown(int x,int y) 
	{
		PostMessage(handle,WM_LBUTTONDOWN,0,new IntPtr(y * 0x10000 + x));
	}

	public static void SendLeftButtonUp(int x,int y) 
	{
		PostMessage(handle,WM_LBUTTONUP,0,new IntPtr(y * 0x10000 + x));
	}

	public static void SendLeftButtonDblClick(int x,int y) 
	{
		PostMessage(handle,WM_LBUTTONDBLCLK,0,new IntPtr(y * 0x10000 + x));
	}

	public static void SendRightButtonDown(int x,int y) 
	{
		PostMessage(handle,WM_RBUTTONDOWN,0,new IntPtr(y * 0x10000 + x));
	}

	public static void SendRightButtonUp(int x,int y) 
	{
		PostMessage(handle,WM_RBUTTONUP,0,new IntPtr(y * 0x10000 + x));
	}

	public static void SendRightButtonDblClick(int x,int y) 
	{
		PostMessage(handle,WM_RBUTTONDBLCLK,0,new IntPtr(y * 0x10000 + x));
	}
	
	public static void SendMouseMove(int x,int y) 
	{
		PostMessage(handle,WM_MOUSEMOVE,0,new IntPtr(y * 0x10000 + x));
	}

	public static void SendKeyDown(int key) 
	{
		PostMessage(handle,WM_KEYDOWN,key,IntPtr.Zero);
	}
	
	public static void SendKeyUp(int key) 
	{
		PostMessage(handle,WM_KEYUP,key,new IntPtr(1));
	}

	public static void SendChar(char c)
	{
		SendMessage(handle, WM_CHAR, c, IntPtr.Zero);
	}
	
	public static void SendString(string s)
	{
		foreach (char c in s) SendChar(c);
	}

    public static bool WinMenuSelectItem(IntPtr hWindow, string sRootMenu, string sSubItem)
    {
        IntPtr hMenu = WindowsAPI.GetMenu(hWindow);
        bool bRV = false;
        uint x1 = 0;
        int hMenuItemID =0;
        for (int i = 0; i < WindowsAPI.GetMenuItemCount(hMenu) && !bRV; i++)
        {
            StringBuilder menuName = new StringBuilder(0x20);
            x1 = (uint)i;
            WindowsAPI.GetMenuString(hMenu, x1, menuName, 0x20, WindowsAPI.MF_BYPOSITION);
            //Console.WriteLine(menuName.ToString());
            if (menuName.ToString() == sRootMenu)
            {
                if (sSubItem.Length == 0)
                {
                    bRV = true;
                    hMenuItemID = WindowsAPI.GetMenuItemID(hMenu, i);
                }
                else
                {
                    IntPtr hSubMenuItem = WindowsAPI.GetSubMenu(hMenu, i);
                    for (int zz = 0; zz < WindowsAPI.GetMenuItemCount(hSubMenuItem); zz++)
                    {
                        x1 = (uint)zz;
                        WindowsAPI.GetMenuString(hSubMenuItem, x1, menuName, 0x20, WindowsAPI.MF_BYPOSITION);
                        //Console.WriteLine(menuName.ToString());
                        if (menuName.ToString() == sSubItem)
                        {
                            bRV = true;
                            hMenuItemID = WindowsAPI.GetMenuItemID(hSubMenuItem, zz);
                        }
                    }
                }
            }
            if (bRV)
            {
                WindowsAPI.PostMessage(hWindow, WindowsAPI.WM_COMMAND, hMenuItemID, IntPtr.Zero);
            }
        }
        return bRV;
    }

    public static bool Visible(IntPtr hWin)
    {
        return ShowWindowAsync(hWin, SW_SHOWNORMAL);
    }
}

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

[StructLayout(LayoutKind.Sequential)]
public struct MENUITEMINFO
{
    public uint cbSize;
    public uint fMask;
    public uint fType;
    public uint fState;
    public int wID;
    public int hSubMenu;
    public int hbmpChecked;
    public int hbmpUnchecked;
    public int dwItemData;
    public string dwTypeData;
    public uint cch;
    public int hbmpItem;
}