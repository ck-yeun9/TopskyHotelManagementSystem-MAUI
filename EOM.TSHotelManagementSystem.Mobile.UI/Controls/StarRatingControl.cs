using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace EOM.TSHotelManagementSystem.Mobile.UI.Controls
{
    /// <summary>
    /// 五星星级评分控件（点击设置分数，支持只读）
    /// </summary>
    public class StarRatingControl : HorizontalStackLayout
    {
        public static readonly BindableProperty ScoreProperty =
            BindableProperty.Create(
                nameof(Score),
                typeof(int),
                typeof(StarRatingControl),
                0,
                BindingMode.TwoWay,
                propertyChanged: OnScoreChanged);

        private readonly Label[] _stars = new Label[5];

        public StarRatingControl()
        {
            Spacing = 2;
            for (var i = 0; i < 5; i++)
            {
                var star = new Label
                {
                    FontSize = 28,
                    TextColor = Colors.LightGray,
                    VerticalOptions = LayoutOptions.Center
                };

                var index = i + 1;
                var tap = new TapGestureRecognizer();
                tap.Tapped += (_, _) =>
                {
                    if (!IsReadOnly)
                        Score = index;
                };
                star.GestureRecognizers.Add(tap);

                _stars[i] = star;
                Children.Add(star);
            }

            Render();
        }

        /// <summary>
        /// 当前分数（1-5）
        /// </summary>
        public int Score
        {
            get => (int)GetValue(ScoreProperty);
            set => SetValue(ScoreProperty, value);
        }

        /// <summary>
        /// 是否只读（只读时不响应点击）
        /// </summary>
        public bool IsReadOnly { get; set; }

        private static void OnScoreChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((StarRatingControl)bindable).Render();
        }

        private void Render()
        {
            for (var i = 0; i < 5; i++)
            {
                var filled = i < Score;
                _stars[i].Text = filled ? "★" : "☆";
                _stars[i].TextColor = filled ? Colors.Gold : Colors.LightGray;
            }
        }
    }
}
