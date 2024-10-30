using LMKitMaestro.ViewModels;
using Mopups.Interfaces;

namespace LMKitMaestro.UI;

public partial class ModelSelectionPopup : PopupView
{
    private ModelSelectionPopupViewModel _modelSelectionPopupViewModel;

    public ModelSelectionPopup(IPopupNavigation popupNavigation, ModelSelectionPopupViewModel modelSelectionPopupViewModel) : base(popupNavigation)
    {
        InitializeComponent();

        _modelSelectionPopupViewModel = modelSelectionPopupViewModel;
        BindingContext = modelSelectionPopupViewModel;
        modelSelectionPopupViewModel.ChatPageViewModel.PropertyChanged += OnChatPageViewModelPropertyChanged;
    }

    async void OnCloseButtonClicked(object? sender, EventArgs e) => await Dismiss();

    async void OnNavigateToModelPageClicked(object? sender, EventArgs e) => await Dismiss();

    private async void OnChatPageViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ChatPageViewModel.SelectedModel))
        {
            if (_modelSelectionPopupViewModel.ChatPageViewModel.SelectedModel != null)
            {
                await Dismiss();
            }
        }
    }
}