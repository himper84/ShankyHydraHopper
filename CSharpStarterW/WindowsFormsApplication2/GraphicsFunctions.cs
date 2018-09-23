using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Collections;
using System.IO;

public class GraphicsFunctions
{
    private static WindowFunctions winfunc = new WindowFunctions();
    private static FileFunctions filefunc = new FileFunctions();
    private string m_scrapeini = "scrapes.ini";
    private int m_dataentry = 0;

    public bool doImagesMatch(ref Bitmap bmp1, ref Bitmap bmp2)
    {
        try
        {
            //create instance or System.Drawing.ImageConverter to convert
            //each image to a byte array
            ImageConverter converter = new ImageConverter();
            //create 2 byte arrays, one for each image
            byte[] imgBytes1 = new byte[1];
            byte[] imgBytes2 = new byte[1];

            //convert images to byte array
            imgBytes1 = (byte[])converter.ConvertTo(bmp1, imgBytes2.GetType());
            imgBytes2 = (byte[])converter.ConvertTo(bmp2, imgBytes1.GetType());

            //now compute a hash for each image from the byte arrays
            SHA256Managed sha = new SHA256Managed();
            byte[] imgHash1 = sha.ComputeHash(imgBytes1);
            byte[] imgHash2 = sha.ComputeHash(imgBytes2);

            //now let's compare the hashes
            for (int i = 0; i < imgHash1.Length && i < imgHash2.Length; i++)
            {
                //whoops, found a non-match, exit the loop
                //with a false value
                if (!(imgHash1[i] == imgHash2[i]))
                    return false;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            return false;
        }
        //we made it this far so the images must match
        return true;
    }

    public string _ScanString(int x1, int y1, int x2, int y2, int color, double range, IntPtr hWin, string site)
    {
        int width = x2 - x1;
        int height = y2 - y1;

        string xstring, xChar;
        bool flag;
        int lastx, blanklines, ypixel;
        xstring = "";

        if (winfunc.WinExists(hWin))
        {
            int[] winpos = winfunc.GetWindowPosition(hWin);
            Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Bitmap bmp2 = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Drawing.Graphics GFX = System.Drawing.Graphics.FromImage(bmp);
            System.Drawing.Graphics GFX2 = System.Drawing.Graphics.FromImage(bmp2);
            GFX.CopyFromScreen(x1 + winpos[0], y1 + winpos[1], 0, 0, new Size(width, height),
                            System.Drawing.CopyPixelOperation.SourceCopy);
            bool alternate = false;
            Color bColor = new Color();

            #region Loop and make sure it's not animated before continuing the scrape.
            do
            {
                if (alternate)
                {
                    GFX.CopyFromScreen(x1 + winpos[0], y1 + winpos[1], 0, 0, new Size(width, height),
                                    System.Drawing.CopyPixelOperation.SourceCopy);
                }
                else
                {
                    GFX2.CopyFromScreen(x1 + winpos[0], y1 + winpos[1], 0, 0, new Size(width, height),
                                    System.Drawing.CopyPixelOperation.SourceCopy);
                }
                alternate = !alternate;
                Thread.Sleep(1);
            } while (doImagesMatch(ref bmp, ref bmp2) == false);
            #endregion


            #region If we're doing this by background color - determine the color by the most pixels of the same color
            if (range < 0)
            {
                Hashtable colors = new Hashtable();
                IDictionaryEnumerator colorkeys = colors.GetEnumerator();

                // Scan for background color...
                for (int bx = 0; bx < bmp.Width; bx++)
                {
                    for (int by = 0; by < bmp.Height; by++)
                    {
                        bColor = bmp.GetPixel(bx, by);
                        string sColor = bColor.ToArgb().ToString("X8").Substring(2, 6);
                        if (colors.ContainsKey(sColor))
                        {
                            colors[sColor] = (Convert.ToInt32(colors[sColor].ToString()) + 1).ToString();
                        }
                        else
                        {
                            colors.Add(sColor, "1");
                        }
                    }
                }
                IDictionaryEnumerator _enumerator = colors.GetEnumerator();
                int largestCount = 0;
                string BackgroundColor = "";
                while (_enumerator.MoveNext())
                {
                    if (Convert.ToInt32(_enumerator.Value) > largestCount)
                    {
                        largestCount = Convert.ToInt32(_enumerator.Value);
                        BackgroundColor = _enumerator.Key.ToString();
                    }
                }
                // FileFunctions.fConsoleWrite("The background color is: " + BackgroundColor);
                color = System.Int32.Parse(BackgroundColor, System.Globalization.NumberStyles.AllowHexSpecifier); ;
            }
            #endregion

            #region Perform the scrape
            xstring = "";
            xChar = "";
            flag = false;
            lastx = 0;
            ypixel = 0;
            blanklines = 0;
            for (int bx = 0; bx < bmp.Width; bx++)
            {
                ypixel = 0;
                for (int by = 0; by < bmp.Height; by++)
                {
                    bColor = bmp.GetPixel(bx, by);
                    string sColor = bColor.ToArgb().ToString("X8").Substring(2, 6);
                    int iColor = System.Int32.Parse(sColor, System.Globalization.NumberStyles.AllowHexSpecifier);
                    if (_ColorRange(iColor, color, range))
                    {
                        xChar = xChar + "1";
                        ypixel = ypixel + 1;
                    }
                    else
                        xChar = xChar + "0";
                }
                if (ypixel == 0)
                {
                    if (flag)
                    {
                        xstring = xstring + _LookupChar(ref bmp, xChar, bx - lastx, height, bx, site);
                        lastx = bx;
                        flag = false;
                    }
                    blanklines = blanklines + 1;
                    xChar = "";
                    if (blanklines > 3)
                    {
                        xstring = xstring + " ";
                        flag = false;
                        blanklines = 0;
                    }
                }
                else
                {
                    blanklines = 0;
                    flag = true;
                    xChar = xChar + "#";
                }
            }
            #endregion
            //au3.Opt("MouseCoordMode", oldMouseCoordMode);
            //au3.Opt("PixelCoordMode", oldPixelCoordMode);
        }
        while (xstring.IndexOf("  ") >= 0)
        {
            xstring = xstring.Replace("  ", " ");
        }
        return xstring;
    }

    public int BinToInt(string binaryNumber)
    {
        int multiplier = 1;
        int converted = 0;

        for (int i = binaryNumber.Length - 1; i >= 0; i--)
        {
            int t = System.Convert.ToInt16(binaryNumber[i].ToString());
            converted = converted + (t * multiplier);
            multiplier = multiplier * 2;
        }
        return converted;
    }

    public string scrapeini
    {
        get
        {
            return m_scrapeini;
        }
        set
        {
            m_scrapeini = value;
        }
    }

    public int dataentry
    {
        get
        {
            return m_dataentry;
        }
        set
        {
            m_dataentry = value;
        }
    }    

    public string _LookupChar(ref Bitmap bmp, string chr, int w, int h, int x, string site)
    {
        string rv;
        rv = "";
        string[] xlines;
        ArrayList lines = new ArrayList();
        dataentry = 1;
        string newlines;
        if (chr.IndexOf("1") > 0)
        {
            xlines = chr.Split('#'); // $lines = StringSplit($chr, "#") ++ autoit line ***
            foreach (string s in xlines)
                lines.Add(s);
            //DisplayAList(lines);

            if (lines.Count > 0)
            {
                while ((lines.Count >= 1) && (lines[0].ToString().IndexOf("1") == -1))
                    lines.RemoveAt(0);// _ArrayDelete($lines, 1) ++ autoit line ***
                while (((lines.Count) >= 1) && (lines[(lines.Count) - 1].ToString().IndexOf("1") == -1))
                    lines.RemoveAt(lines.Count - 1);
                newlines = "";

                for (int ix = 0; ix < lines[0].ToString().Length; ix++)
                {
                    for (int ax = 0; ax < lines.Count; ax++)
                        newlines = newlines + lines[ax].ToString().Substring(ix, 1);// $newlines = $newlines & StringMid($lines[$a], $i, 1) ++ autoit line
                    newlines = newlines + "#";
                }
                xlines = newlines.Split('#');// $lines = StringSplit($newlines, "#") +++ autoit line 
                lines.Clear();

                foreach (string s in xlines)
                    lines.Add(s);

                while ((lines.Count >= 1) && (lines[0].ToString().IndexOf("1") == -1))
                    lines.RemoveAt(0);// _ArrayDelete($lines, 1) ++ autoit line ***
                while (((lines.Count) >= 1) && (lines[(lines.Count) - 1].ToString().IndexOf("1") == -1))
                    lines.RemoveAt(lines.Count - 1);

                newlines = "";
                chr = "";
                for (int i = 0; i < lines.Count; i++)
                    chr = chr + lines[i].ToString() + "#";
                //Console.WriteLine("Reading from " + site + "Chars");
                rv = FileFunctions.IniRead(scrapeini, site + "Chars", chr, "");
            }
            else
                FileFunctions.fConsoleWrite("Not enough lines to be a scan:" + chr);
            if (dataentry == 1 && rv == "")
            {
                // SplashTextOn("ScanArea", "", $w, $h, $x, $y, 17) ++ autoit line 
                //au3.SplashTextOn("ScanArea", "", w, h, x, y, 17);
                //au3.WinSetTrans("ScanArea", "", 130);
                //PlaySound(Application.StartupPath + "/Alert.wav", 0, 0);
                // WeylandsOCR.OCRInput = new WeylandsOCR.OCRInput();
                string value = "";

                Bitmap subBMP = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                System.Drawing.Graphics GFX = Graphics.FromImage(subBMP);
                Rectangle xyz = new Rectangle(x, 0, w, h);

                GFX.DrawImageUnscaledAndClipped(bmp, xyz);

                GFX.Dispose();

                if (ScanInputBox(ref bmp, x, chr, "OCR Self-Correction", "What character is referenced here?", ref value) == DialogResult.OK)
                {
                    if (value.Trim().Length > 0)
                    {
                        //Console.WriteLine("Writing to " + site + "Chars");
                        FileFunctions.IniWrite(scrapeini, site + "Chars", chr, value);
                        rv = value;
                    }
                }
            }
            else if (rv == "")
            {
                //if (FileFunctions.IniRead(scrapeini, "Unknown", chr, "") == "")
                    // IniWrite($scrapeini, "Unknown", $chr , @YEAR & @MON & @MDAY & @HOUR & @MIN & @SEC & @MSEC) +++ autoit line 
                //    FileFunctions.IniWrite(scrapeini, "Unknown", chr, DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString());
            }
        }
        return rv;
    }

    public bool _ColorRange(int RBG, int pixel, double shade)
    {
        bool anti = false;
        int mask, pix;
        double difference;
        if (shade == 0)
            return RBG == pixel;
        if (shade < 0)
        {
            shade *= -1;
            anti = true;
        }
        // $mask = BitAND($RBG, 0xff0000) ++ autoit line 
        mask = (RBG & 0xff0000);
        // $mask = BitShift($mask, 16) ++ autoit line 
        mask = (mask >> 16);
        pix = (pixel & 0xff0000);
        pix = (pix >> 16);
        difference = Math.Abs(mask - pix);
        if ((!anti && difference > shade) || (anti && difference < shade))
            return false;

        mask = (RBG & 0x00ff00);
        mask = (mask >> 8);
        pix = (pixel & 0x00ff00);
        pix = (pix >> 8);
        difference = Math.Abs(mask - pix);
        if ((!anti && difference > shade) || (anti && difference < shade))
            return false;

        mask = (RBG & 0x0000ff);
        pix = (pixel & 0x0000ff);
        difference = Math.Abs(mask - pix);
        if ((!anti && difference > shade) || (anti && difference < shade))
            return false;
        return true;
    }

    public static DialogResult ScanInputBox(ref Bitmap BMP, int x, string PixelDisplay, string title, string promptText, ref string value)
    {
        Form form = new Form();
        Label label = new Label();
        Label label2 = new Label();
        TextBox textBox = new TextBox();
        Button buttonOk = new Button();
        Button buttonCancel = new Button();
        PictureBox pxbox = new PictureBox();
        // PictureBox pxbox2 = new PictureBox();

        form.Text = title;
        label.Text = promptText;
        textBox.Text = value;
        label2.Text = PixelDisplay.Replace("#", "\n").Replace("0", " ").Replace("1", "█");
        label2.Font = new System.Drawing.Font("Courier New", 3.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
        pxbox.Image = BMP;
        //pxbox2.Image = BMP2;

        buttonOk.Text = "OK";
        buttonCancel.Text = "Cancel";
        buttonOk.DialogResult = DialogResult.OK;
        buttonCancel.DialogResult = DialogResult.Cancel;

        label.SetBounds(9, 10, 372, 13);
        textBox.SetBounds(12, 26, 372, 20);
        buttonOk.SetBounds(228, 62, 75, 23);
        buttonCancel.SetBounds(309, 62, 75, 23);
        pxbox.SetBounds(9, 90, BMP.Width, BMP.Height);
        /*            pxbox2.SetBounds(9, 95 + BMP.Height, BMP2.Width * 2, BMP2.Height * 2);
                    pxbox2.SizeMode = PictureBoxSizeMode.CenterImage;
                    pxbox2.SizeMode = PictureBoxSizeMode.Zoom;
                    pxbox2.Image = BMP2;
         */
        label2.SetBounds(x, 95 + BMP.Height, 372, (4 * CountStringOccurrences(PixelDisplay, "#")));

        label.AutoSize = true;
        label2.AutoSize = true;

        //label2.BorderStyle = BorderStyle.Fixed3D;
        //pxbox.BorderStyle = BorderStyle.Fixed3D;
        //pxbox2.BorderStyle = BorderStyle.Fixed3D;


        textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
        buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

        int maxWidth = BMP.Width;// > BMP2.Width ? BMP.Width : BMP2.Width;

        form.ClientSize = new Size(maxWidth + 18, 127 + (4 * CountStringOccurrences(PixelDisplay, "#")));
        form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel, label2, pxbox });
        form.ClientSize = new Size(Math.Max(300, maxWidth + 18), form.ClientSize.Height);
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.MinimizeBox = false;
        form.MaximizeBox = false;
        form.AcceptButton = buttonOk;
        form.CancelButton = buttonCancel;

        /*
        string handle = form.Handle.ToString();
        if(handle.Length>8)
            handle = handle.Substring(handle.Length-8);
        handle = "[HANDLE:" + handle + "]";
        au3.WinActivate(handle, "");
         */
        form.Show();
        form.Activate();
        winfunc.SetWindowToForeground(form.Handle);

        form.Visible = false;
        DialogResult dialogResult = form.ShowDialog();

        value = textBox.Text;
        return dialogResult;
    }

