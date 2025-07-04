using EOM.TSHotelManagementSystem.Mobile.Contract;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    public class AuthService : IAuthService
    {
        private readonly IHttpService _httpService;

        private const string TokenKey = "AuthToken";
        private const string ExpirationKey = "TokenExpiration";
        
        public AuthService(IHttpService httpService)
        {
            _httpService = httpService;
        }

        public bool HasValidToken()
        {
            var token = Preferences.ContainsKey(TokenKey);
            return token;
        }

        public bool IsAuthenticated => Preferences.ContainsKey(TokenKey);

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var response = await _httpService.CustomerAuthorizationRequestAsync<HttpResponse<object>>(
                    "CustomerAccount/Login",
                    body: JsonSerializer.Serialize(new ReadCustomerAccountInputDto
                    {
                        Account = username,
                        Password = password
                    })
                );

                if (response.StatusCode == 200)
                {

                    if (response?.Source != null)
                    {
                        SaveToken(response.Source.UserToken, DateTime.Now.AddDays(7));
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoginAsync Exception: {ex.Message}");
            }

            return false;
        }

        public async Task<bool> RegisterAsync(string username, string email, string password)
        {
            try
            {
                var registerRequest = new
                {
                    Username = username,
                    Email = email,
                    Password = password
                };

                var response = await _httpService.CustomerAuthorizationRequestAsync<HttpResponse<object>>(
                    "CustomerAccount/Register",
                    body: JsonSerializer.Serialize(new ReadCustomerAccountInputDto
                    {
                        Account = username,
                        Password = password,
                        EmailAddress = email
                    })
                );

                if (response.StatusCode == 200)
                {
                    if (response?.Source != null)
                    {
                        SaveToken(response?.Source?.UserToken, DateTime.Now.AddDays(7));
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void SaveToken(string token, DateTime expiration)
        {
            Preferences.Set(TokenKey, token);
            Preferences.Set(ExpirationKey, expiration.ToString("O"));
        }

        public void ClearToken()
        {
            Preferences.Remove(TokenKey);
            Preferences.Remove(ExpirationKey);
        }

        public async Task<bool> ValidateTokenAsync()
        {
            if (!Preferences.ContainsKey(TokenKey)) return false;

            var expirationStr = Preferences.Get(ExpirationKey, string.Empty);
            if (DateTime.TryParse(expirationStr, out DateTime expiration))
            {
                return expiration > DateTime.Now;
            }

            return false;
        }

        public void RefreshToken()
        {
            var expirationStr = Preferences.Get(ExpirationKey, string.Empty);
            if (DateTime.TryParse(expirationStr, out DateTime expiration))
            {
                if (expiration <= DateTime.Now)
                {
                    ClearToken();
                }
            }
            else
            {
                ClearToken();
            }
        }

        public string GetAccessToken()
        {
            return Preferences.Get(TokenKey, string.Empty);
        }
    }
}