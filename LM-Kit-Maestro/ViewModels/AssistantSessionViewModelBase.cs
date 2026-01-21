using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMKit.Maestro.Models;
using LMKit.Maestro.Services;
using MudBlazor;
using System.Collections.ObjectModel;
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

        /// <summary>
        /// Collection of pending attachments to be sent with the next message.
        /// </summary>
        public ObservableCollection<ChatAttachment> PendingAttachments { get; } = new();

        /// <summary>
        /// Whether the current model supports vision (image/PDF attachments).
        /// </summary>
        public bool SupportsVision => LmKitService?.SupportsVision == true;

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

        /// <summary>
        /// Adds an attachment to the pending attachments list.
        /// </summary>
        public void AddAttachment(ChatAttachment attachment)
        {
            PendingAttachments.Add(attachment);
            OnPropertyChanged(nameof(PendingAttachments));
        }

        /// <summary>
        /// Removes an attachment from the pending attachments list.
        /// </summary>
        public void RemoveAttachment(ChatAttachment attachment)
        {
            PendingAttachments.Remove(attachment);
            OnPropertyChanged(nameof(PendingAttachments));
        }

        /// <summary>
        /// Clears all pending attachments.
        /// </summary>
        public void ClearAttachments()
        {
            PendingAttachments.Clear();
            OnPropertyChanged(nameof(PendingAttachments));
        }

        protected abstract void HandleSubmit();

        protected abstract Task HandleCancel(bool shouldAwait);

        protected AssistantViewModelBase(LMKitService lmKitService)
        {
            LmKitService = lmKitService;
            LmKitService.ModelLoaded += OnModelLoadedForVision;
            LmKitService.ModelUnloaded += OnModelUnloadedForVision;
        }

        private void OnModelLoadedForVision(object? sender, LMKitService.NotifyModelStateChangedEventArgs e)
        {
            OnPropertyChanged(nameof(SupportsVision));
        }

        private void OnModelUnloadedForVision(object? sender, LMKitService.NotifyModelStateChangedEventArgs e)
        {
            OnPropertyChanged(nameof(SupportsVision));
            ClearAttachments();
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
