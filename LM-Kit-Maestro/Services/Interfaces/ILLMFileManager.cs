using LMKit.Model;
using System.Collections.ObjectModel;

namespace LMKit.Maestro.Services;

public interface ILLMFileManager
{
    ReadOnlyObservableCollection<ModelCard> SortedModels { get; }
    ReadOnlyObservableCollection<ModelCard> UnsortedModels { get; }
    bool FileCollectingInProgress { get; }
    string ModelStorageDirectory { get; set; }
    event EventHandler? FileCollectingCompleted;
    void Initialize();
    void DeleteModel(ModelCard modelCard);
    bool IsPredefinedModel(ModelCard modelCard);
}