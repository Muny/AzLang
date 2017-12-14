using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzLang;
using AzLang.Extensions;
using NCalc;
using RoutedHTTPInterpreter.cs.Types;

namespace RoutedHTTPInterpreter.cs.Extensions
{
    [Export(typeof(AzLang.Extensions.IExtension))]
    [ExportMetadata("Identifier", "Request Route Extension")]
    public class RequestRouteExtension : IExtension
    {
        public Dictionary<string, Action<LanguageInterpreter, FunctionArgs>> GetStockFunctions()
        {
            return new Dictionary<string, Action<LanguageInterpreter, FunctionArgs>>() { 
                { "httproute_get", requestroute_get }
            };
        }

        public void OnDisabled()
        {

        }

        public void OnEnabled()
        {

        }

        void requestroute_get(LanguageInterpreter sender, FunctionArgs args)
        {
            if (EUtils.CheckArgs(2, args))
            {
                object param = args.EvaluateParameters()[0];
                object param2 = args.EvaluateParameters()[1];

                /**/

                if (EUtils.CheckArgType(param, typeof(string)))
                {
                    if (EUtils.CheckArgType(param2, typeof(UserFunction)) || EUtils.CheckArgType(param2, typeof(String)))
                    {
                        string relative_path = (string)param;

                        if (EUtils.CheckArgType(param2, typeof(UserFunction)))
                        {
                            UserFunction handler = (UserFunction)param2;

                            RoutedHTTPInterpreter.Program.RequestHandlers.Add(relative_path, new RequestHandler(handler, RequestMethod.GET, relative_path));
                        }
                        else
                        {
                            string file = (string)param2;

                            if (!System.IO.File.Exists(file) && !System.IO.Directory.Exists(file))
                            {
                                sender.Interpreter.HandleException(new AzLang.Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "File or folder non-existent", "The specified file or folder '" + file + "' does not exist.", "Check the file path provided for errors."));
                            }
                            else
                            {
                                RoutedHTTPInterpreter.Program.RequestHandlers.Add(relative_path, new RequestHandler(file, RequestMethod.GET, relative_path));
                            }
                        }
                    }
                    else
                    {
                        sender.Interpreter.HandleException(new AzLang.Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Invalid parameter type.", "Function expects parameter of type 'UserFunction' or 'String', got '" + param2.GetType().Name + "'.", ""));
                    }
                }
                else
                {
                    sender.Interpreter.HandleException(new AzLang.Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Invalid parameter type.", "Function expects parameter of type 'String', got '" + param.GetType().Name + "'.", ""));
                }
            }
            else
            {
                sender.Interpreter.HandleException(Consts.Exceptions.ParameterCountMismatch(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, 1));
            }
        }
    }
}
