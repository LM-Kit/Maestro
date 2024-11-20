using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKitMaestro.Services;

namespace LMKitMaestro.ViewModels
{
    public abstract partial class AssistantSessionViewModelBase : ViewModelBase
    {
        [ObservableProperty]
        bool _isEmpty = true;

        [ObservableProperty]
        string _inputText = string.Empty;

        [ObservableProperty]
        bool _awaitingResponse;

        protected IPopupService _popupService;

        protected LMKitService _lmKitService;

        [RelayCommand]
        public void Submit()
        {
            if (_lmKitService.ModelLoadingState != LmKitModelLoadingState.Loaded)
            {
                _popupService.DisplayAlert("No model is loaded", "You need to load a model in order to submit a prompt", "OK");
            }
            else
            {
                AwaitingResponse = true;
                HandleSubmit();
                InputText = string.Empty;
            }
        }

        [RelayCommand]
        public async Task Cancel()
        {
            if (AwaitingResponse)
            {
                await HandleCancel(false);
            }
        }

        protected abstract void HandleSubmit();

        protected abstract Task HandleCancel(bool shouldAwait);

        protected AssistantSessionViewModelBase(IPopupService popupService, LMKitService lmKitService)
        {
            _popupService = popupService;
            _lmKitService = lmKitService;
        }
    }
}
