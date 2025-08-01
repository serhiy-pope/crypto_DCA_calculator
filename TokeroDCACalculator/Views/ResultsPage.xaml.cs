using TokeroDCACalculator.ViewModels;

namespace TokeroDCACalculator.Views;

public partial class ResultsPage : ContentPage
{
	public ResultsPage(ResultsViewModel resultsViewModel)
	{
		InitializeComponent();
        BindingContext = resultsViewModel;
    }

    protected override bool OnBackButtonPressed()
    {
        if (BindingContext is ResultsViewModel resultsViewModel)
        {
            if (resultsViewModel.GoBackCommand.CanExecute(null))
            {
                resultsViewModel.GoBackCommand.Execute(null);
                return true;
            }
        }
        return base.OnBackButtonPressed();
    }

}