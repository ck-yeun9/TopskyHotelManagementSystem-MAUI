using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI;

public partial class BottomNavigationBar : Grid, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public static readonly BindableProperty ActiveTabProperty =
        BindableProperty.Create(nameof(ActiveTab), typeof(string), typeof(BottomNavigationBar), "checkin",
            propertyChanged: OnActiveTabChanged);

    public string ActiveTab
    {
        get => (string)GetValue(ActiveTabProperty);
        set => SetValue(ActiveTabProperty, value);
    }

    private static void OnActiveTabChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BottomNavigationBar bar && newValue is string tabName)
        {
            bar.UpdateActiveTab(tabName);
        }
    }

    public event EventHandler<string> TabSelected;

    private void OnTabTapped(string tabName)
    {
        TabSelected?.Invoke(this, tabName);
    }

    public BottomNavigationBar()
    {
        InitializeComponent();
        UpdateActiveTab(ActiveTab);

        SetupGestures();
    }

    private void SetupGestures()
    {
        AddTabGesture(NewsLayout, "news");
        AddTabGesture(CheckInLayout, "checkin");
        AddTabGesture(ProfileLayout, "profile");
    }

    private void AddTabGesture(View view, string tabName)
    {
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) => OnTabTapped(tabName);
        view.GestureRecognizers.Add(tapGesture);
    }

    public void UpdateActiveTab(string tabName)
    {
        SetTabActiveState(NewsIcon, NewsLabel, tabName == "news", NewsBorder);
        SetTabActiveState(CheckInIcon, CheckInLabel, tabName == "checkin", CheckInBorder);
        SetTabActiveState(ProfileIcon, ProfileLabel, tabName == "profile", ProfileBorder);
    }

    private void SetTabActiveState(Label icon, Label label, bool isActive, Border border = null)
    {
        var activeColor = Color.FromArgb("#FF5722");
        var inactiveColor = Colors.Gray;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            label.TextColor = isActive ? activeColor : inactiveColor;
            label.FontAttributes = isActive ? FontAttributes.Bold : FontAttributes.None;

            if (icon != null)
            {
                icon.TextColor = isActive ? activeColor : inactiveColor;
                var targetScale = isActive ? 1.2 : 1.0;
                var targetTranslate = isActive ? -2 : 0;
                icon.ScaleTo(targetScale, 200, Easing.CubicOut);
                icon.TranslateTo(0, targetTranslate, 200, Easing.CubicOut);
            }

            if (border != null)
            {
                border.BackgroundColor = isActive
                    ? Color.FromArgb("#FFF3E0")
                    : Colors.Transparent;
            }
        });
    }
}