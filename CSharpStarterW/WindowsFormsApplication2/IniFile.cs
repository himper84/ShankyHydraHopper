using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

// Change this to match your program's normal namespace
namespace csFTPStarter
{
    class IniFile   // revision 11
    {
        string Path;
        string EXE = Assembly.GetExecutingAssembly().GetName().Name;
        public static int capacity = 512;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string section, string key, string defaultValue,
        [In, Out] char[] value, int size, string filePath);

        public IniFile(string IniPath = null)
        {
            Path = new FileInfo(IniPath ?? EXE + ".ini").FullName.ToString();
        }

        public static string[] ReadKeys(string section, string filePath)
        {
            // first line will not recognize if ini file is saved in UTF-8 with BOM 
            while (true)
            {
                char[] chars = new char[capacity];
                int size = GetPrivateProfileString(section, null, "", chars, capacity, filePath);

                if (size == 0)
                {
                    return null;
                }

                if (size < capacity - 2)
                {
                    string result = new String(chars, 0, size);
                    string[] keys = result.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                    return keys;
                }

                capacity = capacity * 2;
            }
        }

        public static string[] IniReadSection(string section, string filePath)
        {
            // first line will not recognize if ini file is saved in UTF-8 with BOM 
            while (true)
            {
                char[] chars = new char[capacity];
                int size = GetPrivateProfileString(section, null, "", chars, capacity, filePath);
                if (size == 0)
                {
                    return null;
                }
                if (size < capacity - 2)
                {
                    string result = new String(chars, 0, size);
                    string[] keys = result.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                    return keys;
                }
                capacity = capacity * 2;
            }
        }

        public string Read(string Key, string Section = null, string dKey = "")
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section ?? EXE, Key, dKey, RetVal, 255, Path);
            return RetVal.ToString();
        }

        public void Write(string Key, string Value, string Section = null)
        {
            WritePrivateProfileString(Section ?? EXE, Key, Value, Path);
        }

        public void DeleteKey(string Key, string Section = null)
        {
            Write(Key, null, Section ?? EXE);
        }

        public void DeleteSection(string Section = null)
        {
            Write(null, null, Section ?? EXE);
        }

        public bool KeyExists(string Key, string Section = null)
        {
            return Read(Key, Section).Length > 0;
        }
    }
}

