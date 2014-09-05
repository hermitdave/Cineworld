using Cineworld.Controls;
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

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Cineworld
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
public sealed partial class InAppBrowser : Cineworld.Common.LayoutAwarePage
{
    public InAppBrowser()
    {
        this.InitializeComponent();

        var wrapper = new WebViewWrapper(this.wbCineworld);
        wrapper.Navigating += WrapperNavigating; 
    }

    void WrapperNavigating(object sender, NavigatingEventArgs e)
    {
        this.SpinAndWait(true);
    }

    private Stack<Uri> NavigationStack = new Stack<Uri>();
    public static Uri NavigationUri { get; set; }

    private void SpinAndWait(bool bNewVal)
    {
        WebViewBrush brush = new WebViewBrush();
        brush.SourceName = "wbCineworld";
        brush.Redraw();
        MaskRectangle.Fill = brush; 

        this.wbCineworld.Visibility = bNewVal ? Windows.UI.Xaml.Visibility.Collapsed : Windows.UI.Xaml.Visibility.Visible;
        this.MaskRectangle.Opacity = bNewVal ? 0.5 : 1;

        this.MaskRectangle.Visibility = bNewVal ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;

        this.gProgress.Visibility = bNewVal ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;
        this.prProgress.IsActive = bNewVal;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        this.AllowSearch(false);

        this.wbCineworld.LoadCompleted += wbCineworld_LoadCompleted;

            this.wbCineworld.Navigate(NavigationUri);

            this.SpinAndWait(true);
    }

    void wbCineworld_LoadCompleted(object sender, NavigationEventArgs e)
    {
        this.SpinAndWait(false);
        NavigationStack.Push(e.Uri);
    }

    protected override void GoBack(object sender, RoutedEventArgs e)
    {
        if (this.NavigationStack.Count > 1)
        {
            this.wbCineworld.InvokeScript("eval", new string[1] { "history.go(-1)" });

            // note that this is another Pop - as when the navigate occurs a Push() will happen
            NavigationStack.Pop();
        }
        else
            base.GoBack(sender, e);
    }

    private void btnExitTicketing_Click(object sender, RoutedEventArgs e)
    {
        base.GoBack(sender, e);
    }
}
}
