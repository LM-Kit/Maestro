using LMKit.Maestro.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace LMKit.Maestro.UI.Razor.Components;

public partial class ConversationListItem : ComponentBase
{
    [Parameter] public EventCallback<ConversationViewModel> OnSelect { get; set; }
    [Parameter] public EventCallback<ConversationViewModel> OnShowMore { get; set; }
    [Parameter] public EventCallback<ConversationViewModel> OnDelete { get; set; }
    [Parameter] public required ConversationViewModel ViewModel { get; set; }
    [Parameter] public bool IsSelected { get; set; }

    [Parameter] public MudBlazor.MudTextField<string> ItemTitleRef { get; set; }

    private string? _lastTitle;

    private async void OnRenameClicked()
    {
        ViewModel.IsShowingActionPopup = false;
        ViewModel.IsRenaming = true;
        _lastTitle = ViewModel.Title;
        await Task.Delay(50);
        await ItemTitleRef.FocusAsync();

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

    private void OnKeyPressed(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            if (!string.IsNullOrWhiteSpace(ItemTitleRef.Value))
            {
                ViewModel!.Title = ItemTitleRef.Value.TrimStart().TrimEnd();
            }
            else
            {
                ViewModel.Title = _lastTitle!;
            }

            ViewModel!.IsRenaming = false;
        }
    }
}
