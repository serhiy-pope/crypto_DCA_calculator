using TokeroDCACalculator.Services;
using Application = Microsoft.Maui.Controls.Application;

namespace TokeroDCACalculator
{
    public partial class App : Application
    {
        public App(ICryptoSeederService seederService)
        {
            InitializeComponent();

            MainPage = new AppShell();

            Task.Run(async () => await seederService.SeedAsync());
        }
    }
}
