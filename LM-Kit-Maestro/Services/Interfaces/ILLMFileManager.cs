using System.Collections.ObjectModel;

namespace LMKitMaestro.Services;

public interface ILLMFileManager
{
    ObservableCollection<ModelInfo> UserModels { get; }
    ObservableCollection<Uri> UnsortedModels { get; }
    bool FileCollectingInProgress { get; }

    event EventHandler? FileCollectingCompleted;

    void Initialize();
    void DeleteModel(ModelInfo modelInfo);
}
