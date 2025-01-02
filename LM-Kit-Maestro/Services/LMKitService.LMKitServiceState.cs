using LMKit.Model;
using LMKit.TextGeneration;
using LMKit.TextGeneration.Chat;
using LMKit.Translation;
using System.ComponentModel;
using System.Diagnostics;

namespace LMKit.Maestro.Services;

public partial class LMKitService : INotifyPropertyChanged
{
    public partial class LMKitServiceState : INotifyPropertyChanged
    {
        public LMKitConfig Config { get; }

        public LM? Model { get; }

        public Uri? ModelUri { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}