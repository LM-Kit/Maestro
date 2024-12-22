using LMKit.Model;
using System.Collections.ObjectModel;

namespace LMKit.Maestro.Services;

public interface ILLMFileManager
{
    ObservableCollection<ModelCard> UserModels { get; }
    ObservableCollection<Uri> UnsortedModels { get; }
    bool FileCollectingInProgress { get; }
    string ModelStorageDirectory { get; set; }
    event EventHandler? FileCollectingCompleted;
    void Initialize();
    void DeleteModel(ModelCard modelCard);
    bool IsPredefinedModel(ModelCard modelCard);
}
