using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKit.Maestro.Services;
using MudBlazor;
using System.ComponentModel;

namespace LMKit.Maestro.ViewModels
{
    /// <summary>
    /// Represents a base class for view models that interact with LmKitService.
    /// </summary>
    public abstract partial class AssistantViewModelBase : ViewModelBase
    {
        [ObservableProperty]
        bool _inputTextIsEmpty;

        [ObservableProperty]
        string _inputText = string.Empty;

        [ObservableProperty]
        bool _awaitingResponse;

        public LMKitService LmKitService { get; protected set; }


        [RelayCommand]
        public void Submit()
        {
            AwaitingResponse = true;
            HandleSubmit();
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

        protected AssistantViewModelBase(LMKitService lmKitService)
        {
            LmKitService = lmKitService;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(InputText))
            {
                InputTextIsEmpty = string.IsNullOrWhiteSpace(InputText);
            }
        }
    }
}
