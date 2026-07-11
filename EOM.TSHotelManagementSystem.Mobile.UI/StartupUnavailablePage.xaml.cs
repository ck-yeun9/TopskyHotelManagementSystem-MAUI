using System;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public partial class StartupUnavailablePage : ContentPage
    {
        private readonly Func<Task> _onRetry;
        private bool _retrying;

        public StartupUnavailablePage(Func<Task> onRetry)
        {
            InitializeComponent();
            _onRetry = onRetry ?? throw new ArgumentNullException(nameof(onRetry));

            // 统一提示：不管本地网络问题还是服务端问题，
            // 用户第一反应都应该是检查网络。
            TitleLabel.Text = "应用服务不可用";
            MessageLabel.Text = "请检查本地网络是否可用，如果本地网络可用但服务端无响应，请稍后重试。";
        }

        private void OnRetryClicked(object sender, EventArgs e)
        {
            if (_retrying) return;
            _retrying = true;

            // 先显示"重试中…"状态，然后 fire-and-forget 启动重试流程。
            // 不 await 回调：重试结果由 RunStartupPipelineAsync 自己负责替换 MainPage。
            // 本页在此之后随时可能被替换掉，故不 reset _retrying 也不在 finally 操作控件。
            RetryingLabel.Opacity = 1;
            RetryButton.IsEnabled = false;
            RetryButton.Text = "重试中…";

            _ = _onRetry();
        }
    }
}
