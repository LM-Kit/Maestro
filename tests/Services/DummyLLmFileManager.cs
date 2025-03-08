using LMKit.Maestro.Services;
using LMKit.Model;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

internal class DummyLLmFileManager : ILLMFileManager
{
    private ObservableCollection<ModelCard> _models = new ObservableCollection<ModelCard>();
    private ObservableCollection<ModelCard> _unsortedModels = new ObservableCollection<ModelCard>();
    private LLMFileManager _fileManager;
    public ReadOnlyObservableCollection<ModelCard> Models { get; }

    public ReadOnlyObservableCollection<ModelCard> UnsortedModels { get; }

    public bool FileCollectingInProgress { get; private set; }

    public string ModelStorageDirectory { get; set; }
    public long TotalModelSize { get; set; }
    public int DownloadedCount { get; set; }


    public event EventHandler? FileCollectingCompleted;
    public event EventHandler? ModelDownloadingProgressed;
    public event EventHandler? ModelDownloadingCompleted;
    public event PropertyChangedEventHandler PropertyChanged;
    public event NotifyCollectionChangedEventHandler? SortedModelCollectionChanged;

    public DummyLLmFileManager(IAppSettingsService appSettingsService, HttpClient httpClient)
    {
        _fileManager = new LLMFileManager(appSettingsService, httpClient);
        _fileManager.ModelDownloadingProgressed += _fileManager_ModelDownloadingProgressed;
        Models = new ReadOnlyObservableCollection<ModelCard>(_models);
        UnsortedModels = new ReadOnlyObservableCollection<ModelCard>(_unsortedModels);
    }

    private void _fileManager_ModelDownloadingProgressed(object? sender, EventArgs e)
    {
        this.ModelDownloadingProgressed?.Invoke(this, e);
    }


    ReadOnlyObservableCollection<ModelCard> ILLMFileManager.Models { get; }

    ReadOnlyObservableCollection<ModelCard> ILLMFileManager.UnsortedModels { get; }

    bool ILLMFileManager.FileCollectingInProgress { get; }

    string ILLMFileManager.ModelStorageDirectory { get; set; }

    long ILLMFileManager.TotalModelSize { get; }

    int ILLMFileManager.DownloadedCount { get; }

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

    public void DownloadModel(ModelCard modelCard)
    {
        _fileManager.DownloadModel(modelCard);
    }

    public void CancelModelDownload(ModelCard modelCard)
    {
    }

    public void PauseModelDownload(ModelCard modelCard)
    {
    }

    public void ResumeModelDownload(ModelCard modelCard)
    {
    }
}
