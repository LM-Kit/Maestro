using LMKit.Maestro.ViewModels;

namespace LMKit.Maestro.UI;

public partial class AssistantsPage : ContentPage
{
    private readonly AssistantsPageViewModel _assistantsPageViewModel;

    public AssistantsPage(AssistantsPageViewModel assistantsPageViewModel)
    {
        BindingContext = assistantsPageViewModel;
        _assistantsPageViewModel = assistantsPageViewModel;
        InitializeComponent();
    }
}