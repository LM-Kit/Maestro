using System.ComponentModel;
using LMKit.Maestro.ViewModels;
using Microsoft.AspNetCore.Components;

namespace LMKit.Maestro.UI.Razor.Components;

public partial class ModelSelectionButton
{
    [Parameter] required public ModelListViewModel ModelListViewModel { get; set; }

    public string Text { get; private set; } = Locales.SelectModel;

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        if (firstRender)
        {
            Text = GetModelStateText(ModelListViewModel);
            ModelListViewModel.PropertyChanged += OnModelListViewModelPropertyChanged;
            InvokeAsync(() => StateHasChanged());
        }
    }

    private void OnModelListViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ModelListViewModel.LoadingState))
        {
            Text = GetModelStateText(ModelListViewModel);
            InvokeAsync(() => StateHasChanged());
        }
        else if (e.PropertyName == nameof(ModelListViewModel.LoadingProgress))
        {
            InvokeAsync(() => StateHasChanged());
        }
    }

    private async Task OnButtonClicked()
    {
        ModelSelectionPopupViewModel modelSelectionPopupViewModel = new ModelSelectionPopupViewModel(ModelListViewModel);

        var popup = new ModelSelectionPopup(ModelListViewModel.PopupNavigation, modelSelectionPopupViewModel);

        await ModelListViewModel.PopupNavigation.PushAsync(popup, true);
    }

    private void OnEjectButtonClicked()
    {
        ModelListViewModel.EjectModel();
    }

    private static string GetModelStateText(ModelListViewModel modelListViewModel)
    {
        switch (modelListViewModel.LoadingState)
        {
            default:
            case ModelListViewModel.ModelLoadingState.NotLoaded:
                return Locales.SelectModel;

            case ModelListViewModel.ModelLoadingState.Loaded:
                return modelListViewModel.SelectedModel!.Name;

            case ModelListViewModel.ModelLoadingState.Loading:
                return Locales.LoadingModel;

            case ModelListViewModel.ModelLoadingState.FinishinUp:
                return Locales.FinishingUp;

            case ModelListViewModel.ModelLoadingState.Downloading:
                return Locales.DownloadingModel;
        }
    }

    private static string GetButtonContainerClasses(ModelListViewModel modelListViewModel)
    {
        switch (modelListViewModel.LoadingState)
        {
            default:
            case ModelListViewModel.ModelLoadingState.NotLoaded:
                return "button-model-unloaded";

            case ModelListViewModel.ModelLoadingState.Loaded:
                return "button-model-loaded";

            case ModelListViewModel.ModelLoadingState.FinishinUp:
            case ModelListViewModel.ModelLoadingState.Downloading:
            case ModelListViewModel.ModelLoadingState.Loading:
                return "button-model-loading";
        }
    }
}