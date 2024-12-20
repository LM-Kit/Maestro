using LMKit.Maestro.ViewModels;

namespace LMKit.Maestro.UI;

public partial class AssistantsPage : PageBase
{
    private readonly AssistantsPageViewModel _assistantsPageViewModel;

    public AssistantsPage(AssistantsPageViewModel assistantsPageViewModel)
    {
        BindingContext = assistantsPageViewModel;
        _assistantsPageViewModel = assistantsPageViewModel;
        InitializeComponent();
    }
}