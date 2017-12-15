using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AzLang.Extensions;
using NCalc;

namespace AzLang
{
    public class AzBaseInterpreter
    {
        public AzBaseInterpreter(LanguageInterpreter owner)
        {
            this.Owner = owner;

            NonStockMethods.Add("typeof", _typeof);
            NonStockMethods.Add("sleep", _sleep);
            NonStockMethods.Add("include", _include);
            NonStockMethods.Add("length", _length);
            NonStockMethods.Add("close", _close);
            NonStockMethods.Add("write", _write);
            NonStockMethods.Add("for", _array_for);
            NonStockMethods.Add("md5", _md5);
            NonStockMethods.Add("rand", _rand);
            NonStockMethods.Add("array_append", _array_append);
            NonStockMethods.Add("tolower", _tolower);
        }

        public LanguageInterpreter Owner;

        public Dictionary<string, IExtension> LoadedExtensions = new Dictionary<string,IExtension>();

        public AzBaseInterpreter(Dictionary<string, IExtension> extensions, LanguageInterpreter owner, bool quiet) : this(owner)
        {
            LoadedExtensions = extensions;

            foreach (KeyValuePair<string, IExtension> extPair in extensions)
            {
                Dictionary<string, Action<LanguageInterpreter, FunctionArgs>> extStockFuncs = extPair.Value.GetStockFunctions();

                if (!quiet)
                    Console.WriteLine("Extension: " + extPair.Key + ", stock function count: " + extStockFuncs.Count);

                foreach (KeyValuePair<string, Action<LanguageInterpreter, FunctionArgs>> funcPair in extStockFuncs)
                {
                    NonStockMethods.Add(funcPair.Key, funcPair.Value);

                    if (!quiet)
                        Console.WriteLine("    Loading method from '" + extPair.Key + "': " + funcPair.Key);
                }
            }

            // load plugins here?

            /*UserFunctions.Add("add", new UserFunction(@"
            write('typeof a: ' + typeof(a));
            write('typeof b: ' + typeof(b));

            return a+b;
            ", new string[] { "a", "b" }));*/
        }

        bool debugEnabled = false;

        public Dictionary<string, KeyedVariable> Variables = new Dictionary<string, KeyedVariable>()
        {
            {"undefined", new KeyedVariable("undefined", new Undefined(), typeof(Undefined))}
        };

        public Dictionary<string, KeyedVariable> ScopeCreatedVariables = new Dictionary<string, KeyedVariable>();

        public Dictionary<string, UserFunction> UserFunctions = new Dictionary<string, UserFunction>();

        public Dictionary<string, UserFunction> ScopeCreatedUserFunctions = new Dictionary<string, UserFunction>();

        public List<string> ReservedKeywords = new List<string>()
        { 
            "var", 
            "true", 
            "false",
            "func",
            "return"
        };

        public Dictionary<string, Action<LanguageInterpreter, FunctionArgs>> NonStockMethods = new Dictionary<string, Action<LanguageInterpreter, FunctionArgs>>()
        {

        };

        void _tolower(LanguageInterpreter sender, FunctionArgs args)
        {
            args.HasResult = true;
            args.Result = Variables["undefined"];

            if (args.Parameters.Length > 1)
            {
                HandleException(Consts.Exceptions.TooManyArgs(CurrentFile, CurrentLine));
            }
            else if (args.Parameters.Length < 1)
            {
                HandleException(Consts.Exceptions.NotEnoughArgs(CurrentFile, CurrentLine));
            }
            else
            {
                object[] params_ = args.EvaluateParameters();

                args.Result = params_[0].ToString().ToLower();
            }
        }

