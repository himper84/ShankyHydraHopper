using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Microsoft.Win32;
using System.Net;
using System.Net.Mail;
//using AutoItX3Lib;

namespace csFTPStarter
{
    public partial class MainForm : Form
    {
        AlertForm alert;

        #region Globals
        //our au3 class that gives us au3 functionality
        AutoItX3Lib.AutoItX3Class au3;

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool PostMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        //Import the FindWindow API to find our window
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetMenu(IntPtr hWnd);

        //Import the SetForeground API to activate it
        [DllImportAttribute("User32.dll")]
        private static extern IntPtr SetForegroundWindow(int hWnd);

        [DllImport("User32.Dll")]
        public static extern void GetWindowText(int h, StringBuilder s, int MaxCount);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(int hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        static extern int GetMenuItemCount(IntPtr hMenu);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindow(IntPtr hWnd);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("user32.dll")]
        static extern int GetMenuString(IntPtr hMenu, uint uIDItem, [Out] StringBuilder lpString, int nMaxCount, uint uFlag);
        public  void PressKey(Keys key, bool up)
        {
            const int KEYEVENTF_EXTENDEDKEY = 0x1;
            const int KEYEVENTF_KEYUP = 0x2;
            if (up)
            {
                keybd_event((byte)key, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
            }
            else
            {
                keybd_event((byte)key, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
            }
        }

        [DllImport("KERNEL32.DLL", EntryPoint = "GetPrivateProfileStringW",

        SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]

        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, string lpReturnString, int nSize, string lpFilename);

        [DllImport("winmm.dll")]
        private static extern bool PlaySound(string filename, int module, int flags);
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x20;
        private const int MOUSEEVENTF_MIDDLEUP = 0x40;

        internal const UInt32 MF_BYCOMMAND = 0x00000000;
        internal const UInt32 MF_BYPOSITION = 0x00000400;

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("hydra.dll")]
        private static extern int HydraCheckLicense();


        //static string[] hxShanky;
        static DateTime dtStarted;
        static Hashtable hshHydra;
        static Hashtable hshTables;
        static bool bPause;
        static string[] ipShanky;
        static string[] ipTable;
        static int[] CheckDeadTableAttempts;
        static int hTournReg = 0;
        static int OldBotList = 0;
        public string[] WDAYS = new string[] {"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"};
        static bool ItsTime = false;
        static string lastJoinTourneyMessage = "";

        static string txtTrn = "";
        static bool SuccessfulConnect = false;

        static int iLiveBots, iTablesFinished = 0, iTournamentsRegistered, OldiTablesFinished, tournamentsStarted;
        static bool noTourney;
        static bool bLockThreads = false;
        static int a, userNumSngInt, iBotsToPlay; //, userNumMinsInt;
        static string textProf, userNumSng, iBotsToPLay; // userNumMins
        static bool FoundNewBot;
        static bool WaitingForTournament;
        static ArrayList tTournaments = new ArrayList();
        static ArrayList hTournaments = new ArrayList();
        static Hashtable hshActiveTournamentIDs;
        static string textBot;
        static string ScriptDir;
        static bool tourneyRegOK = false;
        const string appName = "Shanky Sng/MTT Loader";
        const string FTexe = "AmericasCardroom.exe";
        const string FTloc = @"C:\AmericasCardroom\";
        string iniLocation = Directory.GetCurrentDirectory();
        const string sIniFile = "ShankyLoader.ini";
        IniFile MyIni = new IniFile(sIniFile);
        const string bottitle = "[REGEXPCLASS:\\A.*tooltips_class32.*\\z]";
        static string LobbyName = "";
        static string reLobbyName = "";
        static string scrapeini = "scrapes.ini";
        private static int iPlayers = 0;
        const string ftExe = "AmericasCardroom.exe";
        static string UserNamePattern = " As .*$";
        static string TournamentLobbyTitleHash = "";
        static Regex rgx;
        static WindowFunctions winFunc = new WindowFunctions();
        static IntPtr hLobby;
        static GraphicsFunctions gfx = new GraphicsFunctions();
        static FileFunctions ffx = new FileFunctions();
        static string sHydraPath;
        static DateTime tsLastTournamentJoin;
        static Thread threadCheckDeadTables;
        static System.Windows.Forms.Timer tmrCheckNewTables;
        //static System.Windows.Forms.Timer tmrCheckDeadTables;        
        DateTime sEndTimeBlock = DateTime.Now;
        int LastDayUpdated = -1;
        bool bScheduledToRun = false;
        string Message = "";
        bool bScheduleActive = false;
        int tournamentsRegistered = 0;
        int tablesFinished = 0;
        int OldtablesFinished = 0;
        string LastSchedulerMessage = "";
        static System.Windows.Forms.Timer tmrCheckDeadTables = new System.Windows.Forms.Timer();

        public enum ThreadNames
        {
            None,
            CheckForDeadTables,
            TournamentAnnouncements,
        }
        #endregion

        private static void Restart()
        {
            ProcessStartInfo proc = new ProcessStartInfo();
            proc.WindowStyle = ProcessWindowStyle.Hidden;
            proc.FileName = "cmd";
            proc.Arguments = "/C shutdown -f -r";
            Process.Start(proc);
        }

        string GetLine(string fileName, int line)
        {
            using (var sr = new StreamReader(fileName))
            {
                for (int i = 1; i < line; i++)
                    sr.ReadLine();
                return sr.ReadLine();
            }
        }

        public MainForm()
        {
            string message = "Computer needs to reboot to finish installation";
            string caption = "Press OK to restart computer";
            MessageBoxButtons button = MessageBoxButtons.OK;
            DialogResult result;
            int value = 0;
            RegistryKey myKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System");
            if (myKey == null)
            {
                Console.WriteLine("Key doesnt exist.");
                Registry.LocalMachine.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System");
            }
            else
            {
                value = (Int32)myKey.GetValue("EnableLUA");
                Console.WriteLine("Value: " + value);
                if (value == 1)
                {
                    Console.WriteLine("Key value is 1");
                    RegistryKey rkTest = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", true);
                    rkTest.SetValue("EnableLUA", 0);
                    result = MessageBox.Show(message, caption, button);
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        Restart();
                    }
                } 
            }   
            InitializeComponent();
            UpdateListviewToPlayMTT();
            UpdateListviewToPlaySng();
            UpdateListview();
            //threadCheckDeadTables = new Thread(thdCheckDeadTables);
            tmrCheckNewTables = new System.Windows.Forms.Timer();            
            //tmrCheckDeadTables = new System.Windows.Forms.Timer();
            hshActiveTournamentIDs = new Hashtable();
            bPause = false;
            au3 = new AutoItX3Lib.AutoItX3Class();
            if (cmbSite.Text == "ACR")
            {
                LobbyName = "AmericasCardroom Tournament Lobby";
                reLobbyName = "AmericasCardroom Tournament Lobby";
            }
            else if (cmbSite.Text == "BlackChip")
            {
                LobbyName = "BlackChipPoker Tournament Lobby";
                reLobbyName = "BlackChipPoker Tournament Lobby";
            }
            else if (cmbSite.Text == "True Poker")
            {
                LobbyName = "True Poker Tournament Lobby";
                reLobbyName = "True Poker Tournament Lobby";
            }
            else if (cmbSite.Text == "BetOnline")
            {
                LobbyName = "[REGEXPTITLE://ABetOnline Lobby.*Nickname.*//z]";
                reLobbyName = "BetOnline Lobby";
            }
            else if (cmbSite.Text == "Ignition")
            {
                LobbyName = "[REGEXPTITLE://A.*Ignition Casino - Poker Lobby.*//z]";
                reLobbyName = "Ignition Casino - Poker Lobby";
            }
            hLobby = winFunc.GetWindowHandle(reLobbyName);
            ScriptDir = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            scrapeini = ScriptDir + "\\" + scrapeini;
            rgx = new Regex(UserNamePattern);
            gfx.scrapeini = scrapeini;
            gfx.dataentry = 0;
            HotKey.RegisterHotKey(this.Handle, 0, HotKey.KeyModifiers.Shift, Keys.Escape);
            HotKey.RegisterHotKey(this.Handle, 1, HotKey.KeyModifiers.None, Keys.Pause);
            this.FormClosing += MainForm_FormClosing;
            hshHydra = new Hashtable();
            hshTables = new Hashtable();
        }

        public void fConsoleWrite(string Msg)
        {
            string s = Msg;
            string sPathName = System.Reflection.Assembly.GetExecutingAssembly().Location;
            sPathName = sPathName.Substring(0, sPathName.LastIndexOf("\\") + 1);
            string sLogTime;

            string sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";

            string sYear = DateTime.Now.Year.ToString().PadLeft(4, '0');
            string sMonth = DateTime.Now.Month.ToString().PadLeft(2, '0');
            string sDay = DateTime.Now.Day.ToString().PadLeft(2, '0');
            sLogTime = sYear + sMonth + sDay;

            StreamWriter sw = new StreamWriter(sPathName + sLogTime + ".log", true);
            sw.WriteLine(sLogFormat + Msg);
            sw.Flush();
            sw.Close();
            DateTime dtime = DateTime.Now;
            string hour = dtime.Hour.ToString();
            string min = dtime.Minute.ToString();
            string second = dtime.Second.ToString();
            if (SessionLog.Text != "")
                SessionLog.Text = SessionLog.Text + "\n" + hour + ":" + min + ":" + second + " ---> " + Msg;
            else
                SessionLog.Text = hour + ":" + min + ":" + second + " ---> " + Msg;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == HotKey.WM_HOTKEY)
            {
                if (m.WParam.ToInt32() == 0)
                {
                    fConsoleWrite("Exiting - Shift+esc pressed");
                    this.ExitEvent();
                }
                else if (m.WParam.ToInt32() == 1)
                {
                    bPause = !bPause;
                    if (bPause)
                    {
                        //SuspendThreads();
                        bLockThreads = true;
                        fConsoleWrite("Pausing");
                        this.notifyIcon1.Icon = Properties.Resources.pause;
                    }
                    else
                    {
                        //ResumeThreads();
                        bLockThreads = false;
                        fConsoleWrite("UnPausing");
                        this.notifyIcon1.Icon = Properties.Resources.Poker_Chips;
                    }
                    while (bPause)
                        Application.DoEvents();
                }
            }
            base.WndProc(ref m);
        }

        private static void SuspendThreads()
        {
            SuspendThreads(ThreadNames.None);
        }
        private static void SuspendThreads(ThreadNames iExcept)
        {
            if (iExcept != ThreadNames.CheckForDeadTables)
                tmrCheckDeadTables.Enabled = false;
        }

        private static void ResumeThreads()
        {
            ResumeThreads(ThreadNames.None);
        }
        private static void ResumeThreads(ThreadNames iExcept)
        {
            //tmrCheckDeadTables.Enabled = true;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            notifyIcon1.Visible = false;
            HotKey.UnregisterHotKey(this.Handle, a);
            SaveSettings();
            Environment.Exit(1);
            Application.Exit();
        }

        public IntPtr WinGetHandle(string searchTitle)
        {
            return new IntPtr();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Bots Combo Box
            cmbBots.Items.Add("1");
            cmbBots.Items.Add("2");
            cmbBots.Items.Add("3");
            cmbBots.Items.Add("4");
            cmbBots.Items.Add("5");
            cmbBots.Items.Add("6");

            // # of Players Combo Box
            cmbPlayers.Items.Add("2");
            cmbPlayers.Items.Add("6");
            cmbPlayers.Items.Add("7");
            cmbPlayers.Items.Add("9");
            cmbPlayers.Items.Add("15");
            cmbPlayers.Items.Add("270");

            // Type Combo Box
            cmbType.Items.Add("NL");
            cmbType.Items.Add("PL");
            cmbType.Items.Add("FL");

            // Buyin Combo Box
            cmbBuyin.Items.Add("$0+$0");
            cmbBuyin.Items.Add("$0.50+$0.05");
            cmbBuyin.Items.Add("$1+$0.10");
            cmbBuyin.Items.Add("$1.50+$0.11");
            cmbBuyin.Items.Add("$1.50+$0.12");
            cmbBuyin.Items.Add("$1.50+$0.15");
            cmbBuyin.Items.Add("$2+$0.10");
            cmbBuyin.Items.Add("$2+$0.17");
            cmbBuyin.Items.Add("$2.20+$0.16");
            cmbBuyin.Items.Add("$2.40+$0.10");
            cmbBuyin.Items.Add("$3+$0.25");
            cmbBuyin.Items.Add("$3+$0.30");
            cmbBuyin.Items.Add("$5+$0.25");
            cmbBuyin.Items.Add("$5+$0.30");
            cmbBuyin.Items.Add("$5+$0.45");
            cmbBuyin.Items.Add("$5+$0.50");
            cmbBuyin.Items.Add("$5.50+$0.25");
            cmbBuyin.Items.Add("$6+$0.18");
            cmbBuyin.Items.Add("$6+$0.60");
            cmbBuyin.Items.Add("$10+$0.50");
            cmbBuyin.Items.Add("$10+$0.55");
            cmbBuyin.Items.Add("$10+$0.90");
            cmbBuyin.Items.Add("$10+$1");
            cmbBuyin.Items.Add("$11+$0.50");
            cmbBuyin.Items.Add("$12+$0.25");
            cmbBuyin.Items.Add("$15+$0.80");
            cmbBuyin.Items.Add("$15+$1.40");
            cmbBuyin.Items.Add("$15+$1.50");
            cmbBuyin.Items.Add("$20+$1");
            cmbBuyin.Items.Add("$20+$1.75");
            cmbBuyin.Items.Add("$20+$2");
            cmbBuyin.Items.Add("$22+$1");
            cmbBuyin.Items.Add("$24+$1.50");
            cmbBuyin.Items.Add("$30+$1.30");
            cmbBuyin.Items.Add("$30+$1.50");
            cmbBuyin.Items.Add("$30+$2.75");
            cmbBuyin.Items.Add("$30+$3");

            // GameType Combo Box
            cmbGameType.Items.Add("Normal");
            cmbGameType.Items.Add("Turbo");
            cmbGameType.Items.Add("Double or Nothing");
            cmbGameType.Items.Add("Double or Nothing Turbo");
            cmbGameType.Items.Add("Hyper Turbo");
            cmbGameType.Items.Add("On Demand");

            // Poker Site Combo Box
            cmbSite.Items.Add("ACR");
            cmbSite.Items.Add("BlackChip");
            cmbSite.Items.Add("TruePoker");
            cmbSite.Items.Add("BetOnline");
            cmbSite.Items.Add("WilliamHill");
            cmbSite.Items.Add("Ignition");

            txtHydra.Text = "";
            ReadSettings();
        }

