using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutedHTTPInterpreter.cs.Types
{
    public enum RequestMethod
    {
        GET,
        POST
    }

    public enum HandlerType
    {
        UserFunction,
        File
    }

    public class RequestHandler
    {
        public AzLang.UserFunction HandlerFunction;

        public HandlerType Type;

        public RequestMethod Method;

        public string UriAbsPath;

        public string FilePath;

        public RequestHandler(AzLang.UserFunction handlerfunc, RequestMethod method, string path)
        {
            this.HandlerFunction = handlerfunc;
            this.Method = method;
            this.Type = HandlerType.UserFunction;
            this.UriAbsPath = path;
        }

        public RequestHandler(string filePath, RequestMethod method, string path)
        {
            this.FilePath = filePath;
            this.Method = method;
            this.Type = HandlerType.File;
            this.UriAbsPath = path;
        }
    }
}
