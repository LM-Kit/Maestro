namespace LMKit.Maestro.Services;

internal class NavigationService : INavigationService
{
    public Task NavigateToAsync(string route, IDictionary<string, object>? routeParameters = null)
    {
        return routeParameters != null ? 
            Shell.Current.GoToAsync(route, routeParameters) : 
            Shell.Current.GoToAsync(route);
    }
}
