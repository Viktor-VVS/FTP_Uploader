using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;

namespace MyFTP_client
{
   public class myFTP_client 
    {
        private string _Host;
        private string _UserName;
        private string _Password;
        private string _Port;

      
          public bool UploadFast(string filepath , string Name)
          {
              //filepath = <<Путь к загружаемому файлу ,вводить в таком формате D:\work hard\Other dll\msi.dll >> 
              bool res = true;
              FileInfo fileInf = new FileInfo(filepath);
              FtpWebRequest reqFTP;
              reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + _Host + ":" + _Port + "/" + Name));   // Создаем FtpWebRequest object .
              reqFTP.Credentials = new NetworkCredential(_UserName, _Password);   // обеспечить WebPermission нужными правами.
              reqFTP.KeepAlive = false;         // Управляющее соединение по умолчанию всегда true, ставим его в false после команды которая выполнится.
              reqFTP.Method = WebRequestMethods.Ftp.UploadFile;    // команда для выполнения Upload.
              reqFTP.UseBinary = true;        // Указываем байтовый тип передачи данных.
              reqFTP.ContentLength = fileInf.Length;  // Указываем серверу размер загружаемого файла.
              int buffLength = 4096;  // Создаем буфер размером в 4кб.
              byte[] buff = new byte[buffLength];
              int contentLen;
              FileStream fs = fileInf.OpenRead();  // Открываем файловый поток ,для чтения файла который будет загружен.

