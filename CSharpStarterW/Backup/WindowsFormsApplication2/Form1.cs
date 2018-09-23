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

namespace csFTPStarter
{
    public partial class MainForm : Form
    {
        #region Globals
        //our au3 class that gives us au3 functionality
        static AutoItX3Lib.AutoItX3Class au3;

        //Import the FindWindow API to find our window
        [DllImportAttribute("User32.dll")]
        private static extern int FindWindow(String ClassName, String WindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int GetForegroundWindow();

        //Import the SetForeground API to activate it
        [DllImportAttribute("User32.dll")]
        private static extern IntPtr SetForegroundWindow(int hWnd);

        [DllImport("User32.Dll")]
        public static extern void GetWindowText(int h, StringBuilder s, int MaxCount);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(int hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        [DllImport("winmm.dll")]
        private static extern bool PlaySound(string filename, int module, int flags);
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x20;
        private const int MOUSEEVENTF_MIDDLEUP = 0x40;

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);


        //static string[] hxShanky;
        static DateTime dtStarted;
        static Hashtable hshShanky;
        static Hashtable hshTables;
        static bool bPause;
        static IntPtr[] ipShanky;
        static IntPtr[] ipTable;

        static int iLiveBots, iTablesFinished = 0, iTournamentsRegistered, OldiTablesFinished;
        static bool noTourney;
        static bool bLockThreads = false;
        static int a, userNumSngInt, iBotsToPlay; //, userNumMinsInt;
        static string textProf, userNumSng, iBotsToPLay; // userNumMins
        static bool FoundNewBot, WaitingForTournament;
        //static int FindTourney;
        static ArrayList tTournaments = new ArrayList();
        static ArrayList hTournaments = new ArrayList();
        static Hashtable hshActiveTournamentIDs;
        //static object[,] FTPWindows;
        static string textBot;
        //static string[] hxTable;
        static string ScriptDir;
        static bool tourneyRegOK = false;
        const string appName = "FTP Holdem Sng Starter";
        const string FTexe = "FullTiltPoker.exe";
        const string FTloc = "C:\\Program Files\\Full Tilt Poker\\";
        const string sIniFile = "FTPSNG.ini";
        const string bottitle = "^Calculator$";
        static string LobbyName = "[REGEXPTITLE:\\AFull Tilt Poker - Logged In As .*\\z]";
        static string reLobbyName = "^Full Tilt Poker - Logged In As .*$";
        static string scrapeini = "FTPStarterScrapes.ini";
        private static int iPlayers = 0;
        const string ftExe = "FullTiltPoker.exe";
        static string UserNamePattern = " As .*$";
        static string UserNameReplacement = " As CodeStormer.com";
        static string sTableTitleHash = "^.* Hold'em .*$";
        static string TournamentLobbyTitleHash = "^Full Tilt Poker - Tournament.*$";
        static Regex rgx;// = new Regex(UserNamePattern);
        // static int winposAnnouncementX, winposAnnouncementY, winposAnnouncementW, winposAnnouncementH;
        //SSLScrape.PlatformOCRSDK OCR = new SSLScrape.PlatformOCRSDK();
        //WindowFunctions winFunc = new WindowFunctions();
        static WindowFunctions winFunc = new WindowFunctions();
        static IntPtr hLobby;
        static GraphicsFunctions gfx = new GraphicsFunctions();
        static FileFunctions ffx = new FileFunctions();
        static string sShankyPath;
        static DateTime tsLastTournamentJoin;
        static Thread threadUpdateStatusText;// = new Thread(thdUpdateStatusText);
        static Thread threadCheckForNewTables;// = new Thread(thdCheckForNewTables);
        static Thread threadTournamentAnnouncements;// = new Thread(thdTournamentAnnouncements);
        static Thread threadCloseBadWindows;// = new Thread(thdCloseStrayWindows);
        static System.Windows.Forms.Timer tmrUpdateStatusText;
        static System.Windows.Forms.Timer tmrCheckNewTables;
        static System.Windows.Forms.Timer tmrTournamentAnnouncements;
        static System.Windows.Forms.Timer tmrCloseBadWindows;
        public enum ThreadNames
        {
            None,
            UpdateStatus,
            CheckForNewTables,
            TournamentAnnouncements,
            CloseBadWindows
        }
        #endregion

        public MainForm()
        {
            InitializeComponent();
            threadUpdateStatusText = new Thread(thdUpdateStatusText);
            threadCheckForNewTables = new Thread(thdCheckForNewTables);
            threadTournamentAnnouncements = new Thread(thdTournamentAnnouncements);
            threadCloseBadWindows = new Thread(thdCloseStrayWindows);

            tmrUpdateStatusText = new System.Windows.Forms.Timer();
            tmrCheckNewTables = new System.Windows.Forms.Timer();
            tmrTournamentAnnouncements = new System.Windows.Forms.Timer();
            tmrCloseBadWindows = new System.Windows.Forms.Timer();

            hshActiveTournamentIDs = new Hashtable();
            bPause = false;
            au3 = new AutoItX3Lib.AutoItX3Class();
            // sLobby = au3.WinGetHandle("[REGEXPTITLE:\\AFull Tilt Poker - Logged In As .*\\z]", "");
            hLobby = winFunc.GetWindowHandle(reLobbyName);
            //            FileFunctions.fConsoleWrite(hLobby.ToString());
            //            iLobby = FindWindow("QWidget", LobbyName);

            ScriptDir = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            scrapeini = ScriptDir + "\\" + scrapeini;
            rgx = new Regex(UserNamePattern);
            // winFunc.SetScrapeIni(scrapeini);
            gfx.scrapeini = scrapeini;
            gfx.dataentry = 0;
            //SSLScrape.PlatformOCRSDK.OCRSDK_Get_Screen_Ascii(0, 0, 1000, 1000);
            HotKey.RegisterHotKey(this.Handle, 0, HotKey.KeyModifiers.Shift, Keys.Escape);
            HotKey.RegisterHotKey(this.Handle, 1, HotKey.KeyModifiers.None, Keys.Pause);
            this.FormClosing += MainForm_FormClosing;
            hshShanky = new Hashtable();
            hshTables = new Hashtable();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == HotKey.WM_HOTKEY)
            {
                if (m.WParam.ToInt32() == 0)
                {
                    FileFunctions.fConsoleWrite("Exiting - Shift+esc pressed");
                    this.ExitEvent();
                }
                else if (m.WParam.ToInt32() == 1)
                {
                    bPause = !bPause;
                    if (bPause)
                    {
                        SuspendThreads();
                        bLockThreads = true;
                        FileFunctions.fConsoleWrite("Pausing");
                    }
                    else
                    {
                        ResumeThreads();
                        bLockThreads = false;
                        FileFunctions.fConsoleWrite("UnPausing");
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
            if (iExcept != ThreadNames.UpdateStatus)
                tmrUpdateStatusText.Enabled = false;
            //threadUpdateStatusText.Suspend();
            if (iExcept != ThreadNames.TournamentAnnouncements)
                tmrTournamentAnnouncements.Enabled = false;
            //threadTournamentAnnouncements.Suspend();
            if (iExcept != ThreadNames.CloseBadWindows)
                tmrCloseBadWindows.Enabled = false;
            //threadCloseBadWindows.Suspend();
            if (iExcept != ThreadNames.CheckForNewTables)
                tmrCheckNewTables.Enabled = false;
            //threadCheckForNewTables.Suspend();
        }

        private static void ResumeThreads()
        {
            ResumeThreads(ThreadNames.None);
        }
        private static void ResumeThreads(ThreadNames iExcept)
        {
            tmrUpdateStatusText.Enabled = true;
            tmrTournamentAnnouncements.Enabled = true;
            tmrCloseBadWindows.Enabled = true;
            tmrCheckNewTables.Enabled = true;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            for (int a = 0; a < 2; a++)
            {
                HotKey.UnregisterHotKey(this.Handle, a);
            }
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
            cmbPlayers.Items.Add("9");
            cmbPlayers.Items.Add("18");
            cmbPlayers.Items.Add("27");
            cmbPlayers.Items.Add("45");
            cmbPlayers.Items.Add("54");
            cmbPlayers.Items.Add("90");
            cmbPlayers.Items.Add("180");

            // Type Combo Box
            cmbType.Items.Add("NL");
            cmbType.Items.Add("PL");
            cmbType.Items.Add("FL");

            // Buyin Combo Box
            cmbBuyin.Items.Add("20 FTP");
            cmbBuyin.Items.Add("50 FTP");
            cmbBuyin.Items.Add("150 FTP");
            cmbBuyin.Items.Add("275 FTP");
            cmbBuyin.Items.Add("300 FTP");
            cmbBuyin.Items.Add("400 FTP");
            cmbBuyin.Items.Add("600 FTP");
            cmbBuyin.Items.Add("650 FTP");
            cmbBuyin.Items.Add("1200 FTP");
            cmbBuyin.Items.Add("1800 FTP");
            cmbBuyin.Items.Add("2500 FTP");
            cmbBuyin.Items.Add("5500 FTP");
            cmbBuyin.Items.Add("6000 FTP");
            cmbBuyin.Items.Add("$1 + $0.10");
            cmbBuyin.Items.Add("$1 + $0.20");
            cmbBuyin.Items.Add("$1.50 + $0.20");
            cmbBuyin.Items.Add("$2 + $0.25");
            cmbBuyin.Items.Add("$3 + $0.30");
            cmbBuyin.Items.Add("$3.50 + $0.30");
            cmbBuyin.Items.Add("$4 + $0.40");
            cmbBuyin.Items.Add("$5 + $0.50");
            cmbBuyin.Items.Add("$6 + $0.50");
            cmbBuyin.Items.Add("$6 + $0.60");
            cmbBuyin.Items.Add("$7 + $0.50");
            cmbBuyin.Items.Add("$8 + $0.70");
            cmbBuyin.Items.Add("$8 + $0.80");
            cmbBuyin.Items.Add("$10 + $1");
            cmbBuyin.Items.Add("$11 + $1");
            cmbBuyin.Items.Add("$12 + $1");
            cmbBuyin.Items.Add("$14 + $1");
            cmbBuyin.Items.Add("$20 + $2");
            cmbBuyin.Items.Add("$22 + $2");
            cmbBuyin.Items.Add("$24 + $2");
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
            string path = txtShanky.Text;
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
            string shankyPath = "";
            shankyPath = SelectExeFile();
            if (shankyPath.Length > 0)
                txtShanky.Text = shankyPath;
        }

        private void btnProfile_Click(object sender, EventArgs e)
        {
            string profPath = "";
            profPath = SelectTxtFile();
            if (profPath.Length > 0)
                txtProfile.Text = profPath;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            FileFunctions.fConsoleWrite("Starting");
            Hide();
            iPlayers = Convert.ToInt32(cmbPlayers.Text);
            textBot = txtShanky.Text;
            textProf = txtProfile.Text;
            //textBatch = txtBatch.Text;
            // OpenFT();
            if (textBot == "")
                MessageBox.Show("Please enter Bot Path");
            else if (textProf == "")
                MessageBox.Show("Please enter Profile Path");
            else
            {
                //sLobby = au3.WinGetHandle(LobbyName, "");
                hLobby = winFunc.GetWindowHandle(reLobbyName);
                if (hLobby == IntPtr.Zero)
                {
                    MessageBox.Show("Poker site not running");
                    Show();
                    return;
                }
                winFunc.SetWindowToForeground(hLobby);
                /*                if (sLobby.Length >= 8)
                                {
                                    sLobby = sLobby.Substring(sLobby.Length - 8, 8);
                                    Console.Write('*' + sLobby + '*');
                                }
                                sLobby = "[HANDLE:" + sLobby + "]";
                                au3.WinActivate(sLobby, "");
                                iLobby = GetForegroundWindow();
                                SetForegroundWindow(iLobby);
                                //TestMenu();
                                FileFunctions.fConsoleWrite(sLobby);
                                //winHeight = _WinAPI_GetWindowHeight(lobbyHan);
                                //if (winHeight != 568 && winWidth != 802)
                 */
                SaveSettings();
                MoveLobbyWindow();
                StartAutoPilot();
                //UpdateScrapes();
            }

        }

        public void SaveSettings()
        {
            au3.IniWrite(sIniFile, "Settings", "cmbBots", cmbBots.SelectedItem.ToString());
            au3.IniWrite(sIniFile, "Settings", "cmbBuyin", cmbBuyin.SelectedItem.ToString());
            au3.IniWrite(sIniFile, "Settings", "cmbGameType", cmbGameType.SelectedItem.ToString());
            au3.IniWrite(sIniFile, "Settings", "cmbPlayers", cmbPlayers.SelectedItem.ToString());
            au3.IniWrite(sIniFile, "Settings", "cmbType", cmbType.SelectedItem.ToString());
            au3.IniWrite(sIniFile, "Settings", "chkKnockout", chkKnockout.Checked.ToString());
            au3.IniWrite(sIniFile, "Settings", "chkDouble", chkDouble.Checked.ToString());
            //au3.IniWrite(sIniFile, "Settings", "txtBatch", txtBatch.Text);
            au3.IniWrite(sIniFile, "Settings", "txtShanky", txtShanky.Text);
            //au3.IniWrite(sIniFile, "Settings", "txtMaxMins", txtMaxMins.Text);
            au3.IniWrite(sIniFile, "Settings", "txtMaxSngs", txtMaxSngs.Text);
            au3.IniWrite(sIniFile, "Settings", "txtProfile", txtProfile.Text);
            au3.IniWrite(sIniFile, "Settings", "txtPosX1", txtPosX1.Text);
            au3.IniWrite(sIniFile, "Settings", "txtPosX2", txtPosX2.Text);
            au3.IniWrite(sIniFile, "Settings", "txtPosX3", txtPosX3.Text);
            au3.IniWrite(sIniFile, "Settings", "txtPosX4", txtPosX4.Text);
            au3.IniWrite(sIniFile, "Settings", "txtPosX5", txtPosX5.Text);
            au3.IniWrite(sIniFile, "Settings", "txtPosX6", txtPosX6.Text);
            au3.IniWrite(sIniFile, "Settings", "txtPosY1", txtPosY1.Text);
            au3.IniWrite(sIniFile, "Settings", "txtPosY2", txtPosY2.Text);
            au3.IniWrite(sIniFile, "Settings", "txtPosY3", txtPosY3.Text);
            au3.IniWrite(sIniFile, "Settings", "txtPosY4", txtPosY4.Text);
            au3.IniWrite(sIniFile, "Settings", "txtPosY5", txtPosY5.Text);
            au3.IniWrite(sIniFile, "Settings", "txtPosY6", txtPosY6.Text);
            au3.IniWrite(sIniFile, "Settings", "mainformX", this.Location.X.ToString());
            au3.IniWrite(sIniFile, "Settings", "mainformY", this.Location.Y.ToString());
        }
        private void ReadSettings()
        {
            cmbBots.SelectedItem = au3.IniRead(sIniFile, "Settings", "cmbBots", "1");
            cmbBuyin.SelectedItem = au3.IniRead(sIniFile, "Settings", "cmbBuyin", "$1 + $0.10");
            cmbGameType.SelectedItem = au3.IniRead(sIniFile, "Settings", "cmbGameType", "Normal");
            cmbPlayers.SelectedItem = au3.IniRead(sIniFile, "Settings", "cmbPlayers", "9");
            cmbType.SelectedItem = au3.IniRead(sIniFile, "Settings", "cmbType", "NL");
            chkKnockout.Checked = Convert.ToBoolean(au3.IniRead(sIniFile, "Settings", "chkKnockout", "false"));
            chkDouble.Checked = Convert.ToBoolean(au3.IniRead(sIniFile, "Settings", "chkDouble", "false"));
            txtShanky.Text = au3.IniRead(sIniFile, "Settings", "txtShanky", "");
            //txtMaxMins.Text = au3.IniRead(sIniFile, "Settings", "txtMaxMins", "60");
            txtMaxSngs.Text = au3.IniRead(sIniFile, "Settings", "txtMaxSngs", "5");
            txtProfile.Text = au3.IniRead(sIniFile, "Settings", "txtProfile", "");
            txtPosX1.Text = au3.IniRead(sIniFile, "Settings", "txtPosX1", "0");
            txtPosX2.Text = au3.IniRead(sIniFile, "Settings", "txtPosX2", "0");
            txtPosX3.Text = au3.IniRead(sIniFile, "Settings", "txtPosX3", "0");
            txtPosX4.Text = au3.IniRead(sIniFile, "Settings", "txtPosX4", "0");
            txtPosX5.Text = au3.IniRead(sIniFile, "Settings", "txtPosX5", "0");
            txtPosX6.Text = au3.IniRead(sIniFile, "Settings", "txtPosX6", "0");
            txtPosY1.Text = au3.IniRead(sIniFile, "Settings", "txtPosY1", "0");
            txtPosY2.Text = au3.IniRead(sIniFile, "Settings", "txtPosY2", "0");
            txtPosY3.Text = au3.IniRead(sIniFile, "Settings", "txtPosY3", "0");
            txtPosY4.Text = au3.IniRead(sIniFile, "Settings", "txtPosY4", "0");
            txtPosY5.Text = au3.IniRead(sIniFile, "Settings", "txtPosY5", "0");
            txtPosY6.Text = au3.IniRead(sIniFile, "Settings", "txtPosY6", "0");
            int iTempX = Convert.ToInt16(au3.IniRead(sIniFile, "Settings", "mainformX", "0"));
            int iTempY = Convert.ToInt16(au3.IniRead(sIniFile, "Settings", "mainformY", "0"));
            this.Location = new System.Drawing.Point(iTempX, iTempY);
        }

        static string au3Handle(IntPtr ipOrig)
        {
            string sRV = ipOrig.ToString(); //.PadLeft(8, '0');
            if(sRV.Length>8) 
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

        public static void HideBot(IntPtr hWin)
        {
            //string au3hWin = au3Handle(hWin);
            FileFunctions.fConsoleWrite("Hiding Bot");
            WindowsAPI.WinMenuSelectItem(hWin, "&Hide", "");
            Application.DoEvents();
            winFunc.WinWait("^Going Into Hiding$", 5);
            //IntPtr hTemp = winFunc.GetWindowHandle("^Going Into Hiding$");
            if (winFunc.WinExists("^Going Into Hiding$"))
            {
                au3.ControlClick("[REGEXPTITLE:\\AGoing Into Hiding\\z]", "", "Button1", "left", 1, 0, 0);
            }
            Application.DoEvents();
        }

        public static bool BotConnected(IntPtr hWin)
        {
            bool rv = false;
            if (winFunc.WinExists(hWin))
            {
                IntPtr hMain = winFunc.GUICtrlMenu_GetMenu(hWin); // check the _GUICtrlMenu_GetMenu ++
                rv = (winFunc.GUICtrlMenu_GetItemText(hMain, 3) != "S&tart!"); // check the _GUICtrlMenu_GetItemText +++
            }
            return rv;
        }

        /* public int nBotsConnected(string[] hxShanky)
         {
             int rv = 0, a;

             for (a = 0; a < Convert.ToInt32(txtMaxSngs.Text); a++)
             {
                 rv += BotConnected(hxShanky[a]) ? 1 : 0;
             }
             return rv;
         }
         */

        public int nBotsConnected()
        {
            int rv = 0;
            IDictionaryEnumerator en = hshShanky.GetEnumerator();
            while (en.MoveNext())
            {
                rv += BotConnected((IntPtr)en.Key) ? 1 : 0;
            }
            return rv;
        }

        public static int EmptyTableIndex()
        {
            for (int a = 0; a < ipTable.Count(); a++) // check the UBound +++
            {
                if (ipTable[a] == IntPtr.Zero || !winFunc.WinExists(ipTable[a])) // ++ {"Input string was not in a correct format."}
                {
                    return a;
                }
            }
            return -10;
        }

        public static int NumTables()
        {
            int rv = 0;
            int a;
            for (a = 0; a < ipTable.Count(); a++)
            {
                if (ipTable[a] != IntPtr.Zero && winFunc.WinExists(ipTable[a]))
                    rv = rv + 1;
            }
            return rv;
        }

        public void PauseScript()
        {
            int x = 1;
            while (x == 1)
                Thread.Sleep(1000);
        }

        public bool RegisterTourney()
        {
            FileFunctions.fConsoleWrite("Starting RegisterTourney");
            int a;
            int[] iLobbyPosition;// = winFunc.GetWindowPosition(hLobby);

            int textColor;
            textColor = 0x000000;
            string numPlyrs = cmbPlayers.Text;
            int IntNumPlyrs = Convert.ToInt32(numPlyrs);
            //au3.WinActivate(sLobby, "");
            //SetForegroundWindow(iLobby);
            winFunc.SetWindowToForeground(hLobby);
            fMouseClick(30, 285);
            //au3.MouseClick("left", 30, 285, 1, 0);
            Thread.Sleep(200);
            string Game = string.Empty; // Holdem
            string GameType = string.Empty; // Turbo, SupTurbo, DoubleStack
            string Type = string.Empty; // NL
            string Buyin = string.Empty; // Buyin
            string Plrs = string.Empty; // # of players registered in tourney
            bool FoundTourney = false;
            tourneyRegOK = false;
            double range = -.1;
            int iEmptyLineCount = 0;
            for (a = 0; a < 18; a++)
            {
                iLobbyPosition = winFunc.GetWindowPosition(hLobby);

                winFunc.SetWindowToForeground(hLobby);

                int[] iPixelSearch = gfx.PixelSearch(112 + iLobbyPosition[0], 282 + (a * 15) + iLobbyPosition[1], 13, 9, 0xBE0B0B, 50, 1);
                if (iPixelSearch[0] > -1)
                    GameType = "Turbo";
                else
                {
                    //au3.PixelSearch(112 + iLobbyPosition[0], 282 + (a * 15) + iLobbyPosition[1], 125 + iLobbyPosition[0], 291 + (a * 15) + iLobbyPosition[1], 0xF1C101, 50, 1);
                    iPixelSearch = gfx.PixelSearch(112 + iLobbyPosition[0], 282 + (a * 15) + iLobbyPosition[1], 13, 9, 0xF1C101, 50, 1);
                    if (iPixelSearch[0] > -1)
                        GameType = "Super Turbo";
                    else
                        GameType = "Normal";
                }
                textColor = gfx.PixelGetColor(25 + iLobbyPosition[0], 290 + (15 * a) + iLobbyPosition[1], true); // au3.PixelGetColor(25 + iLobbyPosition[0], 290 + (15 * a) + iLobbyPosition[1]);
                Type = gfx.CleanUpString(gfx._ScanString(151, 280 + (a * 15), 181, 292 + (a * 15), textColor, range, hLobby), 1, "Ol", "01");
                Buyin = gfx.CleanUpString(gfx._ScanString(201, 280 + (a * 15), 281, 292 + (a * 15), textColor, range, hLobby), 1, "Ol", "01");
                Plrs = gfx.CleanUpString(gfx._ScanString(380, 280 + (a * 15), 449, 292 + (a * 15), textColor, range, hLobby), 1, "Ol", "01");

                //FileFunctions.fConsoleWrite("*" + Buyin + "*");
                string stype = cmbType.Text;
                string sbuyin = cmbBuyin.Text;
                FileFunctions.fConsoleWrite(GameType + "(" + cmbGameType.SelectedItem.ToString() + ")*"
                    + Type + "(" + stype + ")*"
                    + Buyin + "(" + sbuyin + ")*"
                    + Plrs + "*");
                if (Type == "" && Buyin == "" && Plrs == "")
                {
                    if (iEmptyLineCount++ > 1)
                    {
                        FileFunctions.fConsoleWrite("2 blank rows in a row found - quitting scan");
                        break;
                    }
                }

                if (Type == "" || Buyin == "" || Plrs == "")
                {
                    FileFunctions.fConsoleWrite("Incomplete Read - restarting: Type=" + Type + " Buyin=" + Buyin + " Players=" + Plrs);
                    continue;
                }
                iEmptyLineCount = 0;

                string sPlayers = Plrs.Substring(Plrs.IndexOf("OF") + 2).Trim();
                string sRegistered = Plrs.Substring(0, Plrs.IndexOf("OF") - 1).Trim();

                if (sPlayers == IntNumPlyrs.ToString() && Type == gfx.CleanUpString(stype, 1, "Ol", "01") &&
                    Buyin == gfx.CleanUpString(sbuyin, 1, "Ol", "01") && Convert.ToInt16(sRegistered) <= Convert.ToInt16(sPlayers) / 3 && cmbGameType.SelectedItem.ToString() == GameType)
                {
                    FileFunctions.fConsoleWrite("Clicking on :" + GameType + " " + Plrs + " " + Buyin);
                    fMouseClick("left", 30, (284 + (a * 15)), 1, 100, 0);
                    FoundTourney = true;
                    break;
                }
            }
            if (FoundTourney)
            {
                FileFunctions.fConsoleWrite("Clicking 'Register'");
                fMouseClick(716, 426); // au3.MouseClick("left", 716, 426, 1, 0);
                Application.DoEvents();
                TourneyBuyIn();
                Application.DoEvents();
                winFunc.WinWaitClose(".*Tournament Buy-in.*", 5);
                //tourneyRegOK = true;
                return true;
            }
            return false;
        }

        public string CheckPlayer(string PlyrString, int a, int textColor, string sLobby, double range)
        {
            string Plrs = gfx._ScanString(380, 280 + (a * 15), 449, 292 + (a * 15), textColor, range, hLobby);
            FileFunctions.fConsoleWrite(Plrs + " scrub-> ");
            //Plrs = ((Plrs.IndexOf(" ") + 2).IndexOf("of").Trim() != "");
            FileFunctions.fConsoleWrite(Plrs);
            return Plrs;
        }

        public void TourneyBuyIn()
        {

            string WinTitleTourney = "^Tournament Buy-in - .*$";
            winFunc.WinWait(WinTitleTourney, 5); // au3.WinWait("[REGEXPTITLE:\\ATournament Buy-in - .*\\z]", "", 5);
            if (winFunc.WinExists(WinTitleTourney))
            {
                FileFunctions.fConsoleWrite("Buying in - Tourney Buy In Window Exists");
                IntPtr hTourney = winFunc.GetWindowHandle(WinTitleTourney);
                winFunc.SetWindowToForeground(hTourney);
                //string WinHandle = au3.WinGetHandle(WinTitleTourney, "");
                //int iWinHandle = GetForegroundWindow();
                //+++
                do
                {
                    winFunc.SetWindowToForeground(hTourney);
                    int[] BuyinPos, BuyinPos2;
                    bool bSwitch = false;
                    BuyinPos = winFunc.GetWindowPosition(hTourney);
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(200);
                    Application.DoEvents();
                    BuyinPos2 = winFunc.GetWindowPosition(hTourney);
                    do
                    {
                        if (bSwitch)
                            BuyinPos = winFunc.GetWindowPosition(hTourney);
                        else
                            BuyinPos2 = winFunc.GetWindowPosition(hTourney);
                        bSwitch = !bSwitch;
                        Application.DoEvents();
                        System.Threading.Thread.Sleep(200);
                        Application.DoEvents();
                    } while (BuyinPos[0] != BuyinPos2[0] || BuyinPos[1] != BuyinPos2[1]
                        || BuyinPos[2] != BuyinPos2[2] || BuyinPos[3] != BuyinPos2[3]);

                    /*
                    FileFunctions.fConsoleWrite(BuyinPos[0].ToString() + ","
                        + BuyinPos[1].ToString() + ","
                        + BuyinPos[2].ToString() + ","
                        + BuyinPos[3].ToString());
                    */

                    int counter = 0;
                    int[] findcheck = gfx.PixelSearch(BuyinPos[0] + 32, BuyinPos[1] + 130, 3, BuyinPos[3] - 182, 0x808080, 0x40, 1);
                    bool checkfound = false;
                    if (findcheck[0] > -1)
                    {
                        FileFunctions.fConsoleWrite("Buyin window is stable.");
                        do
                        {
                            winFunc.SetWindowToForeground(hTourney);
                            fMouseClick("left", (findcheck[0]), (findcheck[1] + 5), 1, 100, 0);
                            counter += 1;
                            int[] findcheck2 = gfx.PixelSearch(findcheck[0], findcheck[1] + 2, 5, 5, 0x000000, 0, 1);
                            if (findcheck2[0] == -1)
                                checkfound = false;
                            else
                            {
                                FileFunctions.fConsoleWrite("Auto-buyin to same type of Sit&Go (" + findcheck[0] + 2 + "," + findcheck[1] + 4 + ")");
                                checkfound = true;
                            }
                            Application.DoEvents();
                            Thread.Sleep(100);
                        } while (checkfound == false && counter++<100);
                        if (!checkfound)
                            FileFunctions.fConsoleWrite("Unable to verify checkbox even though we've located it. (Probably due to tables popping up over the buyin window)");
                    }
                    else
                    {
                        FileFunctions.fConsoleWrite("Unable to find checkbox");
                    }
                    winFunc.SetWindowToForeground(hTourney);

                    WindowsAPI.TakeOver(hTourney);
                    WindowsAPI.SwitchWindow(hTourney);
                    WindowsAPI.SendKeyDown(WindowsAPI.VK_SPACE);
                    Thread.Sleep(100);
                    WindowsAPI.SwitchWindow(hTourney);
                    WindowsAPI.SendKeyUp(WindowsAPI.VK_SPACE);

                    winFunc.WinWaitClose(hTourney, 3);

                    if (winFunc.WinExists("^Single Click Deposit.*$") || winFunc.WinExists("^Cashier - .*$") || winFunc.WinExists("^Confirm Personal Details.*$"))
                    {
                        FileFunctions.fConsoleWrite("Deposit window detected - exiting program.");
                        ExitEvent();
                    }
                } while (winFunc.WinExists(hTourney));
                FileFunctions.fConsoleWrite("Buy-in Successful");
                tourneyRegOK = true;
            }
            else
            {
                FileFunctions.fConsoleWrite("No Tournament Buy-in Window detected");
                tourneyRegOK = false;
            }
        }

        public void MoveLobbyWindow()
        {
            //au3.WinMove(sLobby, "", 0, 0, 802, 572);
            WindowsAPI.ShowWindowAsync(hLobby, WindowsAPI.SW_RESTORE);
            winFunc.SetWindowToForeground(hLobby);
            winFunc.SetWindowPosition(hLobby, IntPtr.Zero, 0, 0, 802, 572, 0);
            int lobbyColor;
            lobbyColor = au3.PixelGetColor(32, 148);
            FileFunctions.fConsoleWrite("Check lobby theme ("+lobbyColor.ToString()+")");
            if (lobbyColor != 15706)
            {
                FileFunctions.fConsoleWrite("Changing to classic lobby theme");
                ForceClassicTheme();
            }
            else
                FileFunctions.fConsoleWrite("Lobby is already in classic theme");
            if (winFunc.WinExists("$") == false)
                sngFilters();
        }

        public bool OpenFT()
        {
            if (winFunc.WinExists("REGEXPTITLE:\\AFull Tilt Poker - Logged In As .*\\z") == false)
            {
                int i = 0;
                string FThan;
                int iFThan;
                while (winFunc.WinExists("[REGEXPTITLE:\\AFull Tilt Poker\\z]") == false)
                {
                    FileFunctions.fConsoleWrite("Opening FT");
                    au3.Run(FTloc + FTexe, FTloc, 1);
                    i = i + 1;
                    au3.WinWait("Login", "", 20);
                }
                if (winFunc.WinExists("[REGEXPTITLE:\\AFull Tilt Poker\\z]"))
                {
                    au3.WinActivate("[REGEXPTITLE:\\AFull Tilt Poker\\z]", "");
                    FThan = au3.WinGetHandle("[REGEXPTITLE:\\AFull Tilt Poker\\z]", "");
                    iFThan = GetForegroundWindow();
                }
                else
                    return false;
                if (winFunc.WinExists("Login"))
                    au3.WinActivate("Login", "");
                else
                    //au3.WinActivate("[HANDLE:" + FThan + "]", "");
                    SetForegroundWindow(iFThan);
                Thread.Sleep(1000);
                au3.Send("{ENTER}", 0);
            }
            else
            {
                au3.WinActivate("[REGEXPTITLE:\\AFull Tilt Poker +- +Logged +.*\\z]", "");
                Thread.Sleep(35);
            }
            Thread.Sleep(10);
            return true;
        }

        public void sngFilters()
        {
            int pointsColor;
            string customHan;
            //au3.MouseClick("left", 285, 214, 1, 0);
            fMouseClick(285, 214);
            //au3.MouseClick("left", 408, 251, 1, 0);
            fMouseClick(408, 251);
            //au3.MouseClick("left", 459, 253, 1, 0);
            fMouseClick(459, 253);
            au3.WinWait("Custom Filter - Tournaments", "", 1);
            customHan = au3.WinGetHandle("Custom Filter - Tournaments", "");
            int icustomHan = FindWindow("QWidget", "Custom Filter - Tournaments");
            SetForegroundWindow(icustomHan);
            SetWindowPos(icustomHan, 0, 0, 0, 593, 579, 0);
            //au3.MouseClick("left", 138, 42, 0, 1);
            fMouseClick(138, 42);
            GameChecks();
            FileFunctions.fConsoleWrite("GameChecks Successful");
            GameType();
            FileFunctions.fConsoleWrite("GameType Successful");
            BuyIn();
            FileFunctions.fConsoleWrite("Buy-in Successful");
            Players();
            FileFunctions.fConsoleWrite("Players Successful");
            TourneyType();
            FileFunctions.fConsoleWrite("TourneyType Successful");
            Handed();
            FileFunctions.fConsoleWrite("Handed Successful");
            pointsColor = au3.PixelGetColor(43, 504);
            if (pointsColor == 0)
            {
                Points();
                FileFunctions.fConsoleWrite("Points box unchecked");
            }
            FiltersOK();
        }

        public void GameChecks()
        {
            int heColor, ohlColor, omahaColor, studhlColor, studColor, razzColor, mixedGamesColor;
            heColor = au3.PixelGetColor(43, 96);
            ohlColor = au3.PixelGetColor(174, 96);
            omahaColor = au3.PixelGetColor(306, 96);
            studhlColor = au3.PixelGetColor(42, 122);
            studColor = au3.PixelGetColor(174, 122);
            razzColor = au3.PixelGetColor(306, 122);
            mixedGamesColor = au3.PixelGetColor(437, 120);
            if (heColor == 16777215)
                //au3.MouseClick("left", 43, 96, 1, 0);
                fMouseClick(43, 96);
            if (ohlColor == 0)
                //au3.MouseClick("left", 173, 96, 1, 0);
                fMouseClick(173, 96);
            if (omahaColor == 0)
                //au3.MouseClick("left", 305, 96, 1, 0);
                fMouseClick(305, 96);
            if (studhlColor == 0)
                //au3.MouseClick("left", 43, 118, 1, 0);
                fMouseClick(43, 118);
            if (studColor == 0)
                //au3.MouseClick("left", 173, 120, 1, 0);
                fMouseClick(173, 120);
            if (razzColor == 0)
                //au3.MouseClick("left", 305, 121, 1, 0);
                fMouseClick(305, 121);
            if (mixedGamesColor == 0)
                //au3.MouseClick("left", 437, 121, 1, 0);
                fMouseClick(437, 121);
        }

        public void GameType()
        {
            int nlColor, plColor, flColor, mlColor;
            string userType;
            userType = cmbType.Text;
            nlColor = au3.PixelGetColor(43, 181);
            plColor = au3.PixelGetColor(173, 181);
            flColor = au3.PixelGetColor(306, 181);
            mlColor = au3.PixelGetColor(437, 181);
            if (userType == "NL")
            {
                if (nlColor == 16777215)
                    //au3.MouseClick("left", 42, 178, 1, 0);
                    fMouseClick(42, 178);
                if (plColor == 0)
                    //au3.MouseClick("left", 173, 178, 1, 0);
                    fMouseClick(173, 178);
                if (flColor == 0)
                    //au3.MouseClick("left", 305, 178, 1, 0);
                    fMouseClick(305, 178);
                if (mlColor == 0)
                    //au3.MouseClick("left", 438, 180, 1, 0);
                    fMouseClick(438, 180);
            }
            if (userType == "PL")
            {
                if (nlColor == 0)
                    //au3.MouseClick("left", 42, 178, 1, 0);
                    fMouseClick(42, 178);
                if (plColor == 16777215)
                    //au3.MouseClick("left", 173, 178, 1, 0);
                    fMouseClick(173, 178);
                if (flColor == 0)
                    //au3.MouseClick("left", 305, 178, 1, 0);
                    fMouseClick(305, 178);
                if (mlColor == 0)
                    //au3.MouseClick("left", 438, 180, 1, 0);
                    fMouseClick(438, 180);
            }
            if (userType == "FL")
            {
                if (nlColor == 0)
                    //au3.MouseClick("left", 42, 178, 1, 0);
                    fMouseClick(42, 178);
                if (plColor == 0)
                    //au3.MouseClick("left", 173, 178, 1, 0);
                    fMouseClick(173, 178);
                if (flColor == 16777215)
                    //au3.MouseClick("left", 305, 178, 1, 0);
                    fMouseClick(305, 178);
                if (mlColor == 0)
                    //au3.MouseClick("left", 438, 180, 1, 0);
                    fMouseClick(438, 180);
            }

        }

        public void BuyIn()
        {
            string userBuyin;
            userBuyin = cmbBuyin.Text;
            if (userBuyin == "$1 + $0.10" || userBuyin == "$1 + $0.20" || userBuyin == "$1 + $0.25" || userBuyin == "$1.50 + $0.20" || userBuyin == "$1.80 + $0.20")
            {
                OneMin();
                OneMax();
            }
            if (userBuyin == "$2 + $0.10" || userBuyin == "$2 + $0.15" || userBuyin == "$2 + $0.20" || userBuyin == "$2 + $0.25" || userBuyin == "$3 + $0.30" || userBuyin == "$3.50 + $0.15" || userBuyin == "$3.50 + $0.30")
            {
                OneMin();
                FourMax();
            }
            if (userBuyin == "$4 + $0.40")
            {
                FourMin();
                FourMax();
            }
            if (userBuyin == "$5 + $0.25" || userBuyin == "$5 + $0.40" || userBuyin == "$5 + $0.50")
            {
                FiveMin();
                FiveMax();
            }
            if (userBuyin == "$6 + $0.25" || userBuyin == "$6 + $0.40" || userBuyin == "$6 + $0.50" || userBuyin == "$6 + $0.60")
            {
                SixMin();
                SixMax();
            }
            if (userBuyin == "$7 + $0.25" || userBuyin == "$7 + $0.50" || userBuyin == "$7 + $0.60" || userBuyin == "$7 + $0.70")
            {
                SixMin();
                EightMax();
            }
            if (userBuyin == "$8 + $0.70" || userBuyin == "$8 + $0.80")
            {
                EightMin();
                EightMax();
            }
            if (userBuyin == "$10 + $0.50" || userBuyin == "$10 + $0.75" || userBuyin == "$10 + $1")
            {
                TenMin();
                TenMax();
            }
            if (userBuyin == "$11 + $0.50" || userBuyin == "$11 + $0.75" || userBuyin == "$11 + $1")
            {
                ElevenMin();
                ElevenMax();
            }
            if (userBuyin == "$12 + $1" || userBuyin == "$13 + $0.75")
            {
                ElevenMin();
                FourteenMax();
            }
            if (userBuyin == "$14 + $0.50" || userBuyin == "$14 + $1")
            {
                FourteenMin();
                FourteenMax();
            }
            if (userBuyin == "$20 + $1" || userBuyin == "$20 + $1.50" || userBuyin == "$20 + $2")
            {
                TwentyMin();
                TwentyMax();
            }
            if (userBuyin == "$22 + $1" || userBuyin == "$22 + $1.50" || userBuyin == "$22 + $2")
            {
                TwentyTwoMin();
                TwentyTwoMax();
            }
            if (userBuyin == "$24 + $2")
            {
                TwentyFourMin();
                TwentyFourMax();
            }
            if (userBuyin == "$28 + $1")
            {
                TwentyFourMin();
                ThirtyMax();
            }
        }

        public void Players()
        {
            int oneTableColor, multiTableColor;
            oneTableColor = au3.PixelGetColor(43, 302);
            multiTableColor = au3.PixelGetColor(173, 302);
            if (cmbPlayers.Text == "2" || cmbPlayers.Text == "4" || cmbPlayers.Text == "6" || cmbPlayers.Text == "9")
            {
                if (oneTableColor == 16777215)
                    //au3.MouseClick("left", 42, 304, 1, 0);
                    fMouseClick(42, 304);
                if (multiTableColor == 0)
                    //au3.MouseClick("left", 173, 304, 1, 0);
                    fMouseClick(173, 304);
            }
            if (cmbPlayers.Text == "18" || cmbPlayers.Text == "27" || cmbPlayers.Text == "45" || cmbPlayers.Text == "90" || cmbPlayers.Text == "180")
            {
                if (oneTableColor == 0)
                    //au3.MouseClick("left", 42, 304, 1, 0);
                    fMouseClick(42, 304);
                if (multiTableColor == 16777215)
                    //au3.MouseClick("left", 173, 304, 1, 0);
                    fMouseClick(173, 304);
            }
        }

        public void TourneyType()
        {
            int turboColor, knockoutColor, doubleColor, rebuyColor, addonColor, shootoutColor, satelliteColor, matrixColor;
            turboColor = au3.PixelGetColor(306, 361);
            knockoutColor = au3.PixelGetColor(43, 386);
            doubleColor = au3.PixelGetColor(174, 386);
            rebuyColor = au3.PixelGetColor(43, 361);
            addonColor = au3.PixelGetColor(174, 361);
            shootoutColor = au3.PixelGetColor(438, 361);
            satelliteColor = au3.PixelGetColor(306, 303);
            matrixColor = au3.PixelGetColor(306, 386);
            if (cmbGameType.SelectedItem.ToString() != "Normal")
            {
                if (turboColor == 16777215)
                    //au3.MouseClick("left", 307, 361, 1, 0);
                    fMouseClick(307, 361);
            }
            if (cmbGameType.SelectedItem.ToString() == "Normal")
            {
                if (turboColor == 0)
                    //au3.MouseClick("left", 307, 361, 1, 0);
                    fMouseClick(307, 361);
            }
            if (chkKnockout.Checked)
            {
                if (knockoutColor == 16777215)
                    //au3.MouseClick("left", 43, 382, 1, 0);
                    fMouseClick(43, 382);
            }
            if (!chkKnockout.Checked)
            {
                if (knockoutColor == 0)
                    //au3.MouseClick("left", 43, 382, 1, 0);
                    fMouseClick(43, 382);
            }
            if (chkDouble.Checked)
            {
                if (doubleColor == 16777215)
                    //au3.MouseClick("left", 173, 387, 1, 0);
                    fMouseClick(173, 387);
            }
            if (!chkDouble.Checked)
            {
                if (doubleColor == 0)
                    //au3.MouseClick("left", 173, 387, 1, 0);
                    fMouseClick(173, 387);
            }
            if (chkSatellite.Checked)
            {
                if (satelliteColor == 16777215)
                    //au3.MouseClick("left", 303, 301, 1, 0);
                    fMouseClick(303, 301);
            }
            if (!chkSatellite.Checked)
            {
                if (satelliteColor == 0)
                    //au3.MouseClick("left", 303, 301, 1, 0);
                    fMouseClick(303, 301);
            }
            if (rebuyColor == 0)
                //au3.MouseClick("left", 43, 363, 1, 0);
                fMouseClick(43, 363);
            if (addonColor == 0)
                //au3.MouseClick("left", 174, 363, 1, 0);
                fMouseClick(174, 363);
            if (shootoutColor == 0)
                //au3.MouseClick("left", 439, 360, 1, 0);
                fMouseClick(439, 360);
            if (matrixColor == 0)
                //au3.MouseClick("left", 307, 383, 1, 0);
                fMouseClick(307, 383);
        }
        public void Handed()
        {
            int headsUpColor, sixHandedColor, nineHandedColor;
            headsUpColor = au3.PixelGetColor(42, 445);
            sixHandedColor = au3.PixelGetColor(174, 445);
            nineHandedColor = au3.PixelGetColor(306, 445);
            if (cmbPlayers.Text == "2")
            {
                if (headsUpColor == 16777215)
                    //au3.MouseClick("left", 44, 444, 1, 0);
                    fMouseClick(44, 444);
                if (sixHandedColor == 0)
                    //au3.MouseClick("left", 175, 445, 1, 0);
                    fMouseClick(175, 445);
                if (nineHandedColor == 0)
                    //au3.MouseClick("left", 305, 445, 1, 0);
                    fMouseClick(305, 445);
            }
            else if (cmbPlayers.Text == "6")
            {
                if (headsUpColor == 0)
                    //au3.MouseClick("left", 44, 444, 1, 0);
                    fMouseClick(44, 444);
                if (sixHandedColor == 16777215)
                    //au3.MouseClick("left", 175, 445, 1, 0);
                    fMouseClick(175, 445);
                if (nineHandedColor == 0)
                    //au3.MouseClick("left", 305, 445, 1, 0);
                    fMouseClick(305, 445);
            }
            else
            {
                if (headsUpColor == 0)
                    //au3.MouseClick("left", 44, 444, 1, 0);
                    fMouseClick(44, 444);
                if (sixHandedColor == 0)
                    //au3.MouseClick("left", 175, 445, 1, 0);
                    fMouseClick(175, 445);
                if (nineHandedColor == 16777215)
                    //au3.MouseClick("left", 305, 445, 1, 0);
                    fMouseClick(305, 445);
            }
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


        public void OneMin()
        {
            fMouseClick(241, 244);
            fMouseClick(223, 273);
        }
        public void OneMax()
        {
            fMouseClick(470, 240);
            fMouseClick(453, 273);
        }
        public void FourMin()
        {
            fMouseClick(245, 241);
            fMouseClick(207, 287);
        }
        public void FourMax()
        {
            au3.MouseMove(468, 240, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
            au3.MouseMove(428, 284, 1);
            au3.MouseDown("left");
            au3.MouseMove(427, 283, 1);
            au3.MouseUp("left");
        }
        public void FiveMin()
        {
            au3.MouseMove(243, 238, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
            au3.MouseMove(218, 293, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
        }
        public void FiveMax()
        {
            au3.MouseMove(468, 242, 1);
            au3.MouseDown("left");
            au3.MouseMove(467, 242, 1);
            au3.MouseUp("left");
            au3.MouseMove(440, 295, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
        }
        public void SixMin()
        {
            au3.MouseMove(244, 244, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
            au3.MouseMove(208, 312, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
        }
        public void SixMax()
        {
            au3.MouseMove(466, 242, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
            au3.MouseMove(440, 315, 1);
            au3.MouseDown("left");
            au3.MouseMove(439, 315, 1);
            au3.MouseUp("left");
        }
        public void EightMin()
        {
            au3.MouseMove(246, 242, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
            au3.MouseMove(219, 321, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
        }
        public void EightMax()
        {
            au3.MouseMove(466, 237, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
            au3.MouseMove(437, 324, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
        }
        public void TenMin()
        {
            au3.MouseMove(248, 242, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
            au3.MouseMove(216, 335, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
        }
        public void TenMax()
        {
            au3.MouseMove(466, 243, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
            au3.MouseMove(436, 337, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
        }
        public void ElevenMin()
        {
            au3.MouseMove(245, 241, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
            au3.MouseMove(214, 349, 1);
            au3.MouseDown("left");
            au3.MouseMove(213, 349, 1);
            au3.MouseUp("left");
        }
        public void ElevenMax()
        {
            au3.MouseMove(469, 240, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
            au3.MouseMove(414, 350, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
        }
        public void FourteenMin()
        {
            au3.MouseMove(246, 241, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
            au3.MouseMove(201, 362, 1);
            au3.MouseDown("left");
            au3.MouseMove(200, 362, 1);
            au3.MouseUp("left");
        }
        public void FourteenMax()
        {
            au3.MouseMove(464, 243, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
            au3.MouseMove(420, 365, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
        }
        public void TwentyMin()
        {
            au3.MouseMove(241, 240, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
            au3.MouseMove(207, 375, 1);
            au3.MouseDown("left");
            au3.MouseMove(207, 374, 1);
            au3.MouseUp("left");
        }
        public void TwentyMax()
        {
            au3.MouseMove(461, 239, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
            au3.MouseMove(431, 376, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
        }
        public void TwentyTwoMin()
        {
            au3.MouseMove(239, 238, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
            au3.MouseMove(204, 386, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
        }
        public void TwentyTwoMax()
        {
            au3.MouseMove(466, 242, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
            au3.MouseMove(430, 386, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
        }
        public void TwentyFourMin()
        {
            au3.MouseMove(243, 233, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
            au3.MouseMove(246, 280, 1);
            au3.MouseDown("left");
            au3.MouseMove(248, 294, 1);
            au3.MouseUp("left");
            au3.MouseMove(157, 361, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
        }
        public void TwentyFourMax()
        {
            au3.MouseMove(467, 243, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
            au3.MouseMove(468, 281, 1);
            au3.MouseDown("left");
            au3.MouseMove(467, 299, 1);
            au3.MouseUp("left");
            au3.MouseMove(334, 350, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
        }
        public void ThirtyMax()
        {
            au3.MouseMove(468, 240, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
            au3.MouseMove(468, 286, 1);
            au3.MouseDown("left");
            au3.MouseMove(468, 303, 1);
            au3.MouseUp("left");
            au3.MouseMove(354, 361, 1);
            au3.MouseDown("left");
            au3.MouseUp("left");
        }
        public void Points()
        {
            //au3.MouseClick("left", 43, 504, 1, 0);
            fMouseClick(43, 504);
        }

        public void FiltersOK()
        {
            //au3.MouseClick("left", 452, 537, 1, 0);
            fMouseClick(452, 537);
            //au3.MouseClick("left", 444, 553, 1, 0);
            fMouseClick(444, 553);
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

        private void ForceClassicTheme()
        {
            FileFunctions.fConsoleWrite("Forcing Classic Lobby Theme");
            au3.WinActivate(LobbyName, "");
            au3.WinWaitActive(LobbyName, "", 5);
            au3.Send("{ALT}", 0);
            au3.Send("l", 0);
            au3.Send("h", 0);
            Thread.Sleep(300);
            au3.Send("i", 0);
            Thread.Sleep(1000);
        }

        private void UpdateScrapes()
        {
            int color;
            int count = 1;
            au3.Opt("MouseCoordMode", 1);
            while (count == 1)
            {
                for (int a = 0; a < 18; a++)
                {
                    if (a % 2 == 1) // ++ Mod not recognized
                        color = 0xD8E8F7;
                    else
                        color = 0xFFFFFF;
                    if (a == 0)
                        color = 0x005490;
                    winFunc.SetWindowToForeground(hLobby);
                    //int iLobby = Convert.ToInt32(sLobby);
                    //SetForegroundWindow(iLobby);
                    fMouseClick(25, 280); // au3.MouseClick("left", 25, 280, 1, 0);
                    FileFunctions.fConsoleWrite(gfx._ScanString(25, 280 + (15 * a), 450, 293 + (15 * a), color, -.1, hLobby));
                }
            }
        }

        private void ExitEvent()
        {
            //Do any cleanup here.
            //For example, closing bots/etc.
            notifyIcon1.Visible = false;

            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int oldDataEntry = gfx.dataentry;
            gfx.dataentry = 1;
            IntPtr hWin = winFunc.GetWindowHandle("^Full Tilt Poker - Logged .*$");
            winFunc.SetWindowPosition(hWin, IntPtr.Zero, 0, 0, 802, 568, 0); // au3.WinMove(hWin, "", 0, 0, 802, 568);
            winFunc.SetWindowToForeground(hWin);
            FileFunctions.fConsoleWrite("Starting Update");
            for (int y = 280; y < 280 + (15 * 18); y += 15)
            {
                //   au3.MouseMove(25, y,0);
                // Console.ReadLine();
                FileFunctions.fConsoleWrite(gfx._ScanString(25, y, 461, y + 13, -1, -1, hWin));
                //break;
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
            bool pause;
            pause = false;
            pause = !pause;
            if (pause == true)
                FileFunctions.fConsoleWrite("Pausing");
            else
                FileFunctions.fConsoleWrite("Unpausing");
            while (pause == true)
                Thread.Sleep(1000);
        }

        private void btnSet1_Click(object sender, EventArgs e)
        {
            PositionWindow(1);
        }

        private void PositionWindow(int winNum)
        {
            if (winNum == 1)
            {
                GetDummyPosition(ref txtPosX1, ref txtPosY1, 1);
            }
            else if (winNum == 2)
            {
                GetDummyPosition(ref txtPosX2, ref txtPosY2, 2);
            }
            else if (winNum == 3)
            {
                GetDummyPosition(ref txtPosX3, ref txtPosY3, 3);
            }
            else if (winNum == 4)
            {
                GetDummyPosition(ref txtPosX4, ref txtPosY4, 4);
            }
            else if (winNum == 5)
            {
                GetDummyPosition(ref txtPosX5, ref txtPosY5, 5);
            }
            else
            {
                GetDummyPosition(ref txtPosX6, ref txtPosY6, 6);
            }
        }

        private void GetDummyPosition(ref TextBox txtX, ref TextBox txtY, int winNum)
        {
            DummyWindow formDummy = new DummyWindow(this);
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
            SaveSettings();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            for (int a = 1; a <= 6; a++)
                PositionWindow(a);
        }


        private static void tmrEvntCheckNewTables(Object myObject, EventArgs myEventArgs)
        {
            thdCheckForNewTables();
            CheckForStrayBots();
        }

        private static void tmrEvntCloseBadWindows(Object myObject, EventArgs myEventArgs)
        {
            thdCloseStrayWindows();
        }

        private static void tmrEvntTournamentAnnouncements(Object myObject, EventArgs myEventArgs)
        {
            thdTournamentAnnouncements();
        }

        private static void tmrEvntUpdateStatusText(Object myObject, EventArgs myEventArgs)
        {
            thdUpdateStatusText();
        }

        public void StartAutoPilot()
        {
            sShankyPath = txtShanky.Text;
            textBot = txtShanky.Text;
            textProf = txtProfile.Text;
            //textBatch = txtBatch.Text;
            userNumSng = txtMaxSngs.Text;
            while (!Microsoft.VisualBasic.Information.IsNumeric(userNumSng))
            {
                userNumSng = Microsoft.VisualBasic.Interaction.InputBox("Please enter number of SNGs to play", appName, "1", -1, -1);
            }
            txtMaxSngs.Text = userNumSng;
            userNumSngInt = Convert.ToInt32(userNumSng);
            //userNumMins = txtMaxMins.Text;
/*            while (!Microsoft.VisualBasic.Information.IsNumeric(userNumMins))
            {
                userNumMins = Microsoft.VisualBasic.Interaction.InputBox("Please enter number of mins to play", appName, "90", -1, -1);
            }
 */
            //txtMaxMins.Text = userNumMins;
//            userNumMinsInt = Convert.ToInt32(userNumMins);
            int TournamentsToPlay;
            textProf = txtProfile.Text;
            //textBatch = txtBatch.Text;
            //au3.Run(textBatch, "", 0); // +++ these don't run (think we need exact location
            //FileFunctions.fConsoleWrite("Batch successful");
            //Thread.Sleep(200);
            //au3.Run(textBatch, "", 0); // +++ these don't run (think we need exact location
            //FileFunctions.fConsoleWrite("Batch successful");
            //FileFunctions.BackupLogs(textBot);
            //Thread.Sleep(500);

            while (winFunc.WinExists("Calculator"))
            {
                FileFunctions.fConsoleWrite("Closing loose bot");
                au3.WinClose("Calculator", "");
                Thread.Sleep(100);
            }
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

            //hxShanky = new string[iBotsToPlay];
            //tTournaments = new string[iBotsToPlay];
            //hTournaments = new string[iBotsToPlay];
            //hxTable = new string[iBotsToPlay];
            ipShanky = new IntPtr[iBotsToPlay];
            ipTable = new IntPtr[iBotsToPlay];
            for (a = 0; a < iBotsToPlay; a++)
            {
                ipShanky[a] = IntPtr.Zero;
                ipTable[a] = IntPtr.Zero;
                //hxShanky[a] = "blank";
                //hxTable[a] = "blank";
            }
            int counter = 0;
            while (winFunc.WinExists(TournamentLobbyTitleHash) && counter++<5)
            {
                FileFunctions.fConsoleWrite("Closing tournament window");
                winFunc.WinClose(TournamentLobbyTitleHash);
            }
            FileFunctions.fConsoleWrite("All loose Tournament Lobbies closed");
            string lastMessage = "";

            string sSecsDelay = FileFunctions.IniRead(sIniFile, "Settings", "SecondsBetweenTourneyJoin", "90"); // *** OK - let's set it to a string for now. ++ cannot convert string to int
            dtStarted = new DateTime();
            dtStarted = DateTime.Now; // *** OK 'System.DateTime.Now cannot be used like a method ++
            tsLastTournamentJoin = new DateTime();
            tsLastTournamentJoin = DateTime.Now;
            noTourney = true;
            //string NumMinstoRun = txtMaxMins.Text;
            //int IntNumMinsToRun = Convert.ToInt32(NumMinstoRun);
            /*
            if ((DateTime.Now - dtStarted).TotalSeconds > IntNumMinsToRun)
            {
                FileFunctions.fConsoleWrite("Not going to run - timer says:" + (DateTime.Now - dtStarted).TotalSeconds + " secs."); // ++ *** UNTESTED (errors)
            }
             */
            //string CloseAnnouncmentText = "";

            /*
            threadUpdateStatusText.Priority = ThreadPriority.Lowest;
            threadUpdateStatusText.Name = "UpdateStatusText";
            threadUpdateStatusText.Start();

            threadCheckForNewTables.Priority = ThreadPriority.Normal;
            threadCheckForNewTables.Name = "CheckForNewTables";
            threadCheckForNewTables.Start();

            threadTournamentAnnouncements.Priority = ThreadPriority.Normal;
            threadTournamentAnnouncements.Name = "AnnouncementHandling";
            threadTournamentAnnouncements.Start();

            threadCloseBadWindows.Priority = ThreadPriority.Lowest;
            threadCloseBadWindows.Name = "CloseStrays";
            threadCloseBadWindows.Start();
            */

            tmrCheckNewTables = new System.Windows.Forms.Timer();
            tmrCloseBadWindows = new System.Windows.Forms.Timer();
            tmrTournamentAnnouncements = new System.Windows.Forms.Timer();
            tmrUpdateStatusText = new System.Windows.Forms.Timer();

            tmrCheckNewTables.Interval = 1000;
            tmrCheckNewTables.Tick += new EventHandler(tmrEvntCheckNewTables);
            tmrCheckNewTables.Start();

            tmrCloseBadWindows.Interval = 5000;
            tmrCloseBadWindows.Tick += new EventHandler(tmrEvntCloseBadWindows);
            tmrCloseBadWindows.Start();

            tmrTournamentAnnouncements.Interval = 500;
            tmrTournamentAnnouncements.Tick += new EventHandler(tmrEvntTournamentAnnouncements);
            tmrTournamentAnnouncements.Start();

            tmrUpdateStatusText.Interval = 10000;
            tmrUpdateStatusText.Tick += new EventHandler(tmrEvntUpdateStatusText);
            tmrUpdateStatusText.Start();


            string lastJoinTourneyMessage = "";
            /// **** MAIN LOOP
            while (iTablesFinished < Convert.ToInt32(txtMaxSngs.Text) || WaitingForTournament || NumTables() > 0) // && iTablesFinished < Convert.ToInt32(txtMaxSngs.Text) && (DateTime.Now - dtStarted).Minutes < Convert.ToInt32(NumMinstoRun))
            {
                Console.Write(".");
                if (OldiTablesFinished != iTablesFinished)
                {
                    FileFunctions.fConsoleWrite("Tables finished so far: " + iTablesFinished);
                    OldiTablesFinished = iTablesFinished;
                }
                txtSngsElapsed.Text = (iTournamentsRegistered.ToString() + "/" + userNumSng.ToString());
                txtMinsElapse.Text = (DateTime.Now - dtStarted).Minutes + " mins.";

                //object[,] winlist;
                Hashtable hshWinList;
                if (iLiveBots < iBotsToPlay)
                {
                    if (iTournamentsRegistered < userNumSngInt)
                    {
                        if ((noTourney || (DateTime.Now - tsLastTournamentJoin).TotalSeconds >= Convert.ToInt32(sSecsDelay)))
                        {
                            if (NumTables() <= iLiveBots)
                            {
                                if (!WaitingForTournament)
                                {
                                    //winlist = (object[,])au3.WinList(TableTitle, "");
                                    hshWinList = winFunc.WinList(sTableTitleHash);
                                    if (hshWinList.Count <= NumTables())
                                    {
                                        FileFunctions.fConsoleWrite("Stable - Adding new tournament");
                                        WaitingForTournament = true;
                                        bLockThreads = true;
                                        SuspendThreads();
                                        RegisterTourney();
                                        ResumeThreads();
                                        bLockThreads = false;
                                        if (tourneyRegOK)
                                            iTournamentsRegistered += 1;
                                        else
                                            WaitingForTournament = false;
                                    }
                                    else
                                        lastJoinTourneyMessage = "Not starting new tournament because some tables do not have handles.";
                                }
                                else
                                    lastJoinTourneyMessage = ("Not starting new tournament because we are waiting for a tournament.");
                            }
                            else
                                lastJoinTourneyMessage = "Not starting new tournament because we have more tables(" + NumTables() + ") than bots(" + iLiveBots + ") connected";
                        }
                        else
                            lastJoinTourneyMessage = "Not starting new tournament because we are delaying: " + sSecsDelay + " secs";
                    }
                    else
                        lastJoinTourneyMessage = "Not starting new tournament because tournaments registered(" + iTournamentsRegistered + ") >= # to play(" + userNumSng + ")";
                }
                if (lastMessage != lastJoinTourneyMessage)
                {
                    lastMessage = lastJoinTourneyMessage;
                    FileFunctions.fConsoleWrite(lastJoinTourneyMessage);
                }
                Application.DoEvents();
                // Thread.Sleep(1001 * Convert.ToInt32(sSecsDelay));
                Thread.Sleep(500);
            }
            bLockThreads = true;
            SuspendThreads();

            FileFunctions.fConsoleWrite("Done");
            this.ExitEvent();
        }

        static public void LockThreadsCheck()
        {
            while (bLockThreads)
            {
                Application.DoEvents();
            }
        }

        private static void thdTournamentAnnouncements()
        {
            if (!bLockThreads)
            {
                //LockThreadsCheck();
                Console.Write("*");
                #region Deal with tournament lobbies - if tourney active, minimize - else Close
                if (winFunc.WinExists(TournamentLobbyTitleHash))
                {
                    bLockThreads = true;
                    Hashtable hshWinList = winFunc.WinList(TournamentLobbyTitleHash);
                    IDictionaryEnumerator idWinList = hshWinList.GetEnumerator();

                    while (idWinList.MoveNext())
                    {
                        Application.DoEvents();
                        if (idWinList.Value.ToString() != "" && WindowsAPI.IsWindowVisible((IntPtr)idWinList.Key))
                        {
                            string TID;
                            TID = idWinList.Value.ToString();
                            while (TID.IndexOf(" ") > 0)
                                TID = TID.Substring(TID.IndexOf(" ") + 1);
                            if (winFunc.WinExists(".*(" + TID + ").*$"))
                            {
                                if (!WindowsAPI.IsIconic((IntPtr)idWinList.Key)) //(((au3.WinGetState(au3Handle(idWinList.Key.ToString()), "")) & 16) != 16)
                                    WindowsAPI.ShowWindowAsync((IntPtr)idWinList.Key, WindowsAPI.SW_SHOWMINIMIZED);
                                    //au3.WinSetState(au3Handle(idWinList.Key.ToString()), "", au3.SW_MINIMIZE);
                            }
                            else
                            {
                                if (winFunc.WinExists((IntPtr)idWinList.Key))
                                    winFunc.WinClose((IntPtr)idWinList.Key);
                            }
                        }
                    }
                    bLockThreads = false;
                }
                #endregion

                Application.DoEvents();
                if (winFunc.WinExists("^.* Go Rematch.*$") || winFunc.WinExists("^Tournament Announcement$") || winFunc.WinExists("^Full Tilt Poker$"))
                {
                    bool FoundTable = false;
                    int xOffset = 0;
                    int yOffset = 0;
                    IntPtr ipAnnouncement = IntPtr.Zero;
                    int counter = 0;
                    int[] iTablePosition;
                    string sAnnouncement = "^Tournament Announcement$";

                    if (!winFunc.WinExists(sAnnouncement))
                    {
                        sAnnouncement = "^.* Go Rematch.*$";
                    }
                    if (winFunc.WinExists(sAnnouncement))
                    {
                        ipAnnouncement = winFunc.GetWindowHandle(sAnnouncement);
                    }
                    else
                    {
                        Hashtable hshFTPWindows = winFunc.WinList("^Full Tilt Poker$");
                        if (hshFTPWindows.Count > 0)
                        {
                            IDictionaryEnumerator idFTPWindows = hshFTPWindows.GetEnumerator();

                            while (idFTPWindows.MoveNext())
                            {
                                if (WindowsAPI.IsWindowVisible((IntPtr)idFTPWindows.Key))
                                {
                                    ipAnnouncement = (IntPtr)idFTPWindows.Key;
                                    break;
                                }
                                Application.DoEvents();
                            }
                        }
                    }
                    if (ipAnnouncement != IntPtr.Zero)
                    {
                        bLockThreads = true;
                        SuspendThreads(ThreadNames.TournamentAnnouncements);
                        iTablePosition = winFunc.GetWindowPosition(ipAnnouncement);

                        if (winFunc.WinExists(ipAnnouncement) && WindowsAPI.IsWindowVisible(ipAnnouncement))
                        {
                            FileFunctions.fConsoleWrite("Closing final table announcment");

                            xOffset = (int)((802 - iTablePosition[2]) / 2);
                            yOffset = (int)((574 - iTablePosition[3]) / 2);

                            int FinishedTable = -1;
                            string TempTitle = "";

                            counter = 0;
                            while (!FoundTable && counter < 3)
                            {
                                Application.DoEvents();
                                counter += 1;
                                for (int ia = 0; ia < ipTable.Count(); ia++)
                                {
                                    Application.DoEvents();
                                    //if (hxTable[ia] != "blank" && au3.WinExists("[HANDLE:" + hxTable[ia] + "]", "") == 1 && hxTable[ia] != "" && Convert.ToInt32(hxTable[ia], 16) != 0)
                                    if (ipTable[ia] != IntPtr.Zero && winFunc.WinExists(ipTable[ia]))
                                    {
                                        FileFunctions.fConsoleWrite("Checking table " + rgx.Replace(winFunc.GetWindowTextX(ipTable[ia]), UserNameReplacement));
                                        //int win2posX, win2posY;
                                        //win2posX = au3.WinGetPosX("[HANDLE:" + hxTable[ia] + "]", "");
                                        //win2posY = au3.WinGetPosY("[HANDLE:" + hxTable[ia] + "]", "");
                                        int[] iAnnouncementPosition = winFunc.GetWindowPosition(ipTable[ia]);

                                        if (Math.Abs(Math.Abs(iTablePosition[0] - iAnnouncementPosition[0]) - xOffset) <= 2 &&
                                            Math.Abs(Math.Abs(iTablePosition[1] - iAnnouncementPosition[1]) - yOffset) <= 2)
                                        {
                                            FinishedTable = a;
                                            FoundTable = true;
                                            if (ipShanky[ia] != IntPtr.Zero && winFunc.WinExists(ipShanky[ia])) //(hxShanky[ia] != "" && Convert.ToInt32(hxShanky[ia], 16) != 0 && au3.WinExists("[HANDLE:" + hxShanky[ia] + "]", "") == 1)
                                            {
                                                TempTitle = au3.WinGetTitle(au3Handle(ipTable[ia]), "");

                                                FileFunctions.fConsoleWrite("Found corresponding table - stopping bot");
                                                iLiveBots -= 1;
                                                winFunc.WinClose(ipShanky[ia]);
                                                Application.DoEvents();
                                            }
                                            break;
                                        }
                                        else
                                        {
                                            FileFunctions.fConsoleWrite("Announcement " + iAnnouncementPosition[0] + "," + iAnnouncementPosition[1] +
                                                " <> Table @ (" + iTablePosition[0] + "," + iTablePosition[1] + ") by (" + xOffset + "," + yOffset + ")");
                                            Thread.Sleep(1000);
                                        }
                                    }
                                }
                            }

                            if (!FoundTable)
                                FileFunctions.fConsoleWrite("Couldn't find which window the announcement window belongs to this attempt - aborting until next cycle.");
                            else
                            {
                                if (iPlayers == 2 && winFunc.GetWindowTextX(ipAnnouncement).IndexOf(" Go Rematch") >= 0)
                                {
                                    FileFunctions.fConsoleWrite("Closing Heads-Up rematch window");

                                    au3.WinActivate(au3Handle(ipAnnouncement), "");
                                    Application.DoEvents();
                                    au3.WinWaitActive(au3Handle(ipAnnouncement), "", 1);
                                    au3.Send("{TAB}{SPACE}", 0);
                                    Thread.Sleep(200);
                                    Application.DoEvents();
                                    au3.WinActivate(au3Handle(ipAnnouncement), "");
                                    Application.DoEvents();
                                    au3.WinWaitActive(au3Handle(ipAnnouncement), "", 1);
                                    au3.Send("{TAB}{SPACE}", 0);
                                    Thread.Sleep(1000);
                                    Application.DoEvents();
                                    if (!winFunc.WinExists(ipAnnouncement))
                                        iTablesFinished += 1;
                                }
                                else if (winFunc.GetWindowTextX(ipAnnouncement).IndexOf("Tournament Announcement") >= 0)
                                {
                                    FileFunctions.fConsoleWrite("Closing Tournament Announcement");
                                    winFunc.WinClose(ipAnnouncement);
                                    Application.DoEvents();
                                    winFunc.WinWaitClose(ipAnnouncement, 1);
                                    if (!winFunc.WinExists(ipAnnouncement))
                                        iTablesFinished++;
                                }
                                else
                                {
                                    FileFunctions.fConsoleWrite("Closing Rare 'Full Tilt Poker: Congrats' window");
                                    do
                                    {
                                        au3.WinActivate(au3Handle(ipAnnouncement), "");
                                        Application.DoEvents();
                                        au3.WinWaitActive(au3Handle(ipAnnouncement), "", 1);
                                        if (au3.WinActive(au3Handle(ipAnnouncement), "") == 1)
                                        {
                                            au3.Send("{TAB}{SPACE}", 0);
                                            Application.DoEvents();
                                            Thread.Sleep(200);
                                            au3.WinActivate(au3Handle(ipAnnouncement), "");
                                            Application.DoEvents();
                                            au3.WinWaitActive(au3Handle(ipAnnouncement), "", 1);
                                            au3.Send("{TAB}{SPACE}", 0);
                                        }
                                        winFunc.WinWaitClose(ipAnnouncement, 2);
                                    } while (winFunc.WinExists(ipAnnouncement));
                                    Application.DoEvents();
                                    if ((FinishedTable >= 0 && FinishedTable < ipTable.Count()) && ipTable[FinishedTable] != IntPtr.Zero && winFunc.WinExists(ipTable[FinishedTable]))
                                        winFunc.WinClose(ipTable[FinishedTable]);
                                    if (TempTitle != "")
                                    {
                                        TempTitle = TempTitle.Substring(0, TempTitle.IndexOf("),") - 1);
                                        while (TempTitle.IndexOf("(") > -1)
                                            TempTitle = TempTitle.Substring(TempTitle.IndexOf("(") + 1);
                                        TempTitle = "^Full Tilt Poker - Tournament " + TempTitle + ".*";
                                        winFunc.WinWait(TempTitle, 2);
                                        if (winFunc.WinExists(TempTitle))
                                            winFunc.WinClose(TempTitle);
                                    }
                                    if (!winFunc.WinExists(ipAnnouncement))
                                        iTablesFinished += 1;
                                }
                            }
                        }
                        ResumeThreads(ThreadNames.TournamentAnnouncements);
                        bLockThreads = false;
                    }
                }
                ResumeThreads(ThreadNames.TournamentAnnouncements);
            }
        }

        private static void thdCheckForNewTables()
        {
            if (!bLockThreads)
            {
                Console.Write("+");

                Hashtable hshWinList = winFunc.WinList(sTableTitleHash);
                if (hshWinList.Count > NumTables())
                {
                    SuspendThreads(ThreadNames.CheckForNewTables);

                    int iNewTableIndex = -10;
                    string NewTableTitle = string.Empty;
                    string TourneyNumber = string.Empty;

                    IDictionaryEnumerator en = hshWinList.GetEnumerator();

                    while (en.MoveNext()) //for (int z = 0; z < hshWinList.Count; z++)
                    {
                        if (Array.IndexOf(ipTable, en.Key) == -1) //Make sure the table is visible before we start to play with it.
                        {
                            WindowsAPI.SwitchWindow((IntPtr)en.Key);
                            Application.DoEvents();
                            WindowsAPI.SwitchWindow((IntPtr)en.Key);
                            int[] iPosition = winFunc.GetWindowPosition((IntPtr)en.Key);
                            if (iPosition[2] >= 200 && iPosition[3] >= 200)
                            {
                                FileFunctions.fConsoleWrite("New Table detected.");
                                //This table's handle is unaccounted for.
                                NewTableTitle = en.Value.ToString();

                                TourneyNumber = NewTableTitle;
                                TourneyNumber = TourneyNumber.Substring(0, (TourneyNumber.IndexOf("),")) - 1);
                                while ((TourneyNumber.IndexOf("(")) > 0)
                                    TourneyNumber = TourneyNumber.Substring((TourneyNumber.IndexOf("(")) + 1);

                                if (hshActiveTournamentIDs.ContainsKey(TourneyNumber))
                                {
                                    FileFunctions.fConsoleWrite("Table Change for tournament #" + TourneyNumber);

                                    int iOldTableIndex = (int)hshActiveTournamentIDs[TourneyNumber];

                                    string x, y;
                                    x = au3.IniRead(sIniFile, "settings", "txtPosX" + (iOldTableIndex + 1).ToString(), "0");
                                    y = au3.IniRead(sIniFile, "settings", "txtPosY" + (iOldTableIndex + 1).ToString(), "0");

                                    winFunc.SetWindowPosition((IntPtr)en.Key, IntPtr.Zero, Convert.ToInt32(x), Convert.ToInt32(y), 802, 572, 0); //au3.WinMove("[HANDLE:" + hxTable[oldPointer] + "]", "", Convert.ToInt32(x), Convert.ToInt32(y), 802, 574);

                                    ipTable[iOldTableIndex] = (IntPtr)en.Key;

                                    break;
                                }

                                WaitingForTournament = false;
                                iNewTableIndex = EmptyTableIndex();
                                if (iNewTableIndex == -10)
                                {
                                    FileFunctions.fConsoleWrite("No new table handles avialable!");
                                    return;
                                }

                                ipTable[iNewTableIndex] = (IntPtr)en.Key; //hxTable[iNewTableIndex] = winlist[1, z].ToString();

                                string x1, y1;
                                x1 = au3.IniRead(sIniFile, "settings", "txtPosX" + (iNewTableIndex + 1).ToString(), "0");
                                y1 = au3.IniRead(sIniFile, "settings", "txtPosY" + (iNewTableIndex + 1).ToString(), "0");
                                winFunc.SetWindowPosition((IntPtr)en.Key, IntPtr.Zero, Convert.ToInt32(x1),
                                    Convert.ToInt32(y1), 802, 572, 0); //au3.WinMove(hxTable[iNewTableIndex], "", posx, posy, 802, 574);

                                bool SuccessfulConnect = false;

                                while (!SuccessfulConnect)
                                {
                                    Application.DoEvents();
                                    FoundNewBot = false;
                                    while (!FoundNewBot)
                                    {
                                        Application.DoEvents();

                                        Hashtable hshBotList = winFunc.WinList(bottitle); //winlist = (object[,])au3.WinList(bottitle, "");

                                        if (hshBotList.Count > 0)
                                        {
                                            IDictionaryEnumerator idBots = hshBotList.GetEnumerator();

                                            while (idBots.MoveNext())
                                            {
                                                if (Array.IndexOf(ipShanky, idBots.Key) == -1)
                                                {
                                                    FileFunctions.fConsoleWrite("Bot Opened successfully - handle assigned");

                                                    ipShanky[iNewTableIndex] = (IntPtr)idBots.Key; //hxShanky[iNewTableIndex] = winlist[1, x].ToString();
                                                    FoundNewBot = true;

                                                    int[] iShankyPosition = new int[4];
                                                    iShankyPosition = winFunc.GetWindowPosition(ipShanky[iNewTableIndex]);
                                                    winFunc.SetWindowPosition(ipShanky[iNewTableIndex], IntPtr.Zero,
                                                        Convert.ToInt32(FileFunctions.IniRead(sIniFile, "Settings", "BotX", "0")),
                                                        Screen.PrimaryScreen.Bounds.Width - iShankyPosition[2] - 25, iShankyPosition[2], iShankyPosition[3], 0);

                                                    break;
                                                }
                                            }
                                        }
                                        if (!FoundNewBot)
                                        {
                                            if (hshBotList.Count == 0)
                                            {
                                                BackupLogs(textBot, 1);
                                                FileFunctions.fConsoleWrite("Launching Bot");
                                                Process procRun = new Process();
                                                procRun.StartInfo.FileName = sShankyPath;
                                                procRun.StartInfo.WorkingDirectory = sShankyPath.Substring(0, sShankyPath.LastIndexOf("\\") + 1);
                                                procRun.Start();  //Theoretically we can watch the process to see if it exited/etc.
                                                //procRun.Handle   <<<<  potential increase in speed so we dont need to loop thru all the handles? No - because we mostly launch from existing windows, so we need to loop anyhow. :(
                                                int counter = 0;
                                                while (!winFunc.WinExists(bottitle) && counter < 50)
                                                {
                                                    Application.DoEvents();
                                                    counter++;
                                                    Thread.Sleep(100);
                                                }
                                            }
                                            else
                                            {
                                                BackupLogs(textBot, hshBotList.Count + 1);
                                                IDictionaryEnumerator idBots = hshBotList.GetEnumerator();
                                                idBots.MoveNext();

                                                FileFunctions.fConsoleWrite("Starting New Window from existing bot");
                                                SuspendThreads(ThreadNames.CheckForNewTables);
                                                WindowsAPI.WinMenuSelectItem((IntPtr)idBots.Key, "&Holdem", "&New Window");
                                                ResumeThreads(ThreadNames.CheckForNewTables);
                                                int counter = 0;
                                                int iOrigBotCount = hshBotList.Count;
                                                while (hshBotList.Count == iOrigBotCount && counter < 50)
                                                {
                                                    Application.DoEvents();
                                                    counter++;
                                                    hshBotList = winFunc.WinList(bottitle);
                                                    Thread.Sleep(100);
                                                }
                                            }
                                            Application.DoEvents();
                                        }
                                    }
                                    SuspendThreads(ThreadNames.CheckForNewTables);
                                    WindowsAPI.WinMenuSelectItem(ipShanky[iNewTableIndex], "&Holdem", "&Read Profile...");
                                    Application.DoEvents();
                                    winFunc.WinWait("^Read Profile from File$", 2); //au3.WinWait("[REGEXPTITLE:\\ARead Profile from File\\z]", "", 1);
                                    IntPtr hReadProfile = winFunc.GetWindowHandle("^Read Profile from File$");
                                    au3.ControlSetText("[REGEXPTITLE:\\ARead Profile from File\\z]", "", "Edit1", textProf);
                                    Application.DoEvents();
                                    int xcounter = 0;
                                    while (winFunc.WinExists("^Read Profile from File$") && xcounter++ < 100)
                                    {
                                        au3.ControlClick("[REGEXPTITLE:\\ARead Profile from File\\z]", "", "Button1", "left", 1, 0, 0);
                                        Application.DoEvents();
                                    }
                                    xcounter = 0;
                                    while (winFunc.WinExists("^Read Profile from File$") && xcounter++ < 100)
                                    {
                                        WindowsAPI.SwitchWindow(hReadProfile);
                                        WindowsAPI.PostMessage(hReadProfile, WindowsAPI.WM_KEYDOWN, WindowsAPI.VK_RETURN, 0);
                                        Thread.Sleep(100);
                                        Application.DoEvents();
                                        WindowsAPI.PostMessage(hReadProfile, WindowsAPI.WM_KEYUP, WindowsAPI.VK_RETURN, 0);
                                        Application.DoEvents();
                                    }
                                    FileFunctions.fConsoleWrite("Profile " + textProf + " loaded");
                                    winFunc.WinWaitClose("^Read Profile from File$", 2); //au3.WinWait("[REGEXPTITLE:\\ARead Profile from File\\z]", "", 1);
                                    WindowsAPI.WinMenuSelectItem(ipShanky[iNewTableIndex], "S&tart!", "");
                                    Application.DoEvents();
                                    ResumeThreads(ThreadNames.CheckForNewTables);

                                    //winlist = (object[,])au3.WinList(bottitle, "");
                                    int icounter = 0;
                                    while (icounter < 100 && !BotConnected(ipShanky[iNewTableIndex]))
                                    {
                                        icounter += 1;
                                        Thread.Sleep(100);
                                        Application.DoEvents();
                                    }
                                    if (BotConnected(ipShanky[iNewTableIndex]))
                                    {
                                        FileFunctions.fConsoleWrite("Bot Connected");
                                        //HideBot("[HANDLE:" + hxShanky[iNewTableIndex] + "]");
                                        iLiveBots += 1;
                                        SuccessfulConnect = true;
                                        tsLastTournamentJoin = DateTime.Now; // +++
                                        noTourney = false;
                                        do
                                        {
                                            //au3.WinSetOnTop(au3Handle(ipTable[iNewTableIndex]), "", 1);
                                            //au3.WinActivate(au3Handle(ipTable[iNewTableIndex]), "");
                                            //au3.WinWaitActive(au3Handle(ipTable[iNewTableIndex]), "", 2);
                                            SuspendThreads(ThreadNames.CheckForNewTables);

                                            winFunc.SetWindowToForeground(ipTable[iNewTableIndex]);

                                            //postableX = au3.WinGetPosX("[HANDLE:" + hxTable[iNewTableIndex] + "]", "");
                                            //postableY = au3.WinGetPosY("[HANDLE:" + hxTable[iNewTableIndex] + "]", "");
                                            int[] iTablePosition = winFunc.GetWindowPosition(ipTable[iNewTableIndex]);
                                            string checkColor;

                                            checkColor = gfx.PixelGetColor(iTablePosition[0] + 700, iTablePosition[1] + 542);

                                            if (checkColor == "0B2517" || checkColor == "0F3A5E")// "076686") 0F3A5E
                                            {
                                                fMouseClick("left", iTablePosition[0] + 700, iTablePosition[1] + 542, 1, 100, 0); //au3.MouseClick("left", postableX + 662, postableY + 542, 1, 0);
                                                FileFunctions.fConsoleWrite("Click `I'm Ready` at " + (iTablePosition[0] + 700).ToString() + "," + (iTablePosition[1] + 542).ToString());
                                                break;
                                            }
                                            else
                                            {
                                                FileFunctions.fConsoleWrite("No `I'm Ready` button :" + checkColor + " at " + (iTablePosition[0] + 700).ToString() + "," + (iTablePosition[1] + 542).ToString());
                                            }
                                            ResumeThreads(ThreadNames.CheckForNewTables);

                                            Application.DoEvents();
                                            Thread.Sleep(1000);
                                        } while ((DateTime.Now - tsLastTournamentJoin).TotalSeconds <= 3);
                                    }
                                    else
                                    {
                                        FileFunctions.fConsoleWrite("Something went wrong with table connect.  Retrying from scratch in 4 seconds.");
                                        Application.DoEvents();
                                        winFunc.WinClose(ipShanky[iNewTableIndex]);  // au3.WinClose("[HANDLE:" + hxShanky[iNewTableIndex] + "]", "");
                                        Application.DoEvents();
                                        Thread.Sleep(4000);
                                    }
                                    ResumeThreads(ThreadNames.CheckForNewTables);

                                }
                                FileFunctions.fConsoleWrite("New tournament # discovered: " + TourneyNumber);

                                hshActiveTournamentIDs.Add(TourneyNumber, iNewTableIndex);

                                break;
                            }
                            else
                            {
                                FileFunctions.fConsoleWrite("New table detected - however, it appears to be too small to register (200x200). This could happen if the table takes too long to draw. Make sure your 'animate window' effects are turned off in the poker client and for Windows.");
                            }
                        }
                    }
                }
                ResumeThreads(ThreadNames.CheckForNewTables);

                Application.DoEvents();
            }
        }

        private static void thdUpdateStatusText()
        {

        }

        private static void thdCloseStrayWindows()
        {
            if (!bLockThreads)
            {
                string[] badwindows = {
                                           ".*Close Me Bitch.*",
                                           "^Sessione sponsorizzata$"
                                       };
                int[] iCloseByButton = {
                                           0,
                                           1
                                       };
                Hashtable hshBadWindows;// = winFunc.WinList("^Sessione sponsorizzata$");
                // LockThreadsCheck();
                Console.Write("_");
                for (int a = 0; a < badwindows.Count(); a++)
                {
                    hshBadWindows = winFunc.WinList(badwindows[a]);
                    IDictionaryEnumerator en = hshBadWindows.GetEnumerator();
                    while (en.MoveNext())
                    {
                        if (iCloseByButton[a] == 1)
                            winFunc.ControlClick((IntPtr)en.Key, "", "Button1", "left", 1);
                        else
                            winFunc.WinClose((IntPtr)en.Key);
                    }
                }
                Application.DoEvents();
            }
        }

        public delegate void dUpdatetxtSngsElapsed(string text);
        public delegate void dUpdatetxtMinsElapse(string text);

        private void UpdatetxtSngsElapsed(string text)
        {
            txtSngsElapsed.Text = text;
        }
        private void UpdatetxtMinsElapse(string text)
        {
            txtMinsElapse.Text = text;
        }

        private static void CheckForStrayBots()
        {
            if(!bLockThreads)
            {
                if (winFunc.WinExists(bottitle))
                {
                    Hashtable hshWinList = winFunc.WinList(bottitle);
                    IDictionaryEnumerator en = hshWinList.GetEnumerator();
                    while (en.MoveNext())
                    {
                        IntPtr handleWindow = (IntPtr)en.Key;
//                        string sMenuItemText = winFunc.GUICtrlMenu_GetItemText(winFunc.GUICtrlMenu_GetMenu(handleWindow), 0);
                        IntPtr hMenu = WindowsAPI.GetMenu(handleWindow);
                        StringBuilder sbMenuText = new StringBuilder(0x20);
                        WindowsAPI.GetMenuString(hMenu, (uint)0, sbMenuText, 0x20, WindowsAPI.MF_BYPOSITION);
                        string sMenuItemText = sbMenuText.ToString();    
                        if (sMenuItemText == "&Holdem")
                        {
                            bLockThreads = true;
                            if (!BotConnected(handleWindow))
                            {
                                FileFunctions.fConsoleWrite("Closing stray bot");
                                winFunc.WinClose(handleWindow); //au3.WinClose("[HANDLE:" + winlist[1, ia].ToString() + "]", "");
                                winFunc.WinWaitClose(handleWindow, 1);
                                if (winFunc.WinExists(handleWindow))
                                    winFunc.WinClose(handleWindow);
                                winFunc.WinWaitClose(handleWindow, 1);
                                if (winFunc.WinExists(handleWindow))
                                    au3.WinKill(au3Handle(handleWindow), "");
                            }
                            else 
                            {
                                if (WindowsAPI.IsWindowVisible(handleWindow))
                                {
                                    HideBot(handleWindow);
                                    int counter =0;
                                    while (!WindowsAPI.IsWindowVisible(handleWindow) && counter++ < 50)
                                    {
                                        Application.DoEvents();
                                        Thread.Sleep(100);
                                    }
                                }
                            }
                            bLockThreads = false;
                        }
                    }
                }
            }
        }

        /* Things we can do in threads:
         *      look for abandoned games
         *      look for abandoned bots
         *      register tournaments when necessary
         *      attach bots to games
         *      closing finshed tournaments
         *      */

        private void button9_Click(object sender, EventArgs e)
        {
            ProgramSecurity.CheckSecurity();
            /*
            string x, y;
            for (int a = 1; a <= ipTable.Count(); a++)
            {
                if (winFunc.WinExists(ipTable[a - 1]))
                {
                    x = au3.IniRead(sIniFile, "settings", "txtPosX" + a.ToString(), "0");
                    y = au3.IniRead(sIniFile, "settings", "txtPosY" + a.ToString(), "0");
                    winFunc.SetWindowPosition(ipTable[a - 1], IntPtr.Zero, Convert.ToInt32(x), 
                        Convert.ToInt32(y), 802, 572, 0); //au3.WinMove("[HANDLE:" + hxTable[oldPointer] + "]", "", Convert.ToInt32(x), Convert.ToInt32(y), 802, 574);
                }
            }
             */
        }

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
    }

}
