using LMKit.TextGeneration;

namespace LMKit.Maestro.Services;

public partial class LMKitService
{
    private sealed partial class LMKitRequest
    {
        public sealed class TranslationRequestParameters
        {
            public string InputText { get; set; }

            public Language Language { get; set; }

            public TranslationRequestParameters(string inputText, Language language)
            {
                InputText = inputText;
                Language = language;
            }
        }
    }
}
