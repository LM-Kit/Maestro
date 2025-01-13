using LMKit.Maestro.ViewModels;
using Microsoft.AspNetCore.Components;

namespace LMKit.Maestro.UI.Razor.Components;

public partial class ConversationListItem : ComponentBase
{
    [Parameter] public EventCallback<ConversationViewModel> OnSelect { get; set; }
    [Parameter] public EventCallback<ConversationViewModel> OnShowMore { get; set; }
    [Parameter] public EventCallback<ConversationViewModel> OnDelete { get; set; }
    [Parameter] public required ConversationViewModel ViewModel { get; set; }
    [Parameter] public bool IsSelected { get; set; }

    private void OnRenameClicked()
    {
        ViewModel.IsShowingActionPopup = false;
        ViewModel.IsRenaming = true;
    }

    private void OnSelected()
    {
        ViewModel.IsShowingActionPopup = false;
        OnSelect.InvokeAsync(ViewModel);
    }

    private void OnDeleteClicked()
    {
        ViewModel.IsShowingActionPopup = false;
        OnDelete.InvokeAsync(ViewModel);
    }
}
