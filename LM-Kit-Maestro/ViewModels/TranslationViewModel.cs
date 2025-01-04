using CommunityToolkit.Mvvm.ComponentModel;
using LMKit.Maestro.Services;
using LMKit.TextGeneration;

namespace LMKit.Maestro.ViewModels
{
    public partial class TranslationViewModel : AssistantViewModelBase
    {
        [ObservableProperty]
        Language _outputLanguage = Language.English;

        [ObservableProperty]
        Language _inputLanguage = Language.Undefined;

        [ObservableProperty]
        string? _latestResult;

        [ObservableProperty]
        bool? _lastTranslationIsSuccessful;

        [ObservableProperty]
        Language? _detectedLanguage;

        public EventHandler? TranslationCompleted;
        public EventHandler? TranslationFailed;

        public TranslationViewModel(IPopupService popupService, LMKitService lmKitService) : base(popupService, lmKitService)
        {
        }

        public void DetectLanguage()
        {
            var input = InputText;

            Task.Run(async () =>
            {
                try
                {
                    DetectedLanguage = await _lmKitService.Translation.DetectLanguage(input);
                }
                catch
                {
                    DetectedLanguage = null;
                }
            });
        }

        protected override void HandleSubmit()
        {
            var input = InputText;
            var outputLanguage = OutputLanguage;

            Task.Run(async () =>
            {
                try
                {
                    var result = await _lmKitService.Translation.Translate(input, outputLanguage);

                    OnTranslationResult(result);
                }
                catch (Exception ex)
                {
                    OnTranslationResult(null, ex);
                }

                AwaitingResponse = false;
            });
        }

        public async Task<Language> DetectLanguage(string text)
        {
            return await _lmKitService.Translation.DetectLanguage(text);
        }

        private void OnTranslationResult(string? result, Exception? exception = null)
        {
            LatestResult = result;
            LastTranslationIsSuccessful = exception == null && result != null;

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
