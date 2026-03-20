using EOM.TSHotelManagementSystem.Mobile.Common.Utility;
using EOM.TSHotelManagementSystem.Mobile.Contract;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using RestSharp;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    public class AuthService : IAuthService
    {
        private readonly IHttpService _httpService;

        public AuthService(IHttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<bool> HasValidTokenAsync()
        {
            return !string.IsNullOrEmpty(await GetAccessToken());
        }

        public async Task<bool> IsAuthenticated() => await HasValidTokenAsync();

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var response = await _httpService.RequestAsync(
                    "CustomerAccount/Login",
                    json: JsonSerializer.Serialize(new ReadCustomerAccountInputDto
                    {
                        Account = username,
                        Password = password
                    })
                );

                var sourceResponse = HttpHelper.JsonToModel<SingleOutputDto<ReadCustomerAccountOutputDto>>(response.Message!);

                if (sourceResponse.StatusCode == 200)
                {
                    await SaveAccessTokenAsync(sourceResponse.Source.UserToken, DateTime.Now.AddDays(7));
                    return true;
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
                var response = await _httpService.RequestAsync(
                    "CustomerAccount/Register",
                    json: JsonSerializer.Serialize(new ReadCustomerAccountInputDto
                    {
                        Account = username,
                        Password = password,
                        EmailAddress = email
                    })
                );

                var sourceResponse = HttpHelper.JsonToModel<SingleOutputDto<ReadCustomerAccountOutputDto>>(response.Message!);

                if (sourceResponse.StatusCode == 200)
                {
                    await SaveAccessTokenAsync(sourceResponse.Source.UserToken, DateTime.Now.AddDays(7));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RegisterAsync Exception: {ex.Message}");
            }
            return false;
        }

        public async Task SaveAccessTokenAsync(string token, DateTime expiration)
        {
            try
            {
                await SecureStorage.SetAsync(Constant.AccessTokenKey, token);

                var encryptedExpiration = Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes(expiration.ToString("O")));
                await SecureStorage.SetAsync(Constant.ExpirationKey, encryptedExpiration);
            }
            catch (Exception ex)
            {
                Preferences.Set(Constant.AccessTokenKey, Obfuscate(token));
                Preferences.Set(Constant.ExpirationKey, expiration.ToString("O"));
            }
        }

        public async Task SaveRefreshTokenAsync(string token)
        {
            try
            {
                await SecureStorage.SetAsync(Constant.RefreshTokenKey, token);
            }
            catch (Exception ex)
            {
                Preferences.Set(Constant.AccessTokenKey, Obfuscate(token));
            }
        }

        public async Task ClearTokenAsync()
        {
            try
            {
                SecureStorage.Remove(Constant.AccessTokenKey);
                SecureStorage.Remove(Constant.RefreshTokenKey);
                SecureStorage.Remove(Constant.ExpirationKey);
            }
            finally
            {
                Preferences.Remove(Constant.AccessTokenKey);
                Preferences.Remove(Constant.RefreshTokenKey);
                Preferences.Remove(Constant.ExpirationKey);
            }
        }

        public async Task RefreshTokenAsync()
        {
            try
            {
                var token = await GetAccessToken();
                if (string.IsNullOrEmpty(token)) return;

                var expiration = await GetTokenExpiration();

                if (expiration > DateTime.Now)
                {
                    Debug.WriteLine("令牌尚未过期，无需刷新");
                    return;
                }

                Debug.WriteLine("令牌已过期，启动生物识别验证");
                var authResult = await AuthenticateWithBiometricsAsync();

                if (!authResult)
                {
                    Debug.WriteLine("生物识别验证失败，跳过令牌刷新");
                    return;
                }

                Debug.WriteLine("生物识别验证通过，开始刷新令牌");
                var refreshToken = await GetRefreshToken();
                var response = await _httpService.RequestAsync(
                    "CustomerAccount/RefreshToken",
                    json: refreshToken
                );

                if (response.StatusCode == 200)
                {
                    var newToken = JsonSerializer.Deserialize<string>(response.Message!);
                    if (!string.IsNullOrEmpty(newToken))
                    {
                        await SaveAccessTokenAsync(newToken, DateTime.Now.AddDays(7));
                        Debug.WriteLine("令牌刷新成功");
                    }
                    else
                    {
                        Debug.WriteLine("刷新令牌返回空值");
                    }
                }
                else
                {
                    Debug.WriteLine($"刷新令牌失败，状态码: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RefreshTokenAsync Exception: {ex.Message}");
            }
        }

        public async Task<bool> ValidateAccessTokenAsync()
        {
            var token = await GetAccessToken();
            if (string.IsNullOrEmpty(token)) return false;

            var expiration = await GetTokenExpiration();

            return expiration > DateTime.Now.AddMinutes(10);
        }

        public async Task<string> GetAccessToken()
        {
            try
            {
                var token = await SecureStorage.GetAsync(Constant.AccessTokenKey);
                if (!string.IsNullOrEmpty(token)) return token;

                token = Preferences.Get(Constant.AccessTokenKey, string.Empty);
                return Deobfuscate(token);
            }
            catch { return string.Empty; }
        }

        public async Task<string> GetRefreshToken()
        {
            try
            {
                var token = await SecureStorage.GetAsync(Constant.RefreshTokenKey);

                if (!string.IsNullOrEmpty(token))
                    return Deobfuscate(token);

                token = Preferences.Get(Constant.RefreshTokenKey, string.Empty);
                return Deobfuscate(token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"获取令牌失败: {ex.Message}");
                return string.Empty;
            }
        }

        private async Task<DateTime> GetTokenExpiration()
        {
            try
            {
                var expirationStr = await SecureStorage.GetAsync(Constant.ExpirationKey);

                if (string.IsNullOrEmpty(expirationStr))
                {
                    expirationStr = Preferences.Get(Constant.ExpirationKey, string.Empty);
                }
                else
                {
                    expirationStr = Encoding.UTF8.GetString(
                        Convert.FromBase64String(expirationStr));
                }

                return DateTime.Parse(expirationStr);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        private string Obfuscate(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
        }

        private string Deobfuscate(string input)
        {
            try
            {
                if (string.IsNullOrEmpty(input)) return string.Empty;
                return Encoding.UTF8.GetString(Convert.FromBase64String(input));
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task<bool> AuthenticateWithBiometricsAsync()
        {
            try
            {
                if (!await IsBiometricsSupportedAsync())
                {
                    Debug.WriteLine("设备不支持生物识别");
                    return false;
                }

                var authRequestConfig = new AuthenticationRequestConfiguration("身份验证", "令牌过期，需要验证身份")
                {
                    CancelTitle = "取消",
                    FallbackTitle = "使用密码"
                };

                var authResult = await CrossFingerprint.Current.AuthenticateAsync(authRequestConfig);

                if (authResult.Authenticated)
                {
                    Debug.WriteLine("生物识别验证成功");
                    return true;
                }

                Debug.WriteLine($"生物识别验证失败: {authResult.ErrorMessage}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"生物识别异常: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> IsBiometricsSupportedAsync()
        {
            return await CrossFingerprint.Current.IsAvailableAsync(false);
        }
    }
}
