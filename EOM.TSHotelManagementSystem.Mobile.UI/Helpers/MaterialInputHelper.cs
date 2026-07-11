using Microsoft.Maui.Controls;

namespace EOM.TSHotelManagementSystem.Mobile.UI;

/// <summary>
/// 修复 UraniumUI Material TextField 输入框顶部「一丢丢空白」。
/// 该空白来自控件 ControlTemplate 的硬编码值，且未暴露为可绑定属性，
/// 因此无法用 XAML 样式覆盖（模板里的本地值优先级高于 Style/StyleClass）：
///   - 根 Grid(StyleId=RootGrid) 的 Padding = 0,5,0,0（顶部固定 5px）；
///   - 浮动标签(StyleId=TitleLabel) 的 Margin = 15（四周各 15px）。
/// 这里在页面加载完成后遍历可视化树，把这两处收小。改动仅作用于布局，
/// 不动控件结构，找不到对应元素时安全跳过（不会崩溃）。
/// </summary>
public static class MaterialInputHelper
{
    public static void RemoveInputFieldTopPadding(VisualElement root)
    {
        if (root is null)
            return;

        foreach (var element in root.GetVisualTreeDescendants())
        {
            if (element is Grid grid && grid.StyleId == "RootGrid")
            {
                grid.Padding = new Thickness(0);
            }
            else if (element is Label label && label.StyleId == "TitleLabel")
            {
                // 浮动标签默认 Margin=15（四周）。收小上下间距、保留左侧 15 让文字避开边框圆角/图标。
                label.Margin = new Thickness(15, 4, 15, 4);
            }
        }
    }
}
