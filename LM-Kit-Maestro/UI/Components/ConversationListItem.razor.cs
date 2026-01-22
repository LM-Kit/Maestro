using LMKit.Maestro.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.ComponentModel;
using System.Diagnostics;

namespace LMKit.Maestro.UI.Components;

public partial class ConversationListItem : ComponentBase, IDisposable
{
    [Parameter] public EventCallback<ConversationViewModel> OnSelect { get; set; }
    [Parameter] public EventCallback<ConversationViewModel> OnDelete { get; set; }
    [Parameter] public EventCallback<ConversationViewModel> OnToggleStar { get; set; }
    [Parameter] public required ConversationViewModel ViewModel { get; set; }
    [Parameter] public bool IsSelected { get; set; }

    private MudBlazor.MudTextField<string>? ItemTitleRef;
    private ConversationViewModel? _previousViewModel;

    public string Title { get; private set; } = "";

    private string GetContainerClasses()
    {
        var classes = new List<string>();
        if (IsSelected) classes.Add("item-selected");
        if (ViewModel.IsStarred) classes.Add("item-starred");
        return string.Join(" ", classes);
    }

    private string GetStarIndicatorClasses()
    {
        var classes = new List<string> { "star-indicator" };
        if (ViewModel.IsStarred) classes.Add("starred");
        return string.Join(" ", classes);
    }

    private string GetStarIconClass()
    {
        return ViewModel.IsStarred ? "fas fa-star" : "far fa-star";
    }


    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // Handle ViewModel change - unsubscribe from old, subscribe to new
        if (_previousViewModel != ViewModel)
        {
            if (_previousViewModel != null)
            {
                _previousViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }

            if (ViewModel != null)
            {
                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            }

            _previousViewModel = ViewModel;
        }

        if (!ViewModel.IsRenaming)
        {
            Title = ViewModel.Title;
            IsSelected = ViewModel.IsSelected;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ConversationViewModel.Title))
        {
            // Update local title and refresh UI on the UI thread
            InvokeAsync(() =>
            {
                if (!ViewModel.IsRenaming)
                {
                    Title = ViewModel.Title;
                    StateHasChanged();
                }
            });
        }
        else if (e.PropertyName == nameof(ConversationViewModel.IsStarred))
        {
            InvokeAsync(() => StateHasChanged());
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

    private void OnStarClicked()
    {
        ViewModel.IsShowingActionPopup = false;
        OnToggleStar.InvokeAsync(ViewModel);
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

    public void Dispose()
    {
        if (_previousViewModel != null)
        {
            _previousViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }
}
