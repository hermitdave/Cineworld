using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace CineWorld
{
    public partial class InAppBrowser : PhoneApplicationPage
    {
        public InAppBrowser()
        {
            InitializeComponent();
        }

        private void SpinAndWait(bool bNewVal)
        {
            this.scWaiting.IsSpinning = bNewVal;
            this.wbCineworld.IsEnabled = !bNewVal;
            this.wbCineworld.Opacity = (bNewVal ? 0 : 1);
        }

        private Stack<Uri> NavigationStack = new Stack<Uri>(); 
        public static Uri NavigationUri { get; set; }

        static bool bNotify = true;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.wbCineworld.Navigating += wbCineworld_Navigating;
            this.wbCineworld.Navigated += wbCineworld_Navigated;

            this.wbCineworld.Navigate(NavigationUri);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            this.wbCineworld.Navigating -= wbCineworld_Navigating;
            this.wbCineworld.Navigated -= wbCineworld_Navigated;
        }

        void wbCineworld_Navigating(object sender, NavigatingEventArgs e)
        {
            this.SpinAndWait(true);
        }

        void wbCineworld_Navigated(object sender, NavigationEventArgs e)
        {
            if (bNotify)
            {
                bNotify = false;
                MessageBox.Show("The cineworld website does not display checkbox correctly. Please tap on it and continue");
            }

            this.SpinAndWait(false);
            NavigationStack.Push(e.Uri);
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (NavigationStack.Count > 1)
            {
                // get rid of the topmost item...
                NavigationStack.Pop();
                
                this.wbCineworld.InvokeScript("eval", "history.go(-1)");
                
                // note that this is another Pop - as when the navigate occurs a Push() will happen
                NavigationStack.Pop();
                
                e.Cancel = true;
            }
        }

        private void btnExitTicketing_Click(object sender, EventArgs e)
        {
            if (this.NavigationService.CanGoBack)
                this.NavigationService.GoBack();
        }
    }
}