              try
              {
                  Stream strm = reqFTP.GetRequestStream(); // Создаем поток в котором будет вестись запись.
                  contentLen = fs.Read(buff, 0, buffLength); // Читаем из потока по 4 кb.
                  while (contentLen != 0) 
                  {
                      strm.Write(buff, 0, contentLen); // Записываем содержимое из потока файлов в FTP Upload Поток.
                      contentLen = fs.Read(buff, 0, buffLength);
                  }
                  strm.Close();// Закрываем поток запроса.
                  fs.Close();// Закрываем файловый поток.
              }
              catch (Exception ex)
              {
                  MessageBox.Show("Exception from UploadFast :" + ex.ToString());
                  res = false;
              }
              return res;
          }

          public bool UploadInFolder(string filepath ,string FTPdestination , string Name)
          {
              //filepath = <<Путь к загружаемому файлу ,вводить в таком формате "D:\work hard\Other dll\msi.dll" >> 
              //FTPdestination = << Путь к папке назначения  на ftp сервере в формате "books/folder" >>

              bool res = true;
              FileInfo fileInf = new FileInfo(filepath);
              FtpWebRequest reqFTP;
              reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + _Host + ":" + _Port + "/" + FTPdestination + "/" + Name));   // Создаем обьект FtpWebRequest object и указываем путь на ftp куда записываем .
              reqFTP.Credentials = new NetworkCredential(_UserName, _Password);   // обеспечить WebPermission нужными правами.
              reqFTP.KeepAlive = false;         // Управляющее соединение по умолчанию всегда true, ставим его в false после команды которая выполнится.
              reqFTP.Method = WebRequestMethods.Ftp.UploadFile;    // команда для выполнения Upload.
              reqFTP.UseBinary = true;        // Указываем байтовый тип передачи данных.
              reqFTP.ContentLength = fileInf.Length;  // Указываем серверу размер загружаемого файла.
              int buffLength = 4096;  // Создаем буфер размером в 4кб.
              byte[] buff = new byte[buffLength];
              int contentLen;
              FileStream fs = fileInf.OpenRead();  // Открываем файловый поток ,для чтения файла который будет загружен.

              try
              {
                  Stream strm = reqFTP.GetRequestStream(); // Создаем поток в котором будет вестись запись.
                  contentLen = fs.Read(buff, 0, buffLength); // Читаем из потока по 4 кb.
                  while (contentLen != 0)
                  {
                      strm.Write(buff, 0, contentLen); // Записываем содержимое из потока файлов в FTP Upload Поток.
                      contentLen = fs.Read(buff, 0, buffLength);
                  }
                  strm.Close();// Закрываем поток запроса.
                  fs.Close();// Закрываем файловый поток.

                  
              }
              catch (Exception ex)
              {
                  res = false;
              }
              return res;
          }
          public bool Download(string pathfromserver, string destination)
          {
              bool res = true;
              FtpWebRequest reqFTP;
              try
              {
                  //pathfromserver = <<Путь скачиваемого файла с ftp сервера в формате "books/msi.dll" -при скачивании с папки, или "msi.dll" -при скачивании с корня сервака. >>, 
                  //destination = <<Указание место назначения для скачивания на компьютер в формате D:\work hard >>.
                  
                  string NameServFile = PrepareFullName(pathfromserver, destination); // Извлекаем из uri ftp имя файла и составляем полный путь для скачивания на PC.
                  FileStream outputStream = new FileStream(NameServFile, FileMode.Create);  // Создаем выходной поток и указываем полный путь для скачивания файла.
                  reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + _Host + ":" + _Port + "/" + pathfromserver));  // Создаем FTP Запрос с указанием пути откуда будет скачаваться файл.
                  reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;   // Настраиваем метод Download.
                  reqFTP.UseBinary = true; // Тип Данных
                  reqFTP.Credentials = new NetworkCredential(_UserName, _Password); //  Устанавливаем параметры нашего FTP соединения .
                  FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse(); // Создаем обьект ответ от сервера.
                  Stream ftpStream = response.GetResponseStream(); // Получаем поток ответа от сервера.
                  long cl = response.ContentLength; //Узнаем длину файла на сервере. 
                  int bufferSize = 4096; // Задаем размер буффера обмена 4кб.
                  int readCount;// Счетчик прочтения.
                  byte[] buffer = new byte[bufferSize];// Создаем сам буфер обмена.
                  readCount = ftpStream.Read(buffer, 0, bufferSize); // Начинаем считывать с потока Ftp  информацию размеров в наш буфер.
                  while (readCount > 0) 
                  {
                      outputStream.Write(buffer, 0, readCount); // Записываем в выходной поток для записи на PC.
                      readCount = ftpStream.Read(buffer, 0, bufferSize); // считываем следующие 4кб информации ...и так повторяемся пока не достигнем конца файла.
                  }

                  ftpStream.Close(); // Закрываем поток ftp.
                  outputStream.Close(); // закрываем выходной поток записи.
                  response.Close(); // Закрываем запрос на ответ от сервера.
              }
              catch (Exception ex)
              {
                  res = false;
              }

              return res;
          }

          
          private string GetStatusDescription(FtpWebRequest request) // получаю ссылку на файл на сервере
          {
              using (var response = (FtpWebResponse)request.GetResponse())
              {
                  string getUri = response.ResponseUri.AbsoluteUri.Replace("ftp://","");
                  string successfully = response.StatusDescription;
                  if (successfully.Contains("File successfully transferred") == true)
                      successfully = "Успешно загружено!";
                  else
                      successfully = "Неуспешно!(((";
                  return successfully += "\r\n" + getUri;
              }
          }
          

          private string PrepareFullName(string pathfromserver, string destination)
          {
              //pathfromserver = <<Путь скачиваемого файла с ftp сервера в формате "books/msi.dll" -при скачивании с папки, или "msi.dll" -при скачивании с корня сервака. >>, 
              //destination = <<Указание место назначения для скачивания на компьютер в формате D:\work hard >>.
              if (pathfromserver.Contains("/"))
              {
                  int index = pathfromserver.LastIndexOf("/");
                  return destination + "\\" + pathfromserver.Substring(index + 1);
              }
              return destination + "\\" + pathfromserver;
             
          }


          public string Host
          {
              get
              {
                  return _Host;
              }
              set
              {
                  _Host = value;
              }
          }
          public string UserName
          {
              get
              {
                  return _UserName;
              }
              set
              {
                  _UserName = value;
              }
          }
          public string Password
          {
              get
              {
                  return _Password;
              }
              set
              {
                  _Password = value;
              }
          }
          public string Port
          {
              get
              {
                  return _Port;
              }
              set
              {
                  _Port = value;
              }
          }
    } //Работа с FTP клиентом
}
