using TokeroDCACalculator.ViewModels;

namespace TokeroDCACalculator.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {

        public LoginPage(LoginViewModel loginViewModel)
        {
            InitializeComponent();
            BindingContext = ViewModel = loginViewModel;
        }

        LoginViewModel ViewModel { get; }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ViewModel.UserName = string.Empty;
            ViewModel.Password = string.Empty;
#if DEBUG
            ViewModel.UserName = "user@tokero.com";
            ViewModel.Password = "tokeroTest";           
#endif
        }
    }
}