using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using System.Numerics;
using System.Globalization;

namespace LMKit.Maestro.UI.Razor.Components;

public partial class NumericSetting<T> : ComponentBase where T : struct, INumber<T>
{
    private string _inputText = "";
    [Parameter] public required string Title { get; set; }
    [Parameter] public EventCallback<T> ValueChanged { get; set; }

    [Parameter] public required T MinValue { get; set; }
    [Parameter] public required T MaxValue { get; set; }
    [Parameter] public NumericSettingVariant Variant { get; set; }

    private T _value;

    [Parameter]
    public T Value
    {
        get => _value;
        set
        {
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                _value = value;
                ValueChanged.InvokeAsync(value);
                _inputText = value.ToString()!;
            }
        }
    }

    private void OnInputFocusOut()
    {
        ValidateSettingValue();
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        _inputText = Value.ToString()!;
        InvokeAsync(() => StateHasChanged());
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
        if (T.TryParse(_inputText, new CultureInfo("en-US"), out T parsedValue))
        {
            Value = parsedValue;
        }
        else
        {
            _inputText = Value.ToString()!;
            InvokeAsync(() => StateHasChanged());
        }
    }
}


public enum NumericSettingVariant
{
    Default,
    Optional
}