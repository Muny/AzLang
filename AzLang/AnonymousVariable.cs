using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCalc;

namespace AzLang
{
    public class AnonymousVariable : IConvertible
    {
        public object Value;
        public Type Type;

        public string RawVariable = null;

        public AnonymousVariable(object Value, Type Type)
        {
            this.Value = Value;
            this.Type = Type;
        }

        public AnonymousVariable() { }

        public static AnonymousVariable Serialize(string rawVariable, AzBaseInterpreter interpreter, string file, int line, bool debugEnabled)
        {
            Expression test = new Expression(rawVariable);

            if (debugEnabled)
                interpreter.HandleDebugMsg("new expression: " + rawVariable);

            test.EvaluateFunction += interpreter.HandleAMethods;
            test.EvaluateParameter += interpreter.HandleAParameters;

            object result = null;

            result = test.Evaluate();

            if (debugEnabled)
                interpreter.HandleDebugMsg("eval: " + result);

            if (result == null)
            {
                interpreter.HandleException(new Exception(file, line, "Unknown anonymous variable or function", "'" + rawVariable + "' was unable to be identified as an anonymous variable, keyed variable, function, or an expression.  " + test.Error, ""));

                return new AnonymousVariable(new Undefined(), typeof(Undefined)) { RawVariable = rawVariable };
            }
            else if (result.GetType() == typeof(AnonymousVariable) || result.GetType() == typeof(KeyedVariable) || result.GetType() == typeof(Undefined))
                return (AnonymousVariable)result;
            else
                return new AnonymousVariable(result, result.GetType()) { RawVariable = rawVariable };

            /*if (rawVariable.StartsWith("\""))
            {
                if (rawVariable.EndsWith("\""))
                {
                    return new AnonymousVariable(rawVariable.Trim(new char[] { '"' }), DataType.String);
                }
                else
                {
                    interpreter.HandleException(new Exception(file, line, "Expexted '\"'", "", "End the string value with '\"'"));
                    return null;
                }
            }
            else
            {
                if (rawVariable == "true" || rawVariable == "false")
                {
                    return new AnonymousVariable(bool.Parse(rawVariable), DataType.Bool);
                }
                else
                {
                    int n;
                    bool isNumeric = int.TryParse(rawVariable, out n);

                    if (isNumeric)
                    {
                        return new AnonymousVariable(n, DataType.Int);
                    }
                    else
                    {
                        if (interpreter.Variables.ContainsKey(rawVariable))
                        {
                            return interpreter.Variables[rawVariable];
                        }
                        else
                        {
                            Expression test = new Expression(rawVariable);

                            interpreter.Variables.ToList().ForEach(x => test.Parameters.Add(x.Key, x.Value));

                            object result = null;

                            try
                            {
                                result = test.Evaluate();
                            }
                            catch
                            {

                            }

                            if (result != null)
                            {

                            }
                            else
                            {
                                interpreter.HandleException(new Exception(file, line, "Unknown anonymous variable", "'" + rawVariable + "' was unable to be identifier as an anonymous variable, keyed variable, or an expression.", ""));
                                return null;
                            }

                            //test.Parameters = interpreter.Variables.ToDictionary(item => item.Key, item => (object)item.Value);

                            
                        }
                    }
                }
            }*/
        }

        public string SerializeToJSON()
        {
            string result = "";

            switch (Value.GetType().ToString())
            {
                case "AzLang.UserFunction":
                result = "\"" + Value.ToString().Replace("\"", "\\\\\"") + "\"";
                break;

                case "System.Boolean":
                result = Value.ToString().ToLower();
                break;

                case "System.String":
                result = "\"" + Value.ToString().Replace("\"", "\\\\\"") + "\"";
                break;

                case "AzLang.AnonymousVariableArray":

                result = ((AnonymousVariableArray)Value).SerializeToJSON();

                break;

                default:
                result = Value.ToString();
                break;
            }
            return result;
        }

        public override string ToString()
        {
            if (Value != null && Value.GetType().ToString() == "System.Boolean")
            {
                return Value.ToString().ToLower();
            }
            else
                return base.ToString();
        }

        public TypeCode GetTypeCode()
        {
            throw new NotImplementedException();
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider provider)
        {
            return ToString();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }
    }
}
