using LMKit.TextGeneration;
using LMKit.TextGeneration.Chat;
using LMKit.Translation;
using System.ComponentModel;

namespace LMKit.Maestro.Services;

public partial class LMKitService : INotifyPropertyChanged
{
    public partial class LMKitTranslation : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;


        private readonly LMKitServiceState _state;


        public LMKitTranslation(LMKitServiceState state)
        {
            _state = state;
        }

        public async Task<string?> Translate(string translation, Language outputLanguage)
        {
            var textTranslation = new TextTranslation(_state.Model!);

           return await textTranslation.TranslateAsync(translation, outputLanguage);
        }

        public async Task<Language>DetectLanguage(string text)
        {
            var textTranslation = new TextTranslation(_state.Model!);

            return await textTranslation.DetectLanguageAsync(text);
        }
    }
}