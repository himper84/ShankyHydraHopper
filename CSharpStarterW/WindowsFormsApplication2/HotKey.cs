using System.Runtime.InteropServices;
using System.Windows.Forms;
using System;

class HotKey : IMessageFilter
{

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, KeyModifiers fsModifiers, Keys vk);
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);


    public enum KeyModifiers
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8
    }

    public const int WM_HOTKEY = 0x0312;
    private const int id = 100;


    private IntPtr handle;
    public IntPtr Handle
    {
        get { return handle; }
        set { handle = value; }
    }
    private event EventHandler HotKeyPressed;

    public HotKey(Keys key, KeyModifiers modifier, EventHandler hotKeyPressed)
    {
        HotKeyPressed = hotKeyPressed;
        RegisterHotKey(key, modifier);
        Application.AddMessageFilter(this);
    }

    ~HotKey()
    {
        Application.RemoveMessageFilter(this);
        UnregisterHotKey(handle, id);
    }


    private void RegisterHotKey(Keys key, KeyModifiers modifier)
    {
        if (key == Keys.None)
            return;

        bool isKeyRegisterd = RegisterHotKey(handle, id, modifier, key);
        if (!isKeyRegisterd)
            throw new ApplicationException("Hotkey allready in use");
    }



    public bool PreFilterMessage(ref Message m)
    {
        switch (m.Msg)
        {
            case WM_HOTKEY:
                HotKeyPressed(this, new EventArgs());
                MessageBox.Show("test");
                return true;
        }
        return false;
    }

}

