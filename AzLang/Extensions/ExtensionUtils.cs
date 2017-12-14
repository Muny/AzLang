using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCalc;

namespace AzLang.Extensions
{
    public interface IExtensionHandler
    {
        Dictionary<string, Action<LanguageInterpreter, FunctionArgs>> GetStockFunctions(String ExtensionName);

        Boolean ExtensionExists(String ExtensionName);

        int ExtensionCount();

        string[] GetExtensionNames();

        void EnableExtensions();

        void DisableExtensions();

        void DisableExtension(String ExtensionName);

        void EnableExtension(String ExtensionName);

        Dictionary<string, IExtension> GetExtensions();
    }

    public interface IExtension
    {
        Dictionary<string, Action<LanguageInterpreter, FunctionArgs>> GetStockFunctions();

        void OnEnabled();

        void OnDisabled();
    }

    public interface IExtensionIdentifier
    {
        String Identifier { get; }
    }

    [Export(typeof(IExtensionHandler))]
    public class ExtensionHandler : IExtensionHandler
    {
        [ImportMany]
        IEnumerable<Lazy<IExtension, IExtensionIdentifier>> Extensions;

        public Dictionary<string, Action<LanguageInterpreter, FunctionArgs>> GetStockFunctions(string ExtensionName)
        {
            return Extensions.Where(val => val.Metadata.Identifier.Equals(ExtensionName)).First().Value.GetStockFunctions();
        }

        public Boolean ExtensionExists(string ExtensionName)
        {
            return (Extensions.Where(value => value.Metadata.Identifier == ExtensionName).Count() > 0);
        }

        public int ExtensionCount()
        {
            return Extensions.Count();
        }

        public Dictionary<string, IExtension> GetExtensions()
        {
            return Extensions.ToDictionary(val => val.Metadata.Identifier, val => val.Value);
        }

        public string[] GetExtensionNames()
        {
            return Extensions.ToDictionary(val => val.Metadata.Identifier).Keys.ToArray();
        }

        public void EnableExtensions()
        {
            foreach (IExtension Extension in Extensions)
            {
                Extension.OnEnabled();
            }
        }

        public void DisableExtensions()
        {
            foreach (IExtension Extension in Extensions)
            {
                Extension.OnDisabled();
            }
        }

        public void EnableExtension(string Extension)
        {
            Extensions.Where(val => val.Metadata.Identifier == Extension).First().Value.OnEnabled();
        }

        public void DisableExtension(string Extension)
        {
            Extensions.Where(val => val.Metadata.Identifier == Extension).First().Value.OnDisabled();
        }
    }
}
