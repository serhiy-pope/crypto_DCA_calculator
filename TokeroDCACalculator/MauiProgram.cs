using CommunityToolkit.Maui;
using DevExpress.Maui;
using DevExpress.Maui.Core;
using TokeroDCACalculator.Services;
using TokeroDCACalculator.ViewModels;
using TokeroDCACalculator.Views;

namespace TokeroDCACalculator
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            ThemeManager.ApplyThemeToSystemBars = true;
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseDevExpress(useLocalization: true)
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("roboto-regular.ttf", "Roboto");
                    fonts.AddFont("roboto-medium.ttf", "Roboto-Medium");
                    fonts.AddFont("roboto-bold.ttf", "Roboto-Bold");
                    fonts.AddFont("univia-pro-regular.ttf", "Univia-Pro");
                    fonts.AddFont("univia-pro-medium.ttf", "Univia-Pro Medium");
                    fonts.AddFont("CustomFont.ttf", "AC");
                    fonts.AddFont("Font Awesome 6 Free-Regular-400.otf", "FAR");
                    fonts.AddFont("Font Awesome 6 Free-Solid-900.otf", "FAS");
                    fonts.AddFont("Font Awesome 6 Brands-Regular-400.otf", "FAB");
                });

            DevExpress.Maui.Charts.Initializer.Init();
            DevExpress.Maui.CollectionView.Initializer.Init();
            DevExpress.Maui.Controls.Initializer.Init();
            DevExpress.Maui.Editors.Initializer.Init();
            DevExpress.Maui.DataGrid.Initializer.Init();
            DevExpress.Maui.Scheduler.Initializer.Init();

            #region - Singletons

            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<LoginViewModel>();

            builder.Services.AddSingleton<HomePage>();
            builder.Services.AddSingleton<HomePageViewModel>();

            builder.Services.AddSingleton<ChartsPage>();
            builder.Services.AddSingleton<ChartsViewModel>();

            builder.Services.AddSingleton<AboutPage>();
            builder.Services.AddSingleton<AboutViewModel>();

            #endregion

            #region - Transients

            builder.Services.AddTransient<ResultsPage>();
            builder.Services.AddTransient<ResultsViewModel>(); 
            builder.Services.AddTransient<MultiResultsPage>();
            builder.Services.AddTransient<MultiResultsViewModel>();

            #endregion

            #region - Services

            builder.Services.AddSingleton<ICryptoPriceService, CryptoPriceService>();
            builder.Services.AddSingleton<ICryptoPriceRepository, CryptoPriceRepository>();
            builder.Services.AddSingleton<ICryptoSeederService, CryptoSeederService>();

            #endregion

            return builder.Build();
        }
    }
}
