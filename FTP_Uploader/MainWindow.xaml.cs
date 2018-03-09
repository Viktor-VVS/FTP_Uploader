using MyFTP_client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ionic.Zip;
using System.Security.Cryptography;
using System.Threading;
using System.Runtime.InteropServices;


namespace FTP_Uploader
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.ResizeMode = ResizeMode.CanMinimize;
            InitializeComponent();
            FtpAcc_global = GetSettingsContent();   //Получаем содержимое settings.ini
            if (PrepareCmdline(cmdline_global) == true) // Проверяем если в командной строке есть параметры для работы то запускаем режим cmd , иначе работаем в GUI форме.
            {
                CommandLineWorking(cmdline_global); // Работа в cmd.
                Environment.Exit(0); // Завершаем процесс;
            }
        }

        #region Объекты данных

        public class FtpAccounts //описывает 1 ftp аккаунт.
        {
            public string viewname { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public string port { get; set; }
            public string host { get; set; }
            public string path { get; set; }
            public string linkview { get; set; }
        }

        public struct CmdContent  // структура для получения и сортировки данных в cmd строке.
        {
            public string path;
            public string compress;
            public string ftpaccount;
            public string archive_pass;
            public string name;
        }

        #endregion

        #region Глобальные данные
        string cmdline_global = Environment.CommandLine;
        //Моя тестовая строка   string testcmd = "\"C:\\Users\\Администратор\\Documents\\Visual Studio 2013\\Projects\\FTPUploader\\FTPUploader\\bin\\Debug\\FTPUploader.exe\" #path=\"D:\\work hard\\Other dll\\powrprof.dll\" #compress=rar; #ftpaccount=Viktor; #archive_pass=12345;";
        string selectAccount_global = "";
        List<FtpAccounts> FtpAcc_global = new List<FtpAccounts>();
        string winRarPath_global = "";

        #endregion

        #region Работа с командной строкой
        public void CommandLineWorking(string cmdline)
        {
            try
            {
                if (this.WindowState == WindowState.Normal)
                {
                    this.ShowInTaskbar = false;
                    this.Hide();                      // скрываем с экрана нашу GUI форму.
                }
                CmdContent cmdcont = new CmdContent();
                string password = "";
                cmdcont = ParseCmd(cmdline);  // сортируем нашу cmd строку с данными.
                if (File.Exists(cmdcont.path) == true)
                {
                    if (cmdcont.archive_pass == "random")
                        password = GetRandomPassword();    // проверяем наличие пароля ,если нет то выбираем random пароль.
                    else
                        password = cmdcont.archive_pass;  // иначе выбираем пароль заданный юзером.
                    FtpAccounts curacc = SelectViewName(cmdcont.ftpaccount); // Получаем все параметры выбранного ftp аккаунта.


                    if (cmdcont.compress == "none")  // проверяем на требование архивации , если в параметре none...
                    {
                        string resContent = "";
                        bool uploadRes = UploadNoArchive(curacc, cmdcont.path, cmdcont.name); // То запускаем загрузку на сервер без архивации.
                        if (uploadRes == true)
                            resContent = ShowUploadLink(curacc, cmdcont.name);
                        else
                            resContent = "Failure!";
                        if (uploadRes == true) MessageBoxTimer.Show("SUCCESS", "SUCCESS", MessageBoxTimer.MessageBoxType.OK, 700); // Выдаем на 1 сек окно Успешно или Неуспешно выполнилась загрузка на сервер .
                        else MessageBoxTimer.Show("FAILURE", "FAILURE", MessageBoxTimer.MessageBoxType.OK, 700);
                        //ToClipBoard(resContent, null); // Добавляем в буфер обмена наш результат.
                        Clipboard.SetText(resContent);
                    }
                    else if (cmdcont.compress == "rar") // если в параметре выбран rar...
                    {
                        string resContent = ""; string ftpfilename = "";
                        bool uploadRes = UploadRarArchive(winRarPath_global, curacc, cmdcont.path, cmdcont.name, password, out ftpfilename); // Запускаем Загрузку на сервер с архиватора WinRar.
                        if (uploadRes == true)
                            resContent = ShowUploadLink(curacc, ftpfilename) + "\r\n" + password;
                        else
                            resContent = "Failure!";

                        if (uploadRes == true) MessageBoxTimer.Show("SUCCESS", "SUCCESS", MessageBoxTimer.MessageBoxType.OK, 700); // Выдаем на 1 сек окно Успешно или Неуспешно выполнилась загрузка на сервер .
                        else MessageBoxTimer.Show("FAILURE", "FAILURE", MessageBoxTimer.MessageBoxType.OK, 700);
                        //ToClipBoard(resContent, password);
                        Clipboard.SetText(resContent);

                    }
                    else if (cmdcont.compress == "zip")  // если выбран режим архивации zip.
                    {
                        string resContent = ""; string ftpfilename = "";
                        bool uploadRes = UploadZipArchive(curacc, cmdcont.path, cmdcont.name, password, out ftpfilename); // Запускаем архивацию с библиотеки "Ionic.Zip.dll" , должна лежать рядом с екзешником.
                        if (uploadRes == true)
                            resContent = ShowUploadLink(curacc, ftpfilename) + "\r\n" + password;
                        else
                            resContent = "Failure!";

                        if (uploadRes == true) MessageBoxTimer.Show("SUCCESS", "SUCCESS", MessageBoxTimer.MessageBoxType.OK, 700); // Выдаем на 1 сек окно Успешно или Неуспешно выполнилась загрузка на сервер .
                        else MessageBoxTimer.Show("FAILURE", "FAILURE", MessageBoxTimer.MessageBoxType.OK, 700);
                        //ToClipBoard(resContent, password);
                        Clipboard.SetText(resContent);
                    }
                    else
                    {
                        MessageBox.Show("Не верно задан режим архивации(rar,zip,none)!");
                    }
                }
                else
                {
                    MessageBox.Show("Файл не существует или некорректно указан путь!");
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Ошибка " + exc.ToString());
            }
        }

        public CmdContent ParseCmd(string cmdline)  // Добавляем в структуру CmdContent параметры текущей cmd строки.
        {

            CmdContent res = new CmdContent();
            res.path = Extract.Between(cmdline, "path=\"", "\"");
            res.compress = Extract.Between(cmdline, "compress=", ";");
            res.ftpaccount = Extract.Between(cmdline, "#ftpaccount=", ";");
            res.archive_pass = Extract.Between(cmdline, "archive_pass=", ";");

            FileInfo fin = new FileInfo(res.path);
            res.name = fin.Name;
            return res;
        }

        public bool PrepareCmdline(string cmdline) // Проверка cmd на ввод параметров.
        {
            bool Res = false;
            if (cmdline.Contains("#archive_pass")) // Если в cmd линию  введены корректный параметры для работы , то возвращаем true  и работаем в cmd режиме.
                Res = true;
            return Res;
        }
        #endregion

        #region Методы работы с формой
        private void UPLOAD_Click(object sender, RoutedEventArgs e)
        {

            if (String.IsNullOrEmpty(FilePath.Text) == true)
            {
                MessageBox.Show("Введите путь к файлу для загрузки на сервер!");
                return;
            }
            FtpAccounts curAccount = SelectViewName(selectAccount_global);// Выбираем текущий ftp аккаунт.

            //  пароль архива
            //
            string ArchivePassword = "";
            if (Zip.IsChecked == true || Rar.IsChecked == true)
                ArchivePassword = GetRandomPassword();

            if (String.IsNullOrEmpty(Custom_Pass.Text) == false)
                ArchivePassword = Custom_Pass.Text;

            if (None.IsChecked == true) // если выбрали в Radiobutton Без архивации.
            {
                bool Res = UploadNoArchive(curAccount, FilePath.Text, Name.Text); // Запускаем загрузку без архивации.
                if (Res == true)
                {
                    string cont = ShowUploadLink(curAccount, Name.Text);
                    OutputTextbox.Text = cont;
                    Clipboard.SetText(cont);
                }

                else
                    OutputTextbox.Text = "Failure!";

            }
            else if (Zip.IsChecked == true)
            {
                string ftpfilename = "";
                bool Res = UploadZipArchive(curAccount, FilePath.Text, Name.Text, ArchivePassword, out ftpfilename);// Запускаем zip.
                if (Res == true)
                {
                    string cont = ShowUploadLink(curAccount, ftpfilename) + "\r\n" + ArchivePassword;
                    OutputTextbox.Text = cont;
                    Clipboard.SetText(cont);
                }
                else
                    OutputTextbox.Text = "Failure!";
            }
            else
            {
                string ftpfilename = "";
                bool Res = UploadRarArchive(winRarPath_global, curAccount, FilePath.Text, Name.Text, ArchivePassword, out ftpfilename); // Запуск RaR.
                if (Res == true)
                {
                    string cont = ShowUploadLink(curAccount, ftpfilename) + "\r\n" + ArchivePassword;
                    OutputTextbox.Text = cont;
                    Clipboard.SetText(cont);
                }
                else
                    OutputTextbox.Text = "Failure!";

            }

        }
        private void Clean_Click(object sender, RoutedEventArgs e) // Очистка текстбоксов.
        {
            FilePath.Text = "";
            Name.Text = "";
            Custom_Pass.Text = "";
            OutputTextbox.Text = "";
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e) // Загружаем изначально в Combobox данные о ftp аккаунтах и высвечиваем их viewname.
        {
            List<string> data = new List<string>();
            foreach (var el in FtpAcc_global)
            {
                data.Add(el.viewname);
            }
            var comboBox = sender as ComboBox;
            comboBox.ItemsSource = data;
            comboBox.SelectedIndex = 0;
            selectAccount_global = comboBox.SelectedItem as string;
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) //функция при изменение текущего аккаунта . 
        {
            var comboBox = sender as ComboBox;
            selectAccount_global = comboBox.SelectedItem as string;
        }
        public void OnDragOver(object sender, DragEventArgs e) //  Реализация функции Drag & Drop для перетаскивания  файлов в наш Текстбокс.
        {
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }
        public void OnDragEnter(object sender, DragEventArgs e) //  Реализация функции Drag & Drop для перетаскивания  файлов в наш Текстбокс.
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
        }
        private void FilePath_PreviewDrop(object sender, DragEventArgs e)//  Реализация функции Drag & Drop для перетаскивания  файлов в наш Текстбокс.
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedFilePaths =
                    e.Data.GetData(DataFormats.FileDrop, true) as string[];
                if (File.Exists(droppedFilePaths[0]) == true)  //  Если ставим Directory.Exist  перетаскиваем папку , а если File.Exist  то файл.
                    FilePath.Text = droppedFilePaths[0];
                FileInfo fileInf = new FileInfo(FilePath.Text);
                Name.Text = fileInf.Name;
            }
        }
        #endregion

        #region Рабочие методы
        public bool UploadRarArchive(string winrarpath, FtpAccounts account, string Path, string Name, string Password, out string ftpfilename)
        {
            bool res = false;
            string archivepass = "";
            string WinRarExePath = winrarpath;   // В Данной строке указываем путь к установленной в Windows архиватору WinRar, к ее екзешнику Rar.exe.!!!!!!!!!!!!!!!!!!
            WinRar wRar = new WinRar(WinRarExePath);// Для работы со стандартным WinRar создали файл WinRar.cs и в нем реализовали работу с архиватором .
            FileInfo info = new FileInfo(Path);

            ftpfilename = "";
            string PathInArchive = Extract.BetweenStart(Path, info.Name) + Extract.BetweenStart(Name, ".") + ".rar";
            ftpfilename = Extract.BetweenStart(Name, ".") + ".rar";
            FileInfo ArchiveInfo = new FileInfo(PathInArchive);
            archivepass = Password;

            if (wRar.CreatePasswordRaRFromFile(Path, archivepass, PathInArchive) == false) // Создает запароленный архив и в результате выдает bool  значение.  
            {
                return false;
            }
            myFTP_client ftp = new myFTP_client();    // создаем новый эеземпляр класса ftp_client.
            ftp.Host = account.host;
            ftp.Password = account.password;
            ftp.Port = account.port;                             //Заполняем его.
            ftp.UserName = account.username;
            if (String.IsNullOrEmpty(account.path) == true)
            {
                res = ftp.UploadFast(PathInArchive, ArchiveInfo.Name); // если настройках аккаунта в settings.ini в параметр path оставить пустым, то файл закидываем в корень сервака.
            }
            else
            {
                res = ftp.UploadInFolder(PathInArchive, account.path, ArchiveInfo.Name); //иначе закидываем по указанному в настройках пути сервера.
            }
            File.Delete(PathInArchive); // Удаляем ненужный более архив на нашем компьютере.
            return res;
        }

        public bool UploadZipArchive(FtpAccounts account, string Path, string Name, string Password, out string ftpfilename)
        {
            bool res = false;
            string archivePass = "";
            ftpfilename = "";
            ZipFile zip;
            FileInfo info = new FileInfo(Path);
            string PathInArchive = Extract.BetweenStart(Path, info.Name) + Extract.BetweenStart(Name, ".") + ".zip";
            ftpfilename = Extract.BetweenStart(Name, ".") + ".zip";
            FileInfo ArchiveInfo = new FileInfo(PathInArchive);
            using (zip = new ZipFile(PathInArchive, Encoding.UTF8))
            {

                archivePass = Password;

                zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Default;  //Устанавливаем уровень сжатия
                zip.TempFileFolder = System.IO.Path.GetTempPath();   //Задаем системную директорию TEMP для временных файлов
                zip.Password = archivePass; // Устанавливаем пароль.
                zip.AddFile(Path, "\\"); //Добавляем файл. и "\\" что файл будет находится в корневой папке архива.
                zip.Save(); //Сохраняем архив
                zip = null;
            }
            myFTP_client ftp = new myFTP_client();
            ftp.Host = account.host;
            ftp.Password = account.password;
            ftp.Port = account.port;
            ftp.UserName = account.username;

            if (String.IsNullOrEmpty(account.path) == true)
            {
                res = ftp.UploadFast(PathInArchive, ArchiveInfo.Name);
            }
            else
            {
                res = ftp.UploadInFolder(PathInArchive, account.path, ArchiveInfo.Name);
            }
            File.Delete(PathInArchive);
            return res;
        }

        string ShowUploadLink(FtpAccounts account, string ftpfilename)
        {
            string res = "";

            res = "http://" + account.host + "/" + account.path + "/" + ftpfilename;

            if (String.IsNullOrEmpty(account.linkview) == false)
            {
                res = account.linkview + "/" + ftpfilename;
            }

            return res;

        }

        public bool UploadNoArchive(FtpAccounts account, string Path, string Name)
        {
            bool res = false;
            myFTP_client ftp = new myFTP_client();
            ftp.Host = account.host;
            ftp.Password = account.password;
            ftp.Port = account.port;
            ftp.UserName = account.username;
            if (String.IsNullOrEmpty(account.path) == true) // проверка пути куда заливать файл.
            {
                res = ftp.UploadFast(Path, Name);
            }
            else
            {
                res = ftp.UploadInFolder(Path, account.path, Name);
            }
            return res;
        }

        public FtpAccounts SelectViewName(string viewname)  // по viewname из settings.ini  получаем данный для текущего ftp аккаунта.
        {

            FtpAccounts Res = new FtpAccounts();
            foreach (var el in FtpAcc_global)
            {
                if (el.viewname == selectAccount_global || el.viewname == viewname)// проверяем соответствие имен аккаунта в Gui и в cmd режимах.
                {
                    Res.viewname = el.viewname;
                    Res.username = el.username;
                    Res.port = el.port;
                    Res.password = el.password;   // получаем настройки аккаунта.
                    Res.path = el.path;
                    Res.host = el.host;
                    Res.linkview = el.linkview;
                }
            }
            return Res;
        }

        public List<FtpAccounts> GetSettingsContent()  // получаем настройки ftp аккаунтов.
        {
            List<FtpAccounts> Result = new List<FtpAccounts>();
            try
            {
                string settingsContent = File.ReadAllText(System.AppDomain.CurrentDomain.BaseDirectory + "\\settings.ini");
                winRarPath_global = Extract.Between(settingsContent, "#winrarpath=", ";");
                string[] mass_1 = settingsContent.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                List<string> filteredftplines = mass_1.Where(x => x.Contains("ftpaccount")).ToList();

                foreach (var el in filteredftplines)
                {
                    FtpAccounts curAcc = new FtpAccounts();
                    curAcc.viewname = Extract.Between(el, "viewname=", ",$");
                    curAcc.username = Extract.Between(el, "username=", ",$");
                    curAcc.password = Extract.Between(el, "password=", ",$");
                    curAcc.port = Extract.Between(el, "port=", ",$");
                    curAcc.host = Extract.Between(el, "host=", ",$");
                    curAcc.path = Extract.Between(el, "path=", ",$");
                    curAcc.linkview = Extract.Between(el, "linkview=", ";");
                    Result.Add(curAcc);
                }
            }
            catch(Exception ecc)
            {
                MessageBox.Show("Ошибка чтения файла settings.ini (должен находится рядом с .exe) : " + ecc.Message);
            }
           
            return Result;
        }

        public string GetRandomPassword() // Получаем случайный числовой пароль состоящий от 15-30 случайных цифр.
        {
            string res = "";

            int randval = (new CryptoRandom()).Next(15, 31);
            for (int i = 0; i <= randval; i++)
            {
                res += ((new CryptoRandom()).Next(1, 10)).ToString();
            }
            return res;
        }

        private void ToClipBoard(string content, string password) // Удаление слова "успешно загружено" или "неуспешно загружено", и добавления ссылки http  и пароля  в буфер обмена cmd.
        {
            if (String.IsNullOrEmpty(password) == true)
            {
                string str = Extract.BetweenEnd(content, "http");
                string rs = "http" + str;
                Clipboard.SetText(rs);
            }
            else
            {
                string str = Extract.BetweenEnd(content, "http").Replace("\r\n", " ");
                string rs = "http" + str;
                Clipboard.SetText(rs);
            }
        }

        #endregion
    }
}
