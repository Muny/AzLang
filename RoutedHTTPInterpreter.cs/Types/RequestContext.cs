using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RoutedHTTPInterpreter.cs.Types
{
    public class RequestContext : AzLang.AnonymousVariable
    {
        HttpListenerContext ListenerContext;

        public RequestContext(HttpListenerContext context) => this.ListenerContext = context;
    }
}
