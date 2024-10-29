using LMKitMaestro.Services;
using System.Collections.ObjectModel;

namespace LMKitMaestroTests.Services;

internal class DummyLLmFileManager : ILLMFileManager
{
    public ObservableCollection<ModelInfo> UserModels { get; } = new ObservableCollection<ModelInfo>();

    public ObservableCollection<Uri> UnsortedModels { get; } = new ObservableCollection<Uri>();

    public bool FileCollectingInProgress { get; private set; }

    public event EventHandler? FileCollectingCompleted;

    public void DeleteModel(ModelInfo modelInfo)
    {
        UserModels.Remove(modelInfo);
    }

    public void Initialize()
    {
    }
}
