using CommunityToolkit.Mvvm.ComponentModel;

namespace LMKit.Maestro.Services;

public partial class LLMFileManager : ObservableObject
{
    internal sealed class FileDownloader : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
