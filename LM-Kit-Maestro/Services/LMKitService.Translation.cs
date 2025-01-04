using LMKit.TextGeneration;
using LMKit.Translation;

namespace LMKit.Maestro.Services;

public partial class LMKitService
{
    public partial class LMKitTranslation
    {
        private readonly LMKitServiceState _state;

        public LMKitTranslation(LMKitServiceState state)
        {
            _state = state;
        }

        public async Task<string?> Translate(string translation, Language outputLanguage)
        {
            var textTranslation = new TextTranslation(_state.LoadedModel!);

           return await textTranslation.TranslateAsync(translation, outputLanguage);
        }

        public async Task<Language>DetectLanguage(string text)
        {
            var textTranslation = new TextTranslation(_state.LoadedModel!);

            return await textTranslation.DetectLanguageAsync(text);
        }
    }
}