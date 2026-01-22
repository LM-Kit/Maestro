#if WINDOWS
using System.Runtime.InteropServices;

namespace LMKit.Maestro.Services;

public class WindowsFolderPickerService : IFolderPickerService
{
    private const int BIF_RETURNONLYFSDIRS = 0x0001;
    private const int BIF_NEWDIALOGSTYLE = 0x0040;
    private const int BFFM_INITIALIZED = 1;
    private const int BFFM_SETSELECTION = 0x0467;

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpbi);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern bool SHGetPathFromIDList(IntPtr pidl, IntPtr pszPath);

    [DllImport("ole32.dll")]
    private static extern void CoTaskMemFree(IntPtr ptr);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct BROWSEINFO
    {
        public IntPtr hwndOwner;
        public IntPtr pidlRoot;
        public IntPtr pszDisplayName;
        public string lpszTitle;
        public int ulFlags;
        public BrowseCallbackProc lpfn;
        public IntPtr lParam;
        public int iImage;
    }

    private delegate int BrowseCallbackProc(IntPtr hwnd, int uMsg, IntPtr lParam, IntPtr lpData);

    [ThreadStatic]
    private static string? _initialPath;

    public Task<string?> PickFolderAsync(string? initialPath = null, string? title = null)
    {
        var tcs = new TaskCompletionSource<string?>();
        
        // Must run on UI thread for shell dialogs
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null)
        {
            tcs.SetResult(null);
            return tcs.Task;
        }

        dispatcher.Dispatch(() =>
        {
            try
            {
                var result = ShowDialog(initialPath, title);
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Folder picker error: {ex.Message}");
                tcs.SetResult(null);
            }
        });

        return tcs.Task;
    }

    private static string? ShowDialog(string? initialPath, string? title)
    {
        _initialPath = initialPath;

        var bi = new BROWSEINFO
        {
            hwndOwner = GetActiveWindowHandle(),
            pidlRoot = IntPtr.Zero,
            pszDisplayName = Marshal.AllocHGlobal(260 * 2),
            lpszTitle = title ?? "Select Folder",
            ulFlags = BIF_RETURNONLYFSDIRS | BIF_NEWDIALOGSTYLE,
            lpfn = BrowseCallback,
            lParam = IntPtr.Zero,
            iImage = 0
        };

        try
        {
            IntPtr pidl = SHBrowseForFolder(ref bi);
            if (pidl != IntPtr.Zero)
            {
                try
                {
                    IntPtr pathPtr = Marshal.AllocHGlobal(260 * 2);
                    try
                    {
                        if (SHGetPathFromIDList(pidl, pathPtr))
                        {
                            return Marshal.PtrToStringUni(pathPtr);
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(pathPtr);
                    }
                }
                finally
                {
                    CoTaskMemFree(pidl);
                }
            }
        }
        finally
        {
            Marshal.FreeHGlobal(bi.pszDisplayName);
            _initialPath = null;
        }

        return null;
    }

    private static int BrowseCallback(IntPtr hwnd, int uMsg, IntPtr lParam, IntPtr lpData)
    {
        if (uMsg == BFFM_INITIALIZED && !string.IsNullOrEmpty(_initialPath))
        {
            IntPtr pathPtr = Marshal.StringToHGlobalUni(_initialPath);
            try
            {
                SendMessage(hwnd, BFFM_SETSELECTION, (IntPtr)1, pathPtr);
            }
            finally
            {
                Marshal.FreeHGlobal(pathPtr);
            }
        }
        return 0;
    }

    private static IntPtr GetActiveWindowHandle()
    {
        try
        {
            var window = Application.Current?.Windows.FirstOrDefault();
            if (window?.Handler?.PlatformView is Microsoft.UI.Xaml.Window winUIWindow)
            {
                return WinRT.Interop.WindowNative.GetWindowHandle(winUIWindow);
            }
        }
        catch { }
        return IntPtr.Zero;
    }
}
#endif
