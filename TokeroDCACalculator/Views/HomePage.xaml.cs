using TokeroDCACalculator.ViewModels;

namespace TokeroDCACalculator.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage
    {
        #region - Private fields

        private bool _isBackPressedOnce = false;

        #endregion

        public HomePage(HomePageViewModel homePageViewModel)
        {
            InitializeComponent();
            BindingContext = ViewModel = homePageViewModel;
        }

        HomePageViewModel ViewModel { get; }

        protected override bool OnBackButtonPressed()
        {
            if (_isBackPressedOnce)
                return base.OnBackButtonPressed();

            _isBackPressedOnce = true;
            ViewModel.ShowExitToastShort("Press back again to exit the app");

            Task.Run(async () =>
            {
                await Task.Delay(2000);
                _isBackPressedOnce = false;
            });

            return true;
        }
    }
}