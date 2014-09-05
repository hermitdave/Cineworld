using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace CineWorld
{
    public class Group<T> : IEnumerable<T>
    {
        public Group(string name, IEnumerable<T> items)
        {
            this.Title = name;
            this.Items = new List<T>(items);
        }

        public override bool Equals(object obj)
        {
            Group<T> that = obj as Group<T>;

            return (that != null) && (this.Title.Equals(that.Title));
        }

        public override int GetHashCode()
        {
            return this.Title.GetHashCode();
        }

        public string Title
        {
            get;
            set;
        }

        public IList<T> Items
        {
            get;
            set;
        }

        public bool HasItems
        {
            get
            {
                return Items.Count > 0;
            }
        }

        public Brush GroupHeaderBrush
        {
            get
            {
                if (HasItems)
                    return Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
                else
                    return new SolidColorBrush(Colors.LightGray);
            }
        }

        public Brush JumplistBackgroundBrush
        {
            get
            {
                if (HasItems)
                    return Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
                else
                    return new SolidColorBrush(Colors.LightGray);
            }
        }

        public Brush JumplistForegroundBrush
        {
            get
            {
                if (HasItems)
                    return new SolidColorBrush(Colors.White);
                else
                    return new SolidColorBrush(Colors.DarkGray);
            }
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }

        #endregion
    }
}