        private string SelectExeFile(string initialDirectory)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "executable files (*.exe)|*.exe|All files (*.*)|*.*";
            dialog.InitialDirectory = initialDirectory;
            dialog.Title = "Select an executable file";
            return (dialog.ShowDialog() == DialogResult.OK) ? dialog.FileName : "";
        }

        private string SelectTxtFile(string initialDirectory)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "text files (*.txt)|*.txt|All files (*.*)|*.*";
            dialog.InitialDirectory = initialDirectory;
            dialog.Title = "Select an text file";
            return (dialog.ShowDialog() == DialogResult.OK) ? dialog.FileName : "";
        }

        private string SelectExeFile()
        {
            string path = txtHydra.Text;
            if (path.Length == 0)
                path = "C:\\";
            else
                path = path.Substring(path.LastIndexOf("\\"));
            return SelectExeFile(path);
        }

        private string SelectTxtFile()
        {
            string path = txtProfile.Text;
            if (path.Length == 0)
                path = "C:\\";
            else
                path = path.Substring(path.LastIndexOf("\\"));
            return SelectTxtFile(path);
        }

        private void btnShanky_Click(object sender, EventArgs e)
        {
            string hydraPath = "";
            hydraPath = SelectExeFile();
            if (hydraPath.Length > 0)
                txtHydra.Text = hydraPath;
        }

        private void btnProfile_Click(object sender, EventArgs e)
        {
            string profPath = "";
            profPath = SelectTxtFile();
            if (profPath.Length > 0)
                txtProfile.Text = profPath;
        }

        public void StartNow()
        {
            fConsoleWrite("Starting");
            Hide();
            iPlayers = Convert.ToInt32(cmbPlayers.Text);
            textBot = txtHydra.Text;
            textProf = txtProfile.Text;
            if (cmbSite.Text == "ACR")
            {
                LobbyName = "AmericasCardroom Tournament Lobby";
                reLobbyName = "AmericasCardroom Tournament Lobby";
            }
            else if (cmbSite.Text == "BlackChip")
            {
                LobbyName = "BlackChipPoker Tournament Lobby";
                reLobbyName = "BlackChipPoker Tournament Lobby";
            }
            else if (cmbSite.Text == "True Poker")
            {
                LobbyName = "True Poker Tournament Lobby";
                reLobbyName = "True Poker Tournament Lobby";
            }
            else if (cmbSite.Text == "BetOnline")
            {
                LobbyName = "[REGEXPTITLE://ABetOnline Lobby.*Nickname.*//z]";
                reLobbyName = "BetOnline Lobby";
            }
            else if (cmbSite.Text == "Ignition")
            {
                LobbyName = "[REGEXPTITLE://A.*Ignition Casino - Poker Lobby.*//z]";
                reLobbyName = "Ignition Casino - Poker Lobby";
            }
            else if (cmbSite.Text == "WilliamHill")
            {
                LobbyName = "[REGEXPTITLE://AWilliamHill Poker//z]";
                reLobbyName = "WilliamHill Poker";
            }
            if (textBot == "")
                MessageBox.Show("Please enter Bot Path");
            else if (textProf == "")
                MessageBox.Show("Please enter Profile Path");
            else
            {
                string hLobbyStr = au3.WinGetHandle(reLobbyName);
                hLobby = new IntPtr(Convert.ToInt32(hLobbyStr, 16));
                if (hLobby == IntPtr.Zero)
                {
                    MessageBox.Show("Poker site not running");
                    if (btnStart.Text == "Exit")
                    {
                        btnStart.Text = "Start";
                        btnStart.BackColor = Color.Lime;
                    }
                    Show();
                    fConsoleWrite("Poker site not running: " + LobbyName + " / handle: " + hLobbyStr);
                    return;
                }
                winFunc.SetWindowToForeground(hLobby);
                SetData();
                MoveLobbyWindow();
                StartAutoPilot();
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (btnStart.Text == "Start")
            {
                btnStart.Text = "Exit";
                btnStart.BackColor = Color.Red;
                StartNow();
            }
            else
            {
                ExitEvent();
            }
        }

        public void SaveSettings()
        {
            if (cmbBots.Text != "" && cmbBots.Text != String.Empty)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "cmbBots", cmbBots.Text);
            if (cmbBuyin.Text != "" && cmbBuyin.Text != String.Empty)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "cmbBuyin", cmbBuyin.Text);
            if (cmbGameType.Text != "" && cmbGameType.Text != String.Empty)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "cmbGameType", cmbGameType.Text);
            if (cmbSite.Text != "" && cmbSite.Text != String.Empty)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "cmbSite", cmbSite.Text);
            if (cmbPlayers.Text != "" && cmbPlayers.Text != String.Empty)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "cmbPlayers", cmbPlayers.Text);
            if (cmbType.Text != "" && cmbType.Text != String.Empty)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "cmbType", cmbType.Text);
            if (cmbH.Text != "" && cmbH.Text != String.Empty)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "cmbH", cmbH.Text);
            if (cmbM.Text != "" && cmbM.Text != String.Empty)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "cmbM", cmbM.Text);
            if (cmbMonth.Text != "" && cmbMonth.Text != String.Empty)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "cmbMonth", cmbMonth.Text);
            if (cmbDate.Text != "" && cmbDate.Text != String.Empty)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "cmbDate", cmbDate.Text);
            if (combo1.Text != "" && combo1.Text != String.Empty)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "combo1", combo1.Text);
            if (combo2a.Text != "" && combo2a.Text != String.Empty)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "combo2a", combo2a.Text);
            if (combo2b.Text != "" && combo2b.Text != String.Empty)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "combo2b", combo2b.Text);
            if (combo3.Text != "" && combo3.Text != String.Empty)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "combo3", combo3.Text);
            if (txtPlName.Text != null)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "txtPlName", txtPlName.Text);
            if (txtMaxSngs.Text != null)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "txtMaxSngs", txtMaxSngs.Text);
            if (txtProfile.Text != null)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "txtProfile", txtProfile.Text);
            if (txtHydra.Text != null)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "txtHydra", txtHydra.Text);
            if (txtPosX0.Text != null)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", cmbSite.Text + "x0Txt", txtPosX0.Text);
            if (txtPosX1.Text != null)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", cmbSite.Text + "x1Txt", txtPosX1.Text);
            if (txtPosX2.Text != null)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", cmbSite.Text + "x2Txt", txtPosX2.Text);
            if (txtPosX3.Text != null)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", cmbSite.Text + "x3Txt", txtPosX3.Text);
            if (txtPosX4.Text != null)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", cmbSite.Text + "x4Txt", txtPosX4.Text);
            if (txtPosX5.Text != null)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", cmbSite.Text + "x5Txt", txtPosX5.Text);
            if (txtPosY0.Text != null)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", cmbSite.Text + "y0Txt", txtPosY0.Text);
            if (txtPosY1.Text != null)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", cmbSite.Text + "y1Txt", txtPosY1.Text);
            if (txtPosY2.Text != null)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", cmbSite.Text + "y2Txt", txtPosY2.Text);
            if (txtPosY3.Text != null)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", cmbSite.Text + "y3Txt", txtPosY3.Text);
            if (txtPosY4.Text != null)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", cmbSite.Text + "y4Txt", txtPosY4.Text);
            if (txtPosY5.Text != null)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", cmbSite.Text + "y5Txt", txtPosY5.Text);
            if (txtTourneyPercLess.Text != null)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "txtTourneyPercLess", txtTourneyPercLess.Text);
            if (txtTourneyPercGreat.Text != null)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "txtTourneyPercGreat", txtTourneyPercGreat.Text);
            if (this.Location.X.ToString() != null)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "mainformX", this.Location.X.ToString());
            if (this.Location.Y.ToString() != null)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "mainformY", this.Location.Y.ToString());
            if (radHopperSng.Checked)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "radHopperSng", radHopperSng.Checked.ToString());
            else
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "radHopperSng", "False");
            if (radHopperMTT.Checked)
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "radHopperMTT", radHopperMTT.Checked.ToString());
            else
                FileFunctions.IniWrite(iniLocation + sIniFile, "Settings", "radHopperMTT", "False");
        }
        private void ReadSettings()
        {
            cmbBots.SelectedItem = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "cmbBots", "1");
            cmbBuyin.SelectedItem = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "cmbBuyin", "$1 + $0.10");
            cmbSite.SelectedItem = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "cmbSite", "ACR");
            cmbGameType.SelectedItem = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "cmbGameType", "Normal");
            cmbPlayers.SelectedItem = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "cmbPlayers", "9");
            cmbType.SelectedItem = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "cmbType", "NL");
            cmbH.SelectedItem = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "cmbH", "01");
            cmbM.SelectedItem = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "cmbM", "01");
            cmbMonth.SelectedItem = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "cmbMonth", "01");
            cmbDate.SelectedItem = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "cmbDate", "01");
            combo1.SelectedItem = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "combo1", DateTime.Now.DayOfWeek.ToString());
            combo2a.SelectedItem = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "combo2a", DateTime.Now.Hour.ToString());
            combo2b.SelectedItem = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "combo2b", DateTime.Now.Minute.ToString());
            combo3.SelectedItem = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "combo3", "05");
            txtPlName.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "txtPlName", "");
            txtHydra.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "txtHydra", "");
            txtMaxSngs.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "txtMaxSngs", "5");
            txtProfile.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "txtProfile", "");
            txtHydra.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "txtHydra", "");
            txtPosX0.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", cmbSite.Text + "x0Txt", "0");
            txtPosX1.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", cmbSite.Text + "x1Txt", "0");
            txtPosX2.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", cmbSite.Text + "x2Txt", "0");
            txtPosX3.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", cmbSite.Text + "x3Txt", "0");
            txtPosX4.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", cmbSite.Text + "x4Txt", "0");
            txtPosX5.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", cmbSite.Text + "x5Txt", "0");
            txtPosY0.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", cmbSite.Text + "y0Txt", "0");
            txtPosY1.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", cmbSite.Text + "y1Txt", "0");
            txtPosY2.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", cmbSite.Text + "y2Txt", "0");
            txtPosY3.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", cmbSite.Text + "y3Txt", "0");
            txtPosY4.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", cmbSite.Text + "y4Txt", "0");
            txtPosY5.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", cmbSite.Text + "y5Txt", "0");
            txtTourneyPercLess.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "txtTourneyPercLess", "9");
            txtTourneyPercGreat.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "txtTourneyPercGreat", "1");
            int iTempX = Convert.ToInt16(FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "mainformX", "0"));
            int iTempY = Convert.ToInt16(FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "mainformY", "0"));
            this.Location = new System.Drawing.Point(iTempX, iTempY);
            radHopperSng.Checked = Convert.ToBoolean(FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "radHopperSng", "False"));
            radHopperMTT.Checked = Convert.ToBoolean(FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "radHopperMTT", "False"));
        }

        static string au3Handle(IntPtr ipOrig)
        {
            string sRV = ipOrig.ToString();
            if (sRV.Length > 8)
                sRV = sRV.Substring(sRV.Length - 8);
            sRV = "[HANDLE:" + sRV + "]";
            return sRV;
        }
        static string au3Handle(string sOrig)
        {
            string sRV = sOrig.PadLeft(8, '0');
            sRV = sRV.Substring(sRV.Length - 8);
            sRV = "[HANDLE:" + sRV + "]";
            return sRV;
        }

        public static class StringExtensions
        {
            public static bool IsNullOrWhiteSpace(string value)
            {
                if (value != null)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (!char.IsWhiteSpace(value[i]))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        string GetClassNameOfWindow(IntPtr hwnd)
        {
            string className = "";
            StringBuilder classText = null;
            try
            {
                int cls_max_length = 1000;
                classText = new StringBuilder("", cls_max_length + 5);
                GetClassName(hwnd, classText, cls_max_length + 2);

                if (!String.IsNullOrEmpty(classText.ToString()) && !StringExtensions.IsNullOrWhiteSpace(classText.ToString()))
                    className = classText.ToString();
            }
            catch (Exception ex)
            {
                className = ex.Message;
            }
            finally
            {
                classText = null;
            }
            return className;
        }

        static IntPtr FindWindowByIndex(IntPtr hWndParent, int index)
        {
            if (index == 0)
                return hWndParent;
            else
            {
                int ct = 0;
                IntPtr result = IntPtr.Zero;
                do
                {
                    result = FindWindowEx(hWndParent, result, "VBFloatingPalette", null);
                    if (result != IntPtr.Zero)
                        ++ct;
                }
                while (ct < index && result != IntPtr.Zero);
                return result;
            }
        }

        private void ResizeArray(ref object[,] original, int cols, int rows)
        {
            object[,] newArray = new object[cols, rows];
            Array.Copy(original, newArray, original.Length);
            original = newArray;
        }
        class Product
        {
            int count;
            String title;
            IntPtr check;
            public Product(int a, String b, IntPtr c)
            {
                this.count = a;
                this.title = b;
                this.check = c;
            }
            public String getTitle()
            {
                return this.title;
            }
            public IntPtr getCheck()
            {
                return this.check;
            }
            public int getCount()
            {
                return this.count;
                 }            
        }

        static ArrayList GetAllChildrenWindowHandles(IntPtr hParent, int maxCount)
        {
            ArrayList result = new ArrayList();
            int ct = 0;
            IntPtr prevChild = IntPtr.Zero;
            IntPtr currChild = IntPtr.Zero;
            while (true && ct < maxCount)
            {
                currChild = FindWindowEx(hParent, prevChild, null, null);
                if (currChild == IntPtr.Zero) break;
                result.Add(currChild);
                prevChild = currChild;
                ++ct;
            }
            return result;
        }

        int dayToInt(string day)
        {
            if (day == "Sunday")
                return 0;
            else if (day == "Monday")
                return 1;
            else if (day == "Tuesday")
                return 2;
            else if (day == "Wednesday")
                return 3;
            else if (day == "Thursday")
                return 4;
            else if (day == "Friday")
                return 5;
            else
                return 6;
        }

        public bool ScheduledToRun(bool forceReset = false)
        {            
            DateTime EndTime = DateTime.Now;
            DateTime StartTime = DateTime.Now;
            DateTime datevalue = DateTime.Now;
            bool rv = false;
            int dy = datevalue.Day;
            int mn = datevalue.Month;
            int yy = datevalue.Year;
            int hr = datevalue.Hour;
            int min = datevalue.Minute;
            int minutessince = DateTime.Compare(DateTime.Now, sEndTimeBlock);
            if ((minutessince > 0) || (dy != LastDayUpdated || forceReset))
            {
                if (dy != LastDayUpdated)
                    LastDayUpdated = dy;
                string currdate = hr.ToString() + mn.ToString();
                rv = false;
                String rawschedule = new String(' ', 32000);
                GetPrivateProfileString("schedule", null, null, rawschedule, 32000, iniLocation + sIniFile);
                List<string> stuff = new List<string>(rawschedule.Split('\0'));
                stuff.RemoveRange(stuff.Count - 2, 2);
                foreach (string s in stuff)
                {
                    string[] tvalue = s.Split('~');
                    if (tvalue.Count() > 0)
                    StartTime = Convert.ToDateTime(tvalue[1]);
                    EndTime = StartTime.AddMinutes(Convert.ToInt32(tvalue[2]));
                    bool overnight = false;
                    int minuteCompNowStart = DateTime.Compare(DateTime.Now, StartTime);
                    int minuteCompNowEnd = DateTime.Compare(DateTime.Now, EndTime);
                    if (minuteCompNowStart >= 0 && minuteCompNowEnd <= 0)
                    {
                        string Hour = StartTime.ToString().Substring(StartTime.ToString().IndexOf(" ") + 1);
                        Hour = Hour.Substring(0, 1);
                        DateTime currentDateTime = DateTime.Now;
                        int searchFor = Convert.ToInt32(currentDateTime.DayOfWeek);
                        int currhourInt = Convert.ToInt32(currentDateTime.Hour);
                        int hourInt = Convert.ToInt32(Hour);
                        if ((Convert.ToInt32(currentDateTime.Hour) < Convert.ToInt32(Hour)) && overnight)
                        {
                            searchFor = Convert.ToInt32(currentDateTime.DayOfWeek) - 1;
                            if (searchFor == -1)
                                searchFor = 6;
                        }
                        int monthConvInt = dayToInt(tvalue[0]);
                        if (monthConvInt == searchFor)
                        {
                            sEndTimeBlock = EndTime;
                            rv = true;
                            break;
                        }
                    }
                }
            }
            bScheduledToRun = rv;
            if (bScheduledToRun)
            {
                Message = "Scheduled to run from: " + StartTime.ToString() + " to " + EndTime.ToString();
                if (bScheduleActive)
                {
                    if (lblSchStatus.Text != Message)
                        lblSchStatus.Text = Message;
                }
            }
            else
            {
                if (bScheduleActive && tournamentsStarted < tournamentsRegistered)
                {
                    bScheduledToRun = true;
                    return bScheduledToRun;
                }
                else
                {
                    Message = "IDLE";
                    sEndTimeBlock = DateTime.Now;
                }
            }
            if (Message != "" && Message != LastSchedulerMessage)
            {
                LastSchedulerMessage = Message;
                if (bScheduleActive)
                    fConsoleWrite(Message);
            }
            if (bScheduledToRun)
                if (notifyIcon1.Icon != null)
                    notifyIcon1.Icon = Properties.Resources.greenclock;
            else if (bScheduledToRun == false && bScheduleActive)
            {
                if (notifyIcon1.Icon != null)
                    notifyIcon1.Icon = Properties.Resources.redclock;
            }
            return bScheduledToRun;
        }

        public Object[,] BotWinList()
        {
            au3.Opt("WinTitleMatchMode", 4);
            object[,] winlist = (object[,])au3.WinList("[CLASS:tooltips_class32]");
            object[,] rv = new object[2,2];
            int count = 0;
            rv[0, 0] = count;
            rv[0, 1] = "";
            rv[1, 0] = null;
            rv[1, 1] = IntPtr.Zero;
            if ((int)winlist[0,0] > 0)
            {
                for (int a = 1; a <= (int)winlist[0, 0]; a++)
                {
                    string classlist = au3.WinGetClassList("[HANDLE:" + winlist[1, a] + "]");
                    if (classlist.Contains("VBFloatingPalette"))
                    {
                        count++;
                        ResizeArray(ref rv, 2, count + 2);
                        rv[0, count] = winlist[0, a];                        
                        rv[1, count] = winlist[1, a];                        
                        rv[0, 0] = count;               
                    }
                }                

            }
            return rv;
        }

        public int BotCount()
        {
            object[,] BotList = BotWinList();
            int rv = 0;
            if ((int)BotList[0, 0] > 0)
            {
                for (int a = 1; a <= (int)BotList[0, 0]; a++)
                {
                    if (!BotConnected(BotList[1, a].ToString()) && au3.WinExists("[HANDLE:" + BotList[1, a] + "]") > 0 && "[HANDLE:" + BotList[1, a] + "]" != "")
                    {
                        fConsoleWrite("BC: Closing loose bot");
                        string tempHnd = au3.WinGetHandle("[HANDLE:" + BotList[1, a] + "]");
                        int search = Array.IndexOf(ipShanky, tempHnd);
                        if (search > -1)
                            ipShanky[search] = "";
                        winFunc.WinClose((IntPtr)BotList[1, a]);
                    }

                    else
                        rv += 1;
                }
            }
            if (rv != OldBotList)
                OldBotList = rv;
            return rv;
        }

        public void HideBot(string hWin)
        {
            fConsoleWrite("Hiding Bot");
            //au3.WinActivate("[HANDLE:" + hWin + "]");
            //au3.WinWaitActive("[HANDLE:" + hWin + "]", "", 1);
            au3.WinMenuSelectItem("[HANDLE:" + hWin + "]", "", "&Hide");
            //au3.Send("!hh{ENTER}");
            //Application.DoEvents();
            au3.WinWait("Going Into Hiding", "", 5);
            if (au3.WinExists("Going Into Hiding") == 1)
            {
                au3.ControlFocus("Going Into Hiding", "", "[CLASS:Button; INSTANCE:1]");
                au3.ControlClick("Going Into Hiding", "", "[CLASS:Button; INSTANCE:1]");
                au3.WinWaitClose("Going Into Hiding", "", 1);
                if (au3.WinExists("Going Into Hiding") == 1)
                {
                    au3.ControlFocus("Going Into Hiding", "", "[CLASS:Button; INSTANCE:1]");
                    au3.ControlClick("Going Into Hiding", "", "[CLASS:Button; INSTANCE:1]");
                }
            }
            Application.DoEvents();
        }

        public bool BotConnected(string hWin)
        {
            bool rv = false;
            if (au3.WinExists("[HANDLE:" + hWin + "]") > 0)
            {
                IntPtr newP = new IntPtr(Convert.ToInt32(hWin, 16));
                IntPtr hMain = GetMenu(newP);
                StringBuilder menuName = new StringBuilder(0x20);
                GetMenuString(hMain, 3, menuName, menuName.Capacity, MF_BYPOSITION);
                rv = (winFunc.GUICtrlMenu_GetItemText(hMain, 3) != "S&tart!");
            }
            //Console.WriteLine("RV: " + rv);
            return rv;
        }

        public int nBotsConnected()
        {
            int rv = 0;
            object[,] winlist = null;
            winlist = BotWinList();
            if (winlist != null && (int)winlist[0,0] > 0)
                for (int x = 1; x <= (int)winlist[0, 0]; x++)
                {
                    if (winlist[1, x] == null)
                        continue;
                    rv += BotConnected(winlist[1,x].ToString()) ? 1 : 0;
                }
            return rv;
        }

        public int EmptyTableIndex()
        {
            for (int a = 1; a <= ipTable.Count(); a++) // check the UBound +++
            {
                if (ipTable[a] == "" || au3.WinExists("[HANDLE:" + ipTable[a] + "]") == 0) // ++ {"Input string was not in a correct format."}
                {
                    return a;
                }
            }
            return -10;
        }

        public int NumTables()
        {
            int rv = 0;
            int a;
            object[,] tablelist = null;
            if (radHopperSng.Checked)
                tablelist = TableListSng();
            else
                tablelist = TableListMTT();
            for (a = 1; a <= (int)tablelist[0,0]; a++)
            {
                if (ipTable[a] != "")
                    rv = rv + 1;
            }
            return rv;
        }

        public void PauseScript()
        {
            int x = 1;
            while (x == 1)
                au3.Sleep(1000);
        }

        public bool CheckPlayer(string Players, string Seated = "")
        {
            int plrIndex = Players.IndexOf("/");
            if (plrIndex < 0)
                return false;
            string Players1 = Players.Substring(plrIndex + 1);
            string seated1 = Players.Substring(0, plrIndex);
            if (seated1 == "" || Players == "")
                return false;
            if (seated1 == Players)
                return false;
            return true;
        }

        void CloseLobbies()
        {
            //fConsoleWrite("Checking open tourney lobbies to close");
            //Console.WriteLine("Checking open tourney lobbies to close");
            au3.Opt("WinTitleMatchMode", 4);
            if (cmbSite.Text == "ACR" || cmbSite.Text == "BlackChip" || cmbSite.Text == "True Poker")
                TournamentLobbyTitleHash = "[REGEXPCLASS:\\A.*ATL.*\\z; W:1016; H:739]";
            else if (cmbSite.Text == "BetOnline")
                TournamentLobbyTitleHash = "[REGEXPTITLE:\\A.* #.*-.*Hold'em.*\\z]";
            else if (cmbSite.Text == "Ignition")
                TournamentLobbyTitleHash = "[REGEXPCLASS:\\AChrome_WidgetWin_1\\z]";
            else if (cmbSite.Text == "WilliamHill")
                TournamentLobbyTitleHash = "[REGEXPTITLE:\\A.*(.*)\\z]";
            if (au3.WinExists(TournamentLobbyTitleHash) == 1)
            {
                //fConsoleWrite("Found at least one open tourney lobby");
                //Console.WriteLine("Found at least one open tourney lobby");
                object[,] ttList = (object[,])au3.WinList(TournamentLobbyTitleHash);
                for (int j = 1; j <= (int)ttList[0, 0]; j++)
                {
                    string tlthHwnd = au3.WinGetHandle("[HANDLE:" + ttList[1, j] + "]");
                    IntPtr tlthHwndPtr = new IntPtr(Convert.ToInt32(tlthHwnd, 16));
                    if (WindowsAPI.IsWindowVisible(tlthHwndPtr))
                    {
                        if (cmbSite.Text == "BetOnline")
                        {
                            string titleTT = au3.WinGetTitle("[HANDLE:" + tlthHwnd + "]");
                            if (titleTT.Contains("Level") == false)
                            {
                                au3.WinActivate("[HANDLE:" + tlthHwnd + "]");
                                au3.WinWaitActive("[HANDLE:" + tlthHwnd + "]", "", 1);
                                int tlthPosX = au3.WinGetPosX("[HANDLE:" + tlthHwnd + "]");
                                int tlthPosY = au3.WinGetPosX("[HANDLE:" + tlthHwnd + "]");
                                int colorTT = au3.PixelGetColor(tlthPosX + 14, tlthPosY + 153);
                                if (colorTT == 15198183)
                                {
                                    fConsoleWrite("Closing tournament window");
                                    au3.WinClose("[HANDLE:" + tlthHwnd + "]");
                                }
                            }
                        }
                        else if (cmbSite.Text == "Ignition")
                        {
                            int tournyLobbyWidth = au3.WinGetPosWidth("[HANDLE:" + tlthHwnd + "]");
                            //Console.WriteLine("Tourney lobby width: " + tournyLobbyWidth);
                            int tournyLobbyHeight = au3.WinGetPosHeight("[HANDLE:" + tlthHwnd + "]");
                            //Console.WriteLine("Tourney lobby width: " + tournyLobbyHeight);
                            if (tournyLobbyWidth == 830 && tournyLobbyHeight == 603)
                            {
                                //Console.WriteLine("Closing tournament window");
                                //fConsoleWrite("Closing tournament window");
                                au3.WinClose("[HANDLE:" + tlthHwnd + "]");
                            }
                        }
                        else if (cmbSite.Text == "WilliamHill")
                        {
                            int tournyLobbyWidth = au3.WinGetPosWidth("[HANDLE:" + tlthHwnd + "]");
                            int tournyLobbyHeight = au3.WinGetPosHeight("[HANDLE:" + tlthHwnd + "]");
                            if (tournyLobbyWidth == 1024 && tournyLobbyHeight == 726)
                            {
                                Console.WriteLine("Closing tournament window");
                                fConsoleWrite("Closing tournament window");
                                au3.WinClose("[HANDLE:" + tlthHwnd + "]");
                            }
                        }
                        else if (cmbSite.Text == "ACR" || cmbSite.Text == "BlackChip" || cmbSite.Text == "True Poker")
                        {
                            string tempText = au3.WinGetText("[HANDLE:" + tlthHwnd + "]");
                            string replacement = Regex.Replace(tempText, @"\t|\n|\r", "");
                            //Console.WriteLine("replacement: " + replacement);
                            if (replacement.Contains("poker-game") == false)
                            {
                                Console.WriteLine("Closing tournament window");
                                fConsoleWrite("Closing tournament window");
                                au3.WinClose("[HANDLE:" + tlthHwnd + "]");
                            }
                        }
                    }
                }
            }
        }

        void MoveLobby(string hnd = "", int width = 0, int height = 0)
        {
            if (cmbSite.Text == "ACR" || cmbSite.Text == "BlackChip" || cmbSite.Text == "True Poker")
                au3.WinMove(hnd, "", 0, 0, 1016, 739);
            else if (cmbSite.Text == "BetOnline")
                au3.WinMove(hnd, "", 0, 0, 816, 639);
            else if (cmbSite.Text == "Ignition")
                au3.WinMove(hnd, "", 0, 0, 1030, 757);
            else if (cmbSite.Text == "WilliamHill")
                au3.WinMove(hnd, "", 0, 0, 1024, 726);
        }

        public List<int> GetPositions(string source, string searchString)
        {
            List<int> ret = new List<int>();
            int len = searchString.Length;
            int start = -len;
            while (true)
            {
                start = source.IndexOf(searchString, start + len);
                if (start == -1)
                {
                    break;
                }
                else
                {
                    ret.Add(start);
                }
            }
            return ret;
        }

        public string getTournmentSng()
        {
            var cd = Directory.GetCurrentDirectory();
            String rawtoplay = new String(' ', 32000);
            listToPlaySng.Items.Clear();
            GetPrivateProfileString("Sng", null, null, rawtoplay, 32000, iniLocation + sIniFile);
            List<string> stuff = new List<string>(rawtoplay.Split('\0'));
            stuff.RemoveRange(stuff.Count - 2, 2);
            if (stuff.Count() > 0)
            {
                int j = 0;
                int k = stuff.Count();
                foreach (string s in stuff)
                {
                    string[] tvalue = s.Split('~');
                    if (s == "0")
                    {
                        List<string> tmp = new List<string>(stuff);
                        tmp.RemoveAt(j);
                        stuff = tmp;
                        k = k - 1;
                        j = j - 1;
                    }
                    j += 1;
                }
                var result2 = Enumerable.Range(0, stuff.Count).Where(i => stuff[i].IndexOf(cmbSite.Text) >= 0).ToList();                
                Random random = new Random();
                int maxValue = stuff.Count() - 1;
                if (result2.Count() > 0)
                {
                    int n = random.Next(0, maxValue);
                    return stuff[result2[n]];
                }
                else
                    return "";
            }
            else
                return "";
        }

        public string[] getTourneyValuesSng()
        {
            string[] trv = new string[8];
            string tv;
            tv = getTournmentSng();

            if (tv.IndexOf("~") >= 0)
            {
                trv = tv.Split('~');
            }
            return trv;
        }

        public string[] getTournmentMTT()
        {
            var cd = Directory.GetCurrentDirectory();
            String rawtoplay = new String(' ', 32000);
            listToPlaySng.Items.Clear();
            GetPrivateProfileString("MTT", null, null, rawtoplay, 32000, iniLocation + sIniFile);
            List<string> stuff = new List<string>(rawtoplay.Split('\0'));
            stuff.RemoveRange(stuff.Count - 2, 2);
            string[] tvalueNew = new string[3] { "", "", "" };
            int k = 0;
            if (stuff.Count() > 0)
            {
                int j = 0;
                k = stuff.Count();
                foreach (string s in stuff)
                {
                    string[] tvalue = s.Split('~');
                    if (s == "0")
                    {
                        List<string> tmp = new List<string>(stuff);
                        tmp.RemoveAt(j);
                        stuff = tmp;
                        k = k - 1;
                        j = j - 1;
                    }
                    j += 1;
                }
                j = 0;
                k = stuff.Count();
                foreach (string t in stuff)
                {
                    tvalueNew = t.Split('~');
                    if (tvalueNew.Count() > 0)
                    {
                        string RegTime = tvalueNew[1].Substring(tvalueNew[1].IndexOf(" ") + 1);
                        string AlreadyReg = FileFunctions.IniRead(iniLocation + sIniFile, "registered", RegTime + " " + tvalueNew[3], "0");
                        DateTime RegTime3 = Convert.ToDateTime(RegTime);
                        string RegTime2 = RegTime.Replace(":", "");
                        DateTime date = DateTime.Now;
                        if (date.Subtract(RegTime3).TotalSeconds < 60 && date.Minute == RegTime3.Minute)
                        {
                            int hour = date.Hour;
                            int min = date.Minute;
                            string LocalTime2 = hour.ToString() + "" + min.ToString();
                            if (AlreadyReg != "No" || date.Minute > RegTime3.Minute)
                            {
                                List<string> tmp = new List<string>(stuff);
                                tmp.RemoveAt(j);
                                stuff = tmp;
                                k = k - 1;
                                j = j - 1;
                            }
                        }
                    }
                    else
                        break;
                }
            }
            if (k > 0)
                return tvalueNew;
            else
                return tvalueNew;
        }

        public bool BetOnlineRegisterTourneyMTT()
        {
            fConsoleWrite("Attempting to register for next scheduled tournament");
            tourneyRegOK = false;
            GraphicsFunctions gfx = new GraphicsFunctions();
            string TableName = "";
            string BuyIn = "";
            string hLobby = au3.WinGetHandle("[REGEXPTITLE:\\A.*BetOnline Lobby.*\\z]");
            IntPtr hwLobby = new IntPtr(Convert.ToInt32(hLobby, 16));
            int tempPosX = au3.WinGetPosX(hLobby);
            int tempPosY = au3.WinGetPosY(hLobby);
            string[] trn = getTournmentMTT();
            if (tempPosX != 0 || tempPosY != 0)
                au3.WinMove("[HANDLE:" + hLobby + "]", "", 0, 0);
            au3.WinSetState("[HANDLE:" + hLobby + "]", "", au3.@SW_RESTORE);
            au3.WinActivate("[HANDLE:" + hLobby + "]");
            au3.WinWaitActive("[HANDLE:" + hLobby + "]", "", 2);
            au3.Send("{^HOME}");
            for (int a = 0; a < 19; a++)
            {
                au3.WinActivate("[HANDLE:" + hLobby + "]");
                int textColor = au3.PixelGetColor(12, 274 + (20 * a));
                BuyIn = gfx._ScanString(354, 269 + (a * 20), 427, 280 + (a * 20), textColor, -.01, hwLobby, cmbSite.Text);
                if (BuyIn == "" || BuyIn == String.Empty)
                {
                    //fConsoleWrite("AT: No sng to select on line: " + (a + 1));
                    //Console.WriteLine("Text Color: " + textColor);
                    continue;
                }
                BuyIn = BuyIn.Replace(" ", String.Empty);
                TableName = gfx._ScanString(161, 269 + (a * 20), 277, 280 + (a * 20), textColor, -.01, hwLobby, cmbSite.Text);
                TableName = TableName.Replace(" ", String.Empty);                
                if (TableName == trn[2] && BuyIn == trn[3])
                {
                    fConsoleWrite("Found scheduled tournament " + TableName);
                    au3.WinSetState("[HANDLE:" + hLobby + "]", "", au3.@SW_RESTORE);
                    au3.WinActivate("[HANDLE:" + hLobby + "]");
                    fConsoleWrite("Trying to register now");
                    au3.MouseClick("left", 318, (266 + (20 * a)), 1, 0);
                    au3.Sleep(500);
                    au3.MouseClick("left", 619, 616, 1, 0);
                    au3.Sleep(1500);
                    if (au3.PixelGetColor(513, 439) == 0x4B4B4B)
                    {
                        au3.MouseClick("left", 530, 335, 1, 0);
                        au3.Sleep(500);
                        au3.MouseClick("left", 453, 439, 1, 0);
                        tourneyRegOK = true;
                        fConsoleWrite("Tournament successfully registered");
                        fConsoleWrite("Waiting for tournament to start");
                        //BetOnline~08/16 14:33~$250GTD[Re-E...~$1.10=1
                        deleteToPlayMTTAfterReg(trn[0], trn[1], trn[2], trn[3]);
                        return true;
                    }
                    else
                    {
                        tourneyRegOK = false;
                        TableName = "";
                        return false;
                    }
                }
                else if (trn[3] != BuyIn)
                {
                    fConsoleWrite("Tournament Buyin does not match user criteria");
                    continue;
                }
                else
                    fConsoleWrite("Tournament name does not match user criteria");
            }
            au3.Sleep(1500);
            return false;
        }

        public bool IgnitionRegisterTourneyMTT()
        {
            tourneyRegOK = false;
            thdCheckForNewTables();
            string TableName = "";
            string BuyIn = "";
            string hLobby = au3.WinGetHandle("[REGEXPTITLE:\\A.*Ignition Casino - Poker Lobby.*\\z]");
            IntPtr hwLobby = new IntPtr(Convert.ToInt32(hLobby, 16));
            int tempPosX = au3.WinGetPosX(hLobby);
            int tempPosY = au3.WinGetPosY(hLobby);
            string[] trn = getTournmentMTT();
            if (tempPosX != 0 || tempPosY != 0)
                au3.WinMove("[HANDLE:" + hLobby + "]", "", 0, 0);
            au3.WinSetState("[HANDLE:" + hLobby + "]", "", au3.@SW_RESTORE);
            au3.WinActivate("[HANDLE:" + hLobby + "]");
            au3.WinWaitActive("[HANDLE:" + hLobby + "]", "", 2);
            au3.Send("{^HOME}");
            for (int a = 0; a < 19; a++)
            {
                au3.WinActivate("[HANDLE:" + hLobby + "]");
                int textColor = au3.PixelGetColor(230, 274 + (20 * a));
                BuyIn = gfx._ScanString(749, 269 + (a * 20), 850, 282 + (a * 20), textColor, -150, hwLobby, cmbSite.Text);
                if (BuyIn == "" || BuyIn == String.Empty)
                {
                    fConsoleWrite("AT: No sng to select on line: " + (a + 1));
                    Console.WriteLine("Text Color: " + textColor);
                    continue;
                }
                BuyIn = BuyIn.Replace(" ", String.Empty);
                TableName = gfx._ScanString(383, 269 + (a * 20), 600, 282 + (a * 20), textColor, -150, hwLobby, cmbSite.Text);
                TableName = TableName.Replace(" ", String.Empty);                
                if (TableName == trn[2] && BuyIn == trn[3])
                {
                    fConsoleWrite("Found tournament that matches our criteria: " + TableName);
                    Console.WriteLine("Found tournament that matches our criteria: " + TableName);
                    fConsoleWrite("Attempting to register to tourney");
                    Console.WriteLine("Attempting to register to tourney");
                    au3.WinSetState("[HANDLE:" + hLobby + "]", "", au3.@SW_RESTORE);
                    au3.WinActivate("[HANDLE:" + hLobby + "]");
                    au3.MouseClick("left", 604, (272 + (20 * a)), 2, 0);
                    au3.WinWait("[REGEXPTITLE:\\A#.*\\z; W:830 H:603]", "", 5);
                    if (au3.WinExists("[REGEXPTITLE:\\A#.*\\z; W:830 H:603]") == 1)
                    {
                        au3.Sleep(2000);
                        string tournHnd = au3.WinGetHandle("[REGEXPTITLE:\\A#.*\\z; W:830 H:603]");
                        au3.WinActivate("[HANDLE:" + tournHnd + "]");
                        int tournWindowX = au3.WinGetPosX("[HANDLE:" + tournHnd + "]");
                        int tournWindowY = au3.WinGetPosY("[HANDLE:" + tournHnd + "]");
                        au3.MouseClick("left", tournWindowX + 603, tournWindowY + 538, 1, 0);
                        au3.Sleep(2000);
                        au3.WinActivate("[HANDLE:" + tournHnd + "]");
                        if (au3.PixelGetColor(tournWindowX + 316, tournWindowY + 452) == 0xBC0037)
                        {
                            au3.MouseClick("left", tournWindowX + 316, tournWindowY + 452, 1, 0);
                            au3.WinWait("Poker", "", 5);
                        }
                    }
                    if (au3.WinExists("[TITLE: Poker; W:378 H:195]") == 1)
                    {
                        string succRegHnd = au3.WinGetHandle("[TITLE: Poker; W:378 H:195]");
                        au3.WinActivate("[HANDLE:" + succRegHnd + "]");
                        au3.WinWaitActive("[HANDLE:" + succRegHnd + "]", "", 1);
                        int succRegPosX = au3.WinGetPosX("[HANDLE:" + succRegHnd + "]");
                        int succRegPosY = au3.WinGetPosY("[HANDLE:" + succRegHnd + "]");
                        au3.MouseClick("left", succRegPosX + 305, succRegPosY + 163, 1);
                        tourneyRegOK = true;
                        fConsoleWrite("Successfully registered to tourney");
                        Console.WriteLine("Successfully registered to tourney");
                        au3.WinSetState("[HANDLE:" + hLobby + "]", "", au3.SW_MINIMIZE);
                        return true;
                    }
                    else
                    {
                        tourneyRegOK = false;
                        TableName = "";
                        fConsoleWrite("Tourney Registration failed");
                        Console.WriteLine("Tourney Registration failed");
                        return false;
                    }
                }
                else if (trn[3] != BuyIn)
                {
                    fConsoleWrite("AT: Final Buyins do not match");
                    continue;
                }
                else
                    fConsoleWrite("AT: Player check does not match");
            }
            au3.Sleep(1500);
            return false;
        }

        public bool IgnitionRegisterTourneySng()
        {
            fConsoleWrite("Attempting to find a matching sng to register to");
            Console.WriteLine("Attempting to find a matching sng to register to");
            thdCheckForNewTables();
            tourneyRegOK = false;
            string TableName = "";
            string BuyIn = "";
            string GameType = "";
            string hLobby = au3.WinGetHandle("[REGEXPTITLE:\\A.*Ignition Casino - Poker Lobby.*\\z]");
            IntPtr hwLobby = winFunc.GetWindowHandle("Ignition Casino - Poker Lobby");
            int tempPosX = au3.WinGetPosX(hLobby);
            int tempPosY = au3.WinGetPosY(hLobby);
            string[] trn = getTournmentMTT();
            if (tempPosX != 0 || tempPosY != 0)
                au3.WinMove("[HANDLE:" + hLobby + "]", "", 0, 0);
            au3.WinSetState("[HANDLE:" + hLobby + "]", "", au3.@SW_RESTORE);
            au3.WinActivate("[HANDLE:" + hLobby + "]");
            au3.WinWaitActive("[HANDLE:" + hLobby + "]", "", 2);
            au3.Send("{^HOME}");
            trn = getTourneyValuesSng();
            if (trn.Count() == 0)
            {
                MessageBox.Show("Error", "No entries in the 'to play' list match the current site and is enabled.");
                return false;
            }
            bool entryTrue = false;
            if (trn.Count() == 0)
            {
                au3.Run("toplay.png", Directory.GetCurrentDirectory().ToString());
                au3.Sleep(1000);
                MessageBox.Show("Error", "No entries in the 'to play' list match the current site and is enabled.");
                return false;
            }
            for (int a = 0; a < 19; a++)
            {
                int plrIndex = 0;
                string entry = "";
                string Seated = "";
                string Plrs = "";
                au3.WinActivate("[HANDLE:" + hLobby + "]");
                int alreadyRegOrPlaying = au3.PixelGetColor(242, 274 + (20 * a));
                if (alreadyRegOrPlaying == 0xF7F757)
                {
                    fConsoleWrite("Already playing, checking next line");
                    Console.WriteLine("Already playing, checking next line");
                    continue;
                }
                int textColor = au3.PixelGetColor(233, 275 + (20 * a));
                Console.WriteLine("textColor: " + textColor);
                Plrs = gfx._ScanString(934, 269 + (a * 20), 988, 282 + (a * 20), textColor, -150, hwLobby, cmbSite.Text);
                Console.WriteLine("Plrs: " + Plrs);
                Plrs = Plrs.Replace(" ", String.Empty);
                if (Plrs == "" || Plrs == String.Empty)
                {
                    fConsoleWrite("No sng found on line: " + (a + 1));
                    Console.WriteLine("No sng found on line: " + (a + 1));
                    continue;
                }
                plrIndex = Plrs.IndexOf("/");
                if (plrIndex < 0)
                    continue;
                entry = Plrs.Substring(plrIndex + 1);
                Seated = Plrs.Substring(0, plrIndex);
                if (CheckPlayer(Plrs) == false)
                {
                    Console.WriteLine("Check Player not working plyrs: " + Plrs);
                    continue;
                }
                BuyIn = gfx._ScanString(747, 269 + (a * 20), 854, 282 + (a * 20), textColor, -150, hwLobby, cmbSite.Text);
                if (BuyIn == "" || BuyIn == String.Empty)
                {
                    Console.WriteLine("AT: No sng to select on line: " + (a + 1));
                    continue;
                }
                BuyIn = BuyIn.Replace(" ", String.Empty);
                TableName = gfx._ScanString(383, 269 + (a * 20), 600, 282 + (a * 20), textColor, -.01, hwLobby, cmbSite.Text);
                TableName = TableName.Replace(" ", String.Empty);
                if (TableName.Contains("Turbo") && TableName.Contains("Double-Up"))
                    GameType = "Turbo Double-Up";
                else if (TableName.Contains("Double-Up"))
                    GameType = "Double-Up";
                else if (TableName.Contains("Turbo") && TableName.Contains("Triple-Up"))
                    GameType = "Turbo Triple-Up";
                else if (TableName.Contains("Triple-Up"))
                    GameType = "Triple-Up";
                else if (TableName.Contains("Hyper"))
                    GameType = "Hyper Turbo";
                else if (TableName.Contains("All-In"))
                    GameType = "All-In";
                else if (TableName.Contains("Turbo") && TableName.Contains("Beginner"))
                    GameType = "Turbo Beginner";
                else if (TableName.Contains("Beginner"))
                    GameType = "Beginner";
                else if (TableName.Contains("Turbo"))
                    GameType = "Turbo";
                else
                    GameType = "Normal";
                if (entry.IndexOf(trn[3]) >= 0 && BuyIn.IndexOf(trn[1]) >= 0 && GameType.IndexOf(trn[2]) >= 0)
                {
                    entryTrue = true;
                }
                else
                {
                    continue;
                }
                if (entryTrue == false)
                    continue;
                if (entryTrue == true)
                {
                    fConsoleWrite("Found a sng that matches our criteria: " + TableName);
                    Console.WriteLine("Found a sng that matches our criteria: " + TableName);
                    au3.WinSetState("[HANDLE:" + hLobby + "]", "", au3.@SW_RESTORE);
                    au3.WinActivate("[HANDLE:" + hLobby + "]");
                    au3.MouseClick("left", 604, (272 + (20 * a)), 2, 0);
                    au3.WinWait("[REGEXPTITLE:\\A#.*\\z; W:830 H:603]", "", 5);
                    if (au3.WinExists("[REGEXPTITLE:\\A#.*\\z; W:830 H:603]") == 1)
                    {
                        au3.Sleep(3000);
                        string tournHnd = au3.WinGetHandle("[REGEXPTITLE:\\A#.*\\z; W:830 H:603]");
                        au3.WinActivate("[HANDLE:" + tournHnd + "]");
                        int tournWindowX = au3.WinGetPosX("[HANDLE:" + tournHnd + "]");
                        int tournWindowY = au3.WinGetPosY("[HANDLE:" + tournHnd + "]");
                        au3.MouseClick("left", tournWindowX + 603, tournWindowY + 538, 1, 0);
                        au3.Sleep(3000);
                        au3.WinActivate("[HANDLE:" + tournHnd + "]");
                        if (au3.PixelGetColor(tournWindowX + 316, tournWindowY + 452) == 0xBC0037)
                        {
                            au3.MouseClick("left", tournWindowX + 316, tournWindowY + 452, 1, 0);
                            au3.WinWait("[REGEXPTITLE:\\APoker\\z]", "Chrome Legacy Window", 5);
                        }
                    }
                    if (au3.WinExists("[REGEXPTITLE:\\APoker\\z]", "Chrome Legacy Window") == 1)
                    {
                        string succRegHnd = au3.WinGetHandle("[REGEXPTITLE:\\APoker\\z]", "Chrome Legacy Window");
                        au3.WinActivate("[HANDLE:" + succRegHnd + "]");
                        au3.WinWaitActive("[HANDLE:" + succRegHnd + "]", "", 1);
                        int succRegPosX = au3.WinGetPosX("[HANDLE:" + succRegHnd + "]");
                        int succRegPosY = au3.WinGetPosY("[HANDLE:" + succRegHnd + "]");
                        int succRegHeight = au3.WinGetPosHeight("[HANDLE:" + succRegHnd + "]");
                        int succRegWidth = au3.WinGetPosWidth("[HANDLE:" + succRegHnd + "]");
                        if (succRegHeight <= 200 && succRegHeight >= 175 && succRegWidth >= 350 && succRegWidth <= 390)
                        {
                            au3.MouseClick("left", succRegPosX + 305, succRegPosY + 163, 1, 0);
                            tourneyRegOK = true;
                            fConsoleWrite("Successfully registered to tourney");
                            Console.WriteLine("Successfully registered to tourney");
                            CloseLobbies();
                            au3.WinSetState("[HANDLE:" + hLobby + "]", "", au3.SW_MINIMIZE);
                            return true;
                        }
                        else
                        {
                            fConsoleWrite("Tourney Registration failed");
                            Console.WriteLine("Tourney Registration failed");
                            return false;
                        }
                    }
                    else
                    {
                        tourneyRegOK = false;
                        TableName = "";
                        fConsoleWrite("Tourney Registration failed");
                        Console.WriteLine("Tourney Registration failed");
                        return false;
                    }
                }
                else if (trn[2] != BuyIn)
                {
                    fConsoleWrite("AT: Final Buyins do not match");
                    continue;
                }
                else
                    fConsoleWrite("AT: Player check does not match");
            }
            au3.Sleep(1500);
            return false;
        }


        public bool BetOnlineRegisterTourneySng()
        {
            fConsoleWrite("Attempting to find a matching sng to register to");
            tourneyRegOK = false;
            string TableName = "";
            string BuyIn = "";
            string GameType = "";
            string hLobby = au3.WinGetHandle("[REGEXPTITLE:\\A.*BetOnline Lobby.*\\z]");
            IntPtr hwLobby = winFunc.GetWindowHandle("BetOnline Lobby");
            int tempPosX = au3.WinGetPosX(hLobby);
            int tempPosY = au3.WinGetPosY(hLobby);
            string[] trn = getTournmentMTT();
            if (tempPosX != 0 || tempPosY != 0)
                au3.WinMove("[HANDLE:" + hLobby + "]", "", 0, 0);
            au3.WinSetState("[HANDLE:" + hLobby + "]", "", au3.@SW_RESTORE);
            au3.WinActivate("[HANDLE:" + hLobby + "]");
            au3.WinWaitActive("[HANDLE:" + hLobby + "]", "", 2);
            au3.Send("{^HOME}");
            trn = getTourneyValuesSng();
            if (trn.Count() == 0)
            {
                MessageBox.Show("Error", "No entries in the 'to play' list match the current site and is enabled.");
                return false;
            }
            bool entryTrue = false;
            if (trn.Count() == 0)
            {
                au3.Run("toplay.png", Directory.GetCurrentDirectory().ToString());
                au3.Sleep(1000);
                MessageBox.Show("Error", "No entries in the 'to play' list match the current site and is enabled.");
                return false;
            }
            for (int a = 0; a < 19; a++)
            {
                int plrIndex = 0;
                string entry = "";
                string Seated = "";
                string Plrs = "";
                au3.WinActivate("[HANDLE:" + hLobby + "]");
                int textColor = au3.PixelGetColor(560, 272 + (20 * a));
                Plrs = gfx._ScanString(424, 269 + (a * 20), 465, 280 + (a * 20), textColor, -.01, hwLobby, cmbSite.Text);
                Plrs = Plrs.Replace(" ", String.Empty);
                if (Plrs == "" || Plrs == String.Empty)
                {
                    fConsoleWrite("No sng found on line: " + (a + 1));
                    if (au3.PixelGetColor(548, 423) == 0x4B4B4B)
                    {
                        au3.WinActivate(hLobby);
                        au3.WinWaitActive(hLobby, "", 1);
                        au3.MouseClick("left", 548, 423, 1, 0);
                    }
                    return false;
                }
                plrIndex = Plrs.IndexOf("/");
                if (plrIndex < 0)
                    continue;
                entry = Plrs.Substring(plrIndex+1);
                Seated = Plrs.Substring(0, plrIndex);
                if (CheckPlayer(Plrs) == false)
                {
                    Console.WriteLine("Check Player not working plyrs: " + Plrs);
                    continue;
                }
                BuyIn = gfx._ScanString(333, 269 + (a * 20), 421, 280 + (a * 20), textColor, -.01, hwLobby, cmbSite.Text);
                if (BuyIn == "" || BuyIn == String.Empty)
                {
                    Console.WriteLine("AT: No sng to select on line: " + (a + 1));
                    continue;
                }
                BuyIn = BuyIn.Replace(" ", String.Empty);
                TableName = gfx._ScanString(12, 269 + (a * 20), 234, 280 + (a * 20), textColor, -.01, hwLobby, cmbSite.Text);
                TableName = TableName.Replace(" ", String.Empty);
                if (TableName.Contains("Turbo") && TableName.Contains("Double or Nothing"))
                    GameType = "Turbo Double or Nothing";
                else if (TableName.Contains("Satellite"))
                    GameType = "Satellite";
                else if (TableName.Contains("Turbo"))
                    GameType = "Turbo";
                else if (TableName.Contains("Double or Nothing"))
                    GameType = "Double or Nothing";
                else if (TableName.Contains("Hyper"))
                    GameType = "Hyper";
                else
                    GameType = "Normal";
                if (entry.IndexOf(trn[3]) >= 0 && BuyIn.IndexOf(trn[1]) >= 0 && GameType.IndexOf(trn[2]) >= 0)
                {
                    entryTrue = true;
                }
                else
                {
                    continue;
                }
                if (entryTrue == false)
                    continue;
                if (entryTrue == true)
                {
                    fConsoleWrite("Found a sng that matches our criteria: " + TableName);
                    au3.WinActivate("[HANDLE:" + hLobby + "]");
                    au3.WinWaitActive("[HANDLE:" + hLobby + "]", "", 2);
                    au3.MouseClick("left", 318, (266 + (20 * a)), 1, 0);
                    au3.Sleep(500);
                    au3.MouseClick("left", 697, 616, 1, 0);
                    au3.Sleep(1500);
                    if (au3.PixelGetColor(562, 347) == 0xEE3536)
                    {
                        au3.MouseClick("left", 545, 352, 1, 0);
                        au3.Sleep(500);
                        au3.MouseClick("left", 462, 422, 1, 0);
                        tourneyRegOK = true;
                        fConsoleWrite("Successfully registered and waiting for sng to start!");
                        return true;
                    }
                    else
                    {
                        tourneyRegOK = false;
                        TableName = "";
                        return false;
                    }
                }
                else if (trn[2] != BuyIn)
                {
                    fConsoleWrite("AT: Final Buyins do not match");
                    continue;
                }
                else
                    fConsoleWrite("AT: Player check does not match");
            }
            au3.Sleep(1500);
            return false;
        }        

        public bool ACRRegisterTourneyMTT()
        {
            fConsoleWrite("Starting ACRPoker Register Tourney");
            tourneyRegOK = false;
            IntPtr hLobby = winFunc.GetWindowHandle("AmericasCardroom Tournament Lobby");
            string hLobby1 = au3.WinGetHandle("AmericasCardroom Tournament Lobby");
            if (hLobby == IntPtr.Zero || au3.WinExists("[HANDLE:" + hLobby1 + "]") == 0)
            {
                fConsoleWrite("Lobby not found");
                return false;
            }
            MoveLobby(hLobby1);
            au3.WinSetState("[HANDLE:" + hLobby1 + "]", "", au3.SW_RESTORE);
            int[] off = winFunc.GetWindowPosition(hLobby);
            string ListViewHandle = au3.ControlGetHandle("[HANDLE:" + hLobby1 + "]", "", "IGListCtrl1");
            string countStr = au3.ControlListView("[HANDLE:" + hLobby1 + "]", "", "IGListCtrl1", "GetItemCount", "", "");
            int count = Convert.ToInt32(countStr);
            int noMTTFound = 0;
            string GameListView = "[CLASS:IGListCtrl; INSTANCE:1]";
            string[] trn = getTournmentMTT();
            int offX = au3.WinGetPosX("[HANDLE:" + hLobby1 + "]", "");
            int offY = au3.WinGetPosX("[HANDLE:" + hLobby1 + "]", "");
            for (int Line = 0; Line < count; Line++)
            {
                au3.Sleep(500);
                string Buyin = au3.ControlListView("[HANDLE:" + hLobby1 + "]", "", "IGListCtrl1", "GetText", Line.ToString(), "5");
                Buyin = Buyin.Replace(" ", String.Empty);
                if (Buyin == "" || Buyin == String.Empty)
                {
                    fConsoleWrite("AT: No mtt to select on line: " + (a + 1));
                    noMTTFound += 1;
                    if (noMTTFound >= 3)
                        return false;
                    continue;
                }
                string ID = au3.ControlListView("[HANDLE:" + hLobby1 + "]", "", "IGListCtrl1", "GetText", Line.ToString(), "1");
                ID = ID.Replace(" ", String.Empty);
                string TableScrape = au3.ControlListView("[HANDLE:" + hLobby1 + "]", "", "IGListCtrl1", "GetText", Line.ToString(), "2");
                TableScrape = TableScrape.Replace(" ", String.Empty);
                ID = ID + " " + TableScrape;
                if (ID == trn[2] && Buyin == trn[3])
                {
                    winFunc.SetWindowToForeground(hLobby);
                    au3.WinWaitActive("[HANDLE:" + hLobby1 + "]", "", 1);
                    au3.ControlListView("AmericasCardroom Tournament Lobby", "", GameListView, "Select", Line.ToString(), "");
                    au3.Sleep(2000);
                    au3.WinActivate("[HANDLE:" + hLobby1 + "]");
                    au3.WinWaitActive("[HANDLE:" + hLobby1 + "]", "", 1);
                    au3.MouseClick("left", offX + 857, offY + 525, 1, 0);
                    au3.Sleep(1500);
                    for (int zt = 1; zt < 5; zt++)
                    {
                        object[,] listreg = (object[,])au3.WinList("[REGEXPCLASS:\\A.*ATL:.*\\z]");
                        if ((int)listreg[0, 0] > 0)
                        {
                            for (int a = 1; a <= (int)listreg[0, 0]; a++)
                            {
                                int posregWidth = au3.WinGetPosWidth("[HANDLE:" + listreg[1, a] + "]");
                                int posregHeight = au3.WinGetPosHeight("[HANDLE:" + listreg[1, a] + "]");
                                int posregX = au3.WinGetPosX("[HANDLE:" + listreg[1, a] + "]");
                                int posregY = au3.WinGetPosY("[HANDLE:" + listreg[1, a] + "]");
                                if (posregWidth == 600 && posregHeight == 400)
                                {
                                    au3.WinActivate("[HANDLE:" + listreg[1, a] + "]");
                                    au3.MouseClick("left", posregX + 213, posregY + 197, 1, 0);
                                    break;
                                }
                            }
                            break;
                        }
                        else
                        {
                            if (au3.WinExists("", "You are successfully registered.") == 0)
                            {
                                fConsoleWrite("Could not find Regsiter window");
                                return false;
                            }
                        }
                        au3.Sleep(1000);
                    }
                    string toHwnd = "";
                    int count1 = 0;
                    au3.WinWait("", "You are successfully registered.", 10);
                    int toPosX = 0;
                    int toPosY = 0;
                    if (au3.WinExists("", "You are successfully registered.") == 1)
                    {
                        do
                        {
                            toHwnd = au3.WinGetHandle("", "You are successfully registered.");
                            toPosX = au3.WinGetPosX("HANDLE:" + toHwnd + "]");
                            toPosY = au3.WinGetPosY("HANDLE:" + toHwnd + "]");
                            au3.Sleep(20);
                            count1 += 1;
                        } while (toPosX <= 0 && toPosY <= 0 && count1 < 20);
                        if (toPosX > 0 || toPosY > 0)
                        {
                            fConsoleWrite("ACRREG: Found Successfully Registered window");
                            au3.Sleep(1500);
                            au3.WinActivate("HANDLE:" + toHwnd + "]");
                            au3.WinWaitActive("HANDLE:" + toHwnd + "]", "", 1);
                            au3.ControlClick("AmericasCardroom", "You are successfully registered", "[CLASS:Button; INSTANCE:1]");
                            au3.WinWaitClose("[HANDLE:" + toHwnd + "]", "You are successfully registered", 2);
                            if (au3.WinExists("[HANDLE:" + toHwnd + "]", "") == 1)
                            {
                                au3.WinActivate("HANDLE:" + toHwnd + "]");
                                au3.WinWaitActive("HANDLE:" + toHwnd + "]", "", 1);
                                au3.WinClose("HANDLE:" + toHwnd + "]");
                            }
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("Successfully registered window not found");
                            tourneyRegOK = false;
                            return false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Successfully registered window not found");
                        tourneyRegOK = false;
                        return false;
                    }

                    if (tourneyRegOK == true)
                        return true;
                    else
                    {
                        Console.WriteLine("Successfully registered window not found2");
                        tourneyRegOK = false;
                        return false;
                    }
                }
            }
            return false;
        }


        public bool ACRRegisterTourneySng()
        {
            //au3.AutoItSetOption("WinTitleMatchMode", 4);
            fConsoleWrite("Attempting to find a sng and register to it");
            thdCheckForNewTables();
            //thdCheckDeadTables();
            tourneyRegOK = false;
            CloseLobbies();
            string GameType = "";
            tourneyRegOK = false;
            string LobbyItemCountString = "";
            string TableName = "";
            string BuyIn;
            int Line;

            IntPtr hLobby = IntPtr.Zero;
            string hLobby1 = "";
            string title = "";
            string succregTitle = "";

            if (cmbSite.Text == "ACR")
            {
                title = "AmericasCardroom Tournament Lobby";
                hLobby = winFunc.GetWindowHandle("AmericasCardroom Tournament Lobby");
                hLobby1 = au3.WinGetHandle("AmericasCardroom Tournament Lobby");
                succregTitle = "AmericasCardroom";
            }
            else if (cmbSite.Text == "BlackChip")
            {
                title = "Blackchip Tournament Lobby";
                hLobby = winFunc.GetWindowHandle("Blackchip Tournament Lobby");
                hLobby1 = au3.WinGetHandle("Blackchip Tournament Lobby");
            }
            else
            {
                title = "True Poker Tournament Lobby";
                hLobby = winFunc.GetWindowHandle("True Poker Tournament Lobby");
                hLobby1 = au3.WinGetHandle("True Poker Tournament Lobby");
            }
            winFunc.SetWindowToForeground(hLobby);
            MoveLobby(hLobby1);
            int[] off = winFunc.GetWindowPosition(hLobby);
            string[] trn = getTourneyValuesSng();
            string path = Directory.GetCurrentDirectory();
            if (trn.Count() == 0)
            {
                ShellExecute("toplay.png", "", path + "\\icons", "open");
                au3.Sleep(1000);
                MessageBox.Show("No entries in the 'to play' list match the current site and is enabled.");
                return false;
            }
            string GameListView = "[CLASS:IGListCtrl; INSTANCE:1]";
            string ListViewHandle = au3.ControlGetHandle(hLobby1, "", GameListView);
            LobbyItemCountString = au3.ControlListView(title, "", "[CLASS:IGListCtrl; INSTANCE:1]", "GetItemCount", "", "");
            Console.WriteLine("LobbyItemCountString : " + LobbyItemCountString);
            int LobbyItemCount = Int32.Parse(LobbyItemCountString);
            Console.WriteLine("Count: " + LobbyItemCount);
            int missedsng = 0;
            string Plrs = "";
            string Status = "";
            string entry = "";
            string Seated = "";
            if (LobbyItemCount == 0)
            {
                fConsoleWrite("No sngs visible in lobby...  Refreshing");
                au3.WinActivate("[HANDLE:" + hLobby1 + "]");
                au3.WinWaitActive("[HANDLE:" + hLobby1 + "]", "", 1);
                au3.MouseClick("left", off[0] + 166, off[1] + 208, 1, 0);
                au3.Sleep(500);
                au3.MouseClick("left", off[0] + 63, off[1] + 208, 1, 0);
            }
            decimal regColor = 0;
            if (off.Count() > 0)
            {
                for (Line = 0; Line < LobbyItemCount; Line++)
                {
                    thdCheckForNewTables();
                    winFunc.SetWindowToForeground(hLobby);
                    Thread.Sleep(500);
                    //au3.WinActivate("[HANDLE:" + hLobby1 + "]");
                    regColor = au3.PixelGetColor(off[0] + 823, off[1] + 529);
                    Console.WriteLine("regColor: " + regColor);
                    if (regColor == 0xFFFFFF)
                    {
                        fConsoleWrite("Already registered... checking next line");
                        continue;
                    }
                    Status = au3.ControlListView(title, "", GameListView, "GetText", Line.ToString(), "6");
                    Plrs = au3.ControlListView(title, "", GameListView, "GetText", Line.ToString(), "7");
                    if (Plrs == "")
                    {
                        fConsoleWrite("Incomplete Read - restarting");
                        if (missedsng >= 2)
                            return false;
                        else
                        {
                            missedsng += 1;
                            continue;
                        }
                    }
                    Plrs = Plrs.Replace(" ", String.Empty);
                    if (GameType != "On Demand")
                    {
                        if (Plrs.IndexOf("/") > 0)
                        {
                            entry = Plrs.Substring(Plrs.IndexOf("/") + 1);
                            Seated = Plrs.Substring(0, Plrs.IndexOf("/"));
                        }
                    }
                    TableName = au3.ControlListView(title, "", GameListView, "GetText", Line.ToString(), "2");
                    TableName = TableName.Replace(" ", String.Empty);
                    Console.WriteLine("ACR Reg TableName: " + TableName);
                    if (CheckPlayer(Plrs) == false && TableName.IndexOf("OnDemand") == 0)
                    {
                        fConsoleWrite("Players Not Matching");
                        fConsoleWrite("Players: " + Plrs);
                        continue;
                    }
                    BuyIn = au3.ControlListView(title, "", GameListView, "GetText", Line.ToString(), "5");
                    BuyIn = BuyIn.Replace(" ", String.Empty);
                    Console.WriteLine("ACR Reg BuyIn: " + BuyIn);
                    if (BuyIn.IndexOf("TB") > 0)
                    {
                        Regex.Replace(BuyIn, "TB", "");
                    }
                    if (BuyIn.IndexOf("/") > 0)
                    {
                        string[] buyin1 = BuyIn.Split('/');
                        BuyIn = buyin1[2];
                    }
                    if (BuyIn == "")
                    {
                        fConsoleWrite("AT: No sng to select on line: " + a + 1);
                        continue;
                    }
                    if (TableName.IndexOf("Satellite") > 0)
                        GameType = "Satellite";
                    else if (TableName.IndexOf("OnDemand") > 0)
                        GameType = "On Demand";
                    else if (TableName.IndexOf("FinalTable") > 0)
                        GameType = "Final Table Experience";
                    else if (TableName.IndexOf("Hyper") > 0)
                        GameType = "Hyper Turbo";
                    else if (TableName.IndexOf("Doub") > 0 && TableName.IndexOf("Turbo") > 0)
                        GameType = "Turbo Double or Nothing";
                    else if (TableName.IndexOf("Doub") > 0)
                        GameType = "Double or Nothing";
                    else if (TableName.IndexOf("Turbo") > 0)
                        GameType = "Turbo";
                    else
                        GameType = "Normal";
                    bool entryTrue = false;
                    if (GameType != "On Demand")
                    {
                        if (entry.IndexOf(trn[3]) >= 0 && BuyIn.IndexOf(trn[1]) >= 0 && GameType.IndexOf(trn[2]) >= 0)
                        {
                            entryTrue = true;
                            Console.WriteLine("Entry is true");
                        }
                        else
                        {
                            Console.WriteLine("trn[1]: " + trn[1]);
                            Console.WriteLine("BuyIn: " + BuyIn);
                            Console.WriteLine("trn[2]: " + trn[2]);
                            Console.WriteLine("GameType: " + GameType);
                            Console.WriteLine("trn[3]: " + trn[3]);
                            Console.WriteLine("entry: " + entry);
                            continue;
                        }
                    }
                    else
                    {
                        if (BuyIn.IndexOf(trn[1]) >= 0 && GameType.IndexOf(trn[2]) >= 0)
                        {
                            entryTrue = true;
                            Console.WriteLine("Entry is true");
                        }
                        else
                        {
                            Console.WriteLine("trn[1]: " + trn[1]);
                            Console.WriteLine("BuyIn: " + BuyIn);
                            Console.WriteLine("trn[2]: " + trn[2]);
                            Console.WriteLine("GameType: " + GameType);
                            continue;
                        }
                    }
                    int[] Pos;
                    if (entryTrue == true)
                    {
                        fConsoleWrite("Found a sng that matches our criteria: " + TableName);
                        fConsoleWrite("Attempting to register now");
                        au3.WinActivate("[HANDLE:" + hLobby1 + "]");
                        au3.WinWaitActive("[HANDLE:" + hLobby1 + "]", "", 2);
                        au3.ControlListView(title, "", GameListView, "Select", Line.ToString(), "");
                        Pos = winFunc.GetWindowPosition(hLobby);
                        if (au3.ControlListView(title, "", GameListView, "IsSelected", Line.ToString(), "") == "1")
                        {
                            Console.WriteLine("Line is selected");
                            string text = au3.ControlListView(title, "", GameListView, "GetText", Line.ToString(), "2");
                            text = text.Replace(" ", String.Empty);
                            if (text != TableName)
                                return false;
                        }
                        else
                        {
                            Console.WriteLine("Line not selected");
                            au3.MouseClick("left", 297, (Line * 13) + 282, 1, 0);
                            return false;
                        }
                        Console.WriteLine("PixelGetColor Pos[0] + 864, Pos[1] + 535: " + au3.PixelGetColor(Pos[0] + 864, Pos[1] + 535));
                        if (au3.PixelGetColor(Pos[0] + 864, Pos[1] + 535) == 0)
                        {
                            TableName = "";
                            WaitingForTournament = false;
                            Console.WriteLine("Register button not exist.");
                            return false;                                                       
                        }
                        au3.WinActivate("[HANDLE:" + hLobby1 + "]");
                        au3.WinWaitActive("[HANDLE:" + hLobby1 + "]", "", 2);
                        Console.WriteLine("Found register button, clicking now.");
                        au3.MouseClick("left", Pos[0] + 864, Pos[1] + 535, 1, 0);
                        int toPosX = 0;
                        int toPosY = 0;
                        if (GameType != "On Demand")
                        {
                            au3.WinWait("[REGEXPCLASS:\\A.*ATL:.*\\z; W:600 H:400]", "", 3);
                            if (au3.WinExists("[REGEXPCLASS:\\A.*ATL:.*\\z; W:600 H:400]") == 1)
                            {
                                Console.WriteLine("Found register window.");
                                string regHandle = au3.WinGetHandle("[REGEXPCLASS:\\A.*ATL:.*\\z; W:600 H:400]");
                                au3.WinActivate("[HANDLE:" + regHandle + "]");
                                au3.WinWaitActive("[HANDLE:" + regHandle + "]", "", 1);
                                int regPosX = au3.WinGetPosX("[HANDLE:" + regHandle + "]");
                                int regPosY = au3.WinGetPosY("[HANDLE:" + regHandle + "]");
                                au3.MouseClick("left", regPosX + 211, regPosY + 199, 1, 0);
                            }
                            else
                            {
                                Console.WriteLine("Could not find register window");
                                return false;
                            }
                        }
                        Thread.Sleep(1000);
                        au3.WinWait(succregTitle, "You are successfully registered", 3);
                        object[,] tournlist = null;
                        if (au3.WinExists(succregTitle, "You are successfully registered.") > 0)
                        {
                            tournlist = (object[,])au3.WinList(succregTitle, "You are successfully registered");
                            if (tournlist != null && (int)tournlist[0, 0] > 0)
                            {
                                int totallistInt = (int)tournlist[0, 0];
                                Console.WriteLine("Total List: " + (int)tournlist[0, 0]);
                                string toHwnd = au3.WinGetHandle(succregTitle, "You are successfully registered");
                                toPosX = au3.WinGetPosX("[HANDLE:" + toHwnd + "]");
                                toPosY = au3.WinGetPosY("[HANDLE:" + toHwnd + "]");
                                Console.WriteLine("ACRREG: Found Successfully Registered window");
                                au3.WinActivate("[HANDLE:" + toHwnd + "]");
                                au3.WinWaitActive("[HANDLE:" + toHwnd + "]", "You are successfully registered", 1);
                                //au3.MouseClick("left", toPosX + 155, toPosY + 110, 1, 0);
                                au3.ControlFocus("[HANDLE:" + toHwnd + "]", "", "[CLASS:Button; INSTANCE:1]");
                                au3.ControlClick("[HANDLE:" + toHwnd + "]", "", "[CLASS:Button; INSTANCE:1]");
                                au3.WinWaitClose("[HANDLE:" + toHwnd + "]", "You are successfully registered", 2);
                                if (au3.WinExists("[HANDLE:" + toHwnd + "]") > 0)
                                    Console.WriteLine("Successfully Registered Window still exists, trying to close it.");
                                au3.WinClose("[HANDLE:" + toHwnd + "]");
                                au3.WinWaitClose("[HANDLE:" + toHwnd + "]", "You are successfully registered", 2);
                                if (au3.WinExists("[HANDLE:" + toHwnd + "]") > 0)
                                    Console.WriteLine("Successfully Registered Window still exists, trying to kill it.");
                                au3.WinKill("[HANDLE:" + toHwnd + "]");
                                au3.WinSetState("[HANDLE:" + hLobby1 + "]", "", au3.SW_MINIMIZE);
                                tourneyRegOK = true;
                                fConsoleWrite("Successfully registered to sng");
                                return true;
                            }
                        }                        
                        else
                        {
                            winFunc.SetWindowToForeground(hLobby);
                            if (au3.PixelGetColor(off[0] + 823, off[1] + 529) == 0xFFFFFF)
                            {
                                fConsoleWrite("Successfully registered");
                                Console.WriteLine("Successfully registered");
                                au3.WinSetState("[HANDLE:" + hLobby1 + "]", "", au3.SW_MINIMIZE);
                                tourneyRegOK = true;
                                return true;
                            }
                            fConsoleWrite("Could not successfully register to sng");
                            tourneyRegOK = false;
                            return false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("AT: Player check does not match");
                        return false;
                    }
                }
                if (a >= 26)
                {
                    au3.WinActivate(hLobby1);
                    au3.Sleep(300);
                    au3.MouseClick("left", 380 + off[0], 250 + off[1], 1, 0);
                    au3.Sleep(500);
                    au3.Send("{PGDN 2}");
                }
            }
            return false;
        }

        void ACRHandleLostTables()
        {
            if (au3.WinExists("Player Finished") > 0)
            {
                string hndStr = au3.WinGetHandle("Player Finished");
                int winposX = au3.WinGetPosX("[HANDLE:" + hndStr + "]");
                int winposY = au3.WinGetPosY("[HANDLE:" + hndStr + "]");
                fConsoleWrite("Player Finished Window Found");
                au3.WinActivate("[HANDLE:" + hndStr + "]");
                au3.MouseClick("left", winposX + 222, winposY + 100, 1, 0);
                au3.WinWaitClose("[HANDLE:" + hndStr + "]", "", 2);
                if (au3.WinExists("[HANDLE:" + hndStr + "]") > 0)
                    au3.WinClose("[HANDLE:" + hndStr + "]");
            }
            if (au3.WinExists("Tournament finished.") > 0)
            {
                string hndStr = au3.WinGetHandle("Tournament finished.");
                int winposX = au3.WinGetPosX("[HANDLE:" + hndStr + "]");
                int winposY = au3.WinGetPosY("[HANDLE:" + hndStr + "]");
                fConsoleWrite("Tournament Finished Window Found");
                au3.WinActivate("[HANDLE:" + hndStr + "]");
                au3.MouseClick("left", winposX + 159, winposY + 262, 1, 0);
                au3.WinWaitClose("[HANDLE:" + hndStr + "]", "", 2);
                if (au3.WinExists("[HANDLE:" + hndStr + "]") > 0)
                    au3.WinClose("[HANDLE:" + hndStr + "]");
            }
            if (au3.WinExists("[X:388; Y:108]") > 0)
            {
                string hndStr = au3.WinGetHandle("[X:388; Y:108]");
                fConsoleWrite("Tourney Finished Window Found");
                au3.WinActivate("[HANDLE:" + hndStr + "]");
                au3.WinWaitClose("[HANDLE:" + hndStr + "]", "", 2);
                if (au3.WinExists("[HANDLE:" + hndStr + "]") > 0)
                    au3.WinClose("[HANDLE:" + hndStr + "]");
            }
            if (au3.WinExists("Table removed") > 0)
            {
                string hndStr = au3.WinGetHandle("Table removed");
                fConsoleWrite("Table removed Window Found");
                au3.ControlClick("[HANDLE:" + hndStr + "]", "", "[CLASS:Button; INSTANCE:1]");
                au3.WinWaitClose("[HANDLE:" + hndStr + "]", "", 2);
                if (au3.WinExists("[HANDLE:" + hndStr + "]") > 0)
                    au3.WinClose("[HANDLE:" + hndStr + "]");
            }
        }

        private void ShellExecute(string v1, string v2, string v3, string v4)
        {
            throw new NotImplementedException();
        }

        public void MoveLobbyWindow()
        {
            WindowsAPI.ShowWindowAsync(hLobby, WindowsAPI.SW_RESTORE);
            winFunc.SetWindowToForeground(hLobby);
            if (cmbSite.Text == "ACR" || cmbSite.Text == "BlackChip" || cmbSite.Text == "True Poker")
                winFunc.SetWindowPosition(hLobby, IntPtr.Zero, 0, 0, 1016, 739, 0);
            else if (cmbSite.Text == "BetOnline")
                winFunc.SetWindowPosition(hLobby, IntPtr.Zero, 0, 0, 816, 639, 0);
            else if (cmbSite.Text == "Ignition")
                winFunc.SetWindowPosition(hLobby, IntPtr.Zero, 0, 0, 1030, 757, 0);
            else if (cmbSite.Text == "WilliamHill")
                winFunc.SetWindowPosition(hLobby, IntPtr.Zero, 0, 0, 1024, 726, 0);
        }

        public bool OpenACR()
        {
            if (winFunc.WinExists("AmericasCardroom Lobby Logged in as") == false)
            {
                int i = 0;
                string FThan;
                int iFThan;
                while (winFunc.WinExists("AmericasCardroom Lobby Logged in as") == false)
                {
                    fConsoleWrite("Opening AmericasCardRoom");
                    au3.Run(FTloc + FTexe, FTloc, 1);
                    i = i + 1;
                    au3.WinWait("Login", "", 20);
                }
                if (winFunc.WinExists("AmericasCardroom Lobby Logged in as"))
                {
                    au3.WinActivate("AmericasCardroom Lobby Logged in as", "");
                    FThan = au3.WinGetHandle("AmericasCardroom Lobby Logged in as", "");
                    iFThan = GetForegroundWindow();
                }
                else
                    return false;
                if (winFunc.WinExists("Login"))
                    au3.WinActivate("Login", "");
                else
                    SetForegroundWindow(iFThan);
                au3.Sleep(1000);
                au3.Send("{ENTER}", 0);
            }
            else
            {
                au3.WinActivate("AmericasCardroom Lobby Logged in as", "");
                au3.Sleep(35);
            }
            au3.Sleep(10);
            return true;
        }


        public static void fMouseClick(string button, int x, int y, int clicks, int msDelayUP, int msDelayBetween)
        {
            int oldX = Cursor.Position.X;
            int oldY = Cursor.Position.Y;

            Cursor.Position = new Point(x, y);
            for (int a = 0; a < clicks; a++)
            {
                if (button == "left")
                {
                    mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
                    Thread.Sleep(msDelayUP);
                    mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
                }
                else if (button == "right")
                {
                    mouse_event(MOUSEEVENTF_RIGHTDOWN, x, y, 0, 0);
                    Thread.Sleep(msDelayUP);
                    mouse_event(MOUSEEVENTF_RIGHTUP, x, y, 0, 0);
                }
                else
                {
                    mouse_event(MOUSEEVENTF_MIDDLEDOWN, x, y, 0, 0);
                    Thread.Sleep(msDelayUP);
                    mouse_event(MOUSEEVENTF_MIDDLEUP, x, y, 0, 0);
                }
                Thread.Sleep(msDelayBetween);
            }
            Cursor.Position = new Point(oldX, oldY);
        }
        public void fMouseClick(int x, int y)
        {
            fMouseClick("left", x, y, 1, 0, 0);
        }
        public void fMouseClick(int x, int y, int clicks)
        {
            fMouseClick("left", x, y, clicks, 0, 0);
        }
        public void fMouseClick(string button, int x, int y)
        {
            fMouseClick(button, x, y, 1, 0, 0);
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
                Hide();
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            Hide();
        }
        private void ExitEvent()
        {
            notifyIcon1.Visible = false;
            DialogResult dialogResult = MessageBox.Show("Would you like to save your settings before exiting", "Save Settings", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
                SaveSettings();
            Environment.Exit(1);
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int oldDataEntry = gfx.dataentry;
            gfx.dataentry = 1;
            string scrapeLobbyName = "";
            int numLines = 0;
            int x1 = 0;
            int y1 = 0;
            int x2 = 0;
            int y2 = 0;
            int multiplier = 0;
            int colorx = 0;
            int colory = 0;
            double range = 0;
            int lobbyWidth = 0;
            int lobbyHeight = 0;
            if (cmbSite.Text == "ACR")
            {
                scrapeLobbyName = "AmericasCardroom Tournament Lobby";
            }
            else if (cmbSite.Text == "BlackChip")
            {
                scrapeLobbyName = "BlackChipPoker Tournament Lobby";
            }
            else if (cmbSite.Text == "True Poker")
            {
                scrapeLobbyName = "True Poker Tournament Lobby";
            }
            else if (cmbSite.Text == "BetOnline")
            {
                scrapeLobbyName = "BetOnline Lobby";
                numLines = 16;
                x1 = 11;
                y1 = 269;
                x2 = 556;
                y2 = 280;
                multiplier = 20;
                colorx = 225;
                colory = 272;
                range = -1;
                lobbyWidth = 802;
                lobbyHeight = 568;
            }
            else if (cmbSite.Text == "Ignition")
            {
                scrapeLobbyName = "Ignition Casino - Poker Lobby";
                numLines = 18;
                x1 = 262;
                y1 = 268;
                x2 = 1000;
                y2 = 282;
                multiplier = 20;
                colorx = 231;
                colory = 276;
                range = -150;
                lobbyWidth = 1030;
                lobbyHeight = 757;
            }
            else if (cmbSite.Text == "WilliamHill")
            {
                scrapeLobbyName = "WilliamHill Poker";
                numLines = 18;
                x1 = 38;
                y1 = 277;
                x2 = 680;
                y2 = 291;
                multiplier = 22;
                colorx = 32;
                colory = 282;
                range = -50;
                lobbyWidth = 1024;
                lobbyHeight = 726;
            }
            IntPtr hWin = winFunc.GetWindowHandle(scrapeLobbyName);
            winFunc.SetWindowPosition(hWin, IntPtr.Zero, 0, 0, lobbyWidth, lobbyHeight, 0);
            winFunc.SetWindowToForeground(hWin);
            fConsoleWrite("Starting Update");
            for (int y = 0; y <= numLines; y++)
            {
                int color = au3.PixelGetColor(colorx, colory + (y * multiplier));
                Console.WriteLine(gfx._ScanString(x1, y1 + (y * multiplier), x2, y2 + (y * multiplier), color, range, hWin, cmbSite.Text));
            }
            gfx.dataentry = oldDataEntry;
        }

        private void mnuRestore_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            ExitEvent();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Pause();
        }

        private void Pause()
        {
            {
                bPause = !bPause;
                if (bPause)
                {
                    //SuspendThreads();
                    bLockThreads = true;
                    fConsoleWrite("Pausing");
                    Console.WriteLine("Pausing");
                    this.notifyIcon1.Icon = Properties.Resources.pause;
                    this.mnuPause.Text = "Resume";
                }
                else
                {
                    //ResumeThreads();
                    bLockThreads = false;
                    fConsoleWrite("UnPausing");
                    Console.WriteLine("UnPausing");
                    this.notifyIcon1.Icon = Properties.Resources.Poker_Chips;
                    this.mnuPause.Text = "Pause";
                }
                while (bPause)
                    Application.DoEvents();
            }
        }

        private void btnSet1_Click(object sender, EventArgs e)
        {
            PositionWindow(1);
        }

        private void PositionWindow(int winNum)
        {
            if (winNum == 1)
            {
                if (cmbSite.Text == "ACR" || cmbSite.Text == "BlackChip" || cmbSite.Text == "True Poker")
                    GetDummyPositionACR(ref txtPosX0, ref txtPosY0, 1);
                else if (cmbSite.Text == "BetOnline")
                    GetDummyPositionBO(ref txtPosX0, ref txtPosY0, 1);
                else if (cmbSite.Text == "Ignition")
                    GetDummyPositionIgnition(ref txtPosX0, ref txtPosY0, 1);
            }
            else if (winNum == 2)
            {
                if (cmbSite.Text == "ACR" || cmbSite.Text == "BlackChip" || cmbSite.Text == "True Poker")
                    GetDummyPositionACR(ref txtPosX1, ref txtPosY1, 2);
                else if (cmbSite.Text == "BetOnline")
                    GetDummyPositionBO(ref txtPosX1, ref txtPosY1, 2);
                else if (cmbSite.Text == "Ignition")
                    GetDummyPositionIgnition(ref txtPosX1, ref txtPosY1, 2);
            }
            else if (winNum == 3)
            {
                if (cmbSite.Text == "ACR" || cmbSite.Text == "BlackChip" || cmbSite.Text == "True Poker")
                    GetDummyPositionACR(ref txtPosX2, ref txtPosY2, 3);
                else if (cmbSite.Text == "BetOnline")
                    GetDummyPositionBO(ref txtPosX2, ref txtPosY2, 3);
                else if (cmbSite.Text == "Ignition")
                    GetDummyPositionIgnition(ref txtPosX2, ref txtPosY2, 3);
            }
            else if (winNum == 4)
            {
                if (cmbSite.Text == "ACR" || cmbSite.Text == "BlackChip" || cmbSite.Text == "True Poker")
                    GetDummyPositionACR(ref txtPosX3, ref txtPosY3, 4);
                else if (cmbSite.Text == "BetOnline")
                    GetDummyPositionBO(ref txtPosX3, ref txtPosY3, 4);
                else if (cmbSite.Text == "Ignition")
                    GetDummyPositionIgnition(ref txtPosX3, ref txtPosY3, 4);
            }
            else if (winNum == 5)
            {
                if (cmbSite.Text == "ACR" || cmbSite.Text == "BlackChip" || cmbSite.Text == "True Poker")
                    GetDummyPositionACR(ref txtPosX4, ref txtPosY4, 5);
                else if (cmbSite.Text == "BetOnline")
                    GetDummyPositionBO(ref txtPosX4, ref txtPosY4, 5);
                else if (cmbSite.Text == "Ignition")
                    GetDummyPositionIgnition(ref txtPosX4, ref txtPosY4, 5);
            }
            else
            {
                if (cmbSite.Text == "ACR" || cmbSite.Text == "BlackChip" || cmbSite.Text == "True Poker")
                    GetDummyPositionACR(ref txtPosX5, ref txtPosY5, 6);
                else if (cmbSite.Text == "BetOnline")
                    GetDummyPositionBO(ref txtPosX5, ref txtPosY5, 6);
                else if (cmbSite.Text == "Ignition")
                    GetDummyPositionIgnition(ref txtPosX5, ref txtPosY5, 6);
            }
        }

        private void GetDummyPositionACR(ref TextBox txtX, ref TextBox txtY, int winNum)
        {
            DummyWindow formDummy = new DummyWindow(this);
            formDummy.rvX = Convert.ToInt32(txtX.Text);
            formDummy.rvY = Convert.ToInt32(txtY.Text);
            formDummy.winNum = winNum;
            formDummy.Show();
        }

        private void GetDummyPositionBO(ref TextBox txtX, ref TextBox txtY, int winNum)
        {
            DummyWindowBO formDummy = new DummyWindowBO(this);
            formDummy.rvX = Convert.ToInt32(txtX.Text);
            formDummy.rvY = Convert.ToInt32(txtY.Text);
            formDummy.winNum = winNum;
            formDummy.Show();
        }

        private void GetDummyPositionIgnition(ref TextBox txtX, ref TextBox txtY, int winNum)
        {
            DummyWindowIgnition formDummy = new DummyWindowIgnition(this);
            formDummy.rvX = Convert.ToInt32(txtX.Text);
            formDummy.rvY = Convert.ToInt32(txtY.Text);
            formDummy.winNum = winNum;
            formDummy.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PositionWindow(2);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            PositionWindow(4);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            PositionWindow(3);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            PositionWindow(6);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            PositionWindow(5);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy != true)
            {
                alert = new AlertForm();
                alert.Show();
                backgroundWorker1.RunWorkerAsync();
            }
            SaveSettings();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            for (int a = 1; a <= 6; a++)
                PositionWindow(a);
        }


        private void tmrEvntCheckNewTables(Object myObject, EventArgs myEventArgs)
        {
            thdCheckForNewTables();
            CheckForStrayBots();
        }

        private void tmrEvntCheckDeadTables(Object myObject, EventArgs myEventArgs)
        {
            //thdCheckDeadTables();
            CheckForStrayBots();
        }

        public void AdjustWinPositionsSng()
        {
            int TableNo = 0;
            object[,] winlist = null;
            int size1 = 0;
            int size2 = 0;
            if (cmbSite.Text == "ACR" || cmbSite.Text == "BlackChip" || cmbSite.Text == "True Poker")
            {
                size1 = 733;
                size2 = 553;
            }
            else if (cmbSite.Text == "BetOnline")
            {
                size1 = 643;
                size2 = 483;
            }
            else if (cmbSite.Text == "Ignition")
            {
                size1 = 817;
                size2 = 641;
            }
            if (radHopperSng.Checked)
            {
                if (cmbSite.Text == "ACR" || cmbSite.Text == "BlackChip" || cmbSite.Text == "True Poker")
                    winlist = (object[,])au3.WinList("[REGEXPTITLE:\\A.*Table.*Hold'em.*\\z]");
                else if (cmbSite.Text == "BetOnline")
                    winlist = (object[,])au3.WinList("[REGEXPTITLE:\\A.*#.*Level.*\\z]");
                else if (cmbSite.Text == "Ignition")
                    winlist = (object[,])au3.WinList("[REGEXPTITLE:\\A.*/.*Hold'em.*-.*#.*\\z]");
            }
            else
                winlist = TableListMTT();
            if (winlist != null && (int)winlist[0, 0] > 0)
            {
                for (int a = 1; a <= (int)winlist[0, 0]; a++)
                {
                    TableNo = Array.IndexOf(ipTable, winlist[1, a]);
                    if (TableNo != -1)
                        TableNo = TableNo - 1;
                    //Console.WriteLine("TableNo: " + TableNo);
                    string iniLocation = Directory.GetCurrentDirectory();
                    if (TableNo != -1 && ipTable[TableNo] != "" && ipTable[TableNo] != "0x00000000")
                    {
                        //TableNo = TableNo - 1;
                        string x = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", cmbSite.Text + "x" + TableNo.ToString() + "Txt", "0");
                        string y = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", cmbSite.Text + "y" + TableNo.ToString() + "Txt", "0");
                        int tableposX = au3.WinGetPosX("[HANDLE:" + winlist[1, a] + "]");
                        int tableposY = au3.WinGetPosY("[HANDLE:" + winlist[1, a] + "]");                        
                        if (tableposX != Convert.ToInt32(x) || tableposY != Convert.ToInt32(y))
                        {
                            fConsoleWrite("AWP: Moving table " + winlist[0, a] + " target(" + x + "," + y + ")" + " actual(" + tableposX + "," + tableposY + ")");
                            au3.WinActivate("[HANDLE:" + winlist[1, a] +"]");
                            au3.Sleep(500);
                            au3.WinMove("[HANDLE:" + winlist[1, a] + "]", "", Convert.ToInt32(x), Convert.ToInt32(y), size1, size2);
                        }
                    }
                }
            }
        }

        bool ItIsTime()
        {
            string[] trv = getTournmentMTT();
            int count = 0;
            string line = null;
            string line_to_delete = "";
            if (trv.Count() > 0 && trv[1] != "" && trv[1] != String.Empty)
            {
                line_to_delete = trv[1] + "~" + trv[2] + "~" + trv[3];
                string[] regDate = trv[1].Split(' ');
                string RegTime = regDate[1];
                DateTime RegTime2 = Convert.ToDateTime(RegTime);
                string RegTime3 = trv[1].Substring(trv[1].IndexOf(" ") + 1);
                DateTime RegTime4 = Convert.ToDateTime(RegTime);
                DateTime date = DateTime.Now;
                if ((date.Month == RegTime2.Month) && (date.Day == RegTime2.Day) && (date.Subtract(RegTime4).TotalSeconds < 60 && date.Hour == RegTime4.Hour && date.Minute == RegTime4.Minute))
                {
                    Console.WriteLine("ItIsTime They are equal");
                    return true;
                }
                else if ((date.Minute > RegTime4.Minute) || (date.Month > RegTime2.Month) || ((date.Month == RegTime2.Month) && (date.Day > RegTime2.Day)))
                {
                    Console.WriteLine("TourneyTime < CurrentTime");
                    System.IO.StreamReader file = new System.IO.StreamReader(iniLocation + sIniFile);
                    while ((line = file.ReadLine()) != null)
                    {
                        count += 1;
                        if (line.Contains(line_to_delete))
                        {
                            break;
                        }
                    }
                    file.Close();
                    File_DeleteLine(count, iniLocation + sIniFile);
                    UpdateListviewToPlayMTT();
                    return false;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                lastJoinTourneyMessage = "All finished with registrations.";
                return false;
            }
            return false;
        }

        private void TimerEventProcessor(Object myObject,
                                            EventArgs myEventArgs)
        {
            tmrCheckDeadTables.Stop();
            thdCheckDeadTables();
        }


        public void StartAutoPilot()
        {
            au3.Sleep(500);
            string lastMessage = "";
            System.Windows.Forms.Timer autoPosTimer = new System.Windows.Forms.Timer();            
            tmrCheckDeadTables.Tick += new EventHandler(TimerEventProcessor);

            // Sets the timer interval to 5 seconds.
            tmrCheckDeadTables.Interval = 20000;
            tmrCheckDeadTables.Start();

            autoPosTimer.Start();
            sHydraPath = txtHydra.Text;
            textBot = txtHydra.Text;
            textProf = txtProfile.Text;
            userNumSng = txtMaxSngs.Text;
            while (!Microsoft.VisualBasic.Information.IsNumeric(userNumSng))
            {
                userNumSng = Microsoft.VisualBasic.Interaction.InputBox("Please enter number of SNGs to play", appName, "1", -1, -1);
            }
            txtMaxSngs.Text = userNumSng;
            userNumSngInt = Convert.ToInt32(userNumSng);
            int TournamentsToPlay;
            textProf = txtProfile.Text;
            object[,] botlist = BotWinList();
            if ((int)botlist[0,0] > 0)
            {
                for (int g = 1; g <= (int)botlist[0, 0]; g++)
                 {
                    if (au3.WinExists("[HANDLE:" + botlist[1,g] + "]") == 1)
                    {
                        au3.Send("!{F10}");
                        fConsoleWrite("Closing loose bot");
                        au3.WinClose("[HANDLE:" + botlist[1, g] + "]");
                        au3.Sleep(100);
                    }
                }
            }
            bool bRunOnce = true;
            FoundNewBot = false;
            iLiveBots = 0;
            iTablesFinished = 0;
            iTournamentsRegistered = 0;
            OldiTablesFinished = 0;
            WaitingForTournament = false;
            iBotsToPlay = Convert.ToInt32(cmbBots.Text);
            while (!Microsoft.VisualBasic.Information.IsNumeric(iBotsToPlay))
            {
                iBotsToPLay = Microsoft.VisualBasic.Interaction.InputBox("Please enter number of bots to play at the same time", appName, "1", -1, -1);
            }
            cmbBots.Text = iBotsToPlay.ToString();
            TournamentsToPlay = userNumSngInt;
            ipShanky = new string[7];
            ipTable = new string[7];
            CheckDeadTableAttempts = new int[6];
            for (a = 1; a <= iBotsToPlay; a++)
            {
                ipShanky[a] = "";
                ipTable[a] = "";
                CheckDeadTableAttempts[a] = 0;
            }
            au3.Opt("WinTitleMatchMode", 4);
            if (cmbSite.Text == "ACR" || cmbSite.Text == "BlackChip" || cmbSite.Text == "True Poker")
                TournamentLobbyTitleHash = "[REGEXPCLASS:\\A.*ATL.*\\z; W:1016; H:739]";
            else if (cmbSite.Text == "BetOnline")
                TournamentLobbyTitleHash = "[REGEXPTITLE:\\A.* #.*-.*Hold'em.*\\z]";
            else if (cmbSite.Text == "Ignition")
                TournamentLobbyTitleHash = "[REGEXPCLASS:\\AChrom10.79n_1\\z]"; ;
            if (au3.WinExists(TournamentLobbyTitleHash) == 1)
            {
                //fConsoleWrite("Found at least one open tourney lobby");
                //Console.WriteLine("Found at least one open tourney lobby");
                object[,] ttList = (object[,])au3.WinList(TournamentLobbyTitleHash);
                for (int j = 1; j <= (int)ttList[0, 0]; j++)
                {
                    string tlthHwnd = au3.WinGetHandle("[HANDLE:" + ttList[1, j] + "]");
                    IntPtr tlthHwndPtr = new IntPtr(Convert.ToInt32(tlthHwnd, 16));
                    if (WindowsAPI.IsWindowVisible(tlthHwndPtr))
                    {
                        if (cmbSite.Text == "BetOnline")
                        {
                            string titleTT = au3.WinGetTitle("[HANDLE:" + tlthHwnd + "]");
                            if (titleTT.Contains("Level") == false)
                            {
                                au3.WinActivate("[HANDLE:" + tlthHwnd + "]");
                                au3.WinWaitActive("[HANDLE:" + tlthHwnd + "]", "", 1);
                                int tlthPosX = au3.WinGetPosX("[HANDLE:" + tlthHwnd + "]");
                                int tlthPosY = au3.WinGetPosX("[HANDLE:" + tlthHwnd + "]");
                                int colorTT = au3.PixelGetColor(tlthPosX + 14, tlthPosY + 153);
                                if (colorTT == 15198183)
                                {
                                    fConsoleWrite("Closing tournament window");
                                    au3.WinClose("[HANDLE:" + tlthHwnd + "]");
                                }
                            }
                        }
                        else if (cmbSite.Text == "Ignition")
                        {
                            int tournyLobbyWidth = au3.WinGetPosWidth("[HANDLE:" + tlthHwnd + "]");
                            int tournyLobbyHeight = au3.WinGetPosHeight("[HANDLE:" + tlthHwnd + "]");
                            if (tournyLobbyWidth == 830 && tournyLobbyHeight == 603)
                            {
                                //Console.WriteLine("Closing tournament window");
                                //fConsoleWrite("Closing tournament window");
                                au3.WinClose("[HANDLE:" + tlthHwnd + "]");
                            }
                        }
                        else if (cmbSite.Text == "WilliamHill")
                        {
                            int tournyLobbyWidth = au3.WinGetPosWidth("[HANDLE:" + tlthHwnd + "]");
                            int tournyLobbyHeight = au3.WinGetPosHeight("[HANDLE:" + tlthHwnd + "]");
                            if (tournyLobbyWidth == 1024 && tournyLobbyHeight == 726)
                            {
                                Console.WriteLine("Closing tournament window");
                                fConsoleWrite("Closing tournament window");
                                au3.WinClose("[HANDLE:" + tlthHwnd + "]");
                            }
                        }
                        else if (cmbSite.Text == "ACR" || cmbSite.Text == "BlackChip" || cmbSite.Text == "True Poker")
                        {
                            string tempText = au3.WinGetText("[HANDLE:" + tlthHwnd + "]");
                            string replacement = Regex.Replace(tempText, @"\t|\n|\r", "");
                            //Console.WriteLine("replacement: " + replacement);
                            if (replacement.Contains("poker-game") == false)
                            {
                                Console.WriteLine("Closing tournament window");
                                fConsoleWrite("Closing tournament window");
                                au3.WinClose("[HANDLE:" + tlthHwnd + "]");
                            }
                        }
                    }
                }
            }
            lastMessage = "";

            string sSecsDelay = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "SecondsBetweenTourneyJoin", "90"); // *** OK - let's set it to a string for now. ++ cannot convert string to int
            dtStarted = new DateTime();
            dtStarted = DateTime.Now;
            tsLastTournamentJoin = new DateTime();
            tsLastTournamentJoin = DateTime.Now;
            noTourney = true;
            tmrCheckDeadTables = new System.Windows.Forms.Timer();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();            
            //System.Windows.Forms.Timer tmrRegisterSng = new System.Windows.Forms.Timer();
            //tmrRegisterSng.Start();
            lastJoinTourneyMessage = "";
            /// **** MAIN LOOP

            while (bScheduleActive || (bScheduleActive == false && bRunOnce))
            {
                bool Run = true;
                Application.DoEvents();
                int intMaxSngs = 0;
                string MaxSngs = txtMaxSngs.Text;
                if (MaxSngs != "" && MaxSngs != String.Empty)
                    intMaxSngs = Convert.ToInt32(MaxSngs);
                else
                    intMaxSngs = 2;
                while (Run && (iTablesFinished < intMaxSngs) && ((ScheduledToRun() && bScheduleActive) || (bScheduleActive == false)))
                {
                    Console.Write(".");
                    if (OldiTablesFinished != iTablesFinished)
                    {
                        fConsoleWrite("Tables finished so far: " + iTablesFinished);
                        OldiTablesFinished = iTablesFinished;
                    }
                    //txtSngsElapsed.Text = (iTournamentsRegistered.ToString() + "/" + userNumSng.ToString());

                    object[,] hshWinList = null;
                    object[,] tempBotWinList = BotWinList();
                    for (int l = 1; l <= (int)tempBotWinList[0, 0]; l++)
                    {
                        string tempBotHnd = au3.WinGetHandle("[HANDLE:" + tempBotWinList[1, l] + "]");
                        if (BotConnected(tempBotHnd) == true)
                        {
                            iLiveBots += 1;
                        }
                    }
                    if (radHopperSng.Checked)
                        hshWinList = TableListSng();
                    else
                        hshWinList = TableListMTT();
                    if (nBotsConnected() < Convert.ToInt32(cmbBots.Text))
                    {
                        //Console.WriteLine("iLiveBots < iBotsToPlay");
                        if (iTournamentsRegistered < TournamentsToPlay)
                        {
                            //Console.WriteLine("iTournamentsRegistered < TournamentsToPlay");
                            if (stopWatch.Elapsed.TotalSeconds >= 20 || hshWinList == null || (int)hshWinList[0, 0] == 0)
                            {
                                //Console.WriteLine("stopWatch.Interval >= 15 second");
                                stopWatch.Reset();
                                stopWatch.Start();
                                int intNumTables = Convert.ToInt32(NumTables());
                                int intCmbBots = int.Parse(cmbBots.SelectedItem.ToString());
                                if (intNumTables < intCmbBots)
                                {
                                    Console.WriteLine("intNumTables < intCmbBots");
                                    if (intNumTables <= nBotsConnected())
                                    {
                                        Console.WriteLine("NumTables() <= nBotsConnected()");
                                        if (!WaitingForTournament)
                                        {
                                            Console.WriteLine("!WaitingForTournament");                                            
                                            if (hshWinList == null || ((int)hshWinList[0, 0] <= intNumTables))
                                            {
                                                Console.WriteLine("hshWinList == null || ((int)hshWinList[0, 0] <= NumTables()");
                                                if (bScheduleActive == false || (bScheduleActive && ScheduledToRun()))
                                                {
                                                    Console.WriteLine("bScheduleActive == false || (bScheduleActive && ScheduledToRun()");
                                                    WaitingForTournament = true;
                                                    bLockThreads = true;
                                                    SuspendThreads();
                                                    if (radHopperSng.Checked)
                                                    {
                                                        if (cmbSite.Text == "ACR" || cmbSite.Text == "BlackChip" || cmbSite.Text == "TruePoker")
                                                            ACRRegisterTourneySng();
                                                        else if (cmbSite.Text == "BetOnline")
                                                            BetOnlineRegisterTourneySng();
                                                        else if (cmbSite.Text == "Ignition")
                                                            IgnitionRegisterTourneySng();                                                        
                                                    }
                                                    else
                                                    {
                                                        if (ItIsTime())
                                                        {
                                                            ItsTime = true;
                                                            if (cmbSite.Text == "ACR" || cmbSite.Text == "BlackChip" || cmbSite.Text == "TruePoker")
                                                            {
                                                                if (ACRRegisterTourneyMTT())
                                                                    tourneyRegOK = true;
                                                                else
                                                                    tourneyRegOK = false;
                                                            }
                                                            else if (cmbSite.Text == "BetOnline")
                                                            {
                                                                if (BetOnlineRegisterTourneyMTT())
                                                                    tourneyRegOK = true;
                                                            }
                                                            else if (cmbSite.Text == "Ignition")
                                                            {
                                                                if (IgnitionRegisterTourneyMTT())
                                                                    tourneyRegOK = true;
                                                            }
                                                            else
                                                                tourneyRegOK = false;
                                                        }
                                                        else
                                                        {
                                                            string[] trnTemp = getTournmentMTT();
                                                            ItsTime = false;
                                                            if (trnTemp.Count() > 0)
                                                            {
                                                                if (trnTemp[1] == "" || trnTemp[1] == String.Empty)
                                                                {
                                                                    lastJoinTourneyMessage = "All finished with registrations.";
                                                                    lblScheduledTime.Text = "";
                                                                }
                                                                else
                                                                {
                                                                    lastJoinTourneyMessage = "Not time to register yet.  Will Register at: " + trnTemp[1];
                                                                    lblScheduledTime.Text = "Next scheduled time is: " + trnTemp[1];
                                                                }
                                                            }
                                                        }
                                                    }
                                                    //ResumeThreads();
                                                    bLockThreads = false;
                                                    if (tourneyRegOK)
                                                        iTournamentsRegistered += 1;
                                                    else
                                                        WaitingForTournament = false;
                                                }
                                            }
                                            else
                                            {
                                                lastJoinTourneyMessage = "Not starting new tournament because some tables do not have handles.";
                                                Console.WriteLine("NumTables: " + intNumTables);
                                                Console.WriteLine("(int)hshWinList[0, 0]: " + (int)hshWinList[0, 0]);
                                            }
                                        }
                                        else if (bScheduleActive && ScheduledToRun() == false)
                                        {
                                            lastJoinTourneyMessage = "Not starting new tournament because we are not scheduled to play right now.";
                                            if (NumTables() == 0 && nBotsConnected() == 0 && WaitingForTournament == false)
                                            {
                                                tablesFinished = 0;
                                                tournamentsRegistered = 0;
                                                OldtablesFinished = 0;
                                                for (int a = 1; a <= 6; a++)
                                                {
                                                    tTournaments[a] = "";
                                                    hTournaments[a] = -10;
                                                    ipShanky[a] = "";
                                                    ipTable[a] = "";
                                                }
                                            }
                                            else
                                            {
                                                //ResumeThreads();
                                                bLockThreads = false;
                                                lastJoinTourneyMessage = ("Not starting new tournament because we are waiting for a tournament.");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //ResumeThreads();
                                        bLockThreads = false;
                                        lastJoinTourneyMessage = "Not starting new tournament because we have more tables(" + NumTables() + ") than bots(" + nBotsConnected() + ") connected";
                                        AddBotsToTables();
                                    }
                                }
                                else
                                {
                                    //ResumeThreads();
                                    bLockThreads = false;
                                    lastJoinTourneyMessage = "Not starting new tournament because we have reached the desired simultaneous tables";
                                    AddBotsToTables();
                                }
                            }
                            else
                            {
                                //ResumeThreads();
                                bLockThreads = false;
                                lastJoinTourneyMessage = "Not starting new tournament because we are delaying: " + sSecsDelay + " secs";
                            }
                        }
                        else
                        {
                            //ResumeThreads();
                            bLockThreads = false;
                            lastJoinTourneyMessage = "Not starting new tournament because tournaments registered(" + iTournamentsRegistered + ") >= # to play(" + userNumSng + ")";
                        }
                    }
                    if (lastMessage != lastJoinTourneyMessage)
                    {
                        //Console.WriteLine("livebots wrong");
                        lastMessage = lastJoinTourneyMessage;
                        fConsoleWrite(lastJoinTourneyMessage);
                    }
                    Application.DoEvents();
                    au3.Sleep(500);
                    CloseLobbies();
                    if (lastMessage != lastJoinTourneyMessage)
                    {
                        lastMessage = lastJoinTourneyMessage;
                        fConsoleWrite(lastJoinTourneyMessage);
                    }
                    AdjustWinPositionsSng();
                    UpdateListview();
                    UpdateListviewToPlayMTT();
                    UpdateListviewToPlaySng();
                    //thdCheckDeadTables();
                    thdCheckForNewTables();                    
                    object[,] TableList;
                    if (radHopperSng.Checked)
                        TableList = TableListSng();
                    else
                        TableList = TableListMTT();
                }
            }
            fConsoleWrite("Done");
            this.ExitEvent();
        }

        public void LockThreadsCheck()
        {
            while (bLockThreads)
            {
                Application.DoEvents();
            }
        }

        public void ReadTxtProfile(string hBot)
        {
            string temptxt = "";
            if (!File.Exists(txtTrn))
                temptxt = txtProfile.Text;
            else if (txtTrn == "")
                temptxt = txtProfile.Text;
            else
                temptxt = txtTrn;
            if (File.Exists(temptxt))
            {
                Console.WriteLine("Loading profile: " + temptxt + ")");
                fConsoleWrite("Loading profile: " + temptxt + ")");
                for (int z = 0; z < 2; z++)
                {
                    Console.WriteLine("hBot: " + hBot);
                    IntPtr tempbotHandle = new IntPtr(Convert.ToInt32(hBot, 16));
                    Console.WriteLine("tempbotHandle: " + tempbotHandle);
                    //au3.WinActivate(tempbotHandle);
                    //au3.Sleep(2000);
                    //au3.WinActivate("[HANDLE:" + hBot + "]");
                    //au3.WinWaitActive("[HANDLE:" + hBot + "]", "", 1);
                    winFunc.SetWindowToForeground(tempbotHandle);
                    au3.WinMenuSelectItem("[HANDLE:" + hBot + "]", "", "&Holdem", "&Read Profile...");
                    //au3.Send("!h{ENTER}r", 0);
                    au3.WinWait("Open Profile File", "", 2);
                    au3.WinActivate("Open Profile File");
                    au3.WinWaitActive("Open Profile File", "", 1);
                    //Console.WriteLine("Handle: " + hBot);
                    //au3.WinWait("Open Profile File", "", 2);
                    if (au3.WinExists("Open Profile File") == 1)
                    {
                        //Console.WriteLine("Actually loading the profile now");
                        string opfHandle = au3.WinGetHandle("Open Profile File");
                        au3.WinActivate("[HANDLE:" + opfHandle + "]");
                        au3.WinWaitActive("[HANDLE:" + opfHandle + "]", "", 1);
                        //if (au3.WinGetText("[HANDLE:" + opfHandle + "]") == "")
                        //{
                            //int opfCounter = 0;
                            //while (au3.WinGetText("[HANDLE:" + opfHandle + "]") == "" && opfCounter < 5)
                            //{
                                au3.ControlGetFocus("[HANDLE:" + opfHandle + "]", "[CLASS:Edit; INSTANCE:1]");
                                au3.ControlSetText("[HANDLE:" + opfHandle + "]", "", "[CLASS:Edit; INSTANCE:1]", temptxt);
                                au3.Sleep(500);
                                //opfCounter += 1;
                            //}
                        //}
                        au3.ControlGetFocus("[HANDLE:" + opfHandle + "]", "[CLASS:Button; INSTANCE:1]");
                        au3.ControlClick("[HANDLE:" + opfHandle + "]", "", "[CLASS:Button; INSTANCE:1]");
                        break;
                    }
                    else
                        Console.WriteLine("RP: Open Profile File window not found!");
                    au3.Sleep(200);
                } 
            }
        }

        public void PlayerName()
        {
            string Name = txtPlName.Text;
            string hTempStr = au3.WinGetHandle("[REGEXPTITLE:\\APlayer Name\\z]");
            //IntPtr hTemp = new IntPtr(Convert.ToInt32(hTempStr));
            //if (hTemp != IntPtr.Zero)
            //{
                int counttn = 0;
                while (au3.WinExists("[REGEXPTITLE:\\APlayer Name\\z]") == 1 && counttn < 20)
                {
                    fConsoleWrite("TN: Handling 'Player Name' window = " + Name);
                    au3.ControlFocus("[HANDLE:" + hTempStr + "]", "", "[CLASS:Edit; INSTANCE:1]");
                    au3.ControlSetText("[HANDLE:" + hTempStr + "]", "", "[CLASS:Edit; INSTANCE:1]", Name);
                    au3.Sleep(20);
                    au3.ControlFocus("[HANDLE:" + hTempStr + "]", "", "[CLASS:Button; INSTANCE:1]");
                    au3.ControlClick("[HANDLE:" + hTempStr + "]", "", "[CLASS:Button; INSTANCE:1]");
                    counttn += 1;
                }
            //}
        }

        public void ConfimAP()
        {
            fConsoleWrite("ConfimAP: Start");
            string hTempStr = au3.WinGetHandle("[REGEXPTITLE:\\AConfirm No Auto Play\\z]");
            //IntPtr hTemp = new IntPtr(Convert.ToInt32(hTempStr));
            //if (hTemp != IntPtr.Zero)
            //{
                int counttn = 0;
                while (au3.WinExists("[REGEXPTITLE:\\AConfirm No Auto Play\\z]") == 1 && counttn < 20)
                {
                    fConsoleWrite("CAP: Handling 'Confirm No Auto Play' window");
                    au3.ControlFocus("[HANDLE:" + hTempStr + "]", "", "[CLASS:Button; INSTANCE:2]");
                    au3.ControlClick("[HANDLE:" + hTempStr + "]", "", "[CLASS:Button; INSTANCE:2]", "left", 1);
                    counttn += 1;
                    au3.Sleep(20);
                }
            //}
        }

        public void TableName(string TableNme)
        {
            string hTempStr = au3.WinGetHandle("[REGEXPTITLE:\\ATable Name\\z]");
            Console.WriteLine("TableName: " + TableNme);
            //IntPtr hTemp = new IntPtr(Convert.ToInt32(hTempStr));
            //if (hTemp != IntPtr.Zero)
            //{
                int counttn = 0;
                while (au3.WinExists("[HANDLE:" + hTempStr + "]") == 1 && counttn < 20)
                {
                    fConsoleWrite("TN: Handling 'Table Name' window = " + TableNme);
                    au3.ControlFocus("[HANDLE:" + hTempStr + "]", "", "[CLASS:Edit; INSTANCE:1]");
                    au3.ControlSetText("[HANDLE:" + hTempStr + "]", "", "[CLASS:Edit; INSTANCE:1]", TableNme);
                    au3.Sleep(30);
                    au3.ControlFocus("[HANDLE:" + hTempStr + "]", "", "[CLASS:Button; INSTANCE:1]");
                    au3.ControlClick("[HANDLE:" + hTempStr + "]", "", "[CLASS:Button; INSTANCE:1]");
                    counttn += 1;
                    au3.Sleep(20);
                    au3.WinWait("[REGEXPTITLE:\\APlayer Name\\z]", "", 2);
                    if (au3.WinExists("[REGEXPTITLE:\\APlayer Name\\z]") == 1)
                        PlayerName();
                    if (au3.WinExists("[HANDLE:" + hTempStr + "]") == 0)
                        break;
                }
            //}
        }

        public void CloseTable(string hWin, int a = -1)
        {
            string endTitle;
            //if (a != -1)
            //{
            //    au3.WinClose(ipShanky[a]);
            //    ipShanky[a] = "";
            //}
            
            for (int b = 1; b <= 6; b++)
            {
                if (au3.WinExists("[HANDLE:" + ipTable[b] + "]") == 0)
                {
                    ipTable[b] = "";
                    Console.WriteLine("CT: Table " + b + " reset to empty string");
                }
            }
            if (hWin != "" && hWin != String.Empty && au3.WinExists("[HANDLE:" + hWin + "]") == 1)
            {
                string TableName = "";
                TableName = au3.WinGetTitle("[HANDLE:" + hWin + "]");
                endTitle = hWin;
                int count1 = 0;
                while (au3.WinExists("[HANDLE:" + hWin + "]") == 1 && count1 < 11)
                {
                    fConsoleWrite("CT: Close Table " + TableName);
                    Console.WriteLine("CT: Close Table " + TableName);
                    au3.Sleep(20);
                    au3.WinClose("[HANDLE:" + hWin + "]");
                    au3.WinWaitClose("[HANDLE:" + hWin + "]", "", 1);
                    ACRHandleLostTables();
                    if (a != -1 && ipTable[a] != "")
                    {
                        Console.WriteLine("CT2: Table " + a + " reset to empty string");
                        ipTable[a] = "";
                        Console.WriteLine("Closing bot: " + ipShanky[a]);
                        au3.WinClose("[HANDLE:" + ipShanky[a] + "]");
                        ipShanky[a] = "";
                    }
                    count1 += 1;
                }
                if (au3.WinExists("[HANDLE:" + hWin + "]") == 0)
                {
                    fConsoleWrite(TableName + " is closed");
                    Console.WriteLine(TableName + " is closed");
                    iTablesFinished += 1;
                }
                if (au3.WinExists("[HANDLE:" + hWin + "]") == 0)
                {
                    fConsoleWrite("CT: Subscript title: " + endTitle);
                    Console.WriteLine("CT: Subscript title: " + endTitle);
                    int ArraySearch = Array.IndexOf(ipTable, endTitle);
                    if (ArraySearch > -1)
                    {

                        fConsoleWrite("CT: Clearing from array: " + ipTable[ArraySearch]);
                        Console.WriteLine("CT: Clearing from array: " + ipTable[ArraySearch]);
                        ipTable[ArraySearch] = "";
                        Console.WriteLine("Closing bot: " + ipShanky[ArraySearch]);
                        au3.WinClose("[HANDLE:" + ipShanky[ArraySearch] + "]");
                        ipShanky[ArraySearch] = "";
                    }
                    if (a != -1)
                    {
                        Console.WriteLine("CT3: Table " + a + " reset to 0");
                        ipTable[a] = "";
                    }
                }
                if (au3.WinExists("[HANDLE:" + hWin + "]") == 1)
                    fConsoleWrite("Could not close " + au3.WinGetTitle("[HANDLE:" + hWin + "]"));
            }
        }

        public void thdCheckDeadTables()
        {
            int xpos1 = 0;
            int xpos2 = 0;
            int ypos1 = 0;
            int ypos2 = 0;
            int cdtColor = 0;
            int cdtrange = 0;
            if (cmbSite.Text == "ACR")
            {
                ACRHandleLostTables();
                xpos1 = 16;
                xpos2 = 16;
                ypos1 = 431;
                ypos2 = 432;
                cdtColor = 8026746;
                cdtrange = 2;
            }
            else if (cmbSite.Text == "BlackChip")
            {
                ACRHandleLostTables();
                xpos1 = 16;
                xpos2 = 16;
                ypos1 = 419;
                ypos2 = 419;
                cdtColor = 0x7A7A7A;
                cdtrange = 10;
            }
            else if (cmbSite.Text == "True Poker")
            {
                ACRHandleLostTables();
                xpos1 = 16;
                xpos2 = 16;
                ypos1 = 419;
                ypos2 = 419;
                cdtColor = 0x7A7A7A;
                cdtrange = 10;
            }
            else if (cmbSite.Text == "BetOnline")
            {
                xpos1 = 35;
                xpos2 = 35;
                ypos1 = 357;
                ypos2 = 357;
                cdtColor = 0xFFFFFF;
                cdtrange = 10;
            }
            else if (cmbSite.Text == "Ignition")
            {
                xpos1 = 673;
                xpos2 = 673;
                ypos1 = 488;
                ypos2 = 488;
                cdtColor = 0x9D9D9D;
                cdtrange = 10;
            }
            int error1 = 0;
            int error2 = 0;
            object[,] list = null;
            if (radHopperMTT.Checked)
                list = TableListMTT();
            else
            {
                if (radHopperSng.Checked)
                    list = TableListSng();
                else
                    list = TableListMTT();
            }
            if (list != null)
            {
                for (int a = 1; a <= (int)list[0, 0]; a++)
                {
                    if (ipTable[a] == "")
                        continue;
                    int offsetX = au3.WinGetPosX("[HANDLE:" + ipTable[a] + "]");
                    int offsetY = au3.WinGetPosY("[HANDLE:" + ipTable[a] + "]");
                    IntPtr tempHandleInt = new IntPtr(Convert.ToInt32(ipTable[a], 16));
                    if (au3.WinExists("[HANDLE:" + ipTable[a] + "]") == 1 && ipTable[a] != "")
                    {
                        //au3.WinActivate("[HANDLE:" + ipTable[a] + "]");
                        winFunc.SetWindowToForeground(tempHandleInt);
                        au3.Sleep(500);
                        object cdtArray = au3.PixelSearch(offsetX + xpos1, offsetY + ypos1, offsetX + xpos2, offsetY + ypos2, cdtColor, cdtrange, 1);
                        if (au3.error == 1)
                            error1 = 1;
                        object cdtArray2 = au3.PixelSearch(offsetX + xpos1, offsetY + ypos1, offsetX + xpos2, offsetY + ypos2, cdtColor, cdtrange, 1);
                        if (au3.error == 1)
                            error2 = 1;
                        if (error1 == 1 && error2 == 1)
                        {
                            CheckDeadTableAttempts[a] += 1;
                            if (CheckDeadTableAttempts[a] >= 2)
                            {
                                Console.WriteLine(list[0, a] + " is inactive");
                                fConsoleWrite(list[0, a] + " is inactive");
                                Console.WriteLine("Closing inactive table: " + list[0, a]);
                                fConsoleWrite("Closing inactive table: " + list[0, a]);
                                CloseTable(ipTable[a], a);
                                CheckDeadTableAttempts[a] = 0;
                            }
                            else
                            {
                                FileFunctions.fConsoleWrite("CDT: " + list[0, a] + " looks inactive, count: " + CheckDeadTableAttempts[a]);
                                break;
                            }
                        }
                        else
                            CheckDeadTableAttempts[a] = 0;
                        CheckDeadTableAttempts[a] = 0;
                    }
                }
            }
        }

        void File_DeleteLine(int Line, string Path)
        {
            StringBuilder sb = new StringBuilder();
            using (StreamReader sr = new StreamReader(Path))
            {
                int Countup = 0;
                while (!sr.EndOfStream)
                {
                    Countup++;
                    if (Countup != Line)
                    {
                        using (StringWriter sw = new StringWriter(sb))
                        {
                            sw.WriteLine(sr.ReadLine());
                        }
                    }
                    else
                    {
                        sr.ReadLine();
                    }
                }
            }
            using (StreamWriter sw = new StreamWriter(Path))
            {
                sw.Write(sb.ToString());
            }
        }

        public static string TextFollowing(string searchTxt, string value)
        {
            if (!String.IsNullOrEmpty(searchTxt) && !String.IsNullOrEmpty(value))
            {
                int index = searchTxt.IndexOf(value);
                if (-1 < index)
                {
                    int start = index + value.Length;
                    if (start <= searchTxt.Length)
                    {
                        return searchTxt.Substring(start);
                    }
                }
            }
            return null;
        }

        public void btnAddScheduleClick()
        {
            int di = Array.IndexOf(WDAYS, tabScheduler.Text);
            string newvalue = combo1.Text + "~" + combo2a.Text + ":" + combo2b.Text + "~" + combo3.Text;
            FileFunctions.IniWrite(iniLocation + sIniFile, "Schedule", newvalue, "1");
            UpdateListview();
        }

        public void UpdateListview()
        {
            listSchedule.Items.Clear();
            var cd = Directory.GetCurrentDirectory();
            String rawtoplay = new String(' ', 32000);
            GetPrivateProfileString("Schedule", null, null, rawtoplay, 32000, iniLocation + sIniFile);
            List<string> stuff = new List<string>(rawtoplay.Split('\0'));
            stuff.RemoveRange(stuff.Count - 2, 2);
            int line = 0;
            foreach (string s in stuff)
            {
                ListViewItem item = new ListViewItem();
                string[] tvalue = s.Split('~');
                item.SubItems.Add(tvalue[0]);
                item.SubItems.Add(tvalue[1]);
                item.SubItems.Add(tvalue[2]);
                listSchedule.Items.Add(item);
                line += 1;
            }
        }

        public void UpdateListviewToPlayMTT()
        {
            listToPlayMTT.Items.Clear();
            var cd = Directory.GetCurrentDirectory();
            String rawtoplay = new String(' ', 32000);
            GetPrivateProfileString("MTT", null, null, rawtoplay, 32000, iniLocation + sIniFile);
            List<string> stuff = new List<string>(rawtoplay.Split('\0'));
            stuff.RemoveRange(stuff.Count - 2, 2);
            int line = 0;
            foreach (string s in stuff)
            {
                ListViewItem item = new ListViewItem();
                string[] tvalue = s.Split('~');
                item.SubItems.Add(tvalue[0]);
                item.SubItems.Add(tvalue[1]);
                item.SubItems.Add(tvalue[2]);
                item.SubItems.Add(tvalue[3]);
                listToPlayMTT.Items.Add(item);
                line += 1;
            }
        }

        public void UpdateListviewToPlaySng()
        {
            listToPlaySng.Items.Clear();
            var cd = Directory.GetCurrentDirectory();
            String rawtoplay = new String(' ', 32000);
            listToPlaySng.Items.Clear();
            GetPrivateProfileString("Sng", null, null, rawtoplay, 32000, iniLocation + sIniFile);
            List<string> stuff = new List<string>(rawtoplay.Split('\0'));
            stuff.RemoveRange(stuff.Count - 2, 2);
            int line = 0;
            foreach (string s in stuff)
            {
                ListViewItem item = new ListViewItem();
                string[] tvalue = s.Split('~');
                item.SubItems.Add(tvalue[0]);
                item.SubItems.Add(tvalue[1]);
                item.SubItems.Add(tvalue[2]);
                item.SubItems.Add(tvalue[3]);
                item.SubItems.Add(tvalue[4]);
                item.SubItems.Add(tvalue[5]);
                item.SubItems.Add(tvalue[6]);
                listToPlaySng.Items.Add(item);
                line += 1;
            }
        }

        public void btnDeleteToPlayClickMTT(string subitem1 = "", string subitem2 = "", string subitem3 = "")
        {
            int count = 0;
            string line = null;
            string line_to_delete = "";
            ListViewItem item = listToPlayMTT.SelectedItems.Count > 0 ? listToPlayMTT.SelectedItems[0] : null;
            if (item != null)
            {
                line_to_delete = item.SubItems[1].Text + "~" + item.SubItems[2].Text + "~" + item.SubItems[3].Text + "~" + item.SubItems[4].Text;
            }
            System.IO.StreamReader file = new System.IO.StreamReader(iniLocation + sIniFile);
            while ((line = file.ReadLine()) != null)
            {
                count += 1;
                if (line.Contains(line_to_delete))
                {
                    Console.WriteLine("Found string");
                    break;
                }
            }
            file.Close();
            File_DeleteLine(count, iniLocation + sIniFile);
            UpdateListviewToPlayMTT();
        }

        public void deleteToPlayMTTAfterReg(string subitem1 = "", string subitem2 = "", string subitem3 = "", string subitem4 = "")
        {
            int count = 0;
            string line = null;
            string line_to_delete = "";
            ListViewItem item = listToPlayMTT.SelectedItems.Count > 0 ? listToPlayMTT.SelectedItems[0] : null;
            if (item != null)
            {
                line_to_delete = subitem1 + "~" + subitem2 + "~" + subitem3 + "~" + subitem4;
            }
            System.IO.StreamReader file = new System.IO.StreamReader(iniLocation + sIniFile);
            while ((line = file.ReadLine()) != null)
            {
                count += 1;
                if (line.Contains(line_to_delete))
                {
                    Console.WriteLine("Found string");
                    break;
                }
            }
            file.Close();
            File_DeleteLine(count, iniLocation + sIniFile);
            UpdateListviewToPlayMTT();
        }

        public void btnDeleteToPlayClickSng()
        {
            int count = 0;
            string line = null;
            string line_to_delete = "";
            ListViewItem item = listToPlaySng.SelectedItems.Count > 0 ? listToPlaySng.SelectedItems[0] : null;
            if (item != null)
            {
                line_to_delete = item.SubItems[1].Text + "~" + item.SubItems[2].Text + "~" + item.SubItems[3].Text + "~" + item.SubItems[4].Text + "~" + item.SubItems[5].Text + "~" + item.SubItems[6].Text + "~" + item.SubItems[7].Text;
            }
            System.IO.StreamReader file = new System.IO.StreamReader(iniLocation + sIniFile);
            while ((line = file.ReadLine()) != null)
            {
                count += 1;
                if (line.Contains(line_to_delete))
                {
                    Console.WriteLine("Found string");
                    break;
                }
            }
            file.Close();
            File_DeleteLine(count, iniLocation + sIniFile);
            UpdateListviewToPlaySng();
        }        

        public void btnAddToPlayClickMTT()
        {
            string IDScrape = "";
            string BuyinScrape = "";
            string TableScrape = "";
            string hwLobby = "";
            IntPtr hwLobbyHnd = IntPtr.Zero;
            if (cmbSite.Text == "ACR" || cmbSite.Text == "BlackChip" || cmbSite.Text == "True Poker")
            {
                Console.WriteLine("Adding AmericasCardRoom entries");
                hwLobby = au3.WinGetHandle("[REGEXPTITLE:\\A.*Tournament Lobby.*\\z]");
                //hwLobbyHnd = new IntPtr(Convert.ToInt32(hwLobby, 16));
                if (au3.WinExists("[HANDLE:" + hwLobby + "]") == 0)
                {
                    MessageBox.Show("Poker Site Error", "Please Login to ACR");
                    return;
                }
                int offX = au3.WinGetPosX("[HANDLE:" + hwLobby + "]");
                int offY = au3.WinGetPosY("[HANDLE:" + hwLobby + "]");
                //int[] off = winFunc.GetWindowPosition(hwLobbyHnd);
                string countString = au3.ControlListView("[HANDLE:" + hwLobby + "]", "", "IGListCtrl1", "GetItemCount", "", "");
                int count = Int32.Parse(countString);
                if (offX >= 0 && offY >= 0)
                {
                    Console.WriteLine("Count: " + count);
                    for (int Line = 0; Line <= count - 1; Line++)
                    {
                        string isSelected = au3.ControlListView("[HANDLE:" + hwLobby + "]", "", "IGListCtrl1", "IsSelected", Line.ToString(), "");
                        if (Convert.ToInt32(isSelected) == 1)
                        {
                            BuyinScrape = au3.ControlListView("[HANDLE:" + hwLobby + "]", "", "IGListCtrl1", "GetText", Line.ToString(), "5");
                            BuyinScrape = BuyinScrape.Replace(" ", String.Empty);
                            IDScrape = au3.ControlListView("[HANDLE:" + hwLobby + "]", "", "IGListCtrl1", "GetText", Line.ToString(), "1");
                            IDScrape = IDScrape.Replace(" ", String.Empty);
                            TableScrape = au3.ControlListView("[HANDLE:" + hwLobby + "]", "", "IGListCtrl1", "GetText", Line.ToString(), "2");
                            TableScrape = TableScrape.Replace(" ", String.Empty);
                            IDScrape = IDScrape + " " + TableScrape;
                            break;
                        }
                    }
                }
            }
            else if (cmbSite.Text == "BetOnline")
            {
                Console.WriteLine("Adding BetOnline entries");
                hwLobby = au3.WinGetHandle("[REGEXPTITLE:\\A.*BetOnline Lobby.*\\z]");
                hwLobbyHnd = new IntPtr(Convert.ToInt32(hwLobby, 16));
                au3.WinMove("[HANDLE:" + hwLobby + "]", "", 0, 0);
                au3.Sleep(500);
                if (au3.WinExists("[HANDLE:" + hwLobby + "]") == 0)
                {
                    MessageBox.Show("Poker Site Error", "Please Login to BetOnline");
                    return;
                }
                int[] off = winFunc.GetWindowPosition(hwLobbyHnd);
                for (int a = 0; a < 19; a++)
                {
                    au3.WinActivate("[HANDLE:" + hwLobby + "]");
                    int textColor = au3.PixelGetColor(12, 274 + (20 * a));
                    if (textColor == 15611190)
                    {
                        BuyinScrape = gfx._ScanString(354, 269 + (a * 20), 427, 280 + (a * 20), textColor, -.01, hwLobbyHnd, cmbSite.Text);
                        BuyinScrape = BuyinScrape.Replace(" ", String.Empty);
                        IDScrape = gfx._ScanString(161, 269 + (a * 20), 277, 280 + (a * 20), textColor, -.01, hwLobbyHnd, cmbSite.Text);
                        IDScrape = IDScrape.Replace(" ", String.Empty);
                        break;
                    }
                }
            }
            else if (cmbSite.Text == "Ignition")
            {
                Console.WriteLine("Adding Ignition entries");
                hwLobby = au3.WinGetHandle("[REGEXPTITLE:\\A.*Ignition Casino - Poker Lobby.*\\z]");
                hwLobbyHnd = winFunc.GetWindowHandle("Ignition Casino - Poker Lobby");
                au3.WinMove("[HANDLE:" + hwLobby + "]", "", 0, 0);
                au3.Sleep(500);
                if (au3.WinExists("[HANDLE:" + hwLobby + "]") == 0)
                {
                    MessageBox.Show("Poker Site Error", "Please Login to Ignition");
                    return;
                }
                int[] off = winFunc.GetWindowPosition(hwLobbyHnd);
                for (int a = 0; a < 20; a++)
                {
                    au3.WinActivate("[HANDLE:" + hwLobby + "]");
                    int textColor = au3.PixelGetColor(245, 274 + (20 * a));
                    Console.WriteLine("Line: " + a + " --- color: " + textColor);
                    if (textColor == 0x9F2626)
                    {
                        if (a == 0)
                            au3.MouseClick("left", 263, 269 + ((a + 1) * 20), 1, 0);
                        else
                            au3.MouseClick("left", 263, 269 + ((a - 1) * 20), 1, 0);
                        Thread.Sleep(300);
                        BuyinScrape = gfx._ScanString(747, 269 + (a * 20), 854, 282 + (a * 20), textColor, -150, hwLobbyHnd, cmbSite.Text);
                        BuyinScrape = BuyinScrape.Replace(" ", String.Empty);
                        Console.WriteLine("BuyinScrape: " + BuyinScrape);
                        IDScrape = gfx._ScanString(263, 269 + (a * 20), 380, 282 + (a * 20), textColor, -150, hwLobbyHnd, cmbSite.Text);
                        IDScrape = IDScrape.Replace(" ", String.Empty);
                        TableScrape = gfx._ScanString(383, 269 + (a * 20), 600, 282 + (a * 20), textColor, -150, hwLobbyHnd, cmbSite.Text);
                        TableScrape = TableScrape.Replace(" ", String.Empty);
                        IDScrape = IDScrape + " " + TableScrape;
                        break;
                    }
                }
            }
            string newvalue = cmbSite.Text + "~" + cmbMonth.Text + "/" + cmbDate.Text + " " + cmbH.Text + ":" + cmbM.Text + "~" + IDScrape + "~" + BuyinScrape;
            FileFunctions.IniWrite(iniLocation + sIniFile, "MTT", newvalue, "1");
            UpdateListviewToPlayMTT();
        }

        public void btnAddToPlayClickSng()
        {
            string aItems = cmbBuyin.Text;
            string newvalue = cmbSite.Text + "~" + aItems + "~" + cmbGameType.Text + "~" + cmbPlayers.Text + "~" + cmbType.Text + "~" + txtTourneyPercLess.Text + "~" + txtTourneyPercGreat.Text;
            FileFunctions.IniWrite(iniLocation + sIniFile, "Sng", newvalue, "1");
            UpdateListviewToPlaySng();
        }

        private void btnAddToPlayMTT_Click(object sender, EventArgs e)
        {
            btnAddToPlayClickMTT();
        }

        private void btnDeleteToPlayMTT_Click(object sender, EventArgs e)
        {
            btnDeleteToPlayClickMTT();
        }

        private void listToPlayMTT_Click(object sender, EventArgs e)
        {
            var firstSelectedItem = listToPlayMTT.SelectedItems[0];
        }

        private void btnAddToPlaySng_Click(object sender, EventArgs e)
        {
            btnAddToPlayClickSng();
        }

        private void btnDeleteToPlaySng_Click(object sender, EventArgs e)
        {
            btnDeleteToPlayClickSng();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            for (int i = 1; i <= 10; i++)
            {
                {                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
                    worker.ReportProgress(i * 10);
                    System.Threading.Thread.Sleep(100);
                }
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            alert.Message = "Saving in progess";
            alert.ProgressValue = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            alert.Close();
        }

        public void AddBotsToTables()
        {
            object[,] botwinlist = null;
            botwinlist = BotWinList();
            bool ShankyExists = false;
            int hactiveShanky = 1;
            int hActive = 1;
            object[,] tablelist = null;
            Process myProcess = new Process();
            if (radHopperSng.Checked)
                tablelist = TableListSng();
            else
                tablelist = TableListMTT();
            if (tablelist != null)
            {
                for (int y = 1; y <= (int)tablelist[0, 0]; y++)
                {
                    if (ipTable[y] != "" && ipTable[y] != String.Empty && ((ipShanky[y] == "" || ipShanky[y] == String.Empty) || (BotConnected(ipShanky[y]) == false && (ipShanky[y] != "" && ipShanky[y] != String.Empty))))
                    {
                        fConsoleWrite("Found table *" + au3.WinGetTitle("[HANDLE:" + ipTable[y] + "]") + "* without bot.  Attempting to connect a bot to table");
                        hActive = y;
                        string tableNme = au3.WinGetTitle("[HANDLE:" + ipTable[y] + "]");
                        object[,] tempbotwinlist = null;
                        tempbotwinlist = BotWinList();
                        for (int c = 1; c <= (int)tempbotwinlist[0,0]; c++)
                        {
                            if (ipShanky[c] != "")
                            {
                                ShankyExists = true;
                                hactiveShanky = c;
                                break;
                            }
                        }
                        if (ShankyExists == false)
                        {
                            fConsoleWrite("Launching shanky poker bot from: " + txtHydra.Text);
                            Console.WriteLine("Launching shanky poker bot: " + hActive);
                            Process procRun = new Process();
                            procRun.StartInfo.FileName = txtHydra.Text;
                            procRun.StartInfo.WorkingDirectory = txtHydra.Text.Substring(0, txtHydra.Text.LastIndexOf("\\") + 1);
                            procRun.Start();  //Theoretically we can watch the process to see if it exited/etc.

                            //ProcessStartInfo info = new ProcessStartInfo(txtHydra.Text);
                            //info.UseShellExecute = true;
                            //info.Verb = "runas";
                            //Process.Start(info);
                            au3.WinWait(bottitle, "", 10);
                            for (int z = 0; z < 20; z++)
                            {
                                tempbotwinlist = null;
                                tempbotwinlist = BotWinList();
                                if (tempbotwinlist != null && (int)tempbotwinlist[0, 0] > 0 || z >= 20)
                                {
                                    break;
                                }
                                au3.Sleep(500);
                            }
                            if (tempbotwinlist != null && (int)tempbotwinlist[0, 0] > 0)
                            {
                                string tempHnd = au3.WinGetHandle("[HANDLE:" + tempbotwinlist[1, 1] + "]");
                                ipShanky[hActive] = tempHnd;
                                au3.Sleep(500);
                            }
                        }
                        else
                        {
                            botwinlist = BotWinList();
                            FileFunctions.fConsoleWrite("ABTT: New window from bot " + ipShanky[hactiveShanky]);
                            int Temp = (int)botwinlist[0, 0];
                            //au3.WinActivate("[HANDLE:" + ipShanky[hactiveShanky] + "]");
                            //au3.WinWaitActive("[HANDLE:" + ipShanky[hactiveShanky] + "]", "", 1);
                            au3.WinMenuSelectItem("[HANDLE:" + ipShanky[hactiveShanky] + "]", "", "&Holdem", "&New Window");
                            //au3.Send("!h{ENTER}n");
                            for (int z = 0; z < 20; z++)
                            {
                                botwinlist = BotWinList();
                                if ((int)botwinlist[0, 0] > 0 || z >= 20)
                                    break;
                                au3.Sleep(500);
                            }
                            au3.Sleep(1000);
                            for (int b = 1; b <= (int)botwinlist[0, 0]; b++)
                            {
                                string tempHnd = au3.WinGetHandle("[HANDLE:" + botwinlist[1, b] + "]");
                                Console.WriteLine("tempHnd: " + tempHnd);
                                Console.WriteLine("index: " + b);
                                if (Array.IndexOf(ipShanky, tempHnd) == -1 && tempHnd != "0x00000000")
                                {
                                    FileFunctions.fConsoleWrite("ABTT: Spotted new bot " + hActive);
                                    ipShanky[hActive] = tempHnd;
                                    break;
                                }
                                else
                                {
                                    
                                }
                            }
                        }

                        if (txtProfile.Text != "")
                        {
                            Console.WriteLine("ipShanky[hActive]: " + ipShanky[hActive]);
                            Console.WriteLine("hActive: " + hActive);
                            ReadTxtProfile(ipShanky[hActive]);
                            fConsoleWrite("Profile successfully loaded");
                        }
                        au3.Sleep(200);
                        //au3.WinActivate("[HANDLE:" + ipShanky[hActive] + "]");
                        //au3.WinWaitActive("[HANDLE:" + ipShanky[hActive] + "]", "", 1);
                        au3.WinMenuSelectItem("[HANDLE:" + ipShanky[hActive] + "]", "", "S&tart!");
                        //au3.Send("!t");
                        au3.WinWait("Player Name", "", 2);
                        if (au3.WinExists("Player Name") == 1)
                            PlayerName();
                        if (au3.WinExists("Confirm No Auto Play") == 1)
                            ConfimAP();
                        au3.WinWait("[REGEXPTITLE:\\ATable Name\\z]", "", 1);
                        if (au3.WinExists("Table Name") == 1)
                            TableName(au3.WinGetTitle("[HANDLE:" + ipTable[hActive] + "]"));
                        int counter = 0;
                        while (BotConnected(ipShanky[hActive]) == false && counter < 20)
                        {
                            //IntPtr tempbotHandleStart = new IntPtr(Convert.ToInt32(ipShanky[hActive], 16));
                            //Console.WriteLine("tempbotHandleStart: " + tempbotHandleStart);
                            //winFunc.SetWindowToForeground(tempbotHandleStart);
                            au3.Sleep(2000);
                            au3.WinActivate("[HANDLE:" + ipShanky[hActive] + "]");
                            au3.WinWaitActive("[HANDLE:" + ipShanky[hActive] + "]", "", 1);
                            au3.WinMenuSelectItem("[HANDLE:" + ipShanky[hActive] + "]", "", "S&tart!");
                            //au3.Send("!t", 0);
                            au3.Sleep(1000);
                            counter += 1;
                        }
                        if (BotConnected(ipShanky[hActive]))
                        {
                            SuccessfulConnect = true;
                            fConsoleWrite("Bot successfully connected and started");
                            FileFunctions.fConsoleWrite("Tournaments started and bots connected: " + tournamentsStarted);
                            HideBot(ipShanky[hActive]);
                            AdjustWinPositionsSng();
                            lblScheduledTime.Text = "";                            
                            return;
                        }
                        else
                        {
                            FileFunctions.fConsoleWrite("Something went wrong with table connect.  Retrying from scratch in 1 second.");
                            au3.Sleep(1000);
                            if (au3.WinExists("Wait For Table?") == 1)
                            {
                                au3.WinActivate("Wait For Table?");
                                au3.ControlClick("Wait For Table?", "", "[CLASS:Button; INSTANCE:2]");
                            }
                            au3.Sleep(500);
                            if (BotConnected(ipShanky[hActive]) && au3.WinExists(ipShanky[hActive]) == 1)
                            {
                                HideBot(ipShanky[hActive]);
                                break;
                            }
                            else if (au3.WinExists(ipShanky[hActive]) == 1)
                            {
                                //au3.WinActivate("[HANDLE:" + ipShanky[hActive] + "]");
                                //au3.WinWaitActive("[HANDLE:" + ipShanky[hActive] + "]", "", 1);
                                au3.WinMenuSelectItem("[HANDLE:" + ipShanky[hActive] + "]", "", "S&tart!");
                                //au3.Send("!t");
                                au3.Sleep(200);
                                if (BotConnected(ipShanky[hActive]))
                                {
                                    HideBot(ipShanky[hActive]);
                                    return;
                                }
                                else
                                {
                                    Console.WriteLine("Bot never connected ABTT");
                                    au3.WinClose("[HANDLE:" + ipShanky[hActive] + "]");
                                    ipShanky[hActive] = "";
                                    thdCheckForNewTables();
                                }
                            }
                            else
                            {
                                thdCheckForNewTables();
                                return;
                            }
                        }
                        au3.Sleep(100);
                    }
                }
            }
        }

        private object[,] TableListMTT()
        {
            au3.Opt("WinTitleMatchMode", 4);
            object[,] winlist = null;
            if (cmbSite.Text == "ACR" || cmbSite.Text == "BlackChip" || cmbSite.Text == "True Poker")
                winlist = (object[,])au3.WinList("[REGEXPTITLE:\\A.*Table.*Hold'em.*\\z]");
            else if (cmbSite.Text == "BetOnline")
                winlist = (object[,])au3.WinList("[REGEXPTITLE:\\A.*#.*Level.*\\z]");
            else if (cmbSite.Text == "Ignition")
                winlist = (object[,])au3.WinList("[REGEXPTITLE:\\A.*/.*Hold'em.*TBL.*#.*\\z]");
            return winlist;
        }

        public void btnDeleteScheduleClick()
        {
            int count = 0;
            string line = null;
            string line_to_delete = "";
            ListViewItem item = listSchedule.SelectedItems.Count > 0 ? listSchedule.SelectedItems[0] : null;
            if (item != null)
            {
                line_to_delete = item.SubItems[1].Text + "~" + item.SubItems[2].Text + "~" + item.SubItems[3].Text;
            }
            System.IO.StreamReader file = new System.IO.StreamReader(iniLocation + sIniFile);
            while ((line = file.ReadLine()) != null)
            {
                count += 1;
                if (line.Contains(line_to_delete))
                {
                    break;
                }
            }
            file.Close();
            File_DeleteLine(count, iniLocation + sIniFile);
            UpdateListview();
        }

        private void btnDeleteSchedule_Click(object sender, EventArgs e)
        {
            btnDeleteScheduleClick();
        }

        private void btnAddSchedule_Click(object sender, EventArgs e)
        {
            btnAddScheduleClick();
        }

        private void btnUpdateSchedule_Click(object sender, EventArgs e)
        {
            btnDeleteScheduleClick();
            btnAddScheduleClick();
        }

        private void btnStartScheduler_Click(object sender, EventArgs e)
        {
            if (bScheduleActive == false)
            {
                bScheduleActive = true;
                if (btnStart.Text == "Start")
                {
                    btnStart.Text = "Exit";
                    btnStart.BackColor = Color.Red;
                    StartNow();
                }
                else
                    ExitEvent();                
            }
        }

        private void SetData()
        {
            if (cmbSite.Text == "ACR" || cmbSite.Text == "Blackchip" || cmbSite.Text == "TruePoker")
            {
                cmbType.Items.Clear();
                cmbType.Text = "";
                cmbType.Text = "NL";
                cmbType.Items.Add("NL");
                cmbType.Items.Add("PL");
                cmbType.Items.Add("Fixed");

                cmbPlayers.Items.Clear();
                cmbPlayers.Text = "";
                cmbPlayers.Text = "9";
                cmbPlayers.Items.Add("2");
                cmbPlayers.Items.Add("6");
                cmbPlayers.Items.Add("9");

                cmbBuyin.Items.Clear();
                cmbBuyin.Text = "";
                cmbBuyin.Text = "$0.50+$0.05";
                cmbBuyin.Items.Add("$0+$0");
                cmbBuyin.Items.Add("$0.50+$0.05");
                cmbBuyin.Items.Add("$1+$0.10");
                cmbBuyin.Items.Add("$1.50+$0.11");
                cmbBuyin.Items.Add("$1.50+$0.12");
                cmbBuyin.Items.Add("$1.50+$0.15");
                cmbBuyin.Items.Add("$2+$0.10");
                cmbBuyin.Items.Add("$2+$0.17");
                cmbBuyin.Items.Add("$2.20+$0.16");
                cmbBuyin.Items.Add("$2.40+$0.10");
                cmbBuyin.Items.Add("$3+$0.25");
                cmbBuyin.Items.Add("$3+$0.30");
                cmbBuyin.Items.Add("$5+$0.25");
                cmbBuyin.Items.Add("$5+$0.30");
                cmbBuyin.Items.Add("$5+$0.45");
                cmbBuyin.Items.Add("$5+$0.50");
                cmbBuyin.Items.Add("$5.50+$0.25");
                cmbBuyin.Items.Add("$6+$0.18");
                cmbBuyin.Items.Add("$6+$0.60");
                cmbBuyin.Items.Add("$10+$0.50");
                cmbBuyin.Items.Add("$10+$0.55");
                cmbBuyin.Items.Add("$10+$0.90");
                cmbBuyin.Items.Add("$10+$1");
                cmbBuyin.Items.Add("$11+$0.50");
                cmbBuyin.Items.Add("$12+$0.25");
                cmbBuyin.Items.Add("$15+$0.80");
                cmbBuyin.Items.Add("$15+$1.40");
                cmbBuyin.Items.Add("$15+$1.50");
                cmbBuyin.Items.Add("$20+$1");
                cmbBuyin.Items.Add("$20+$1.75");
                cmbBuyin.Items.Add("$20+$2");
                cmbBuyin.Items.Add("$22+$1");
                cmbBuyin.Items.Add("$24+$1.50");
                cmbBuyin.Items.Add("$30+$1.30");
                cmbBuyin.Items.Add("$30+$1.50");
                cmbBuyin.Items.Add("$30+$2.75");
                cmbBuyin.Items.Add("$30+$3");

                cmbGameType.Items.Clear();
                cmbGameType.Text = "";
                cmbGameType.Text = "Normal";
                cmbGameType.Items.Add("Normal");
                cmbGameType.Items.Add("Turbo");
                cmbGameType.Items.Add("Hyper Turbo");
                cmbGameType.Items.Add("Turbo Double or Nothing");
                cmbGameType.Items.Add("Double or Nothing");
                cmbGameType.Items.Add("Final Table Experience");
                cmbGameType.Items.Add("On Demand");
                cmbGameType.Items.Add("Satellite");

                txtPosX0.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "ACR" + "x0Txt", "0");
                txtPosX1.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "ACR" + "x1Txt", "0");
                txtPosX2.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "ACR" + "x2Txt", "0");
                txtPosX3.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "ACR" + "x3Txt", "0");
                txtPosX4.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "ACR" + "x4Txt", "0");
                txtPosX5.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "ACR" + "x5Txt", "0");
                txtPosY0.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "ACR" + "y0Txt", "0");
                txtPosY1.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "ACR" + "y1Txt", "0");
                txtPosY2.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "ACR" + "y2Txt", "0");
                txtPosY3.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "ACR" + "y3Txt", "0");
                txtPosY4.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "ACR" + "y4Txt", "0");
                txtPosY5.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "ACR" + "y5Txt", "0");
            }
            else if (cmbSite.Text == "BetOnline")
            {
                cmbType.Items.Clear();
                cmbType.Text = "";
                cmbType.Text = "NL";
                cmbType.Items.Add("NL"); 
                cmbType.Items.Add("PL");
                cmbType.Items.Add("FL");

                cmbPlayers.Items.Clear();
                cmbPlayers.Text = "";
                cmbPlayers.Text = "9";
                cmbPlayers.Items.Add("2");
                cmbPlayers.Items.Add("4");
                cmbPlayers.Items.Add("5");
                cmbPlayers.Items.Add("6");
                cmbPlayers.Items.Add("9");
                cmbPlayers.Items.Add("10");

                cmbBuyin.Items.Clear();
                cmbBuyin.Text = "";
                cmbBuyin.Text = "$1.50";
                cmbBuyin.Items.Add("$3");
                cmbBuyin.Items.Add("$3.50");
                cmbBuyin.Items.Add("$7");
                cmbBuyin.Items.Add("$15");
                cmbBuyin.Items.Add("$30");
                cmbBuyin.Items.Add("$60");
                cmbBuyin.Items.Add("$100");
                cmbBuyin.Items.Add("$109");
                cmbBuyin.Items.Add("$200");
                cmbBuyin.Items.Add("$215");

                cmbGameType.Items.Clear();
                cmbGameType.Text = "";
                cmbGameType.Text = "Normal";
                cmbGameType.Items.Add("Normal");
                cmbGameType.Items.Add("Turbo");
                cmbGameType.Items.Add("Hyper");
                cmbGameType.Items.Add("Turbo Double or Nothing");
                cmbGameType.Items.Add("Double or Nothing");
                cmbGameType.Items.Add("Satellite");

                txtPosX0.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "BetOnline" + "x0Txt", "0");
                txtPosX1.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "BetOnline" + "x1Txt", "0");
                txtPosX2.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "BetOnline" + "x2Txt", "0");
                txtPosX3.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "BetOnline" + "x3Txt", "0");
                txtPosX4.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "BetOnline" + "x4Txt", "0");
                txtPosX5.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "BetOnline" + "x5Txt", "0");
                txtPosY0.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "BetOnline" + "y0Txt", "0");
                txtPosY1.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "BetOnline" + "y1Txt", "0");
                txtPosY2.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "BetOnline" + "y2Txt", "0");
                txtPosY3.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "BetOnline" + "y3Txt", "0");
                txtPosY4.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "BetOnline" + "y4Txt", "0");
                txtPosY5.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "BetOnline" + "y5Txt", "0");
            }
            else if (cmbSite.Text == "Ignition")
            {
                cmbType.Items.Clear();
                cmbType.Text = "";
                cmbType.Text = "NL";
                cmbType.Items.Add("NL");
                cmbType.Items.Add("PL");
                cmbType.Items.Add("Fixed");

                cmbPlayers.Items.Clear();
                cmbPlayers.Text = "";
                cmbPlayers.Text = "9";
                cmbPlayers.Items.Add("2");
                cmbPlayers.Items.Add("6");
                cmbPlayers.Items.Add("9");
                cmbPlayers.Items.Add("27");

                cmbBuyin.Items.Clear();
                cmbBuyin.Text = "";
                cmbBuyin.Text = "$1+$0.05";
                cmbBuyin.Items.Add("$1+$0.10");
                cmbBuyin.Items.Add("$2+$0.20");
                cmbBuyin.Items.Add("$2.50+$0.25");
                cmbBuyin.Items.Add("$3+$0.30");
                cmbBuyin.Items.Add("$4+$0.40");
                cmbBuyin.Items.Add("$5 +$0.25");
                cmbBuyin.Items.Add("$5+$0.50");
                cmbBuyin.Items.Add("$7+$0.70");
                cmbBuyin.Items.Add("$8+$0.80");
                cmbBuyin.Items.Add("$10+$0.50");
                cmbBuyin.Items.Add("$10+$1");
                cmbBuyin.Items.Add("$15+$1.50");
                cmbBuyin.Items.Add("$16+$1.50");
                cmbBuyin.Items.Add("$20+$1");
                cmbBuyin.Items.Add("$25+$1.25");
                cmbBuyin.Items.Add("$25+$2.25");
                cmbBuyin.Items.Add("$25+$2.50");
                cmbBuyin.Items.Add("$32+$3");
                cmbBuyin.Items.Add("$40+$4");
                cmbBuyin.Items.Add("$50+$2.50");
                cmbBuyin.Items.Add("$50+$4.50");
                cmbBuyin.Items.Add("$50+$5");

                cmbGameType.Items.Clear();
                cmbGameType.Text = "";
                cmbGameType.Text = "Normal";
                cmbGameType.Items.Add("Normal");
                cmbGameType.Items.Add("All-In");
                cmbGameType.Items.Add("Turbo");
                cmbGameType.Items.Add("Hyper Turbo");
                cmbGameType.Items.Add("Turbo Double-Up");
                cmbGameType.Items.Add("Double-Up");
                cmbGameType.Items.Add("Turbo Triple-Up");
                cmbGameType.Items.Add("Triple-Up");
                cmbGameType.Items.Add("Turbo Beginner");
                cmbGameType.Items.Add("Beginner");

                txtPosX0.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "Ignition" + "x0Txt", "0");
                txtPosX1.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "Ignition" + "x1Txt", "0");
                txtPosX2.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "Ignition" + "x2Txt", "0");
                txtPosX3.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "Ignition" + "x3Txt", "0");
                txtPosX4.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "Ignition" + "x4Txt", "0");
                txtPosX5.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "Ignition" + "x5Txt", "0");
                txtPosY0.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "Ignition" + "y0Txt", "0");
                txtPosY1.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "Ignition" + "y1Txt", "0");
                txtPosY2.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "Ignition" + "y2Txt", "0");
                txtPosY3.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "Ignition" + "y3Txt", "0");
                txtPosY4.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "Ignition" + "y4Txt", "0");
                txtPosY5.Text = FileFunctions.IniRead(iniLocation + sIniFile, "Settings", "Ignition" + "y5Txt", "0");

            }
        }

        private void cmbSite_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetData();
        }

        private void lnkMTT_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://bonusbots.com/support/index.php/topic,10652.0.html");
        }

        private void lnkSNG_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.c-sharpcorner.com");
        }

        public object[,] TableListSng()
        {
            au3.Opt("WinTitleMatchMode", 4);
            object[,] winlist = null;
            if (cmbSite.Text == "ACR" || cmbSite.Text == "BlackChip" || cmbSite.Text == "True Poker")
                winlist = (object[,])au3.WinList("[REGEXPTITLE:\\A.*Table.*Hold'em.*\\z]");
            else if (cmbSite.Text == "BetOnline")
                winlist = (object[,])au3.WinList("[REGEXPTITLE:\\A.*#.*Level.*\\z]");
            else if (cmbSite.Text == "Ignition")
                winlist = (object[,])au3.WinList("[REGEXPTITLE:\\A.*Hold'em.*TBL.*\\z]");
            return winlist;
        }

        public void HandleBodogWindows()
        {
            object[,] winlist = null;
            winlist = (object[,])au3.WinList("[REGEXPTITLE:\\APoker\\z; W:346 H:177]");
            if (winlist != null && (int)winlist[0,0] > 0)
            {
                //Console.WriteLine("Found Bodog xtra Window");
                for (int a = 1; a <= (int)winlist[0, 0]; a++)
                {
                    if (au3.WinExists("[HANDLE:" + winlist[1, a] + "]") == 1)
                    {
                        int hbwWidth = au3.WinGetPosWidth("[HANDLE:" + winlist[1, a] + "]");
                        int hbwHeight = au3.WinGetPosHeight("[HANDLE:" + winlist[1, a] + "]");
                        if (hbwWidth == 346 && hbwHeight == 177)
                        {
                            //Console.WriteLine("Window State: " + au3.WinGetState("[HANDLE:" + winlist[1, a] + "]"));
                            au3.WinActivate("[HANDLE:" + winlist[1, a] + "]");
                            au3.WinWaitActive("[HANDLE:" + winlist[1, a] + "]", "", 1);
                            int hbwX = au3.WinGetPosX("[HANDLE:" + winlist[1, a] + "]");
                            int hbwY = au3.WinGetPosY("[HANDLE:" + winlist[1, a] + "]");
                            au3.MouseClick("left", hbwX, hbwY, 1, 0);
                        }
                    }
                }
            }
        }

        public void CloseBodogStartWindow()
        {
            object[,] winlist = null;
            if (au3.WinExists("[REGEXPTITLE:\\APoker\\z; W:346; H:177]") == 1)
            {
                winlist = (object[,])au3.WinList("[REGEXPTITLE:\\APoker\\z; W:346; H:177]");
                if (winlist != null && (int)winlist[0,0] > 0)
                {
                    for (int a = 1; a <= (int)winlist[0, 0]; a++)
                    {
                        string cbswHnd = au3.WinGetHandle("[HANDLE:" + winlist[1, a] + "]");
                        au3.WinActivate("[HANDLE:" + winlist[1, a] + "]");
                        au3.WinWaitActive("[HANDLE:" + winlist[1, a] + "]", "", 1);
                        int cbswX = au3.WinGetPosX("[HANDLE:" + winlist[1, a] + "]");
                        int cbswY = au3.WinGetPosY("[HANDLE:" + winlist[1, a] + "]");
                        au3.MouseClick("left", cbswX + 272, cbswY + 145, 1, 0);
                        return;
                    }
                }
            }
        }

        public void thdCheckForNewTables()
        {
            if (cmbSite.Text == "Ignition")
                CloseBodogStartWindow();
            object[,] winlist = null;
            if (radHopperMTT.Checked)
                winlist = TableListMTT();
            else
            {
                winlist = TableListSng();
            }
            if (winlist != null)
            {
                if ((int)winlist[0, 0] > NumTables())
                {
                    //SuspendThreads(ThreadNames.CheckForNewTables);
                    string NewTableTitle = string.Empty;
                    string TourneyNumber = string.Empty;
                    for (int a = 1; a <= (int)winlist[0, 0]; a++)
                    {
                        string title = winlist[0, a].ToString();
                        int enTitleIndex = Array.IndexOf(ipTable, winlist[1, a]);
                        if (enTitleIndex == -1)
                        {
                            if (ipTable[a] == "" || ipTable[a] == null)
                            {
                                ipTable[a] = winlist[1, a].ToString();
                            }
                            else
                            {
                                Console.WriteLine("ipTable[a]!= empty and ipTable[a] != null");
                                continue;
                            }
                            WaitingForTournament = false;
                            fConsoleWrite("New sng found:*" + title + "*");
                            tournamentsStarted += 1;
                            string totalSngs = txtMaxSngs.Text;
                            txtSngsElapsed.Text = "";
                            txtSngsElapsed.Text = tournamentsStarted.ToString() + "/" + totalSngs;
                            Console.WriteLine(tournamentsStarted.ToString() + "/" + totalSngs);
                            if (hTournReg > 0)
                                hTournReg -= 1;                            
                            au3.WinActivate("[HANDLE:" + winlist[1, a] + "]");
                            if (cmbSite.Text == "Ignition")
                                CloseBodogStartWindow();
                            AdjustWinPositionsSng();
                            AddBotsToTables();
                            break;
                        }
                        else
                        {
                            au3.Sleep(500);
                        }
                    }
                }
            }
            Application.DoEvents();
        }
        private void CheckForStrayBots(string TableName = "")
        {
            object[,] BotList = null;
            BotList = BotWinList();
            int count = 0;
            int counter = 0;
            if (BotList != null && (int)BotList[0, 0] > 0)
            {
                for (int a = 1; a <= (int)BotList[0,0]; a++)
                {
                    if (au3.WinExists(ipShanky[a]) == 1 && ipShanky[a] != "" && BotConnected(ipShanky[a]) == false)
                    {
                        fConsoleWrite("Closing Dead Bot " + a + ". WClose");
                        count += 1;
                        if (TableName != "")
                        {
                            fConsoleWrite("CDB Bot not connected for table: " + TableName + "... closing bot: " + a);
                            Console.WriteLine("CDB Bot not connected for table: " + TableName + "... closing bot: " + a);
                        }
                        else
                        {
                            fConsoleWrite("CDB Bot not connected for table ... closing bot: " + a);
                            Console.WriteLine("CDB Bot not connected for table ... closing bot: " + a);
                        }
                        au3.Sleep(500);
                        while (au3.WinExists(ipShanky[a]) == 1 && counter <= 20)
                        {
                            fConsoleWrite("CDT: Closing loose bot");
                            //au3.WinActivate("[HANDLE:" + ipShanky[a] + "]");
                            //au3.WinWaitActive("[HANDLE:" + ipShanky[a] + "]", "", 1);
                            au3.WinMenuSelectItem("[HANDLE:" + ipShanky[a] + "]", "", "&Holdem", "S&tart!");
                            //au3.Send("!h{ENTER}x");
                            au3.Sleep(100);
                            counter += 1;
                        }
                        if (au3.WinExists(ipShanky[a]) == 0)
                        {
                            Console.WriteLine("CDB: Bot " + a + " reset to -10");
                            ipShanky[a] = "";
                        }
                    }
                }
            }
        }

        public void sendEmail(string emailAddress, string host, int port, string password)
        {
            var fromAddress = new MailAddress(emailAddress, "From Me"); // txtEmailAddress.Text
            var toAddress = new MailAddress(emailAddress, "To Me"); // txtEmailAddress.Text
            string fromPassword = password; // email password from txtEmailPassword.Text
            const string subject = "Pokerbot Update";
            const string body = "Here is the update you requested";

            var smtp = new SmtpClient
            {
                Host = host, // cmbEmailProvider - > triggers change of data in txtEmailProvider.Text
                Port = port, // txtPort.Text
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }

        /* Things we can do in threads:
         *      look for abandoned games
         *      look for abandoned bots
         *      register tournaments when necessary
         *      attach bots to games
         *      closing finshed tournaments
         *      */

        public static void BackupLogs(string sBotExe, int ibotnum)
        {
            FileFunctions.fConsoleWrite("Backing up log:" + ibotnum.ToString());
            string sPath = sBotExe.Substring(0, sBotExe.LastIndexOf("\\") + 1);
            string sLogTime;

            string sYear = DateTime.Now.Year.ToString().PadLeft(4, '0');
            string sMonth = DateTime.Now.Month.ToString().PadLeft(2, '0');
            string sDay = DateTime.Now.Day.ToString().PadLeft(2, '0');
            string sHour = DateTime.Now.Hour.ToString().PadLeft(2, '0');
            string sMinute = DateTime.Now.Minute.ToString().PadLeft(2, '0');
            string sSecond = DateTime.Now.Second.ToString().PadLeft(2, '0');
            string sMS = DateTime.Now.Millisecond.ToString().PadLeft(4, '0');
            sLogTime = sYear + sMonth + sDay + sHour + sMinute + sSecond + sMS;

            string slogname = "holdem";
            if (ibotnum > 1)
                slogname = slogname + ibotnum.ToString();
            slogname = slogname + ".log";

            try
            {
                if (File.Exists(sPath + slogname))
                {
                    File.Copy(sPath + slogname, sPath + sLogTime + "-" + slogname, true);
                    //FileFunctions.fConsoleWrite("Log file back up successful");
                }
                else
                {
                    FileFunctions.fConsoleWrite("No log file to backup");
                }
            }
            catch
            {
                FileFunctions.fConsoleWrite("Error Backing up Bot Log");
            }
        }

        public void fSessionLog(string Msg)
        {
            DateTime dtime = DateTime.Now;
            string hour = dtime.Hour.ToString();
            string min = dtime.Minute.ToString();
            string second = dtime.Second.ToString();
            SessionLog.Text = hour + ":" + min + ":" + second + " ---> " + Msg;
        }
    }

}
