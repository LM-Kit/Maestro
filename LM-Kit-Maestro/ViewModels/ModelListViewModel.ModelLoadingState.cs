namespace LMKit.Maestro.ViewModels;

public partial class ModelListViewModel : ViewModelBase
{
    public enum ModelLoadingState
    {
        NotLoaded,
        Loading,
        FinishinUp,
        Loaded,
        Downloading
    }
}