#if WINDOWS
using System.Runtime.InteropServices;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace LMKit.Maestro.Platforms.Windows;

public static class FolderPickerHelper
{
    public static async Task<string?> PickFolderAsync(string? initialPath = null)
    {
        try
        {
            var picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");
            
            // Get the window handle
            var hwnd = GetActiveWindowHandle();
            InitializeWithWindow.Initialize(picker, hwnd);
            
            // Set suggested start location based on initial path
            if (!string.IsNullOrEmpty(initialPath) && Directory.Exists(initialPath))
            {
                // Try to suggest the location - this is a hint, not guaranteed
                picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
                
                // Alternative: Use IInitializeWithWindow to set the initial folder
                try
                {
                    var folder = await StorageFolder.GetFolderFromPathAsync(initialPath);
                    // Unfortunately, FolderPicker doesn't have a way to set initial folder directly
                    // The best we can do is use SuggestedStartLocation
                }
                catch
                {
                    // Ignore if folder can't be accessed
                }
            }
            else
            {
                picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            }
            
            var result = await picker.PickSingleFolderAsync();
            return result?.Path;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"FolderPickerHelper error: {ex.Message}");
            return null;
        }
    }
    
    private static IntPtr GetActiveWindowHandle()
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window?.Handler?.PlatformView is Microsoft.UI.Xaml.Window winUIWindow)
        {
            return WindowNative.GetWindowHandle(winUIWindow);
        }
        return IntPtr.Zero;
    }
}
#endif
