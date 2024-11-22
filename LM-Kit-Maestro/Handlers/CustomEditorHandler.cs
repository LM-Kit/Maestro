using System;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

#if ANDROID
using Android.Graphics.Drawables;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
#endif

#if WINDOWS
using Microsoft.UI.Xaml.Controls;
#endif

namespace LMKitMaestro.Handlers;

public partial class CustomEditorHandler : EditorHandler
{
    public CustomEditorHandler()
    {
    }

    public CustomEditorHandler(IPropertyMapper mapper = null) : base(mapper)
    {
    }
}

#if ANDROID
public partial class CustomEditorHandler : EditorHandler
{
    protected override AppCompatEditText CreatePlatformView()
    {
        var nativeView = base.CreatePlatformView();

        using (var gradientDrawable = new GradientDrawable())
        {
            gradientDrawable.SetColor(global::Android.Graphics.Color.Transparent);
            nativeView.SetBackground(gradientDrawable);
            nativeView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToAndroid());
        }

        return nativeView;
    }
}
#endif

#if IOS || MACCATALYST
public partial class CustomEditorHandler : EditorHandler
{
    /* No any custom implementation required
     * Just keeping this handler to prevent build errors.
     */
}
#endif

#if WINDOWS
public partial class CustomEditorHandler : EditorHandler
{
    protected override TextBox CreatePlatformView()
    {
        var nativeView = base.CreatePlatformView();

        nativeView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
        nativeView.Style = null;
        return nativeView;
    }
}
#endif