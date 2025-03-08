using LMKit.Maestro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maestro.Tests.Services
{
    internal class DummyAppSettingsService : IAppSettingsService
    {
        public Uri? LastLoadedModelUri { get; set; }
        public string ModelStorageDirectory { get; set; } = LMKitDefaultSettings.DefaultModelStorageDirectory;
        public string SystemPrompt { get; set; } = LMKitDefaultSettings.DefaultSystemPrompt;
        public int MaximumCompletionTokens { get; set; }
        public int RequestTimeout { get; set; }
        public int ContextSize { get; set; }
        public SamplingMode SamplingMode { get; set; }
        public RandomSamplingConfig RandomSamplingConfig { get; set; }
        public Mirostat2SamplingConfig Mirostat2SamplingConfig { get; set; }
        public TopNSigmaSamplingConfig TopNSigmaSamplingConfig { get; set; }
        public bool EnableLowPerformanceModels { get; set; }
    }
}
