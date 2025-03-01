using LMKit.Maestro.ViewModels;
using SimpleToolkit.SimpleShell;

namespace LMKit.Maestro
{
    public partial class AppShell : Shell
    {
        public AppShellViewModel AppShellViewModel { get; }

        public AppShell(AppShellViewModel appShellViewModel)
        {
            InitializeComponent();
            AppShellViewModel = appShellViewModel;
            BindingContext = appShellViewModel;
        }


        //[RelayCommand]
        //private async Task Navigate(ShellSection shellItem)
        //{
        //    var currentSection = CurrentShellSection;

        //    foreach (ShellSection shellSection in ShellSections)
        //    {
        //        if (shellSection == CurrentShellSection)
        //        {

        //        }
        //    }
        //    if (!CurrentState.Location.OriginalString.Contains(shellItem.Route))
        //    {
        //        _appShellViewModel.SelectedTab = GetSelectedTab(shellItem.Route);
        //        await this.GoToAsync($"//{shellItem.Route}", true);
        //    }
        //}

        protected override void OnNavigated(ShellNavigatedEventArgs args)
        {
            base.OnNavigated(args);

        }
    }
}