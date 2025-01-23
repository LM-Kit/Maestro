using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using System.Numerics;

namespace LMKit.Maestro.UI.Razor.Components;

public partial class NumericSetting<T> : ComponentBase where T : struct, INumber<T>
{
    private string _inputText = "";

    [Parameter] public required string Title { get; set; }
    [Parameter] public required T Value { get; set; }
    [Parameter] public required T MinValue { get; set; }
    [Parameter] public required T MaxValue { get; set; }

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
        //if (int.TryParse(_inputText, out int parsedValue))
        //{
        //    Value = parsedValue;
        //}
        //else
        //{
        //    _inputText = Value.ToString()!;
        //}
    }
}