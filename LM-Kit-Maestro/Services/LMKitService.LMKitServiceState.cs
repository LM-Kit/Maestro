using LMKit.Model;

namespace LMKit.Maestro.Services;

public partial class LMKitService
{
    public partial class LMKitServiceState
    {
        public LMKitConfig Config { get; } = new LMKitConfig();
        
        public SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1);
        
        public LM? LoadedModel { get; set; }
        
        public Uri? LoadedModelUri { get; set; }

        public LMKitModelLoadingState ModelLoadingState { get; set; }
    }
}