        void _array_append(LanguageInterpreter sender, FunctionArgs args)
        {
            args.HasResult = true;
            args.Result = Variables["undefined"];

            if (args.Parameters.Length > 2)
            {
                HandleException(Consts.Exceptions.TooManyArgs(CurrentFile, CurrentLine));
            }
            else if (args.Parameters.Length < 2)
            {
                HandleException(Consts.Exceptions.NotEnoughArgs(CurrentFile, CurrentLine));
            }
            else
            {
                object[] params_ = args.EvaluateParameters();

                if (params_[0].GetType() == typeof(AnonymousVariableArray))
                {

                    AnonymousVariableArray array = (AnonymousVariableArray)params_[0];

                    AnonymousVariable item = new AnonymousVariable(params_[1], params_[1].GetType());

                    array.Add(item);
                }
                else
                {
                    sender.Interpreter.HandleException(new AzLang.Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Invalid parameter type.", "Function expects parameter of type 'Int32', got '" + params_[0].GetType().Name + "'.", ""));
                }
            }
        }

        void _rand(LanguageInterpreter sender, FunctionArgs args)
        {
            args.HasResult = true;
            args.Result = Variables["undefined"];

            if (args.Parameters.Length > 2)
            {
                HandleException(Consts.Exceptions.TooManyArgs(CurrentFile, CurrentLine));
            }
            else if (args.Parameters.Length < 2)
            {
                HandleException(Consts.Exceptions.NotEnoughArgs(CurrentFile, CurrentLine));
            }
            else
            {
                object[] params_ = args.EvaluateParameters();

                if (params_[0].GetType() == typeof(int))
                {
                    if (params_[1].GetType() == typeof(int))
                    {
                        args.Result = Rando.Next((int)params_[0], (int)params_[1]);
                    }
                    else
                    {
                        sender.Interpreter.HandleException(new AzLang.Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Invalid parameter type.", "Function expects parameter of type 'Int32', got '" + params_[0].GetType().Name + "'.", ""));
                    }
                }
                else
                {
                    sender.Interpreter.HandleException(new AzLang.Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Invalid parameter type.", "Function expects parameter of type 'Int32', got '" + params_[0].GetType().Name + "'.", ""));
                }
            }
        }

        Random Rando = new Random();

        void _md5(LanguageInterpreter sender, FunctionArgs args)
        {
            args.HasResult = true;
            args.Result = Variables["undefined"];

            if (args.Parameters.Length > 1)
            {
                HandleException(Consts.Exceptions.TooManyArgs(CurrentFile, CurrentLine));
            }
            else if (args.Parameters.Length < 1)
            {
                HandleException(Consts.Exceptions.NotEnoughArgs(CurrentFile, CurrentLine));
            }
            else
            {
                object[] params_ = args.EvaluateParameters();

                if (params_[0].GetType() == typeof(string))
                {
                    args.Result = CalculateMD5Hash((string)params_[0]);
                }
                else
                {
                    sender.Interpreter.HandleException(new AzLang.Exception(sender.Interpreter.CurrentFile, sender.Interpreter.CurrentLine, "Invalid parameter type.", "Function expects parameter of type 'String', got '" + params_[0].GetType().Name + "'.", ""));
                }
            }
        }

        public string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        void _array_for(LanguageInterpreter sender, FunctionArgs args)
        {
            args.HasResult = true;
            args.Result = Variables["undefined"];

            if (args.Parameters.Length > 5)
            {
                HandleException(Consts.Exceptions.TooManyArgs(CurrentFile, CurrentLine));
            }
            else if (args.Parameters.Length < 5)
            {
                HandleException(Consts.Exceptions.NotEnoughArgs(CurrentFile, CurrentLine));
            }
            else
            {
                object[] params_ = args.EvaluateParameters();

                if (params_[0].GetType() == typeof(UserFunction))
                {
                    if (params_[1].GetType() == typeof(Int32))
                    {
                        if (params_[2].GetType() == typeof(Int32))
                        {
                            if (params_[3].GetType() == typeof(Int32))
                            {
                                if (params_[4].GetType() == typeof(Boolean))
                                {

                                    UserFunction function = (UserFunction)params_[0];

                                    int startValue = (int)params_[1];

                                    int endValue = (int)params_[2];

                                    int deltaValue = (int)params_[3];

                                    bool direction = (bool)params_[4];

                                    if (direction)
                                    {
                                        for (int i = startValue; i < endValue + deltaValue; i += deltaValue)
                                        {
                                            if (i > endValue)
                                                break;

                                            if (i < startValue)
                                                break;

                                            FunctionArgs func_params = new FunctionArgs();

                                            func_params.Parameters = new Expression[1];

                                            func_params.Parameters[0] = new Expression(i.ToString());

                                            ExecuteUserFunction(function, func_params);

                                            if (func_params.Result != null)
                                            {
                                                if (func_params.Result.GetType() == typeof(bool) && (bool)func_params.Result == false)
                                                    break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int i = startValue; i > endValue + deltaValue; i += deltaValue)
                                        {
                                            if (i < endValue)
                                                break;

                                            if (i > startValue)
                                                break;

                                            FunctionArgs func_params = new FunctionArgs();

                                            func_params.Parameters = new Expression[1];

                                            func_params.Parameters[0] = new Expression(i.ToString());

                                            ExecuteUserFunction(function, func_params);

                                            if (func_params.Result != null)
                                            {
                                                if (func_params.Result.GetType() == typeof(bool) && (bool)func_params.Result == false)
                                                    break;
                                            }
                                        }
                                    }

                                    
                                }
                                else
                                {
                                    HandleException(new Exception(CurrentFile, CurrentLine, "Invalid type", "Function 'for' expects fifth argument of type 'Boolean', type provided: '" + params_[4].GetType().Name + "'", ""));
                                }

                            }
                            else
                            {
                                HandleException(new Exception(CurrentFile, CurrentLine, "Invalid type", "Function 'for' expects fourth argument of type 'Int32', type provided: '" + params_[3].GetType().Name + "'", ""));
                            }
                        }
                        else
                        {
                            HandleException(new Exception(CurrentFile, CurrentLine, "Invalid type", "Function 'for' expects third argument of type 'Int32', type provided: '" + params_[2].GetType().Name + "'", ""));
                        }
                    }
                    else
                    {
                        HandleException(new Exception(CurrentFile, CurrentLine, "Invalid type", "Function 'for' expects second argument of type 'Int32', type provided: '" + params_[1].GetType().Name + "'", ""));
                    }
                }
                else
                {
                    HandleException(new Exception(CurrentFile, CurrentLine, "Invalid type", "Function 'for' expects first argument of type 'UserFunction', type provided: '" + params_[0].GetType().Name + "'", ""));
                }
            }
        }

        void _write(LanguageInterpreter sender, FunctionArgs args)
        {
            args.HasResult = true;
            args.Result = Variables["undefined"];

            if (args.Parameters.Length > 1)
            {
                HandleException(Consts.Exceptions.TooManyArgs(CurrentFile, CurrentLine));
            }
            else if (args.Parameters.Length < 1)
            {
                HandleException(Consts.Exceptions.NotEnoughArgs(CurrentFile, CurrentLine));
            }
            else
            {
                object[] params_ = args.EvaluateParameters();

                if (debugEnabled)
                    HandleDebugMsg("_write: " + params_[0] + "");

                try
                {
                    HandleWrite(params_[0].ToString());
                }
                catch (System.Exception ex)
                {
                    HandleException(new Exception(CurrentFile, CurrentLine, ex.Message, "", ""));
                }
            }
        }

        void _close(LanguageInterpreter sender, FunctionArgs args)
        {
            Environment.Exit(0);
        }

        void _sleep(LanguageInterpreter sender, FunctionArgs args)
        {
            args.HasResult = false;

            if (args.Parameters.Length > 1)
            {
                HandleException(Consts.Exceptions.TooManyArgs(CurrentFile, CurrentLine));
            }
            else if (args.Parameters.Length < 1)
            {
                HandleException(Consts.Exceptions.NotEnoughArgs(CurrentFile, CurrentLine));
            }
            else
            {
                object[] params_ = args.EvaluateParameters();

                if (params_[0].GetType() == typeof(Int32))
                {
                    Thread.Sleep((int)params_[0]);
                }
                else
                {
                    HandleException(new Exception(CurrentFile, CurrentLine, "Invalid type", "The sleep function requires 1 parameter of type Int32; provided type: " + params_[0].GetType().Name, "Change type of argument to Int32"));
                }
            }
        }

        void _typeof(LanguageInterpreter sender, FunctionArgs args)
        {
            args.HasResult = true;

            if (args.Parameters.Length > 1)
            {
                HandleException(Consts.Exceptions.TooManyArgs(CurrentFile, CurrentLine));
            }
            else if (args.Parameters.Length < 1)
            {
                HandleException(Consts.Exceptions.NotEnoughArgs(CurrentFile, CurrentLine));
            }
            else
            {
                object obj = args.EvaluateParameters()[0];

                args.Result = obj.GetType().Name;
                    
            }
        }

        void _include(LanguageInterpreter sender, FunctionArgs args)
        {
            args.HasResult = false;

            if (args.Parameters.Length > 1)
            {
                HandleException(Consts.Exceptions.TooManyArgs(CurrentFile, CurrentLine));
            }
            else if (args.Parameters.Length < 1)
            {
                HandleException(Consts.Exceptions.NotEnoughArgs(CurrentFile, CurrentLine));
            }
            else
            {
                object[] params_ = args.EvaluateParameters();

                if (params_[0].GetType() == typeof(string))
                {
                    if (File.Exists(params_[0].ToString()))
                    {
                        AzBaseInterpreter interpreter = new AzBaseInterpreter(this.Owner)
                        {
                            HandleDebugMsg = this.HandleDebugMsg,
                            HandleException = this.HandleException,
                            HandleNonEvaluatedLine = this.HandleNonEvaluatedLine,
                            HandleWrite = this.HandleWrite,
                            HandleDoneExecuting = this.HandleDoneExecuting
                        };

                        interpreter.UserFunctions = this.UserFunctions;
                        interpreter.Variables = this.Variables;
                        interpreter.NonStockMethods = this.NonStockMethods;

                        interpreter.Interpret(File.ReadAllText(params_[0].ToString()), debugEnabled, false, params_[0].ToString());
                    }
                    else
                    {
                        HandleException(new Exception(CurrentFile, CurrentLine, "File non-existant.", "The file specified does not exist.", "Check to make sure the specified file name is correct."));
                    }
                }
                else
                {
                    HandleException(new Exception(CurrentFile, CurrentLine, "Invalid type", "The include function requires 1 parameter of type String; provided type: " + params_[0].GetType().Name, "Change type of argument to String"));
                }
            }
        }

        void _length(LanguageInterpreter sender, FunctionArgs args)
        {
            args.HasResult = true;

            if (args.Parameters.Length < 1)
            {
                HandleException(Consts.Exceptions.NotEnoughArgs(CurrentFile, CurrentLine));
            }
            else if (args.Parameters.Length > 1)
            {
                HandleException(Consts.Exceptions.TooManyArgs(CurrentFile, CurrentLine));
            }
            else
            {
                object[] params_ = args.EvaluateParameters();

                string type = params_[0].GetType().ToString();

                if (debugEnabled)
                    HandleDebugMsg("leng param type: " + type);

                switch (type)
                {
                    case "AzLang.AnonymousVariableArray":
                    {
                        AnonymousVariableArray theList = (AnonymousVariableArray)params_[0];

                        args.Result = theList.Count;

                        break;
                    }

                    case "System.Int32":
                    args.Result = sizeof(int);
                    break;

                    case "System.Decimal":
                    args.Result = sizeof(decimal);
                    break;

                    case "System.Double":
                    args.Result = sizeof(double);
                    break;

                    case "System.Boolean":
                    args.Result = sizeof(bool);
                    break;

                    case "System.String":
                    args.Result = ((string)params_[0]).Length;
                    break;
                }

            }
        }

        public void ExecuteUserFunction(UserFunction function, FunctionArgs args)
        {
            args.HasResult = true;



            AzBaseInterpreter interpreter = new AzBaseInterpreter(this.Owner)
            {
                HandleDebugMsg = this.HandleDebugMsg,
                HandleException = this.HandleException,
                HandleNonEvaluatedLine = this.HandleNonEvaluatedLine,
                HandleWrite = this.HandleWrite,
                HandleDoneExecuting = this.HandleDoneExecuting,
                HandleReturn = (AnonymousVariable returnVar) =>
                {
                    args.Result = returnVar.Value;
                }
            };

            interpreter.UserFunctions = this.UserFunctions;
            interpreter.Variables = this.Variables;
            interpreter.NonStockMethods = this.NonStockMethods;

            if (args.Parameters.Length > function.VariableKeys.Length)
            {
                HandleException(Consts.Exceptions.TooManyArgs(CurrentFile + ":" + function.Name, CurrentLine));
            }
            else if (args.Parameters.Length < function.VariableKeys.Length)
            {
                HandleException(Consts.Exceptions.NotEnoughArgs(CurrentFile + ":" + function.Name, CurrentLine));
            }
            else
            {

                object[] params_ = args.EvaluateParameters();

                if (debugEnabled)
                    HandleDebugMsg("userfunc params_ len: " + params_.Length);

                for (int i = 0; i < params_.Length; i++)
                {
                    if (debugEnabled)
                        HandleDebugMsg("Adding func var: " + function.VariableKeys[i]);

                    if (!interpreter.Variables.ContainsKey(function.VariableKeys[i]))
                    {
                        KeyedVariable var = new KeyedVariable(function.VariableKeys[i], params_[i], params_[i].GetType());
                        interpreter.Variables.Add(function.VariableKeys[i], var);
                        interpreter.ScopeCreatedVariables.Add(function.VariableKeys[i], var);
                    }
                    else
                    {
                        //interpreter.Variables[function.VariableKeys[i]] = new KeyedVariable(function.VariableKeys[i], params_[i], params_[i].GetType());
                        // shouldn't be able to make the same variable in a nested scope...

                        HandleException(new Exception(CurrentFile, CurrentLine, "A variable with the same name already exists in the current scope", "The variable name '" + function.VariableKeys[i] + "' already exists in the current scope.", "Use a different parameter variable name."));
                    }
                }

                string code = "<$az\n" + function.Code + "\naz$>";

                interpreter.Interpret(code, debugEnabled, false, CurrentFile + ":func<" + function.Name + ">");
            }
        }

        public void ExecuteUserFunction(string name, FunctionArgs args)
        {
            ExecuteUserFunction(UserFunctions[name], args);
        }

        public void HandleAMethods(string name, FunctionArgs args)
        {
            if (debugEnabled)
                HandleDebugMsg("Handling amethod: " + name);

            if (NonStockMethods.ContainsKey(name))
            {

                NonStockMethods[name].Invoke(this.Owner, args);

                
            }
            else if (UserFunctions.ContainsKey(name))
            {
                ExecuteUserFunction(name, args);
            }
            else if (Variables.ContainsKey(name))
            {
                if (debugEnabled)
                    HandleDebugMsg("handle amethod variable type: " + Variables[name].Type.ToString());

                if (Variables[name].Type.ToString() == "System.Action`1[NCalc.FunctionArgs]")
                {
                    Action<FunctionArgs> stockmethod = (Action<FunctionArgs>)Variables[name].Value;

                    stockmethod(args);
                }
                else if (Variables[name].Type.ToString() == "AzLang.UserFunction")
                {
                    UserFunction function = (UserFunction)Variables[name].Value;

                    ExecuteUserFunction(function, args);
                }
                else
                {

                    if (args.Parameters.Length > 1)
                    {
                        HandleException(Consts.Exceptions.TooManyArgs(CurrentFile, CurrentLine));
                    }
                    else if (args.Parameters.Length < 1)
                    {
                        HandleException(Consts.Exceptions.NotEnoughArgs(CurrentFile, CurrentLine));
                    }
                    else
                    {

                        object[] params_ = args.EvaluateParameters();

                        args.evald_params = params_;

                        if (Variables[name].Type.ToString() == "AzLang.AnonymousVariableArray")
                        {
                            AnonymousVariableArray theList = (AnonymousVariableArray)Variables[name].Value;

                            if (params_[0].GetType() == typeof(int))
                            {
                                if ((int)params_[0] < theList.Count && (int)params_[0] > -1)
                                {
                                    if (theList[(int)params_[0]].Type == typeof(bool))
                                        args.Result = theList[(int)params_[0]];
                                    else
                                        args.Result = theList[(int)params_[0]].Value;

                                    if (debugEnabled)
                                        HandleDebugMsg("list index selector value: " + args.Result);
                                }
                                else
                                {
                                    HandleException(new Exception(CurrentFile, CurrentLine, "Index out of range", "Index '" + params_[0].ToString() + "' exceeded the range of List: '" + name + "'.", ""));
                                    args.Result = new Undefined();
                                }
                            }
                            else
                            {
                                HandleException(new Exception(CurrentFile, CurrentLine, "Invalid type", "The index selector for type 'List' requires the specified index to be of type 'Int32'", ""));
                                args.Result = new Undefined();
                            }
                        }
                        else
                        {
                            HandleException(new Exception(CurrentFile, CurrentLine, "Index selector requires a variable of type 'List'", "", ""));
                            args.Result = new Undefined();
                        }
                    }
                }
            }
            else
            {

                HandleException(new Exception(CurrentFile, CurrentLine, "Unknown method was called", "The method '" + name + "' does not exist in the current scope.", "Check to make sure the function you typed is correct."));

                args.HasResult = true;
                args.Result = new Undefined();

                
            }
        }

        public void HandleAParameters(string name, ParameterArgs args)
        {
            if (debugEnabled)
                HandleDebugMsg("Handling parameter: " + name);

            if (Variables.ContainsKey(name))
            {
                if (debugEnabled)
                    HandleDebugMsg("parameter handled: " + name);

                args.HasResult = true;

                args.Result = Variables[name].Value;
            }
            else if (UserFunctions.ContainsKey(name))
            {
                if (debugEnabled)
                    HandleDebugMsg("function handled as parameter: " + name);

                args.HasResult = true;

                args.Result = UserFunctions[name];
            }
            else if (NonStockMethods.ContainsKey(name))
            {
                if (debugEnabled)
                    HandleDebugMsg("stock function handled as parameter: " + name);

                args.HasResult = true;

                args.Result = NonStockMethods[name];
            }
            else
            {
                HandleException(new Exception(CurrentFile, CurrentLine, "Unknown anonymous variable or function", "'" + name + "' was unable to be identified as an anonymous variable, keyed variable, function, or an expression.  ", ""));


                args.HasResult = true;
                args.Result = new AnonymousVariable(new Undefined(), typeof(Undefined));

                /*if (name.EndsWith(")") && name.Contains("("))
                {
                    string indexstr = GetSubstrBetweenStrs(name, "(", ")");

                    HandleDebugMsg("indexstr: " + indexstr);

                    string varName = name.Split('(')[0];

                    AnonymousVariable indexVar = AnonymousVariable.Serialize(indexstr, this, file, currLine, debugEnabled);

                    HandleDebugMsg("indexvar type: " + indexVar.Type.Name);
                    HandleDebugMsg("indexvar value: " + indexVar.Value.ToString());
                    HandleDebugMsg("var name: " + varName);

                    if (Variables.ContainsKey(varName))
                    {
                        if (Variables[varName].Type.ToString().StartsWith("System.Collections.Generic.List"))
                        {
                            List<AnonymousVariable> theList = (List<AnonymousVariable>)Variables[varName].Value;

                            if (indexVar.Type == typeof(int))
                            {
                                if ((int)indexVar.Value < theList.Count && (int)indexVar.Value > -1)
                                {
                                    args.Result = theList[(int)indexVar.Value];
                                }
                                else
                                {
                                    HandleException(new Exception(file, currLine, "Index out of range", "Index '" + indexVar.Value + "' exceeded the range of List: '" + varName + "'.", ""));
                                    args.Result = null;
                                }
                            }
                            else
                            {
                                HandleException(new Exception(file, currLine, "Invalid type", "The index selector for type 'List' requires the specified index to be of type 'Int32'", ""));
                                args.Result = null;
                            }
                        }
                        else
                        {
                            HandleException(new Exception(file, currLine, "Index selector requires variable of type 'List'", "", ""));
                            args.Result = null;
                        }
                    }
                    else
                    {
                        args.HasResult = false;
                        args.Result = null;
                    }
                }
                else
                {
                    args.HasResult = false;
                    args.Result = null;
                }*/
            }
        }

        public int CurrentLine = 0;
        public string CurrentFile = "";

        public void Interpret(string code, bool debugEnabled, bool isMainScript, string file = "VM000")
        {
            this.CurrentFile = file;

            this.debugEnabled = debugEnabled;

            string[] lines = code.Split(new char[] { '\n' }, StringSplitOptions.None);

            if (debugEnabled)
                HandleDebugMsg("Interpreting " + lines.Length + " lines");

            List<int> linesToSkip = new List<int>();

            bool isInCodeBlock = false;

            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                int currLine = lineIndex + 1;

                this.CurrentLine = currLine;

                string line = lines[lineIndex].Trim();

                if (isInCodeBlock && line != "az$>" && line != "" && !linesToSkip.Contains(lineIndex) && line != "}" && line != "} else {")
                {
                    if (debugEnabled)
                        HandleDebugMsg("Interpreting line " + currLine + ": " + line);

                    // If not a comment:
                    if (!line.StartsWith("//"))
                    {
                        if (debugEnabled)
                            HandleDebugMsg("line " + currLine + " is not a comment");

                        char[] lineChars = line.ToCharArray();

                        if (debugEnabled)
                            HandleDebugMsg("line " + currLine + " contains " + lineChars.Length + " chars");

                        if (!line.EndsWith(";") && !line.EndsWith("{") && !(lineIndex != lines.Length - 1 && lines[lineIndex + 1].StartsWith("{")))
                        {
                            HandleException(new Exception(file, currLine, "No line end key; expected ';', '{', or '}'", "", "End the current line with a ';', '{' or '}'"));
                        }

                        // is a method call
                        if (CheckForCharInLine(lineChars, '(') && !(ReservedKeywords.Contains(line.Split(' ')[0])) && !(Variables.ContainsKey(line.Split(' ')[0])))
                        {
                            if (debugEnabled)
                                HandleDebugMsg("line " + currLine + " is a method call");

                            string methodName = sep(line, "(").Trim();

                            if (debugEnabled)
                                HandleDebugMsg("line " + currLine + " calls method: " + methodName);

                            AnonymousVariable[] args = GetMethodCallArguments(methodName, line, file, currLine);

                            if (debugEnabled)
                                HandleDebugMsg("line " + currLine + " has " + args.Length + " args");

                            if (methodName != "")
                            {
                                switch (methodName)
                                {
                                    case "if":
                                    {
                                        if (args.Length > 0)
                                        {
                                            if (args.Length > 1)
                                            {
                                                HandleException(Consts.Exceptions.TooManyArgs(file, currLine));
                                            }
                                            else
                                            {
                                                if (args[0].Type == typeof(Boolean))
                                                {
                                                    if ((bool)args[0].Value == true)
                                                    {
                                                        bool foundElseBracket = false;

                                                        int startElse = -1;

                                                        int nestedIndex = 0;

                                                        for (int i = currLine /* this could be currIndex, but then I'd have to + 1 */; i < lines.Length; i++)
                                                        {
                                                            if (!lines[i].Trim().StartsWith("//"))
                                                            {
                                                                if (lines[i].Trim().EndsWith(") {"))
                                                                {
                                                                    if (debugEnabled)
                                                                    {
                                                                        HandleDebugMsg("BLAHAHAH:::" + lines[i].Trim());
                                                                    }
                                                                    nestedIndex++;
                                                                }
                                                                else if (lines[i].Trim() == "} else {")
                                                                {
                                                                    if (nestedIndex == 0)
                                                                    {
                                                                        startElse = i;
                                                                        foundElseBracket = true;
                                                                        break;
                                                                    }
                                                                    else
                                                                    {

                                                                    }
                                                                }
                                                                else if (lines[i].Trim() == "}")
                                                                {
                                                                    nestedIndex--;
                                                                }
                                                            }
                                                        }

                                                        if (foundElseBracket)
                                                        {
                                                            if (debugEnabled)
                                                                HandleDebugMsg("found else bracket in: " + line);

                                                            bool foundEndBracket = false;

                                                            int endLine = -1;

                                                            int nestedCount = 0;

                                                            for (int i = startElse; i < lines.Length; i++)
                                                            {
                                                                if (!lines[i].Trim().StartsWith("//"))
                                                                {
                                                                    if (lines[i].Trim().EndsWith(") {"))
                                                                    {
                                                                        nestedCount++;
                                                                    }
                                                                    if (lines[i].Trim() == "}")
                                                                    {
                                                                        if (nestedCount == 0)
                                                                        {
                                                                            foundEndBracket = true;
                                                                            endLine = i;
                                                                            break;
                                                                        }
                                                                        else
                                                                            nestedCount--;
                                                                    }
                                                                }
                                                            }

                                                            if (foundEndBracket)
                                                            {
                                                                for (int i = startElse; i < endLine; i++)
                                                                {
                                                                    if (debugEnabled)
                                                                        HandleDebugMsg("(true handler) adding future line skip: " + i);

                                                                    linesToSkip.Add(i);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                HandleException(new Exception(file, currLine, "Expected '}'", "A '}' is expected to close the else scope.", ""));
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // do nothing?
                                                        }
                                                    }
                                                    else
                                                    {
                                                        bool foundEndBracket = false;

                                                        int endLine = -1;

                                                        int nestedCount = 0;

                                                        for (int i = currLine /* this could be currIndex, but then I'd have to + 1 */; i < lines.Length; i++)
                                                        {
                                                            if (!lines[i].Trim().StartsWith("//"))
                                                            {
                                                                if (lines[i].Trim().EndsWith(") {"))
                                                                {
                                                                    nestedCount++;
                                                                }
                                                                else if (lines[i].Trim() == "}" || lines[i].Trim() == "} else {")
                                                                {
                                                                    if (nestedCount == 0)
                                                                    {
                                                                        foundEndBracket = true;
                                                                        endLine = i;
                                                                        break;
                                                                    }

                                                                    if (lines[i].Trim() == "}")
                                                                        nestedCount--;
                                                                }
                                                            }

                                                            if (debugEnabled)
                                                                HandleDebugMsg("(false handler) nested count: " + nestedCount);
                                                        }

                                                        if (foundEndBracket)
                                                        {
                                                            if (debugEnabled)
                                                                HandleDebugMsg("found end bracket(in false handler): " + lines[endLine] + ":" + endLine);

                                                            for (int i = currLine; i < endLine; i++)
                                                            {
                                                                if (debugEnabled)
                                                                    HandleDebugMsg("(false handler) adding future line skip: " + i);

                                                                linesToSkip.Add(i);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            HandleException(Consts.Exceptions.NoScopeEnd(file, currLine));
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    HandleException(new Exception(file, currLine, "Invalid type.", "'if' statement requires 1 argument of type 'Boolean'", ""));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            HandleException(Consts.Exceptions.NotEnoughArgs(file, currLine));
                                        }

                                        break;
                                    }

                                    default:
                                    {
                                        if (NonStockMethods.ContainsKey(methodName) || UserFunctions.ContainsKey(methodName) || Variables.ContainsKey(methodName))
                                        {
                                            if (debugEnabled)
                                                HandleDebugMsg("Handling non stock method: " + methodName);

                                            FunctionArgs _args = new FunctionArgs();

                                            _args.Parameters = new Expression[args.Length];

                                            for (int i = 0; i < args.Length; i++)
                                            {

                                                if (args[i] != null)
                                                    _args.Parameters[i] = new Expression(args[i]);
                                            }

                                            HandleAMethods(methodName, _args);

                                            if (Variables.ContainsKey(methodName) && Variables[methodName].Type.ToString() == "AzLang.AnonymousVariableArray")
                                            {
                                                AnonymousVariable variableToModify = (AnonymousVariable)_args.Result;

                                                object index = args[0].RawVariable;

                                                if (debugEnabled)
                                                    HandleDebugMsg("index (should be string): " + index);

                                                if (line.LastIndexOf(")(") > -1 && variableToModify.GetType() == typeof(UserFunction))
                                                {
                                                    

                                                    int the_index = line.LastIndexOf(")(")+1;

                                                    string funcargs_str = line.Remove(0, the_index);

                                                    funcargs_str = funcargs_str.Remove(funcargs_str.Length - 1, 1);

                                                    AnonymousVariable[] funcargs = GetMethodCallArguments(methodName, funcargs_str, file, currLine);

                                                    if (debugEnabled)
                                                    {
                                                        HandleDebugMsg("userfunc fixer args: " + funcargs_str);

                                                        foreach (AnonymousVariable var in funcargs)
                                                        {
                                                            HandleDebugMsg("funcarg: " + var.Value);
                                                        }

                                                        //HandleDebugMsg("test: " + ((UserFunction)variableToModify).Name);
                                                    }

                                                    FunctionArgs _final_funcargs = new FunctionArgs();

                                                    _final_funcargs.Parameters = new Expression[funcargs.Length];

                                                    for (int i = 0; i < funcargs.Length; i++)
                                                    {
                                                        _final_funcargs.Parameters[i] = new Expression(funcargs[i]);
                                                    }

                                                    ExecuteUserFunction((UserFunction)variableToModify, _final_funcargs);
                                                }
                                                else
                                                {
                                                    AnonymousVariable newVar = HandleVariableSetter(line, methodName.Length + 3 + index.ToString().Length, methodName + "(" + index + ")");

                                                    if (newVar.Type != typeof(Undefined))
                                                    {

                                                        if (debugEnabled)
                                                            HandleDebugMsg("testVar value: " + newVar.Value);

                                                        variableToModify.Value = newVar.Value;
                                                    }
                                                }

                                                
                                            }

                                        }
                                        else
                                        {
                                            HandleException(new Exception(file, currLine, "Unknown global method", "Unknown global method was called: '" + methodName + "'", ""));
                                        }
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                HandleException(new Exception(file, currLine, "No method name specified", "", "Specify a method name to call."));
                            }
                        }
                        else if (line.StartsWith("var "))
                        {
                            if (debugEnabled)
                                HandleDebugMsg("line " + currLine + " is a variable setter");

                            string variableKey = "";

                            int curIndex = 4;

                            for (; curIndex < lineChars.Length; curIndex++)
                            {
                                if (lineChars[curIndex] == ' ')
                                    break;
                                else
                                    variableKey += lineChars[curIndex];
                            }

                            bool variableContainsDigits = false;

                            for (int i = 0; i < variableKey.Length; i++)
                            {
                                if (char.IsDigit(variableKey[i]))
                                {
                                    variableContainsDigits = true;
                                    break;
                                }
                            }

                            if (variableContainsDigits)
                            {
                                HandleException(new Exception(file, currLine, "Variable name must not contain any non-alphabetic characters.", "'" + variableKey + "' contains non-alphabetic characters.", "Rename any non-alphabetic characters."));
                            }
                            else if (ReservedKeywords.Contains(variableKey))
                            {
                                HandleException(new Exception(file, currLine, "Variable name must not match any reserved keywords.", "'" + variableKey + "' is a reserved keyword.", "Rename the variable."));
                            }
                            else
                            {
                                if (debugEnabled)
                                    HandleDebugMsg("  variable key is: " + variableKey);

                                bool hasEqual = false;

                                for (; curIndex < lineChars.Length; curIndex++)
                                {
                                    if (lineChars[curIndex] == '=')
                                    {
                                        hasEqual = true;
                                        break;
                                    }
                                }

                                if (hasEqual)
                                {
                                    string variableCand = line.Substring(curIndex + 1).Trim();
                                    variableCand = variableCand.Remove(variableCand.Length - 1, 1);

                                    if (debugEnabled)
                                        HandleDebugMsg("  variable data candidate: " + variableCand);

                                    AnonymousVariable anonvar = ParseVariable(variableCand);

                                    KeyedVariable variable = new KeyedVariable(variableKey, anonvar);

                                    if (debugEnabled)
                                    {
                                        HandleDebugMsg("  variable type: " + variable.Type);
                                        HandleDebugMsg("  variable value: " + variable.Value);
                                    }

                                    if (!Variables.ContainsKey(variableKey))
                                    {
                                        Variables.Add(variableKey, variable);

                                        ScopeCreatedVariables.Add(variableKey, variable);
                                    }
                                    else
                                    {
                                        HandleException(new Exception(file, currLine, "A variable with the same key already exists.", "Only one instance of a variable can exist with the desired name.", "Rename the variable."));
                                    }

                                    if (debugEnabled)
                                        HandleDebugMsg("Global variables length: " + Variables.Count);
                                }
                                else
                                {
                                    HandleException(new Exception(file, currLine, "Variable setter does not set variable value.", "", "Add '= <data>' to complete variable setter."));
                                }
                            }
                        }
                        else if (line.StartsWith("return "))
                        {
                            string rightVar = line.Remove(0, 7);
                            rightVar = rightVar.Remove(rightVar.Length - 1, 1);

                            if (HandleReturn != null)
                                HandleReturn(ParseVariable(rightVar));

                            break;
                        }
                        else if (line.StartsWith("func ") && line.EndsWith("{"))
                        {
                            string funcStuff = line;

                            string name = "";

                            for (int i = 5; i < funcStuff.Length; i++)
                            {
                                if (funcStuff[i] != '(')
                                    name += funcStuff[i];
                                else
                                {
                                    funcStuff = funcStuff.Remove(0, i);
                                    break;
                                }
                            }                            

                            if (!UserFunctions.ContainsKey(name))
                            {
                                funcStuff = funcStuff.Remove(funcStuff.Length - 2, 2);

                                if (debugEnabled)
                                {
                                    HandleDebugMsg("userfunc name: " + name);
                                    HandleDebugMsg("userfunc stuff: " + funcStuff);
                                }

                                string[] rawElements = GetSubstrBetweenStrs(funcStuff, "(", ")").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                                string[] paramKeys = new string[rawElements.Length];

                                for (int i = 0; i < rawElements.Length; i++)
                                {
                                    if (debugEnabled)
                                        HandleDebugMsg(rawElements[i].Trim() + ";");

                                    paramKeys[i] = rawElements[i].Trim();
                                }

                                string functionCode = "";

                                int nestedCount = 0;

                                for (int i = currLine /* this could be currIndex, but then I'd have to + 1 */; i < lines.Length; i++)
                                {
                                    if (!lines[i].Trim().StartsWith("//"))
                                    {
                                        if (!lines[i].Trim().Equals("}"))
                                        {
                                            if (lines[i].Trim() == "{" || lines[i].Trim().EndsWith(" {"))
                                            {
                                                if (lines[i].Trim() != "} else {")
                                                    nestedCount++;
                                            }

                                            functionCode += lines[i].Trim() + "\n";
                                            linesToSkip.Add(i);
                                        }
                                        else
                                        {
                                            if (debugEnabled)
                                                HandleDebugMsg("nested count: " + nestedCount);

                                            if (nestedCount == 0)
                                                break;
                                            else
                                            {
                                                nestedCount--;
                                                functionCode += lines[i].Trim() + "\n";
                                                linesToSkip.Add(i);
                                            }
                                        }
                                    }
                                }

                                if (debugEnabled)
                                    HandleDebugMsg("function code: " + functionCode);

                                UserFunction function = new UserFunction(functionCode, paramKeys, name);

                                UserFunctions.Add(name, function);

                                ScopeCreatedUserFunctions.Add(name, function);
                            }
                            else
                            {
                                HandleException(new Exception(file, currLine, "A function with the same name already exists in the current scope.", "A function with the name '" + name + "' already exists in the current scope.", "Use a different function name."));
                            }
                        }
                        else
                        {
                            bool keyWasFound = false;

                            foreach(KeyValuePair<string, KeyedVariable> pair in Variables)
                            {
                                if (line.StartsWith(pair.Key + " "))
                                {
                                    string[] partitions = line.Remove(line.Length - 1, 1).Split(' ');

                                    string variableKey = partitions[0];

                                    HandleVariableSetter(line, variableKey);

                                    keyWasFound = true;
                                    break;
                                }
                            }

                            if (!keyWasFound)
                                HandleException(new Exception(file, currLine, "Unknown key.", "The line " + currLine + " was unable to be executed because it did not contain a known key.", "Make sure what you typed is a valid identifier."));
                        }

                    }
                    else
                    {
                        if (debugEnabled)
                            HandleDebugMsg("line " + currLine + " is a comment");
                    }
                }
                else
                {
                    

                    if (!isInCodeBlock && line.StartsWith("[DebugEnabled, "))
                    {
                        string bf = line.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries)[1];
                        debugEnabled = bool.Parse(bf.Remove(bf.Length - 1, 1));
                        this.debugEnabled = debugEnabled;
                    }
                    else if (line == "<$az")
                    {
                        if (isInCodeBlock)
                        {
                            HandleException(new Exception(file, currLine, "Unexpected code section start.", "An unexpected '<$az' was encountered.", "Remove any unnecessary code sections."));
                        }
                        else
                        {
                            isInCodeBlock = true;
                        }
                    }
                    else if (line == "az$>")
                    {
                        if (isInCodeBlock)
                            isInCodeBlock = false;
                        else
                        {
                            // shouldn't matter...we don't want to tamper with any data they're outputting...
                        }
                    }
                    else if (!isInCodeBlock)
                    {
                        this.HandleNonEvaluatedLine(lines[lineIndex]);
                    }

                    if (debugEnabled)
                        HandleDebugMsg("Skipping empty or flagged line: " + currLine);
                }
            }

            if (!isMainScript)
            {
                foreach (KeyValuePair<string, KeyedVariable> pair in ScopeCreatedVariables)
                {
                    Variables.Remove(pair.Key);
                }

                foreach (KeyValuePair<string, UserFunction> pair in ScopeCreatedUserFunctions)
                {
                    UserFunctions.Remove(pair.Key);
                }

                HandleDoneExecuting();
            }
        }

        AnonymousVariable HandleVariableSetter(string line, int startIndex, string variableName)
        {
            bool hasOperator = false;

            string op = "";

            int curIndex = startIndex;

            if (debugEnabled)
                HandleDebugMsg("drrr:::===:: curIndex: " + curIndex);

            for (; curIndex < line.Length; curIndex++)
            {
                if (line[curIndex] == ' ')
                {
                    break;
                }
                else
                {
                    op += line[curIndex].ToString();
                    hasOperator = true;
                }
            }

            if (hasOperator)
            {
                if (debugEnabled)
                    HandleDebugMsg("has operator!: " + op);

                string variableCand = line.Substring(curIndex + 1).Trim();
                variableCand = variableCand.Remove(variableCand.Length - 1, 1);

                if (debugEnabled)
                    HandleDebugMsg("  variable data candidate: " + variableCand);

                

                Expression test = null;
                object result = null;

                switch (op)
                {
                    case "=":
                    AnonymousVariable rightVar = ParseVariable(variableCand);

                    result = rightVar.Value;
                    break;

                    case "-=":
                    test = new Expression(variableName + " - " + variableCand);
                    break;

                    case "+=":
                    test = new Expression(variableName + " + " + variableCand);
                    break;

                    case "*=":
                    test = new Expression(variableName + " * " + variableCand);
                    break;

                    case "/=":
                    test = new Expression(variableName + " / " + variableCand);
                    break;
                }

                if (result != null)
                {
                    //Variables[variableName] = new KeyedVariable(variableName, result, result.GetType());

                    return new AnonymousVariable(result, result.GetType());
                }
                else
                {
                    if (test != null)
                    {
                        test.EvaluateFunction += HandleAMethods;
                        test.EvaluateParameter += HandleAParameters;

                        try
                        {
                            result = test.Evaluate();
                        }
                        catch (System.Exception ex)
                        {
                            HandleException(new Exception(CurrentFile, CurrentLine, "Exception occured while evaluating expression.", ex.Message, ""));

                            return new Undefined();
                        }

                        if (debugEnabled)
                            HandleDebugMsg("  expression: " + test.ParsedExpression.ToString());

                        if (result != null)
                        {
                            //Variables[variableName] = new KeyedVariable(variableName, result, result.GetType());

                            return new AnonymousVariable(result, result.GetType());
                        }

                    }
                    else
                    {
                        HandleException(new Exception(CurrentFile, CurrentLine, "Unknown operator", "An unknown operator was provided.", ""));

                        return new Undefined();
                    }
                }

                /*AnonymousVariable anonvar = ParseVariable(variableCand);



                KeyedVariable variable = new KeyedVariable(variableKey, anonvar);

                if (debugEnabled)
                {
                    HandleDebugMsg("  variable type: " + variable.Type);
                    HandleDebugMsg("  variable value: " + variable.Value);
                }

                if (!Variables.ContainsKey(variableKey))
                {
                    Variables.Add(variableKey, variable);
                }
                else
                {
                    HandleException(new Exception(file, currLine, "A variable with the same key already exists.", "Only one instance of a variable can exist with the desired name.", "Rename the variable."));
                }

                if (debugEnabled)
                    HandleDebugMsg("Global variables length: " + Variables.Count);*/
            }
            else
            {
                HandleException(new Exception(CurrentFile, CurrentLine, "Variable setter does not set variable value.", "", "Add '= <data>' to complete variable setter."));

                return new Undefined();
            }

            return new Undefined();


            // IGNORE FOR NOW

            /*string op = partitions[1];

            string val = partitions[2];

            if (debugEnabled)
            {
                HandleDebugMsg("val: " + val);
                HandleDebugMsg("::::::===op:: " + op);
            }

            if (Variables.ContainsKey(variableKey))
            {
                // todo add fix

                AnonymousVariable rightVar = ParseVariable(val);

                Expression test = null;
                object result = null;

                switch (op)
                {
                    case "=":
                    result = rightVar.Value;
                    break;

                    case "-=":
                    test = new Expression(variableKey + " - " + rightVar.Value.ToString());
                    break;

                    case "+=":
                    test = new Expression(variableKey + " + " + rightVar.Value.ToString());
                    break;

                    case "*=":
                    test = new Expression(variableKey + " * " + rightVar.Value.ToString());
                    break;

                    case "/=":
                    test = new Expression(variableKey + " / " + rightVar.Value.ToString());
                    break;
                }

                if (result != null)
                {
                    Variables[variableKey] = new KeyedVariable(variableKey, result, result.GetType());
                }
                else
                {
                    if (test != null)
                    {
                        test.EvaluateFunction += HandleAMethods;
                        test.EvaluateParameter += HandleAParameters;

                        try
                        {
                            result = test.Evaluate();
                        }
                        catch (System.Exception ex)
                        {
                            HandleException(new Exception(file, currLine, "Exception occured while evaluating expression.", ex.Message, ""));
                        }

                        if (debugEnabled)
                            HandleDebugMsg("  expression: " + test.ParsedExpression.ToString());

                        if (result != null)
                        {
                            Variables[variableKey] = new KeyedVariable(variableKey, result, result.GetType());
                        }

                    }
                    else
                    {
                        HandleException(new Exception(file, currLine, "Unknown operator", "An unknown operator was provided.", ""));
                    }
                }
            }
            else
            {
                HandleException(new Exception(file, currLine, "Unknown variable key", "An unknown variable key was provided.", "Check to make sure you're using the right variable name."));
            }*/
        }

        void HandleVariableSetter(string line, string variableName)
        {
            Variables[variableName] = new KeyedVariable(variableName, HandleVariableSetter(line, variableName.Length + 1, variableName));
        }

        AnonymousVariable ParseVariable(string varStr)
        {
            if (debugEnabled)
                HandleDebugMsg("parseVariable: " + varStr);

            if (varStr.StartsWith("[") && varStr.EndsWith("]"))
            {
                // is an array

                if (debugEnabled)
                    HandleDebugMsg(varStr + " is an array");

                AnonymousVariableArray arrayElements = GetElements(varStr, "[", "]", ',');

                if (debugEnabled)
                {
                    foreach (AnonymousVariable obj in arrayElements)
                    {
                        HandleDebugMsg("    array element: " + obj.Value.ToString());
                    }
                }

                return new AnonymousVariable(arrayElements, arrayElements.GetType());
            }
            else if (varStr.StartsWith("{") && varStr.EndsWith("}"))
            {
                // is an object

                /*if (debugEnabled)
                    HandleDebugMsg(varStr + " is an object");

                AnonymousVariableDictionary objItems = GetObjectItems(varStr, "{", "}", ',');

                if (debugEnabled)
                {
                    foreach (KeyValuePair<string, AnonymousVariable> item in objItems)
                    {
                        HandleDebugMsg("    object item key: " + item.Key);
                        HandleDebugMsg("    object item value: " + item.Value.ToString());
                    }
                }

                return new AnonymousVariable(objItems, objItems.GetType());*/

                throw new NotImplementedException();
            }
            else
            {
                bool fixedInlineMethodCall = false;

                string oldVarstr = "";

                if (varStr.LastIndexOf(")(") > -1)
                {
                    int index = varStr.LastIndexOf(")(") + 1;

                    oldVarstr = varStr;

                    varStr = varStr.Remove(index, varStr.Length - index);

                    if (debugEnabled)
                        HandleDebugMsg("new parse variable: " + varStr);

                    fixedInlineMethodCall = true;
                    
                }

                Expression test = null;

                test = new Expression(varStr);

                test.EvaluateFunction += HandleAMethods;
                test.EvaluateParameter += HandleAParameters;

                object result = null;

                try
                {
                    result = test.Evaluate();
                }
                catch (System.Exception ex)
                {
                    HandleException(new Exception(CurrentFile, CurrentLine, "Exception occured while evaluating expression.", ex.Message, ""));
                }

                if (debugEnabled && test.ParsedExpression != null)
                    HandleDebugMsg("  expression: " + test.ParsedExpression.ToString());

                if (result != null)
                {
                    if (debugEnabled)
                        HandleDebugMsg("parse variable result type: " + result.GetType().ToString());

                    if (fixedInlineMethodCall && result.GetType() == typeof(UserFunction))
                    {
                        int the_index = oldVarstr.LastIndexOf(")(")+1;

                        //throw new System.Exception("oldVarstr: " + oldVarstr);

                        string funcargs_str = oldVarstr.Remove(0, the_index);

                        AnonymousVariable[] funcargs = GetMethodCallArguments(((UserFunction)result).Name, funcargs_str, CurrentFile, CurrentLine);

                        if (debugEnabled)
                        {
                            HandleDebugMsg("userfunc fixer args: " + funcargs_str);

                            foreach (AnonymousVariable var in funcargs)
                            {
                                HandleDebugMsg("funcarg: " + var.Value);
                            }

                            //HandleDebugMsg("test: " + ((UserFunction)variableToModify).Name);
                        }

                        FunctionArgs _final_funcargs = new FunctionArgs();

                        _final_funcargs.Parameters = new Expression[funcargs.Length];

                        for (int i = 0; i < funcargs.Length; i++)
                        {
                            _final_funcargs.Parameters[i] = new Expression(funcargs[i]);
                        }

                        ExecuteUserFunction((UserFunction)result, _final_funcargs);

                        // handle args

                        return new AnonymousVariable(_final_funcargs.Result, _final_funcargs.Result.GetType());
                    }
                    else
                    {
                        return new AnonymousVariable(result, result.GetType());
                    }

                    
                }

                return null;
            }
        }

        AnonymousVariableArray GetElements(string raw, string begin, string end, char separator)
        {
            AnonymousVariableArray elements = new AnonymousVariableArray();

            string[] rawElements = GetSubstrBetweenStrs(raw, begin, end).Split(new char[] { separator }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < rawElements.Length; i++)
            {
                elements.Add(AnonymousVariable.Serialize(rawElements[i], this, CurrentFile, CurrentLine, debugEnabled));
            }

            return elements;
        }

        AnonymousVariable[] GetMethodCallArguments(string methodName, string _line, string file, int currLine)
        {
            string line = _line;

            AnonymousVariable[] arr = null;

            if(Variables.ContainsKey(methodName) && Variables[methodName].Type == typeof(AnonymousVariableArray))
            {
                if (debugEnabled)
                    HandleDebugMsg("fixing arr call method: " + methodName + ", " + line);

                if (line.LastIndexOf(")(") != -1)
                {
                    if (debugEnabled)
                        HandleDebugMsg("found it");

                    int index = line.LastIndexOf(")(")+1;

                    line = line.Remove(index, line.Length - index);

                    if (debugEnabled)
                        HandleDebugMsg("fixed argument line: " + line);
                }
            }

            char[] argsRawInbetween = GetSubstrBetweenStrs(line, "(", ")").ToCharArray();

            if (debugEnabled)
                HandleDebugMsg("  raw inbetween: " + new string(argsRawInbetween));

            List<string> result = new List<string>();
            int start = 0;
            bool inParentheses = false;

            bool inQuotes = false;

            int nestCount = 0;

            for (int current = 0; current < argsRawInbetween.Length; current++)
            {
                if (argsRawInbetween[current] == '(') { inParentheses = true; nestCount++; }
                if (argsRawInbetween[current] == ')') { nestCount--; if (nestCount < 1) { inParentheses = false; } }// toggle state

                if (argsRawInbetween[current] == '\'' && (current < argsRawInbetween.Length-1 && argsRawInbetween[current+1] != '.')) { inQuotes = !inQuotes; }

                bool atLastChar = (current == argsRawInbetween.Length - 1);
                if (atLastChar) result.Add(new string(argsRawInbetween).Substring(start));
                else if (argsRawInbetween[current] == ',' && !inParentheses && !inQuotes)
                {
                    result.Add(new string(argsRawInbetween).Substring(start, current - start));
                    start = current + 1;
                }

                if (!inQuotes && argsRawInbetween[current] == '.' && (current > 0 && argsRawInbetween[current - 1] != '\''))
                    argsRawInbetween[current] = '_';
            }

            string[] rawArguments = result.ToArray();

            //string[] rawArguments = argsRawInbetween.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            arr = new AnonymousVariable[rawArguments.Length];

            for (int i = 0; i < rawArguments.Length; i++ )
            {
                string rawArg = rawArguments[i].Trim();

                if (debugEnabled)
                    HandleDebugMsg("  arg: " + rawArg);

                AnonymousVariable anonvar = AnonymousVariable.Serialize(rawArg, this, file, currLine, debugEnabled);

                if (debugEnabled && anonvar != null && anonvar.Value != null)
                {
                    HandleDebugMsg("    anonArg value: " + anonvar.Value.ToString());
                    HandleDebugMsg("    anonArg type: " + anonvar.Type.ToString());
                }

                arr[i] = anonvar;
            }

            return arr;
        }



        string GetSubstrBetweenStrs(string str, string front, string end)
        {
            int pFrom = str.IndexOf(front) + front.Length;
            int pTo = str.LastIndexOf(end);

            return str.Substring(pFrom, pTo - pFrom);
        }

        string sep(string s, string needle)
        {
            int l = s.IndexOf(needle);
            if (l > 0)
            {
                return s.Substring(0, l);
            }
            return "";
        }

        bool CheckForCharInLine(char[] line, char charTocheckfor)
        {
            bool contains = false;

            bool isInString = false;

            foreach (char c in line)
            {
                if (c == '"')
                {
                    if (isInString)
                        isInString = false;
                    else
                        isInString = true;
                }

                if (c == charTocheckfor && !isInString)
                {
                    contains = true;
                }
            }

            return contains;
        }

        public void Dispose()
        {
            foreach (KeyValuePair<string, KeyedVariable> pair in ScopeCreatedVariables)
            {
                Variables.Remove(pair.Key);
            }

            foreach (KeyValuePair<string, UserFunction> pair in ScopeCreatedUserFunctions)
            {
                UserFunctions.Remove(pair.Key);
            }

            HandleDoneExecuting();

            HandleWrite = null;
            HandleDebugMsg = null;
            HandleException = null;
            HandleNonEvaluatedLine = null;
            HandleDoneExecuting = null;
            HandleReturn = null;
        }

        public Action<string> HandleWrite;

        public Action<string> HandleDebugMsg;

        public Action<Exception> HandleException;

        public Action<string> HandleNonEvaluatedLine;

        public Action HandleDoneExecuting;

        public Action<AnonymousVariable> HandleReturn;
    }
}
