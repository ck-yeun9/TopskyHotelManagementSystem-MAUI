namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    public interface IAuthService
    {
        bool HasValidToken();
        Task<bool> LoginAsync(string username, string password);
        Task<bool> RegisterAsync(string username, string email, string password);
        void SaveToken(string token, DateTime expiration);
        void ClearToken();
        Task<bool> ValidateTokenAsync();
        void RefreshToken();
        string GetAccessToken();
    }
}
