using System.ComponentModel;

namespace LMKit.Maestro.ViewModels;

public interface IViewModelBase : INotifyPropertyChanged
{
    Task OnInitializedAsync();
    Task Loaded();
}