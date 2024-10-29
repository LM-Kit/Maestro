using LMKitMaestro.Services;
using System.Collections.ObjectModel;

namespace LMKitMaestroTests.Services;

internal class DummyLLmFileManager : ILLMFileManager
{
    public ObservableCollection<ModelInfo> UserModels { get; } = new ObservableCollection<ModelInfo>();

    public ObservableCollection<Uri> UnsortedModels { get; } = new ObservableCollection<Uri>();

    public bool FileCollectingInProgress { get; private set; }

#pragma warning disable 67
    public event EventHandler? FileCollectingCompleted;
#pragma warning restore 67

    public void DeleteModel(ModelInfo modelInfo)
    {
        UserModels.Remove(modelInfo);
    }

    public void Initialize()
    {
    }
}
