using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using System.Numerics;

namespace LMKit.Maestro.UI.Razor;

public partial class Setting<T> : ComponentBase where T : struct, INumber<T>
{
    private string _inputText = "";

    [Parameter] public required object Value { get; set; }
    [Parameter] public object? MinValue { get; set; }
    [Parameter] public object? MaxValue { get; set; }

    private void OnInputFocusOut()
    {
        ValidateSettingValue();
    }

    private void OnKeyDown(KeyboardEventArgs keyboardEventArgs)
    {
        if (keyboardEventArgs.Key == "Enter")
        {
            ValidateSettingValue();
        }
    }

    private void ValidateSettingValue()
    {
        if (int.TryParse(_inputText, out int parsedValue))
        {
            Value = parsedValue;
        }
        else
        {
            _inputText = Value.ToString()!;
        }
    }
}