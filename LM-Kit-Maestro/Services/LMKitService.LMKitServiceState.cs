using LMKit.Model;
using System.ComponentModel;

namespace LMKit.Maestro.Services;

public partial class LMKitService
{
    public partial class LMKitServiceState : INotifyPropertyChanged
    {
        public LMKitConfig Config { get; } = new LMKitConfig();
        
        public SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1);
        
        public LM? Model { get; set; }
        
        public Uri? ModelUri { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private LMKitModelLoadingState _modelLoadingState;
        public LMKitModelLoadingState ModelLoadingState
        {
            get => _modelLoadingState;
            set
            {
                _modelLoadingState = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ModelLoadingState)));
            }
        }
    }
}