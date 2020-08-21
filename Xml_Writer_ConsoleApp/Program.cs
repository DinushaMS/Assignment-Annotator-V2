using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace Xml_Writer_ConsoleApp
{
    class Program
    {
        static string xmlPath = @"C:\Users\dinus\source\repos\Assignment-Annotator-V2\PDF_Assignment_Annotator\bin\Release\update.xml";
        static string[] tasks = { 
        "update",
        "update",
        "update"
        };
        static string[] fileUrls = {
        @"https://raw.githubusercontent.com/DinushaMS/Assignment-Annotator-V2/master/PDF_Assignment_Annotator/bin/Release/PDF_Assignment_Annotator.exe",
        @"https://raw.githubusercontent.com/DinushaMS/Assignment-Annotator-V2/master/PDF_Assignment_Annotator/bin/Release/PDF_Annotation.dll",
        @"https://raw.githubusercontent.com/DinushaMS/Assignment-Annotator-V2/master/PDF_Assignment_Annotator/bin/Release/SharpUpdate.dll"
        };

        static string[] publishedFilePaths = {
        @"C:\Users\dinus\source\repos\Assignment-Annotator-V2\PDF_Assignment_Annotator\bin\Release\PDF_Assignment_Annotator.exe",
        @"C:\Users\dinus\source\repos\Assignment-Annotator-V2\PDF_Assignment_Annotator\bin\Release\PDF_Annotation.dll",
        @"C:\Users\dinus\source\repos\Assignment-Annotator-V2\PDF_Assignment_Annotator\bin\Release\SharpUpdate.dll"
        };
        static string[] updateDescriptions = {
        "",
        "",
        ""
        };
        static string[] updateLaunchArgs = {
        "",
        "",
        ""
        };
        static void Main(string[] args)
        {
            MakeXML(xmlPath);
            Console.WriteLine("xml built!");
        }

        private static string GetMD5(string filePath)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            //System.IO.FileStream stream = new System.IO.FileStream(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            md5.ComputeHash(stream);

            stream.Close();

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < md5.Hash.Length; i++)
                sb.Append(md5.Hash[i].ToString("x2"));

            return sb.ToString().ToLowerInvariant();
        }

        private static string GetVersion(string filePath)
        {
            FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);
            return myFileVersionInfo.FileVersion;
        }

        private static void MakeXML(string xmlFilePath)
        {
            using (StreamWriter sw = new StreamWriter(xmlFilePath))
            {
                sw.WriteLine("<?xml version=\"1.0\"?>");
                sw.WriteLine($"<sharpUpdate>");
                for (int i = 0; i < publishedFilePaths.Length; i++)
                {
                    sw.WriteLine($"\t<{tasks[i]}>");
                    sw.WriteLine($"\t\t<version>{GetVersion(publishedFilePaths[i])}</version>");
                    sw.WriteLine($"\t\t<url>{fileUrls[i]}</url>");
                    sw.WriteLine($"\t\t<filePath>./{Path.GetFileName(publishedFilePaths[i])}</filePath>");
                    sw.WriteLine($"\t\t<md5>{GetMD5(publishedFilePaths[i])}</md5>");
                    sw.WriteLine($"\t\t<description>{updateDescriptions[i]}<description>");
                    sw.WriteLine($"\t\t<launchArgs>{updateLaunchArgs[i]}</launchArgs>");
                    sw.WriteLine($"\t</{tasks[i]}>");
                }
                sw.WriteLine($"</sharpUpdate>");
            }
        }

        private static void MakeXML_bk(string xmlFilePath)
        {
            FileStream fs = new FileStream(xmlFilePath, FileMode.Create);
            XmlTextWriter w = new XmlTextWriter(fs, Encoding.UTF8);
            w.WriteStartDocument();
            //begining of sharpUpdate
            w.WriteStartElement("sharpUpdate");
            
            for (int i = 0; i < publishedFilePaths.Length; i++)
            {
                //begining of update/add/remove
                w.WriteStartElement(tasks[i]);
                //version
                w.WriteStartElement("version");
                w.WriteString("1.0.0.2");
                w.WriteEndElement();
                //url
                w.WriteStartElement("url");
                w.WriteString(fileUrls[i]);
                w.WriteEndElement();
                //filePath
                w.WriteStartElement("filePath");
                w.WriteString(publishedFilePaths[i]);
                w.WriteEndElement();
                //md5
                w.WriteStartElement("md5");
                w.WriteString(GetMD5(publishedFilePaths[i]));
                w.WriteEndElement();
                //description
                w.WriteStartElement("description");
                w.WriteString(updateDescriptions[i]);
                w.WriteEndElement();
                //launchArgs
                w.WriteStartElement("launchArgs");
                w.WriteString(updateLaunchArgs[i]);
                w.WriteEndElement();
                //end of update/add/remove
                w.WriteEndElement();

            }
            //end of sharpUpdate
            w.WriteEndElement();
            //w.WriteEndDocument();

            w.Flush();
            fs.Close();
        }
    }
}
