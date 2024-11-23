using LMKit.Maestro.ViewModels;
using Mopups.Interfaces;

namespace LMKit.Maestro.UI;

public partial class ModelSelectionPopup : PopupView
{
    private ModelSelectionPopupViewModel _modelSelectionPopupViewModel;

    public ModelSelectionPopup(IPopupNavigation popupNavigation, ModelSelectionPopupViewModel modelSelectionPopupViewModel) : base(popupNavigation)
    {
        InitializeComponent();

        _modelSelectionPopupViewModel = modelSelectionPopupViewModel;
        BindingContext = modelSelectionPopupViewModel;
        modelSelectionPopupViewModel.ModelListViewModel.PropertyChanged += OnModelListSelectedModelPropertyChanged;
    }

    async void OnCloseButtonClicked(object? sender, EventArgs e) => await Dismiss();

    async void OnNavigateToModelPageClicked(object? sender, EventArgs e) => await Dismiss();

    private async void OnModelListSelectedModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_modelSelectionPopupViewModel.ModelListViewModel.SelectedModel))
        {
            if (_modelSelectionPopupViewModel.ModelListViewModel.SelectedModel != null)
            {
                await Dismiss();
            }
        }
    }
}