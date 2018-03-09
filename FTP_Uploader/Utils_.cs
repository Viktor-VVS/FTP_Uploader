using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FTP_Uploader
{
    public class WinRar
    {

        private string commandLine;
        private string winrarexepath;

        public WinRar(string WinRarExePath)
        {
            this.winrarexepath = WinRarExePath;
        }

        private bool buildRar()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(winrarexepath, commandLine);
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            using (Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
            }
            return true;

        }
        public bool CreateRarFromFile(string fpath, string Rarpath)
        {
            commandLine = " a -m5 -scU -ep" + " \"" + Rarpath + "\"" + " \"" + fpath + "\"";
            return buildRar();
        }

        public bool CreatePasswordRaRFromFile(string fpath, string pass, string Rarpath)
        {
            commandLine = " a -m5 -scU -ep -hp" + pass + " \"" + Rarpath + "\"" + " \"" + fpath + "\"";
            return buildRar();
        }


    } // Класс для работы с WinRar

    public class StringsParser
    {
        public string Content { get; private set; }
        public string extractedString { get; private set; }
        public int movePosition { get; set; }
        public int extractPosition { get; set; }
        public int startPosition { get; set; }
        public StringsParser(string content)
        {
            Content = content;
            extractPosition = 0;
            movePosition = 0;
            extractedString = ""; // позиция которая нужна для Extract/вырезки
            startPosition = 0; //позиция с которой начнается каждый следующ поиск 

        }

        public bool backSearchTo(string findOccur, bool caseSensitive = true)
        {
            //if ((movePosition = Content.LastIndexOf(findOccur, 0, startPosition, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase)) != -1)
            if ((movePosition = Content.LastIndexOf(findOccur, 0, startPosition, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)) != -1)
            {
                //startPosition = movePosition + findOccur.Length;
                startPosition = movePosition;
                return true;
            }
            else
                return false;
        }

        public bool backSearchTo_NotIncluding(string findOccur, bool caseSensitive = true)
        {
            //if ((movePosition = Content.LastIndexOf(findOccur, 0, startPosition, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase)) != -1)
            if ((movePosition = Content.LastIndexOf(findOccur, 0, startPosition, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)) != -1)

            {
                //startPosition = movePosition + findOccur.Length;
                startPosition = movePosition;
                movePosition = startPosition + findOccur.Length;
                return true;
            }
            else
                return false;
        }

        // set cursor at the begin found findOccur
        public bool searchTo(string findOccur, bool caseSensitive = true)
        {
            //if ((movePosition = Content.IndexOf(findOccur, startPosition, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase)) != -1)
            if ((movePosition = Content.IndexOf(findOccur, startPosition, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)) != -1)
            {
                startPosition = movePosition + findOccur.Length;
                return true;
            }
            else
                return false;
        }
        // set cursor after found findOccur
        public bool searchTo_NotIncluding(string findOccur, bool caseSensitive = true)
        {
            //if ((movePosition = Content.IndexOf(findOccur, startPosition, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase)) != -1)
            if ((movePosition = Content.IndexOf(findOccur, startPosition, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)) != -1)
            {
                startPosition = movePosition + findOccur.Length;
                movePosition = startPosition;
                return true;
            }
            else
                return false;
        }
        public bool exctractTo(string findOccur, bool caseSensitive = true)
        {
            //if ((extractPosition = Content.IndexOf(findOccur, startPosition, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase)) != -1)
            if ((extractPosition = Content.IndexOf(findOccur, startPosition, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)) != -1)
            {
                extractPosition = extractPosition + findOccur.Length;
                startPosition = extractPosition;
                extractedString = Content.Substring(movePosition, extractPosition - movePosition);
                return true;
            }
            else
                return false;
        }
        public bool exctractTo_NotIncluding(string findOccur, bool caseSensitive = true)
        {
            //if ((extractPosition = Content.IndexOf(findOccur, startPosition, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase)) != -1)
            if ((extractPosition = Content.IndexOf(findOccur, startPosition, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)) != -1)
            {
                startPosition = extractPosition + findOccur.Length;
                extractedString = Content.Substring(movePosition, extractPosition - movePosition);


                return true;
            }
            else
                return false;
        }

        public bool exctractToEnd()
        {
            //if ((extractPosition = Content.IndexOf(findOccur, startPosition, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase)) != -1)

            extractPosition = Content.Length;
            startPosition = extractPosition;
            extractedString = Content.Substring(movePosition, extractPosition - movePosition);

            return true;
        }

        public bool exctractToEnd2()
        {
            //if ((extractPosition = Content.IndexOf(findOccur, startPosition, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase)) != -1)

            extractPosition = Content.Length;
            extractedString = Content.Substring(startPosition, extractPosition - startPosition);

            return true;
        }

        public void replaceExtractedWith(string newContent)
        {
            int extractLen = extractPosition - movePosition;
            Content = Content.Remove(movePosition, extractLen);
            Content = Content.Insert(movePosition, newContent);
            startPosition += newContent.Length - extractLen;
        }
        public void Reset()
        {
            extractPosition = 0;
            movePosition = 0;
            extractedString = "";
            startPosition = 0;
        }

    }  //Класс для парсинга строк.

    public static class Extract
    {
        public static string Between(string Src, string start1, string start2)
        {
            StringsParser sp = new StringsParser(Src);
            sp.searchTo_NotIncluding(start1);
            sp.exctractTo_NotIncluding(start2);
            return sp.extractedString;
        }

        public static string BetweenEnd(string Src, string start1)
        {
            StringsParser sp = new StringsParser(Src);
            sp.searchTo_NotIncluding(start1);
            sp.exctractToEnd2();
            return sp.extractedString;
        }

        public static string BetweenStart(string Src, string start2)
        {
            StringsParser sp = new StringsParser(Src);
            sp.exctractTo_NotIncluding(start2);
            return sp.extractedString;
        }


        public static string BetweenInclude(string Src, string start1, string start2)
        {
            StringsParser sp = new StringsParser(Src);
            sp.searchTo(start1);
            sp.exctractTo(start2);
            return sp.extractedString;
        }
    } // Класс для работы с строками.

    public class MessageBoxTimer // Реализация класса Messgabox с таймером на закрытие окна.
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern int MessageBoxTimeout(IntPtr hwnd, String text, String title, uint type, Int16 wLanguageId, Int32 milliseconds);

        public enum MessageBoxReturnStatus
        {
            OK = 1,
            Cancel = 2,
            Abort = 3,
            Retry = 4,
            Ignore = 5,
            Yes = 6,
            No = 7,
            TryAgain = 10,
            Continue = 11
        }

        public enum MessageBoxType
        {
            OK = 0,
            OK_Cancel = 1,
            Abort_Retry_Ignore = 2,
            Yes_No_Cancel = 3,
            Yes_No = 4,
            Retry_Cancel = 5,
            Cancel_TryAgain_Continue = 6
        }

        public static MessageBoxReturnStatus Show(string title, string text, MessageBoxType type, int milliseconds)
        {
            int returnValue = MessageBoxTimeout(IntPtr.Zero, text, title, Convert.ToUInt32(type), 1, milliseconds);

            return (MessageBoxReturnStatus)returnValue;
        }
    }

    public class CryptoRandom : RandomNumberGenerator   // Реалицазия получения случайных чисел.
    {
        private static RandomNumberGenerator r;

        ///<summary>
        /// Creates an instance of the default implementation of a cryptographic random number generator that can be used to generate random data.
        ///</summary>
        public CryptoRandom()
        {
            r = RandomNumberGenerator.Create();
        }

        ///<summary>
        /// Fills the elements of a specified array of bytes with random numbers.
        ///</summary>
        ///<param name=”buffer”>An array of bytes to contain random numbers.</param>
        public override void GetBytes(byte[] buffer)
        {
            r.GetBytes(buffer);
        }

        ///<summary>
        /// Returns a random number between 0.0 and 1.0.
        ///</summary>
        public double NextDouble()
        {
            byte[] b = new byte[4];
            r.GetBytes(b);
            return (double)BitConverter.ToUInt32(b, 0) / UInt32.MaxValue;
        }

        ///<summary>
        /// Returns a random number within the specified range.
        ///</summary>
        ///<param name=”minValue”>The inclusive lower bound of the random number returned.</param>
        ///<param name=”maxValue”>The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
        public int Next(int minValue, int maxValue)
        {
            return (int)Math.Round(NextDouble() * (maxValue - minValue - 1)) + minValue;
        }

        ///<summary>
        /// Returns a nonnegative random number.
        ///</summary>
        public int Next()
        {
            return Next(0, Int32.MaxValue);
        }

        ///<summary>
        /// Returns a nonnegative random number less than the specified maximum
        ///</summary>
        ///<param name=”maxValue”>The inclusive upper bound of the random number returned. maxValue must be greater than or equal 0</param>
        public int Next(int maxValue)
        {
            return Next(0, maxValue);
        }

        public override void GetNonZeroBytes(byte[] data)
        {
            throw new NotImplementedException();
        }
    } //Генератор случайных чисел.
}
