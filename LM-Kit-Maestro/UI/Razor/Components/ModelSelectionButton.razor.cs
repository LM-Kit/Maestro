using System.ComponentModel;
using LMKit.Maestro.ViewModels;
using Microsoft.AspNetCore.Components;

namespace LMKit.Maestro.UI.Razor.Components;

public partial class ModelSelectionButton
{
     [Parameter] public ModelListViewModel ModelListViewModel { get; set; }


    private string _text = Locales.SelectModel;


    private string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                InvokeAsync(() => StateHasChanged());
            }
        }
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        Text = GetModelStateText(ModelListViewModel);
        ModelListViewModel.PropertyChanged += OnModelListViewModelPropertyChanged;
    }

    private void OnModelListViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ModelListViewModel.LoadingState))
        {
            Text = GetModelStateText(ModelListViewModel);
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
}