using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzLang
{
    public class AnonymousVariableArray : AnonymousVariable, IList<AnonymousVariable>
    {
        List<AnonymousVariable> Variables;

        public AnonymousVariableArray()
        {
            Variables = new List<AnonymousVariable>();
        }

        public void Add(AnonymousVariable variable)
        {
            Variables.Add(variable);
        }

        public void Clear()
        {
            Variables.Clear();
        }

        public bool Contains(AnonymousVariable variable)
        {
            return Variables.Contains(variable);
        }

        public int Count
        {
            get
            {
                return Variables.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(AnonymousVariable variable)
        {
            return Variables.Remove(variable);
        }

        public int IndexOf(AnonymousVariable variable)
        {
            return Variables.IndexOf(variable);
        }

        public void RemoveAt(int index)
        {
            Variables.RemoveAt(index);
        }

        public void Insert(int index, AnonymousVariable variable)
        {
            Variables.Insert(index, variable);
        }

        public void CopyTo(AnonymousVariable[] arr, int arrayIndex)
        {
            Variables.CopyTo(arr, arrayIndex);
        }

        public IEnumerator<AnonymousVariable> GetEnumerator()
        {
            return (IEnumerator<AnonymousVariable>)Variables.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public AnonymousVariable this[int index]
        {
            get
            {
                return Variables[index];
            }
            set
            {
                Variables[index] = value;
            }
        }

        public new string SerializeToJSON()
        {
            string json = "[";

            foreach(AnonymousVariable variable in Variables)
            {
                json += variable.SerializeToJSON() + ", ";
            }

            if (json.Length > 1)
                json = json.Remove(json.Length - 2, 2);

            json += "]";

            return json;
        }
    }
}
