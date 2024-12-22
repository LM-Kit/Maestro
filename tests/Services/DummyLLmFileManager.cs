using LMKit.Maestro.Services;
using LMKit.Model;
using System.Collections.ObjectModel;

namespace LMKit.Maestro.Tests.Services;

internal class DummyLLmFileManager : ILLMFileManager
{
    private ObservableCollection<ModelCard> _sortedModels = new ObservableCollection<ModelCard>();
    private ObservableCollection<ModelCard> _unsortedModels = new ObservableCollection<ModelCard>();

    public ReadOnlyObservableCollection<ModelCard> SortedModels { get; }

    public ReadOnlyObservableCollection<ModelCard> UnsortedModels { get; }


    public DummyLLmFileManager()
    {
        SortedModels = new ReadOnlyObservableCollection<ModelCard>(_sortedModels);
        UnsortedModels = new ReadOnlyObservableCollection<ModelCard>(_unsortedModels);
    }

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
        _sortedModels.Remove(modelInfo);
    }

    public void Initialize()
    {
    }

    public bool IsPredefinedModel(ModelCard modelCard)
    {
        throw new NotImplementedException();
    }
}
