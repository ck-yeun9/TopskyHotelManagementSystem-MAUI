using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace EOM.TSHotelManagementSystem.Mobile.UI.Controls;

/// <summary>
/// 可复用顶部标题栏：标题在整条导航栏绝对居中，不受返回按钮挤压。
/// 用法：关闭页面自带 Shell 导航栏（Shell.NavBarIsVisible="False"），在页面顶部放本控件。
/// </summary>
public partial class PageHeader : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(PageHeader), default(string));

    public static readonly BindableProperty ShowBackProperty =
        BindableProperty.Create(nameof(ShowBack), typeof(bool), typeof(PageHeader), true);

    public static readonly BindableProperty RightContentProperty =
        BindableProperty.Create(nameof(RightContent), typeof(View), typeof(PageHeader), default(View));

    public static readonly BindableProperty TransparentBackgroundProperty =
        BindableProperty.Create(nameof(TransparentBackground), typeof(bool), typeof(PageHeader), false);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool ShowBack
    {
        get => (bool)GetValue(ShowBackProperty);
        set => SetValue(ShowBackProperty, value);
    }

    public View RightContent
    {
        get => (View)GetValue(RightContentProperty);
        set => SetValue(RightContentProperty, value);
    }

    /// <summary>
    /// 为 true 时顶栏背景透明（如登录页整页渐变需透出背景时使用）；默认 false 走主题实色栏。
    /// </summary>
    public bool TransparentBackground
    {
        get => (bool)GetValue(TransparentBackgroundProperty);
        set => SetValue(TransparentBackgroundProperty, value);
    }

    // 必须在 InitializeComponent 之前完成初始化，否则 XAML 中的
    // {Binding BackCommand} 在 InitializeComponent 阶段求值时拿到 null，
    // 而普通 CLR 属性不会发 PropertyChanged，绑定永远不会更新 → 点击无效。
    public ICommand BackCommand { get; } = new Command(GoBack);

    // 统一返回逻辑：优先 Shell 路由返回 GoToAsync("..")，它对
    // GoToAsync(route) 路由导航和 Navigation.PushAsync 推入的页面都有效；
    // 若不在 Shell 环境（理论极少），回退到当前导航栈的 PopAsync。
    private static async void GoBack()
    {
        try
        {
            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
            var nav = mainPage?.Navigation;
            if (nav is not null && nav.NavigationStack.Count > 1)
                await nav.PopAsync();
        }
        catch
        {
            // 返回失败（如已在根页）静默忽略，避免崩溃。
        }
    }

    public PageHeader()
    {
        InitializeComponent();
    }
}
