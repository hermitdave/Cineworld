using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cineworld
{
    // Workaround: data binding works best with an enumeration of objects that does not implement IList
    public class ItemCollection<T> : IEnumerable<object>
    {
        private System.Collections.ObjectModel.ObservableCollection<T> itemCollection = new System.Collections.ObjectModel.ObservableCollection<T>();

        public IEnumerator<Object> GetEnumerator()
        {
            return (IEnumerator<object>)itemCollection.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T t)
        {
            itemCollection.Add(t);
        }

        //public IEnumerator<object> IEnumerable.GetEnumerator()
        //{
        //    return (IEnumerator<object>)itemCollection.GetEnumerator();
        //}

        //public System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        //{
        //    return itemCollection.GetEnumerator();
        //}
    }
}
