namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    public interface IAuthService
    {
        Task<bool> HasValidTokenAsync();
        Task<bool> LoginAsync(string username, string password);
        Task<bool> RegisterAsync(string username, string email, string password);
        Task SaveAccessTokenAsync(string token, DateTime expiration);
        Task SaveRefreshTokenAsync(string token);
        Task ClearTokenAsync();
        Task RefreshTokenAsync();
        Task<bool> ValidateAccessTokenAsync();
        Task<string> GetAccessToken();

        Task<bool> IsBiometricEnabledAsync();
        Task SaveBiometricCredentialsAsync(string username, string password);
        Task<bool> LoginWithBiometricAsync();
        Task ClearBiometricCredentialsAsync();
    }
}
