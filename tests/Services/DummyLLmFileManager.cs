using LMKit.Maestro.Services;
using LMKit.Model;
using System.Collections.ObjectModel;

namespace LMKit.Maestro.Tests.Services;

internal class DummyLLmFileManager : ILLMFileManager
{
    public ObservableCollection<ModelCard> UserModels { get; } = new ObservableCollection<ModelCard>();

    public ObservableCollection<Uri> UnsortedModels { get; } = new ObservableCollection<Uri>();

    public bool FileCollectingInProgress { get; private set; }
    public string ModelStorageDirectory
    {
        get
        {
            return "";
        }
        set
        {

        }
    }

#pragma warning disable 67
    public event EventHandler? FileCollectingCompleted;
#pragma warning restore 67

    public void DeleteModel(ModelCard modelInfo)
    {
        UserModels.Remove(modelInfo);
    }

    public void Initialize()
    {
    }
}
