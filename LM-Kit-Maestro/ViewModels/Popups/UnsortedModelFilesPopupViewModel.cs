using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace LMKit.Maestro.ViewModels;

public partial class UnsortedModelFilesPopupViewModel : ViewModelBase
{
    [ObservableProperty]
    ObservableCollection<string> _unsortedModelFiles = new ObservableCollection<string>();

    public void Load(ICollection<string> unsortedModelFiles)
    {
        UnsortedModelFiles.Clear();

        foreach (var unsortedModelFile in unsortedModelFiles)
        {
            UnsortedModelFiles.Add(unsortedModelFile);
        }
    }
}
