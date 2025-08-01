using System.Windows.Input;
using TokeroDCACalculator.Views;

namespace TokeroDCACalculator.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://tokero.com/en/about-us/"));

            GoBackCommand = new Command(ExecuteGoBackCommand);
        }

        #region - Commands

        public ICommand OpenWebCommand { get; }
        public Command GoBackCommand { get; }

        #endregion

        #region - Private methods

        private async void ExecuteGoBackCommand()
        {
            await Shell.Current.GoToAsync($"//{nameof(HomePage)}", true);
        }

        #endregion

    }
}