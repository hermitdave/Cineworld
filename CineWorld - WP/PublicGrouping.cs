using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;

namespace CineWorld
{
    public class PublicGrouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        private readonly IGrouping<TKey, TElement> _internalGrouping;

        public PublicGrouping(IGrouping<TKey, TElement> internalGrouping)
        {
            _internalGrouping = internalGrouping;
        }

        public override bool Equals(object obj)
        {
            PublicGrouping<TKey, TElement> that = obj as PublicGrouping<TKey, TElement>;

            return (that != null) && (this.Key.Equals(that.Key));
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        #region IGrouping<TKey,TElement> Members

        public TKey Key
        {
            get { return _internalGrouping.Key; }
        }

        #endregion

        #region IEnumerable<TElement> Members

        public IEnumerator<TElement> GetEnumerator()
        {
            return _internalGrouping.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _internalGrouping.GetEnumerator();
        }

        #endregion
    }
}
