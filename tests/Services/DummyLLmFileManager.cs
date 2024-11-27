using LMKit.Maestro.Services;
using System.Collections.ObjectModel;

namespace LMKit.Maestro.Tests.Services;

internal class DummyLLmFileManager : ILLMFileManager
{
    public ObservableCollection<ModelInfo> UserModels { get; } = new ObservableCollection<ModelInfo>();

    public ObservableCollection<Uri> UnsortedModels { get; } = new ObservableCollection<Uri>();

    public bool FileCollectingInProgress { get; private set; }
    public string ModelsFolderPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
