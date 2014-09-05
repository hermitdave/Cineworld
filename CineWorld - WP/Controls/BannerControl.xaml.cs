using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Cineworld
{
    public partial class BannerControl : UserControl
    {
        public BannerControl()
        {
            InitializeComponent();
        }

        //public static readonly DependencyProperty MarginProperty = DependencyProperty.RegisterAttached("Margin", typeof(Thickness), typeof(BannerControl), new PropertyMetadata("24,17,0,28", OnViewTitleChanged));
        //private static void OnMarginChanged(DependencyObject theTarget, DependencyPropertyChangedEventArgs e)
        //{
        //    BannerControl bc = theTarget as BannerControl;
        //    if (bc != null)
        //    {
        //        bc.TitlePanel.Margin = (Thickness)e.NewValue;
        //    }
        //}

        //public new Thickness Margin
        //{
        //    get { return (Thickness)GetValue(MarginProperty); }
        //    set { SetValue(MarginProperty, value); }
        //}

        public static readonly DependencyProperty ViewTitleProperty = DependencyProperty.RegisterAttached("ViewTitle", typeof(String), typeof(BannerControl), new PropertyMetadata("", OnViewTitleChanged));
        private static void OnViewTitleChanged(DependencyObject theTarget, DependencyPropertyChangedEventArgs e)
        {
            BannerControl bc = theTarget as BannerControl;
            if (bc != null)
            {
                string value = e.NewValue as string;
                if (String.IsNullOrEmpty(value))
                {
                    bc.tbTitle.Text = String.Empty;
                    bc.tbTitle.Visibility = Visibility.Collapsed;
                }
                else
                {
                    bc.tbTitle.Text = value;
                    bc.tbTitle.Visibility = Visibility.Visible;
                }
            }
        }

        public string ViewTitle
        {
            get { return GetValue(ViewTitleProperty).ToString(); }
            set { SetValue(ViewTitleProperty, value); }
        }
    }
}
