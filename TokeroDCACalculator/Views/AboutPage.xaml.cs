using TokeroDCACalculator.ViewModels;

namespace TokeroDCACalculator.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutPage : ContentPage
    {
        public AboutPage(AboutViewModel aboutViewModel)
        {
            InitializeComponent();
            BindingContext = aboutViewModel;
        }

        protected override bool OnBackButtonPressed()
        {
            if (BindingContext is AboutViewModel aboutViewModel)
            {
                if (aboutViewModel.GoBackCommand.CanExecute(null))
                {
                    aboutViewModel.GoBackCommand.Execute(null);
                    return true;
                }
            }
            return base.OnBackButtonPressed();
        }
    }
}