    public static int CountStringOccurrences(string text, string pattern)
    {
        // Loop through all instances of the string 'text'.
        int count = 0;
        int i = 0;
        while ((i = text.IndexOf(pattern, i)) != -1)
        {
            i += pattern.Length;
            count++;
        }
        return count;
    }

    public int[] PixelSearch(int x1, int y1, int width, int height, int color, int range, int Step)
    {
        //int width = x2 - x1;
        //int height = y2 - y1;
        if (Step < 1)
            Step = 1;
        int[] rv = new int[2];
        rv[0] = -1;
        //FileFunctions.fConsoleWrite("x:" + x1.ToString() + " y:" +
        //    y1.ToString());
        Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        System.Drawing.Graphics GFX = System.Drawing.Graphics.FromImage(bmp);
        GFX.CopyFromScreen(x1, y1, 0, 0, new Size(width, height), System.Drawing.CopyPixelOperation.SourceCopy);
        Color bColor = new Color();

        for (int by = 0; by < bmp.Height && rv[0] == -1; by += Step)
        {
            for (int bx = 0; bx < bmp.Width && rv[0] == -1; bx += Step)
                {
                bColor = bmp.GetPixel(bx, by);
                string sColor = bColor.ToArgb().ToString("X8").Substring(2, 6);
                int iColor = System.Int32.Parse(sColor, System.Globalization.NumberStyles.AllowHexSpecifier);
                if (_ColorRange(iColor, color, range))
                {
                    rv[0] = x1 + bx;
                    rv[1] = y1 + by;
                }
            }
        }
        return rv;
    }

