using System.Collections.ObjectModel;

namespace LMKit.Maestro.Services;

public interface ILLMFileManager
{
    ObservableCollection<ModelInfo> UserModels { get; }
    ObservableCollection<Uri> UnsortedModels { get; }
    bool FileCollectingInProgress { get; }
    string ModelsFolderPath { get; set; }
    event EventHandler? FileCollectingCompleted;
    void Initialize();
    void DeleteModel(ModelInfo modelInfo);
}
