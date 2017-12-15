using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCalc;
using Codeplex.Data;

namespace AzLang.Extensions.DefaultExtensions
{
    [Export(typeof(AzLang.Extensions.IExtension))]
    [ExportMetadata("Identifier", "JSON extension")]
    public class JSON_Extension : IExtension
    {
        public Dictionary<string, Action<LanguageInterpreter, FunctionArgs>> GetStockFunctions()
        {
            return new Dictionary<string, Action<LanguageInterpreter, FunctionArgs>>() { 
                { "json_parse", json_parse },
                { "json_stringify", json_stringify }
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

        void json_parse(LanguageInterpreter sender, FunctionArgs args)
        {
            if (EUtils.CheckArgs(1, args))
            {
                object param = args.EvaluateParameters()[0];

                if (EUtils.CheckArgType(param, typeof(string)))
                {
                    string json_str = (string)param;

                    dynamic parsed_json = DynamicJson.Parse(json_str);

                    if (parsed_json.IsArray)
                    {
                        AnonymousVariableArray finalArray = VariableArrayFromDynamic(parsed_json);

                        args.HasResult = true;
                        args.Result = finalArray;
                    }
                    else
                    {
                        sender.Interpreter.HandleException(new Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Unimplemented", "Only arrays are implemented in the JSON parser.", ""));
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

        AnonymousVariableArray VariableArrayFromDynamic(dynamic val)
        {
            dynamic[] arrayValues = (dynamic[])val;

            AnonymousVariableArray finalArray = new AnonymousVariableArray();

            for (int i = 0; i < arrayValues.Length; i++)
            {
                if (arrayValues[i].GetType() == typeof(DynamicJson) && arrayValues[i].IsArray)
                {
                    AnonymousVariableArray arr = VariableArrayFromDynamic(arrayValues[i]);

                    finalArray.Add(new AnonymousVariable(arr, arr.GetType()));
                }
                else
                    finalArray.Add(new AnonymousVariable(arrayValues[i], arrayValues[i].GetType()));
            }

            return finalArray;
        }

        void json_stringify(LanguageInterpreter sender, FunctionArgs args)
        {
            if (EUtils.CheckArgs(1, args))
            {
                object param = args.EvaluateParameters()[0];

                if (EUtils.CheckArgType(param, typeof(AnonymousVariableArray)))
                {
                    AnonymousVariableArray array = (AnonymousVariableArray)param;

                    args.HasResult = true;
                    args.Result = array.SerializeToJSON();
                }
                else
                {
                    sender.Interpreter.HandleException(new Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Invalid parameter type.", "Function expects parameter of type 'AnonymousVariableArray', got '" + param.GetType().Name + "'.", ""));
                }
            }
            else
            {
                sender.Interpreter.HandleException(Consts.Exceptions.ParameterCountMismatch(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, 1));
            }
        }
    }

}
