using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzLang
{
    public class AnonymousVariableDictionary : AnonymousVariable, IDictionary<string, AnonymousVariable>
    {
        public Dictionary<string, AnonymousVariable> _Dictionary = new Dictionary<string, AnonymousVariable>();

        public void Add(string key, AnonymousVariable value)
        {
            _Dictionary.Add(key, value);
        }

        public void Clear()
        {
            _Dictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, AnonymousVariable> pair)
        {
            return _Dictionary.Contains(pair);
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool ContainsKey(string key)
        {
            return _Dictionary.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return _Dictionary.Keys; }
        }

        bool IDictionary<string, AnonymousVariable>.Remove(string key)
        {
            return _Dictionary.Remove(key);
        }

        public bool TryGetValue(string key, out AnonymousVariable value)
        {
            return _Dictionary.TryGetValue(key, out value);
        }

        public ICollection<AnonymousVariable> Values
        {
            get { return _Dictionary.Values; }
        }

        public AnonymousVariable this[string key]
        {
            get
            {
                return _Dictionary[key];
            }
            set
            {
                _Dictionary[key] = value;
            }
        }

        public void Add(KeyValuePair<string, AnonymousVariable> item)
        {
            _Dictionary.Add(item.Key, item.Value);
        }

        public void CopyTo(KeyValuePair<string, AnonymousVariable>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return _Dictionary.Count; }
        }

        public bool Remove(KeyValuePair<string, AnonymousVariable> item)
        {
            return _Dictionary.Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<string, AnonymousVariable>> GetEnumerator()
        {
            return _Dictionary.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
