using CommunityToolkit.Mvvm.ComponentModel;
using LMKitMaestro.Services;
using LMKit.TextGeneration;

namespace LMKitMaestro.ViewModels
{
    public partial class TranslationViewModel : AssistantSessionViewModelBase
    {
        [ObservableProperty]
        Language _language;

        public EventHandler? TranslationCompleted;
        public EventHandler? TranslationFailed;

        public TranslationViewModel(IPopupService popupService, LMKitService lmKitService) : base(popupService, lmKitService)
        {
        }

        protected override void HandleSubmit()
        {
            string prompt = InputText;
            //OnNewlySubmittedPrompt(prompt);

            Task.Run(async () =>
            {
                try
                {
                    var result = await _lmKitService.SubmitTranslation(InputText, Language);
                    OnTranslationResult(result);
                }
                catch (Exception ex)
                {
                    OnTranslationResult(null, ex);
                }

                AwaitingResponse = false;
            });
        }

        private void OnTranslationResult(string? result, Exception? exception = null)
        {
            if (exception != null)
            {
                TranslationFailed?.Invoke(this, new TranslationCompletedEventArgs(null, exception));
            }
            else
            {
                TranslationCompleted?.Invoke(this, new TranslationCompletedEventArgs(result, null));
            }
        }

        protected override async Task HandleCancel(bool shouldAwait)
        {
            //todo
        }

        public sealed class TranslationCompletedEventArgs : EventArgs
        {
            public Exception? Exception { get; }

            public string? Translation { get; }

            public TranslationCompletedEventArgs(string? translation, Exception? exception = null)
            {
                Translation = translation;
                Exception = exception;
            }
        }
    }
}
