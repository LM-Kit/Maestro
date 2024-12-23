using LMKit.Maestro.Services;
using LMKit.Model;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace LMKit.Maestro.Tests.Services;

internal class DummyLLmFileManager : ILLMFileManager
{
    private ObservableCollection<ModelCard> _models = new ObservableCollection<ModelCard>();
    private ObservableCollection<ModelCard> _unsortedModels = new ObservableCollection<ModelCard>();

    public ReadOnlyObservableCollection<ModelCard> Models { get; }

    public ReadOnlyObservableCollection<ModelCard> UnsortedModels { get; }


    public DummyLLmFileManager()
    {
        Models = new ReadOnlyObservableCollection<ModelCard>(_models);
        UnsortedModels = new ReadOnlyObservableCollection<ModelCard>(_unsortedModels);
    }

    public bool FileCollectingInProgress { get; private set; }
    
    public string ModelStorageDirectory { get; set; }
    public long TotalModelSize { get; set; }
    public int DownloadedCount { get; set; }


#pragma warning disable 67
    public event EventHandler? FileCollectingCompleted;
    public event PropertyChangedEventHandler PropertyChanged;
    public event NotifyCollectionChangedEventHandler? SortedModelCollectionChanged;
#pragma warning restore 67

    public void DeleteModel(ModelCard modelInfo)
    {
        _models.Remove(modelInfo);
    }

    public void Initialize()
    {
    }

    public bool IsPredefinedModel(ModelCard modelCard)
    {
        return false;
    }

    public void OnModelDownloaded(ModelCard modelInfo)
    {
    }
}
