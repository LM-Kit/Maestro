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
            });
        }

        private void OnTranslationResult(string? result, Exception? exception = null)
        {
            if (exception != null)
            {
                TranslationFailed?.Invoke(this, EventArgs.Empty);  
            }
            else
            {
                TranslationCompleted?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
