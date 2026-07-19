using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace EOM.TSHotelManagementSystem.Mobile.UI.Controls;

/// <summary>
/// RadioButton 子类，补回 Command / CommandParameter 能力。
/// .NET MAUI 的 RadioButton 未内置 Command（不像旧版 Xamarin.Forms），
/// 此子类在选中（IsChecked 变为 true）时执行 Command(CommandParameter)，
/// 兼容旧 XAML 里 "RadioButton Command=\"{Binding ...}\" 的写法，保持原有功能不变。
/// </summary>
public class CommandRadioButton : RadioButton
{
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(CommandRadioButton),
        default(ICommand));

    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter),
        typeof(object),
        typeof(CommandRadioButton),
        default(object));

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public CommandRadioButton()
    {
        CheckedChanged += OnCheckedChanged;
    }

    private void OnCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        // 仅在“被选中”时触发；取消选中不执行命令（与单选按钮语义一致）。
        if (!e.Value)
        {
            return;
        }

        var cmd = Command;
        if (cmd != null && cmd.CanExecute(CommandParameter))
        {
            cmd.Execute(CommandParameter);
        }
    }
}
