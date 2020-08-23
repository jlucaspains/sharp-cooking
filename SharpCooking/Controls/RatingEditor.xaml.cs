using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SharpCooking.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile), PropertyChanged.DoNotNotify]
    public partial class RatingEditor : ContentView
    {
        public RatingEditor()
        {
            InitializeComponent();
            TapGestureCommand = new Command<int>(TapGestureRecognized);
        }

        public Command TapGestureCommand { get; private set; }

        public static readonly BindableProperty RatingProperty =
           BindableProperty.Create(nameof(Rating),
                                   typeof(int),
                                   typeof(RatingEditor),
                                   default(int),
                                   propertyChanged: OnRatingChanged);

        public int Rating
        {
            set { SetValue(RatingProperty, value); }
            get { return (int)GetValue(RatingProperty); }
        }

        public static readonly BindableProperty IsDisabledProperty =
           BindableProperty.Create(nameof(IsDisabled),
                                   typeof(bool),
                                   typeof(RatingEditor),
                                   default(bool));

        public bool IsDisabled
        {
            set { SetValue(IsDisabledProperty, value); }
            get { return (bool)GetValue(IsDisabledProperty); }
        }

        private void TapGestureRecognized(int value)
        {
            if (IsDisabled) return;

            Rating = value;

            UpdateStars(this, Rating);
        }

        private static void OnRatingChanged(BindableObject bindable, object oldValue, object newValue)
        {
            UpdateStars((RatingEditor)bindable, (int)newValue);
        }

        private static void UpdateStars(RatingEditor editor, int value)
        {
            editor.starSelectedOne.IsVisible = value >= 1;
            editor.starSelectedTwo.IsVisible = value >= 2;
            editor.starSelectedThree.IsVisible = value >= 3;
            editor.starSelectedFour.IsVisible = value >= 4;
            editor.starSelectedFive.IsVisible = value >= 5;
        }

        private void TapGestureRecognizer_Tapped_5(object sender, System.EventArgs e)
        {
            TapGestureRecognized(5);
        }

        private void TapGestureRecognizer_Tapped_4(object sender, System.EventArgs e)
        {
            TapGestureRecognized(4);
        }

        private void TapGestureRecognizer_Tapped_3(object sender, System.EventArgs e)
        {
            TapGestureRecognized(3);
        }

        private void TapGestureRecognizer_Tapped_2(object sender, System.EventArgs e)
        {
            TapGestureRecognized(2);
        }

        private void TapGestureRecognizer_Tapped_1(object sender, System.EventArgs e)
        {
            TapGestureRecognized(1);
        }
    }
}