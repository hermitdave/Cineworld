using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Cineworld
{
    public partial class SpinnerControl
    {
        #region Fields

        public static readonly DependencyProperty IsSpinningProperty = DependencyProperty.Register("IsSpinning", typeof(bool), typeof(SpinnerControl), new PropertyMetadata(OnIsSpinningPropertyChanged));

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register("Status", typeof(string), typeof(SpinnerControl), new PropertyMetadata(OnStatusPropertyChanged));

        public static new readonly DependencyProperty ForegroundProperty = DependencyProperty.Register("Foreground", typeof(Color), typeof(SpinnerControl), new PropertyMetadata(OnForegroundPropertyChanged));

        public static new readonly DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Color), typeof(SpinnerControl), new PropertyMetadata(OnBackgroundPropertyChanged));
        #endregion

        #region Constructors

        public SpinnerControl()
        {
            InitializeComponent();

            this.Foreground = (Color)Application.Current.Resources["PhoneAccentColor"];
        }

        #endregion

        #region Properties

        public bool IsSpinning
        {
            get { return (bool)GetValue(IsSpinningProperty); }
            set { SetValue(IsSpinningProperty, value); }
        }

        public string Status
        {
            get { return (string)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public new Color Foreground
        {
            get { return (Color)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public new Color Background
        {
            get { return (Color)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        #endregion

        #region Private Methods

        private static void OnIsSpinningPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            SpinnerControl spinnerControl = dependencyObject as SpinnerControl;

            if (spinnerControl != null)
            {
                if (spinnerControl.IsSpinning)
                {
                    spinnerControl.StoryBoardAnimateSpinner.Begin();
                }
                else
                {
                    spinnerControl.StoryBoardAnimateSpinner.Stop();
                }

                spinnerControl.Visibility = spinnerControl.IsSpinning ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private static void OnStatusPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            SpinnerControl spinnerControl = dependencyObject as SpinnerControl;

            if (spinnerControl != null)
            {
                spinnerControl.textBlockStatus.Text = spinnerControl.Status;
            }
        }

        private static void OnForegroundPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            SpinnerControl spinnerControl = dependencyObject as SpinnerControl;

            if (spinnerControl != null)
            {
                Color accentcol = spinnerControl.Foreground;

                SolidColorBrush brush = new SolidColorBrush(accentcol);

                spinnerControl.textBlockStatus.Foreground = new SolidColorBrush(accentcol);

                foreach (var child in spinnerControl.ElParent.Children)
                {
                    if (child is Ellipse)
                    {
                        (child as Ellipse).Fill = brush;
                    }
                }
            }
        }

        private static void OnBackgroundPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            SpinnerControl spinnerControl = dependencyObject as SpinnerControl;

            if (spinnerControl != null)
            {
                Color accentcol = spinnerControl.Background;

                SolidColorBrush brush = new SolidColorBrush(accentcol);

                spinnerControl.gSpinner.Background = brush;
            }
        }
        #endregion
    }
}