    public string PixelGetColor(int x, int y)
    {
        Bitmap bmp = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        System.Drawing.Graphics GFX = System.Drawing.Graphics.FromImage(bmp);
        GFX.CopyFromScreen(x, y, 0, 0, new Size(1, 1), System.Drawing.CopyPixelOperation.SourceCopy);
        return bmp.GetPixel(0, 0).ToArgb().ToString("X8").Substring(2, 6);
    }

    public int PixelGetColor(int x, int y, bool bRetInt)
    {
        Bitmap bmp = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        System.Drawing.Graphics GFX = System.Drawing.Graphics.FromImage(bmp);
        GFX.CopyFromScreen(x, y, 0, 0, new Size(1, 1), System.Drawing.CopyPixelOperation.SourceCopy);
        return Int32.Parse(bmp.GetPixel(0, 0).ToArgb().ToString("X8").Substring(2, 6), 
            System.Globalization.NumberStyles.AllowHexSpecifier);
    }

    public string CleanUpString(string sOrig, int iCase, string sReplace, string sWith)
    {
        sOrig = sOrig.Trim();
        while (sOrig.IndexOf("  ") > -1)
            sOrig = sOrig.Replace("  ", " ");
        if (sReplace.Length > 0)
        {
            for (int a = 0; a < sReplace.Length; a++)
            {
                sOrig = sOrig.Replace(sReplace.Substring(a, 1), sWith.Substring(a, 1));
            }
        }
        if (iCase == 1)
            sOrig = sOrig.ToUpper();
        if (iCase == -1)
            sOrig = sOrig.ToLower();
        return sOrig;
    }
}