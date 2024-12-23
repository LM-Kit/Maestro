using LMKit.Model;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace LMKit.Maestro.Services;

public interface ILLMFileManager
{
    ReadOnlyObservableCollection<ModelCard> Models { get; }
    ReadOnlyObservableCollection<ModelCard> UnsortedModels { get; }
    bool FileCollectingInProgress { get; }
    string ModelStorageDirectory { get; set; }

    long TotalModelSize { get; }
    int DownloadedCount { get; }

    event EventHandler? FileCollectingCompleted;
    void Initialize();
    void DeleteModel(ModelCard modelCard);
    bool IsPredefinedModel(ModelCard modelCard);
    void OnModelDownloaded(ModelCard modelInfo);

    public event PropertyChangedEventHandler PropertyChanged;
    public event NotifyCollectionChangedEventHandler? SortedModelCollectionChanged;
}