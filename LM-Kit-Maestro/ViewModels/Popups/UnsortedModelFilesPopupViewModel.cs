using CommunityToolkit.Mvvm.ComponentModel;
using LMKit.Model;
using System.Collections.ObjectModel;

namespace LMKit.Maestro.ViewModels;

public partial class UnsortedModelFilesPopupViewModel : ViewModelBase
{
    [ObservableProperty]
    ReadOnlyObservableCollection<ModelCard> _unsortedModelFiles;

    public void Load(ReadOnlyObservableCollection<ModelCard> unsortedModels)
    {
        UnsortedModelFiles = unsortedModels;
    }
}
