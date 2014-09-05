using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace CineWorld
{
    public class Group<T> : List<T>
    {
        public Group(object name, IEnumerable<T> items) : base(items)
        {
            this.GroupTitle = name;
        }

        public override bool Equals(object obj)
        {
            Group<T> that = obj as Group<T>;

            return (that != null) && (this.GroupTitle.Equals(that.GroupTitle));
        }

        public override int GetHashCode()
        {
            return this.GroupTitle.GetHashCode();
        }

        public object GroupTitle
        {
            get;
            set;
        }

        public bool HasItems
        {
            get
            {
                return this.Count > 0;
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
    }
}
