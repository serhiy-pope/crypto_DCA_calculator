using TokeroDCACalculator.Views;

namespace TokeroDCACalculator.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        #region - Private fields

        private string userName;
        private string password;

        #endregion

        public LoginViewModel()
        {
            LoginCommand = new Command(OnLoginClicked, ValidateLogin);
            PropertyChanged += (_, __) => LoginCommand.ChangeCanExecute();
        }

        #region - Public Properties

        public string UserName
        {
            get => this.userName;
            set => SetProperty(ref this.userName, value);
        }

        public string Password
        {
            get => this.password;
            set => SetProperty(ref this.password, value);
        }

        #endregion

        #region - Commands

        public Command LoginCommand { get; }

        #endregion

        #region - Private methods

        private async void OnLoginClicked()
        {
            await Shell.Current.GoToAsync($"//{nameof(HomePage)}", true);
        }

        private bool ValidateLogin()
        {
            return !string.IsNullOrWhiteSpace(UserName)
                && !string.IsNullOrWhiteSpace(Password);
        }

        #endregion
    }
}