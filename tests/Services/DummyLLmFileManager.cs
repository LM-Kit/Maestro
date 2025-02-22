using LMKit.Maestro.Services;
using LMKit.Model;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Maestro.Tests.Services;

internal class DummyLLmFileManager : ILLMFileManager
{
    private ObservableCollection<ModelCard> _models = new ObservableCollection<ModelCard>();
    private ObservableCollection<ModelCard> _unsortedModels = new ObservableCollection<ModelCard>();

    public ReadOnlyObservableCollection<ModelCard> Models { get; }

    public ReadOnlyObservableCollection<ModelCard> UnsortedModels { get; }

    public bool FileCollectingInProgress { get; private set; }

    public string ModelStorageDirectory { get; set; }
    public long TotalModelSize { get; set; }
    public int DownloadedCount { get; set; }

    public DummyLLmFileManager()
    {
        Models = new ReadOnlyObservableCollection<ModelCard>(_models);
        UnsortedModels = new ReadOnlyObservableCollection<ModelCard>(_unsortedModels);
    }

    event PropertyChangedEventHandler ILLMFileManager.PropertyChanged
    {
        add
        {
        }

        remove
        {
        }
    }

    event NotifyCollectionChangedEventHandler? ILLMFileManager.SortedModelCollectionChanged
    {
        add
        {
        }
        remove
        {
        }
    }

    ReadOnlyObservableCollection<ModelCard> ILLMFileManager.Models { get; }

    ReadOnlyObservableCollection<ModelCard> ILLMFileManager.UnsortedModels { get; }

    bool ILLMFileManager.FileCollectingInProgress { get; }

    string ILLMFileManager.ModelStorageDirectory { get; set ; }

    long ILLMFileManager.TotalModelSize { get; }

    int ILLMFileManager.DownloadedCount { get; }


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
