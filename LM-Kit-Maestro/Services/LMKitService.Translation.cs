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

        private readonly LMKitConfig _config;

        public LMKitTranslation(LMKitConfig config)
        {
            _config = config;
        }

        public async Task<string?> Translate(string translation, Language outputLanguage)
        {
            return null;
        }
    }
}