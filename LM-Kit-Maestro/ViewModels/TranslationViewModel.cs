using CommunityToolkit.Mvvm.ComponentModel;
using LMKitMaestro.Services;
using LMKit.TextGeneration;

namespace LMKitMaestro.ViewModels
{
    public partial class TranslationViewModel : AssistantSessionViewModelBase
    {
        [ObservableProperty]
        Language _language;

        public TranslationViewModel(IPopupService popupService, LMKitService lmKitService) : base(popupService, lmKitService)
        {
        }

        protected override void HandleSubmit()
        {
            string prompt = InputText;
            //OnNewlySubmittedPrompt(prompt);

            LMKitService.PromptResult? promptResult = null;

            Task.Run(async () =>
            {
                try
                {
                    _lmKitService.SubmitTranslation(InputText, Language);
                    //OnPromptResult(promptResult);
                }
                catch (Exception ex)
                {
                    //OnPromptResult(null, ex);
                }
            });
        }
    }
}
