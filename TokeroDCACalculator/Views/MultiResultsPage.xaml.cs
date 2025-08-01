using TokeroDCACalculator.ViewModels;

namespace TokeroDCACalculator.Views;

public partial class MultiResultsPage : ContentPage
{
	public MultiResultsPage(MultiResultsViewModel multiResultsViewModel)
	{
		InitializeComponent();
        BindingContext = multiResultsViewModel;
    }

    protected override bool OnBackButtonPressed()
    {
        if (BindingContext is MultiResultsViewModel multiResultsViewModel)
        {
            if (multiResultsViewModel.GoBackCommand.CanExecute(null))
            {
                multiResultsViewModel.GoBackCommand.Execute(null);
                return true;
            }
        }
        return base.OnBackButtonPressed();
    }

}