using Microsoft.Maui.Handlers;
using System.Windows.Input;
using LMKit.Maestro.Controls;

#if ANDROID
using Android.Views;
#endif
#if WINDOWS
using Microsoft.UI.Xaml.Input;
#endif
#if IOS || MACCATALYST
using Foundation;
using UIKit;
#endif

namespace LMKit.Maestro.Handlers;

/// <summary>
/// A handler for <see cref="StatefulContentView"/>.
/// </summary>
public partial class StatefulContentViewHandler : ContentViewHandler
{
    public static IPropertyMapper<StatefulContentView, StatefulContentViewHandler> StatefulContentViewMapper
    => new PropertyMapper<StatefulContentView, StatefulContentViewHandler>(ContentViewHandler.Mapper)
    {
        [nameof(StatefulContentView.IsFocusable)] = MapIsFocusable,
    };

    public StatefulContentViewHandler() : base(StatefulContentViewMapper)
    {
    }

    public StatefulContentView StatefulView => (VirtualView as StatefulContentView)!;

    private void ExecuteCommandIfCan(ICommand command)
    {
        if (command?.CanExecute(StatefulView.CommandParameter) ?? false)
        {
            command.Execute(StatefulView.CommandParameter);
        }
    }
#if (NET8_0 || NET9_0) && !ANDROID && !IOS && !MACCATALYST && !WINDOWS
    public static void MapIsFocusable(StatefulContentViewHandler handler, StatefulContentView view)
    {

    }
#endif
}
