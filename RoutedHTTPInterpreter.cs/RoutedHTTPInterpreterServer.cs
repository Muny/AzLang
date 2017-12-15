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
    public class RoutedHTTPInterpreterServer
    {

        private static Assembly assembly = typeof(RoutedHTTPInterpreter.Program).Assembly;

        private static Bitmap ResourceIcon(string name)
        {
            return (Bitmap)System.Drawing.Bitmap.FromStream(assembly.GetManifestResourceStream("RoutedHTTPInterpreter.cs.Resources." + name + ".png"));
        }

        AzLang.LanguageInterpreter Interpreter = null;

        public Dictionary<string, RoutedHTTPInterpreter.cs.Types.RequestHandler> RequestHandlers = new Dictionary<string, cs.Types.RequestHandler>();

        private HttpServer server;
        
        public RoutedHTTPInterpreterServer()
        {
            Interpreter = new AzLang.LanguageInterpreter(new string[] { /*System.Reflection.Assembly.GetExecutingAssembly().Location*/ "/home/kevin/src/csharp-projects/AzLang/RequestRouteExtension/bin/Debug/netcoreapp2.0/RequestRouteExtension.dll" },
                (string msg) =>
                {
                    Console.WriteLine(msg);
                },
                (string msg) =>
                {
                    Console.WriteLine("[DEBUG] " + msg);
                },
                (AzLang.Exception exception) =>
                {
                    string msg = "[EXCEPTION] " + exception.File + ":" + exception.Line + "\n  Message: " + exception.Message + "\n  Detail: " + exception.Detail + "\n  Suggestions: " + exception.Suggestions + "\n";

                    Console.WriteLine(msg);
                },
                (string line) =>
                {
                    //Interpreter.Interpreter.HandleWrite(line);

                },
                () =>
                {

                }
            );

            Interpreter.ExtensionHandler.GetExtensions().Where(e => e.Key == "Request Route Extension").First().Value.SetOwner(this);

            Interpreter.Interpreter.NonStockMethods.Add("flush", (LanguageInterpreter sender, NCalc.FunctionArgs args) =>
            {
                throw new System.Exception("No request context defined.");
            });

            Interpreter.Interpreter.NonStockMethods.Add("web_get_client_ip", (LanguageInterpreter sender, NCalc.FunctionArgs args) =>
            {
                throw new System.Exception("No request context defined.");
            });

            Interpreter.Interpreter.NonStockMethods.Add("web_get_client_port", (LanguageInterpreter sender, NCalc.FunctionArgs args) =>
            {
                throw new System.Exception("No request context defined.");
            });

            Interpreter.Interpret(File.ReadAllText("Agenda.az"), false, "Agenda.az");

            Console.WriteLine(Environment.ProcessorCount + " processors...");
            server = new HttpServer(Environment.ProcessorCount/2);

            Console.WriteLine((Environment.ProcessorCount / 2) + " worker threads");

            server.ProcessRequest += server_ProcessRequest;

            Console.Write("starting server...");

            server.Start(8080);

            Console.WriteLine("done");

            Console.WriteLine("listening on http://127.0.0.1:8080");
        }

        public void Reload()
        {
            Console.Write("[WARN] Reloading script...\n");

            Interpreter.RegenInterpreter();

            RequestHandlers.Clear();

            Interpreter.Interpret(File.ReadAllText("Agenda.az"), false, "Agenda.az");

            Console.WriteLine("[WARN] Done reloading.");
        }

        public void Stop()
        {
            server.Stop();
        }


        void server_ProcessRequest(HttpListenerContext context)
        {
            
            /*context.Response.AddHeader("Content-Type", "application/x-force-download");
            context.Response.ContentLength64 = data.Length;*/

            context.Response.StatusCode = 200;



            Interpreter.Interpreter.HandleWrite = (string msg) =>
            {
                writeStrToContext(context, msg);
            };

            Interpreter.Interpreter.HandleNonEvaluatedLine = (string line) =>
            {
                Interpreter.Interpreter.HandleWrite(line);

            };

            Interpreter.Interpreter.HandleException = (AzLang.Exception exception) =>
            {
                //context.Response.OutputStream..writeFailure();

                string msg = "[EXCEPTION] " + exception.File + ":" + exception.Line + "\n  Message: " + exception.Message + "\n  Detail: " + exception.Detail + "\n  Suggestions: " + exception.Suggestions + "\n";

                Console.WriteLine(msg);

                writeStrToContext(context, msg);

                context.Response.OutputStream.Close();

                //Reload();
            };

            Interpreter.Interpreter.NonStockMethods["flush"] = (LanguageInterpreter sender, NCalc.FunctionArgs args) =>
            {
                if (context.Response.OutputStream.CanWrite)
                    context.Response.OutputStream.Flush();
            };

            Interpreter.Interpreter.NonStockMethods["status_code"] = (LanguageInterpreter sender, NCalc.FunctionArgs args) =>
            {
                args.HasResult = true;

                if (EUtils.CheckArgs(1, args))
                {
                    object param = args.EvaluateParameters()[0];

                    if (EUtils.CheckArgType(param, typeof(int)))
                    {
                        int code = (int)param;

                        context.Response.StatusCode = code;
                    }
                    else
                    {
                        sender.Interpreter.HandleException(new AzLang.Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Invalid parameter type.", "Function expects parameter of type 'Int32', got '" + param.GetType().Name + "'.", ""));
                    }
                }
                else
                {
                    sender.Interpreter.HandleException(AzLang.Consts.Exceptions.ParameterCountMismatch(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, 1));
                }
            };

            Interpreter.Interpreter.NonStockMethods["web_get_client_ip"] = (LanguageInterpreter sender, NCalc.FunctionArgs args) =>
            {
                args.HasResult = true;

                try
                {
                    args.Result = ((IPEndPoint)context.Request.RemoteEndPoint).Address.ToString();
                }
                catch
                {

                }
            };

            Interpreter.Interpreter.NonStockMethods["web_get_client_port"] = (LanguageInterpreter sender, NCalc.FunctionArgs args) =>
            {
                args.HasResult = true;
                try
                {
                    args.Result = ((IPEndPoint)context.Request.RemoteEndPoint).Port.ToString();
                }
                catch
                {

                }
            };

            Interpreter.Interpreter.NonStockMethods["get"] = (LanguageInterpreter sender, NCalc.FunctionArgs args) =>
            {
                args.HasResult = true;

                if (EUtils.CheckArgs(1, args))
                {
                    object param = args.EvaluateParameters()[0];

                    if (EUtils.CheckArgType(param, typeof(string)))
                    {
                        string get_var = (string)param;

                        if (context.Request.QueryString.AllKeys.Contains(get_var))
                        {
                            args.Result = context.Request.QueryString[get_var];
                        }
                        else
                        {
                            sender.Interpreter.HandleException(new AzLang.Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Undefined array key.", "The specified GET variable is non-existent.", ""));
                        }
                    }
                    else
                    {
                        sender.Interpreter.HandleException(new AzLang.Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Invalid parameter type.", "Function expects parameter of type 'String', got '" + param.GetType().Name + "'.", ""));
                    }
                }
                else
                {
                    sender.Interpreter.HandleException(AzLang.Consts.Exceptions.ParameterCountMismatch(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, 1));
                }
            };

            Interpreter.Interpreter.NonStockMethods["cookies_contains"] = (LanguageInterpreter sender, NCalc.FunctionArgs args) =>
            {
                args.HasResult = true;

                if (EUtils.CheckArgs(1, args))
                {
                    object param = args.EvaluateParameters()[0];

                    if (EUtils.CheckArgType(param, typeof(string)))
                    {
                        string cookie_name = (string)param;

                        args.Result = context.Request.Cookies[cookie_name] != null;
                    }
                    else
                    {
                        sender.Interpreter.HandleException(new AzLang.Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Invalid parameter type.", "Function expects parameter of type 'String', got '" + param.GetType().Name + "'.", ""));
                    }
                }
                else
                {
                    sender.Interpreter.HandleException(AzLang.Consts.Exceptions.ParameterCountMismatch(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, 1));
                }
            };

            Interpreter.Interpreter.NonStockMethods["cookies"] = (LanguageInterpreter sender, NCalc.FunctionArgs args) =>
            {
                args.HasResult = true;

                if (EUtils.CheckArgs(1, args))
                {
                    object param = args.EvaluateParameters()[0];

                    if (EUtils.CheckArgType(param, typeof(string)))
                    {
                        string cookie_name = (string)param;

                        if (context.Request.Cookies[cookie_name] != null)
                        {
                            args.Result = context.Request.Cookies[cookie_name].Value;
                        }
                        else
                        {
                            sender.Interpreter.HandleException(new AzLang.Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Undefined array key.", "The specified Cookie name is non-existent.", ""));
                        }
                    }
                    else
                    {
                        sender.Interpreter.HandleException(new AzLang.Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Invalid parameter type.", "Function expects parameter of type 'String', got '" + param.GetType().Name + "'.", ""));
                    }
                }
                else
                {
                    sender.Interpreter.HandleException(AzLang.Consts.Exceptions.ParameterCountMismatch(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, 1));
                }
            };

            Interpreter.Interpreter.NonStockMethods["header"] = (LanguageInterpreter sender, NCalc.FunctionArgs args) =>
            {
                args.HasResult = true;

                if (EUtils.CheckArgs(2, args))
                {
                    object param = args.EvaluateParameters()[0];
                    object param2 = args.EvaluateParameters()[1];

                    if (EUtils.CheckArgType(param, typeof(string)))
                    {
                        if (EUtils.CheckArgType(param2, typeof(string)))
                        {
                            string header = (string)param;
                            string value = (string)param2;

                            context.Response.Headers.Add(header, value);
                        }
                        else
                        {
                            sender.Interpreter.HandleException(new AzLang.Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Invalid parameter type.", "Function expects parameter of type 'String', got '" + param.GetType().Name + "'.", ""));
                        }
                    }
                    else
                    {
                        sender.Interpreter.HandleException(new AzLang.Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Invalid parameter type.", "Function expects parameter of type 'String', got '" + param.GetType().Name + "'.", ""));
                    }
                }
                else
                {
                    sender.Interpreter.HandleException(AzLang.Consts.Exceptions.ParameterCountMismatch(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, 1));
                }
            };

            if (context.Request.Url.AbsolutePath.StartsWith("/_azlang_res/"))
            {
                context.Response.Headers["Cache-Control"] = "max-age=120";

                string req_icon = context.Request.Url.AbsolutePath.Remove(0, 13);

                req_icon = req_icon.Split('.')[0];

                if (File.Exists("Resources/" + req_icon + ".png"))
                {
                    
                    byte[] image = imageToByteArray((Image)ResourceIcon(req_icon));

                    context.Response.OutputStream.Write(image, 0, image.Length);
                }
                else
                {
                    Handle404(context, context.Request.Url.AbsolutePath);
                }
            }
            else
            {

                Dictionary<string, RequestHandler> matches = RequestHandlers.Where(val => val.Key == context.Request.Url.AbsolutePath && val.Value.Method.ToString().ToLower() == context.Request.HttpMethod.ToLower()).ToDictionary(val => val.Key, val => val.Value);

                if (matches.Count > 0)
                {
                    RequestHandler match = matches.First().Value;



                    if (match.Type == HandlerType.UserFunction)
                    {

                        NCalc.FunctionArgs handler_args = new NCalc.FunctionArgs();

                        handler_args.Parameters = new NCalc.Expression[1];

                        handler_args.Parameters[0] = new NCalc.Expression(new RequestContext(context));

                        Interpreter.ExecuteUserFunction(match.HandlerFunction, handler_args);
                    }
                    else if (match.Type == HandlerType.File)
                    {
                        if (File.Exists(match.FilePath))
                        {
                            byte[] bytes = File.ReadAllBytes(match.FilePath);

                            context.Response.OutputStream.Write(bytes, 0, bytes.Length);

                            //writeStrToContext(context, File.ReadAllText(match.FilePath));
                        }
                        else
                        {
                            Handle404(context, match.FilePath);

                            //Interpreter.Interpreter.HandleException(new AzLang.Exception(Interpreter.Interpreter.CurrentFile, Interpreter.Interpreter.CurrentLine, "File '" + match.FilePath + "' is non-existent", "", ""));


                        }
                    }

                    Console.WriteLine(((IPEndPoint)context.Request.RemoteEndPoint).Address.ToString() + ":" + ((IPEndPoint)context.Request.RemoteEndPoint).Port.ToString() + "; request handled: " + match.UriAbsPath + ", method: " + match.Method);
                }
                else
                {
                    Dictionary<string, RequestHandler> dir_matches = RequestHandlers.Where(val => context.Request.Url.AbsolutePath.StartsWith(val.Key) && val.Value.Method.ToString().ToLower() == context.Request.HttpMethod.ToLower() && val.Value.Type == HandlerType.File).ToDictionary(val => val.Key, val => val.Value);

                    if (dir_matches.Count > 0)
                    {
                        RequestHandler dir_match = dir_matches.First().Value;

                        if (Directory.Exists(dir_match.FilePath))
                        {
                            string relative_path = context.Request.Url.AbsolutePath.Remove(context.Request.Url.AbsolutePath.IndexOf(dir_match.UriAbsPath), dir_match.UriAbsPath.Length);

                            if (File.Exists(dir_match.FilePath + relative_path))
                            {
                                byte[] bytes = File.ReadAllBytes(dir_match.FilePath + relative_path);

                                context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                            }
                            else
                            {
                                if (Directory.Exists(dir_match.FilePath + relative_path))
                                {
                                    HandleDirectoryListing(context, dir_match.FilePath + relative_path);
                                }
                                else
                                {
                                    Handle404(context, dir_match.FilePath + relative_path.Remove(0, 1));
                                }
                            }
                        }
                        else
                        {
                            Handle404(context, dir_match.FilePath);
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = 503;

                        writeStrToContext(context, "<h1>Request handler not defined.</h1>The request handler for \"<em>" + context.Request.Url.AbsolutePath + "</em>\" via method <b>" + context.Request.HttpMethod + "</b> is non-existent.");
                    }
                }
            }

            //context.Response.OutputStream.Write(data, 0, data.Length);

            try
            {
                if (context.Response.OutputStream.CanWrite)
                    context.Response.Close();
            }
            catch
            {

            }
        }

        static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        void HandleDirectoryListing(HttpListenerContext context, string path)
        {
            string[] directories = Directory.GetDirectories(path);

            string[] files = Directory.GetFiles(path);

            StringBuilder htmlOutput = new StringBuilder();

            htmlOutput.Append("<html><head><title>Index of /" + path.Remove(path.Length-1, 1) + "</title><style>");

            htmlOutput.Append(RoutedHTTPInterpreter.cs.Properties.Resources.style);

            htmlOutput.Append("</style></head><body><div class=\"wrapper\"><table><tr><th><img width=\"16px\" height=\"16px\" src=\"/_azlang_res/blank.png\" alt=\"[ICO]\" /></th><th><a href=\"\">Name</a></th><th><a href=\"\">Last modified</a></th><th><a href=\"\">Size</a></th></tr><tr><td align=\"top\"><img src=\"/_azlang_res/folder-home.png\" alt=\"[DIR]\" /></td><td><a href=\"REPLACE_WITH_UPPER_DIRECTORY\">Parent Directory</a></td><td>&nbsp;</td><td align=\"right\">  - </td></tr>");

            // handle listing

            foreach(string str in directories)
            {
                DirectoryInfo di = new DirectoryInfo(str);

                htmlOutput.Append("<tr><td valign=\"top\"><img width=\"16px\" height=\"16px\" src=\"/_azlang_res/folder.png\" alt=\"[DIR]\" /></td><td><a href=\"\">" + di.Name + "</a></td><td align=\"right\">" + di.LastWriteTime.ToShortDateString() + " " + di.LastWriteTime.ToShortTimeString() + "  </td><td align=\"right\">  - </td></tr>");
            }

            foreach (string str in files)
            {
                FileInfo fi = new FileInfo(str);

                htmlOutput.Append("<tr><td valign=\"top\"><img width=\"16px\" height=\"16px\" src=\"/_azlang_res/" + GetFileIcon(fi.Extension) + ".png\" /></td><td><a href=\"\">" + fi.Name + "</a></td><td align=\"right\">" + fi.LastWriteTime.ToShortDateString() + " " + fi.LastWriteTime.ToShortTimeString() + "  </td><td align=\"right\">  " + BytesToString(fi.Length) + " </td></tr>");
            }

            htmlOutput.Append("</table></div><script>document.getElementsByTagName('tr')[1].className = 'parent';</script></body></html>");

            writeStrToContext(context, htmlOutput.ToString());

            
        }

        static string GetFileIcon(string extension)
        {
            string result = "default";

            if (extension.Length > 0)
                extension = extension.Remove(0, 1).ToLower();

            switch (extension)
            {
                case "mp3":
                case "m4a":
                case "flac":
                    result = "audio";
                break;

                case "iso":
                case "img":
                    result = "cd";
                break;

                case "doc":
                case "docx":
                    result = "doc";
                break;

                case "jpg":
                case "jpeg":
                    result = "jpg";
                break;

                case "json":
                    result = "js";
                break;

                case "script":
                case "bat":
                case "sh":
                    result = "script";
                break;

                case "text":
                case "txt":
                    result = "text";
                break;

                case "mp4":
                case "avi":
                case "flv":
                case "mov":
                    result = "video";
                break;

                default:
                    result = extension;
                break;
            }

            return result;
        }

        static byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, imageIn.RawFormat);
            return ms.ToArray();
        }

        static void Handle404(HttpListenerContext context, string file)
        {
            context.Response.StatusCode = 404;

            writeStrToContext(context, "<h1>Request handler-defined resource non-existent.</h1>The request handler for \"<em>" + context.Request.Url.AbsolutePath + "</em>\" via method <b>" + context.Request.HttpMethod + "</b> includes resource '" + file + "', but the resource was unavailable.");
        }

        static void writeStrToContext(HttpListenerContext context, string msg)
        {
            try
            {
                if (context.Response.OutputStream.CanWrite)
                {
                    byte[] bytes = Encoding.Default.GetBytes(msg);

                    try
                    {
                        context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                    }
                    catch (System.Exception e)
                    {
                        Console.WriteLine("Exception: " + e.ToString());
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Exception: " + e.ToString());
            }
        }
    }
}