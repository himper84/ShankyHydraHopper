using System;
using System.IO;
using System.Runtime.InteropServices;

public class FileFunctions
{
    [DllImport("kernel32", EntryPoint = "WritePrivateProfileStringW", CharSet = CharSet.Unicode,
        SetLastError = true, ExactSpelling = true)]
    private static extern int WritePrivateProfileString(string lpApplicationName,
        string lpKeyName, string lpString, string lpFileName);

    [DllImport("kernel32", EntryPoint = "GetPrivateProfileStringW", CharSet = CharSet.Unicode,
        SetLastError = true, ExactSpelling = true)]
    private static extern int GetPrivateProfileString(string lpApplicationName,
        string lpKeyName, string lpDefault, string lpReturnedString, int nSize, string lpFileName);

    public static string IniRead(string INIPath, string SectionName, string KeyName, string DefaultValue)
    {

        // Allocate some room.
        string sData = new string(' ', 1024);
        int n = GetPrivateProfileString(SectionName, KeyName, DefaultValue, sData, sData.Length, INIPath);

        if (n > 0)
        {
            // return whatever it gave us
            return sData.Substring(0, n);
        }
        else
        {
            return "";
        }
    }
    
    public static string IniRead(string INIPath, string SectionName, string KeyName)
    {

        // Overload 1 assumes zero-length default
        return IniRead(INIPath, SectionName, KeyName, "");

    }

    public static string IniRead(string INIPath, string SectionName)
    {

        // Overload 2 returns all keys in a given section of the given file
        return IniRead(INIPath, SectionName, null, "");

    }

    public static string IniRead(string INIPath)
    {
        // Overload 3 returns all section names given just path
        return IniRead(INIPath, null, null, "");
    }

    public static void IniWrite(string INIPath, string SectionName, string KeyName, string TheValue)
    {

        WritePrivateProfileString(SectionName, KeyName, TheValue, INIPath);

    }

    public static void IniDelete(string INIPath, string SectionName, string KeyName)
    {
        // delete single line from section

        WritePrivateProfileString(SectionName, KeyName, null, INIPath);

    }

    public static void IniDelete(string INIPath, string SectionName)
    {
        // Delete section from INI file
        WritePrivateProfileString(SectionName, null, null, INIPath);
    }

    public static void fConsoleWrite(string Msg)
    {
        Console.WriteLine(Msg);
        string sPathName = System.Reflection.Assembly.GetExecutingAssembly().Location;
        //sPathName = sPathName.Substring(0, sPathName.IndexOf("\\", -1)-1);
        sPathName = sPathName.Substring(0, sPathName.LastIndexOf("\\")+1);
        string sLogTime;
            
        string sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";

        string sYear = DateTime.Now.Year.ToString().PadLeft(4,'0');
        string sMonth = DateTime.Now.Month.ToString().PadLeft(2, '0');
        string sDay = DateTime.Now.Day.ToString().PadLeft(2, '0');
        sLogTime = sYear + sMonth + sDay;

        StreamWriter sw = new StreamWriter(sPathName + sLogTime + ".log", true);
        sw.WriteLine(sLogFormat + Msg);
        sw.Flush();
        sw.Close();
    }

    public static void BackupLogs(string sBotExe)
    {
        fConsoleWrite("Backing up logs");
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

        try
        {
            if (File.Exists(sPath + "holdem.log"))
            {
                File.Copy(sBotExe + "holdem.log", sPath + sLogTime + ".log", true);
                fConsoleWrite("Log file back up successful");
            }
            else
            {
                fConsoleWrite("No log file to backup");
            }
        }
        catch
        {
            fConsoleWrite("Error Backing up Bot Log");
        }
    }
}