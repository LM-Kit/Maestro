using LMKit.Maestro.ViewModels;
using Majorsoft.Blazor.Components.Common.JsInterop.ElementInfo;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace LMKit.Maestro.UI.Razor.Components;

public partial class ListItemWrapper<T> : ComponentBase where T : ViewModelBase
{
    private ElementReference _parentDiv;
    private int _popoverX;
    private int _popoverY;

    public bool IsShowingActionSheet { get; private set; }

    [Parameter] public required ListItemViewModel<T> ListItemViewModel { get; set; }

    [Parameter] public required RenderFragment ChildContent { get; set; }

    [Parameter] public required RenderFragment ActionSheet { get; set; }

    [Parameter] public EventCallback<bool> IsShowingActionSheetChanged { get; set; }

    private async Task OnItemClicked(MouseEventArgs e)
    {
    }

    private async Task HandleRightClick(MouseEventArgs e)
    {
        if (e.Button == 2)
        {
            var rect = await _parentDiv.GetClientRectAsync();

            _popoverX = (int)(e.ClientX - rect.Left);
            _popoverY = (int)(e.ClientY - rect.Top);
            IsShowingActionSheet = !IsShowingActionSheet;

            await IsShowingActionSheetChanged.InvokeAsync(IsShowingActionSheet);
            await InvokeAsync(StateHasChanged);
        }
    }
}