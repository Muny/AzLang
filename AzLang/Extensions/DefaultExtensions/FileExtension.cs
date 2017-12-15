using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCalc;

namespace AzLang.Extensions.DefaultExtensions
{
    [Export(typeof(AzLang.Extensions.IExtension))]
    [ExportMetadata("Identifier", "File extension")]
    public class File_Extension : IExtension
    {
        public Dictionary<string, Action<LanguageInterpreter, FunctionArgs>> GetStockFunctions()
        {
            return new Dictionary<string, Action<LanguageInterpreter, FunctionArgs>>() { 
                { "file_create", File_Extension_Operations.Create }, 
                { "file_delete", File_Extension_Operations.Delete },
                { "file_read", File_Extension_Operations.Read },
                { "file_write", File_Extension_Operations.Write },
                { "file_append", File_Extension_Operations.Append },
                { "file_exists", File_Extension_Operations.Exists}
            };
        }

        public void OnDisabled()
        {

        }

        public void OnEnabled()
        {

        }

        public void SetOwner(object owner)
        {
            
        }
    }

    public static class File_Extension_Operations
    {
        public static void Create(LanguageInterpreter sender, FunctionArgs args)
        {
            if (EUtils.CheckArgs(1, args))
            {
                object param = args.EvaluateParameters()[0];

                if (EUtils.CheckArgType(param, typeof(string)))
                {
                    string filepath = (string)param;

                    if (!File.Exists(filepath))
                    {
                        // could just use .Dispose(); ...
                        using (File.Create(filepath)) { }
                    }
                    else
                    {
                        sender.Interpreter.HandleException(new Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Error creating file.", "A file already exists at the specified path.", ""));
                    }
                }
                else
                {
                    sender.Interpreter.HandleException(new Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Invalid parameter type.", "Function expects parameter of type 'String', got '" + param.GetType().Name + "'.", ""));
                }
            }
            else
            {
                sender.Interpreter.HandleException(Consts.Exceptions.ParameterCountMismatch(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, 1));
            }
        }

        public static void Exists(LanguageInterpreter sender, FunctionArgs args)
        {
            if (EUtils.CheckArgs(1, args))
            {
                object param = args.EvaluateParameters()[0];

                if (EUtils.CheckArgType(param, typeof(string)))
                {
                    args.HasResult = true;
                    args.Result = File.Exists((string)param);
                }
                else
                {
                    sender.Interpreter.HandleException(new Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Invalid parameter type.", "Function expects parameter of type 'String', got '" + param.GetType().Name + "'.", ""));
                }
            }
            else
            {
                sender.Interpreter.HandleException(Consts.Exceptions.ParameterCountMismatch(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, 1));
            }
        }

        public static void Delete(LanguageInterpreter sender, FunctionArgs args)
        {
            if (EUtils.CheckArgs(1, args))
            {
                object param = args.EvaluateParameters()[0];

                if (EUtils.CheckArgType(param, typeof(string)))
                {
                    string filepath = (string)param;

                    if (File.Exists(filepath))
                    {
                        File.Delete(filepath);
                    }
                    else
                    {
                        sender.Interpreter.HandleException(new Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Error deleting file.", "The specified file does not exist.", ""));
                    }
                }
                else
                {
                    sender.Interpreter.HandleException(new Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Invalid parameter type.", "Function expects parameter of type 'String', got '" + param.GetType().Name + "'.", ""));
                }
            }
            else
            {
                sender.Interpreter.HandleException(Consts.Exceptions.ParameterCountMismatch(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, 1));
            }
        }

        public static void Read(LanguageInterpreter sender, FunctionArgs args)
        {
            if (EUtils.CheckArgs(1, args))
            {
                object param = args.EvaluateParameters()[0];

                if (EUtils.CheckArgType(param, typeof(string)))
                {
                    string filepath = (string)param;

                    if (File.Exists(filepath))
                    {
                        args.HasResult = true;
                        args.Result = File.ReadAllText(filepath);
                    }
                    else
                    {
                        sender.Interpreter.HandleException(new Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Error reading file.", "The specified file does not exist.", ""));
                    }
                }
                else
                {
                    sender.Interpreter.HandleException(new Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Invalid parameter type.", "Function expects parameter of type 'String', got '" + param.GetType().Name + "'.", ""));
                }
            }
            else
            {
                sender.Interpreter.HandleException(Consts.Exceptions.ParameterCountMismatch(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, 1));
            }
        }

        public static void Write(LanguageInterpreter sender, FunctionArgs args)
        {
            if (EUtils.CheckArgs(2, args))
            {
                object param = args.EvaluateParameters()[0];
                object param2 = args.EvaluateParameters()[1];

                if (EUtils.CheckArgType(param, typeof(string)))
                {
                    if (EUtils.CheckArgType(param2, typeof(string)))
                    {
                        string filepath = (string)param;
                        string text = (string)param2;

                        if (File.Exists(filepath))
                        {
                            args.HasResult = true;
                            File.WriteAllText(filepath, text);
                        }
                        else
                        {
                            sender.Interpreter.HandleException(new Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Error writing to file.", "The specified file does not exist.", ""));
                        }
                    }
                    else
                    {
                        sender.Interpreter.HandleException(new Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Invalid parameter type.", "Function expects parameter of type 'String', got '" + param2.GetType().Name + "'.", ""));
                    }
                }
                else
                {
                    sender.Interpreter.HandleException(new Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Invalid parameter type.", "Function expects parameter of type 'String', got '" + param.GetType().Name + "'.", ""));
                }
            }
            else
            {
                sender.Interpreter.HandleException(Consts.Exceptions.ParameterCountMismatch(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, 1));
            }
        }

        public static void Append(LanguageInterpreter sender, FunctionArgs args)
        {
            if (EUtils.CheckArgs(2, args))
            {
                object param = args.EvaluateParameters()[0];
                object param2 = args.EvaluateParameters()[1];

                if (EUtils.CheckArgType(param, typeof(string)))
                {
                    if (EUtils.CheckArgType(param2, typeof(string)))
                    {
                        string filepath = (string)param;
                        string text = (string)param2;

                        if (File.Exists(filepath))
                        {
                            args.HasResult = true;
                            File.AppendAllText(filepath, text);
                        }
                        else
                        {
                            sender.Interpreter.HandleException(new Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Error appending to file.", "The specified file does not exist.", ""));
                        }
                    }
                    else
                    {
                        sender.Interpreter.HandleException(new Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Invalid parameter type.", "Function expects parameter of type 'String', got '" + param2.GetType().Name + "'.", ""));
                    }
                }
                else
                {
                    sender.Interpreter.HandleException(new Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Invalid parameter type.", "Function expects parameter of type 'String', got '" + param.GetType().Name + "'.", ""));
                }
            }
            else
            {
                sender.Interpreter.HandleException(Consts.Exceptions.ParameterCountMismatch(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, 1));
            }
        }
    }
}
