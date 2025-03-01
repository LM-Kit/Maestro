using LMKit.Maestro.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Diagnostics;

namespace LMKit.Maestro.UI.Components;

public partial class ConversationListItem : ComponentBase
{
    [Parameter] public EventCallback<ConversationViewModel> OnSelect { get; set; }
    [Parameter] public EventCallback<ConversationViewModel> OnDelete { get; set; }
    [Parameter] public required ConversationViewModel ViewModel { get; set; }
    [Parameter] public bool IsSelected { get; set; }

    private MudBlazor.MudTextField<string>? ItemTitleRef;

    public string Title { get; private set; } = "";


    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (!ViewModel.IsRenaming)
        {
            Title = ViewModel.Title;
            IsSelected = ViewModel.IsSelected;
        }
    }

    private void OnShowMoreClicked()
    {
        ViewModel.IsShowingActionPopup = true;
    }

    private void OnClickOutsideShowMore()
    {
        ViewModel.IsShowingActionPopup = false;
    }

    private async void OnRenameClicked()
    {
        ViewModel.IsShowingActionPopup = false;
        ViewModel.IsRenaming = true;

        await ItemTitleRef!.FocusAsync();
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

    private void OnTitleFocusOut(Microsoft.AspNetCore.Components.Web.FocusEventArgs e)
    {
        ValidateTitle();
    }

    private void OnKeyPressed(KeyboardEventArgs e)
    {
        Trace.WriteLine(Title);

        if (e.Key == "Enter")
        {
            ValidateTitle();
        }
    }

    private void ValidateTitle()
    {
        if (!string.IsNullOrWhiteSpace(Title))
        {
            ViewModel!.Title = Title.TrimStart().TrimEnd();
        }
        else
        {
            Title = ViewModel.Title;
        }

        ViewModel!.IsRenaming = false;
    }
}
