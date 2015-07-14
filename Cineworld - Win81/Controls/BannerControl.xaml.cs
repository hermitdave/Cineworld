using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Cineworld
{
    public sealed partial class BannerControl : UserControl
    {
        public BannerControl()
        {
            this.InitializeComponent();
        }

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
