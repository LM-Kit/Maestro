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

    //event EventHandler? ModelDownloadingProgressed;
    //event EventHandler? ModelDownloadingCompleted;
    //void DeleteModel(ModelCard modelCard);
    //void DownloadModel(ModelCard modelCard);
    //void CancelModelDownload(ModelCard modelCard);
    //void PauseModelDownload(ModelCard modelCard);
    //void ResumeModelDownload(ModelCard modelCard);
    //void OnModelDownloaded(ModelCard modelInfo);

    event PropertyChangedEventHandler PropertyChanged;
     event NotifyCollectionChangedEventHandler? SortedModelCollectionChanged;
}
