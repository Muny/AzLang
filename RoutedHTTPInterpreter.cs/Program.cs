using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AzLang;
using AzLang.Extensions;
using RoutedHTTPInterpreter.cs.Types;
using System.Reflection;

namespace RoutedHTTPInterpreter
{
    class Program
    {
        
        /*static Dictionary<string, Bitmap> DirectoryListingIcons = new Dictionary<string, Bitmap>() {
            {"archive", RoutedHTTPInterpreter.cs.Properties.Resources.archive},
            {"audio", RoutedHTTPInterpreter.cs.Properties.Resources.audio},
            {"authors", RoutedHTTPInterpreter.cs.Properties.Resources.authors},
            {"bin", RoutedHTTPInterpreter.cs.Properties.Resources.bin},
            {"blank", RoutedHTTPInterpreter.cs.Properties.Resources.blank},
            {"bmp", RoutedHTTPInterpreter.cs.Properties.Resources.bmp},
            {"c", RoutedHTTPInterpreter.cs.Properties.Resources.c},
            {"calc", RoutedHTTPInterpreter.cs.Properties.Resources.calc},
            {"cd", RoutedHTTPInterpreter.cs.Properties.Resources.cd},
            {"copying", RoutedHTTPInterpreter.cs.Properties.Resources.copying},
            {"cpp", RoutedHTTPInterpreter.cs.Properties.Resources.cpp},
            {"css", RoutedHTTPInterpreter.cs.Properties.Resources.css},
            {"deb", RoutedHTTPInterpreter.cs.Properties.Resources.deb},
            {"default", RoutedHTTPInterpreter.cs.Properties.Resources._default},
            {"diff", RoutedHTTPInterpreter.cs.Properties.Resources.diff},
            {"doc", RoutedHTTPInterpreter.cs.Properties.Resources.doc},
            {"draw", RoutedHTTPInterpreter.cs.Properties.Resources.draw},
            {"eps", RoutedHTTPInterpreter.cs.Properties.Resources.eps},
            {"exe", RoutedHTTPInterpreter.cs.Properties.Resources.exe},
            {"folder", RoutedHTTPInterpreter.cs.Properties.Resources.folder},
            {"folder-home", RoutedHTTPInterpreter.cs.Properties.Resources.folder_home},
            {"folder-open", RoutedHTTPInterpreter.cs.Properties.Resources.folder_open},
            {"folder-page", RoutedHTTPInterpreter.cs.Properties.Resources.folder_page},
            {"folder-parent", RoutedHTTPInterpreter.cs.Properties.Resources.folder_parent},
            {"folder-parent-old", RoutedHTTPInterpreter.cs.Properties.Resources.folder_parent_old},
            {"gif", RoutedHTTPInterpreter.cs.Properties.Resources.gif},
            {"gzip", RoutedHTTPInterpreter.cs.Properties.Resources.gzip},
            {"h", RoutedHTTPInterpreter.cs.Properties.Resources.h},
            {"hpp", RoutedHTTPInterpreter.cs.Properties.Resources.hpp},
            {"html", RoutedHTTPInterpreter.cs.Properties.Resources.html},
            {"ico", RoutedHTTPInterpreter.cs.Properties.Resources.ico},
            {"image", RoutedHTTPInterpreter.cs.Properties.Resources.image},
            {"install", RoutedHTTPInterpreter.cs.Properties.Resources.install},
            {"java", RoutedHTTPInterpreter.cs.Properties.Resources.java},
            {"jpg", RoutedHTTPInterpreter.cs.Properties.Resources.jpg},
            {"js", RoutedHTTPInterpreter.cs.Properties.Resources.js},
            {"json", RoutedHTTPInterpreter.cs.Properties.Resources.json},
            {"log", RoutedHTTPInterpreter.cs.Properties.Resources.log},
            {"makefile", RoutedHTTPInterpreter.cs.Properties.Resources.makefile},
            {"markdown", RoutedHTTPInterpreter.cs.Properties.Resources.markdown},
            {"package", RoutedHTTPInterpreter.cs.Properties.Resources.package},
            {"pdf", RoutedHTTPInterpreter.cs.Properties.Resources.pdf},
            {"php", RoutedHTTPInterpreter.cs.Properties.Resources.php},
            {"playlist", RoutedHTTPInterpreter.cs.Properties.Resources.playlist},
            {"png", RoutedHTTPInterpreter.cs.Properties.Resources.png},
            {"pres", RoutedHTTPInterpreter.cs.Properties.Resources.pres},
            {"ps", RoutedHTTPInterpreter.cs.Properties.Resources.ps},
            {"psd", RoutedHTTPInterpreter.cs.Properties.Resources.psd},
            {"py", RoutedHTTPInterpreter.cs.Properties.Resources.py},
            {"rar", RoutedHTTPInterpreter.cs.Properties.Resources.rar},
            {"rb", RoutedHTTPInterpreter.cs.Properties.Resources.rb},
            {"readme", RoutedHTTPInterpreter.cs.Properties.Resources.readme},
            {"rpm", RoutedHTTPInterpreter.cs.Properties.Resources.rpm},
            {"rss", RoutedHTTPInterpreter.cs.Properties.Resources.rss},
            {"rtf", RoutedHTTPInterpreter.cs.Properties.Resources.rtf},
            {"script", RoutedHTTPInterpreter.cs.Properties.Resources.script},
            {"source", RoutedHTTPInterpreter.cs.Properties.Resources.source},
            {"sql", RoutedHTTPInterpreter.cs.Properties.Resources.sql},
            {"tar", RoutedHTTPInterpreter.cs.Properties.Resources.tar},
            {"tex", RoutedHTTPInterpreter.cs.Properties.Resources.tex},
            {"text", RoutedHTTPInterpreter.cs.Properties.Resources.text},
            {"tiff", RoutedHTTPInterpreter.cs.Properties.Resources.tiff},
            {"unknown", RoutedHTTPInterpreter.cs.Properties.Resources.unknown},
            {"vcal", RoutedHTTPInterpreter.cs.Properties.Resources.vcal},
            {"video", RoutedHTTPInterpreter.cs.Properties.Resources.video},
            {"xml", RoutedHTTPInterpreter.cs.Properties.Resources.xml},
            {"zip", RoutedHTTPInterpreter.cs.Properties.Resources.zip}
        };*/

        static void Main(string[] args_)
        {
            RoutedHTTPInterpreterServer rhttpi = new RoutedHTTPInterpreterServer();

            //Console.Write("setting priority...");

            //System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime;

            Console.WriteLine("done");

            /*Console.Write("genning data...");

            data = new byte[1000000000];

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)i;
            }

            Console.WriteLine("done");*/

            while(true)
            {
                string cmd = Console.ReadLine();

                switch(cmd)
                {
                    case "reload":
                        rhttpi.Reload();
                        break;

                    case "stop":
                        rhttpi.Stop();
                        return;
                }

                Console.WriteLine("new handler count: " + rhttpi.RequestHandlers.Count);
            }
        }

        //static byte[] data;

        
    }
}
