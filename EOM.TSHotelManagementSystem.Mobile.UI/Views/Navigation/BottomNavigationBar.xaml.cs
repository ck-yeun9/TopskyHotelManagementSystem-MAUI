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

    public BottomNavigationBar()
    {
        InitializeComponent();
        UpdateActiveTab(ActiveTab);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            SetupGestures();
        });
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

    private void OnTabTapped(string tabName)
    {
        if (BindingContext is MainPageViewModel viewModel)
        {
            viewModel.ActiveTab = tabName;
        }
        else
        {
            ActiveTab = tabName;
        }
    }

    public void UpdateActiveTab(string tabName)
    {
        SetTabActiveState(NewsIcon, NewsLabel, tabName == "news");
        SetTabActiveState(CheckInIcon, CheckInLabel, tabName == "checkin");
        SetTabActiveState(ProfileIcon, ProfileLabel, tabName == "profile");
    }

    private void SetTabActiveState(Image icon, Label label, bool isActive)
    {
        var activeColor = Color.FromArgb("#FF5722");
        var inactiveColor = Colors.Gray;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            label.TextColor = isActive ? activeColor : inactiveColor;
            label.FontAttributes = isActive ? FontAttributes.Bold : FontAttributes.None;

            if (icon != null)
            {
                icon.BackgroundColor = isActive ? activeColor : inactiveColor;
            }
        });
    }
}