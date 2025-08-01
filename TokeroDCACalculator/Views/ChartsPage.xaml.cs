using TokeroDCACalculator.ViewModels;

namespace TokeroDCACalculator.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChartsPage : ContentPage
    {
        public ChartsPage(ChartsViewModel chartsViewModel)
        {
            InitializeComponent();
            BindingContext = ViewModel = chartsViewModel;
        }

        ChartsViewModel ViewModel { get; }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ViewModel.OnAppearing();
        }

        protected override bool OnBackButtonPressed()
        {
            if (BindingContext is ChartsViewModel chartsViewModel)
            {
                if (chartsViewModel.GoBackCommand.CanExecute(null))
                {
                    chartsViewModel.GoBackCommand.Execute(null);
                    return true;
                }
            }
            return base.OnBackButtonPressed();
        }
    }
}