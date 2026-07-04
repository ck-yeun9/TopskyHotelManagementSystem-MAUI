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

                if (string.IsNullOrWhiteSpace(response?.Message))
                {
                    return false;
                }

                var sourceResponse = HttpHelper.JsonToModel<SingleOutputDto<ReadCustomerAccountOutputDto>>(response.Message);

                if (sourceResponse?.Success == true && sourceResponse.Data?.UserToken != null)
                {
                    await SaveAccessTokenAsync(sourceResponse.Data.UserToken, DateTime.Now.AddDays(7));

                    if (!string.IsNullOrEmpty(sourceResponse.Data.RefreshToken))
                    {
                        await SaveRefreshTokenAsync(sourceResponse.Data.RefreshToken);
                    }

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

                if (string.IsNullOrWhiteSpace(response?.Message))
                {
                    return false;
                }

                var sourceResponse = HttpHelper.JsonToModel<SingleOutputDto<ReadCustomerAccountOutputDto>>(response.Message);

                if (sourceResponse?.Success == true && sourceResponse.Data?.UserToken != null)
                {
                    await SaveAccessTokenAsync(sourceResponse.Data.UserToken, DateTime.Now.AddDays(7));
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
                var accessToken = await GetAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                {
                    Debug.WriteLine("RefreshTokenAsync: 无AccessToken，跳过刷新");
                    return;
                }

                var expiration = await GetTokenExpiration();
                if (expiration > DateTime.Now)
                {
                    Debug.WriteLine("RefreshTokenAsync: 令牌尚未过期，无需刷新");
                    return;
                }

                Debug.WriteLine("RefreshTokenAsync: 令牌已过期，启动生物识别验证");
                var authResult = await AuthenticateWithBiometricsAsync();

                if (!authResult)
                {
                    Debug.WriteLine("RefreshTokenAsync: 生物识别验证失败");
                    await ClearTokenAsync();
                    return;
                }

                Debug.WriteLine("RefreshTokenAsync: 生物识别验证通过，开始刷新令牌");
                var refreshToken = await GetRefreshToken();

                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    Debug.WriteLine("RefreshTokenAsync: 无RefreshToken，需要重新登录");
                    await ClearTokenAsync();
                    return;
                }

                var json = JsonSerializer.Serialize(new { RefreshToken = refreshToken });
                var response = await _httpService.RequestAsync("Login/RefreshToken", json: json);

                if (string.IsNullOrWhiteSpace(response?.Message))
                {
                    Debug.WriteLine("RefreshTokenAsync: 服务器无响应");
                    await ClearTokenAsync();
                    return;
                }

                var result = HttpHelper.JsonToModel<SingleOutputDto<RefreshTokenResponseDto>>(response.Message);

                if (result?.Success == true && result.Data != null)
                {
                    if (!string.IsNullOrEmpty(result.Data.AccessToken))
                    {
                        await SaveAccessTokenAsync(result.Data.AccessToken, DateTime.Now.AddDays(7));
                        Debug.WriteLine("RefreshTokenAsync: AccessToken刷新成功");
                    }

                    if (!string.IsNullOrEmpty(result.Data.RefreshToken))
                    {
                        await SaveRefreshTokenAsync(result.Data.RefreshToken);
                        Debug.WriteLine("RefreshTokenAsync: RefreshToken更新成功");
                    }
                }
                else
                {
                    Debug.WriteLine($"RefreshTokenAsync: 刷新失败 - {result?.Message}");
                    await ClearTokenAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RefreshTokenAsync Exception: {ex.Message}");
                await ClearTokenAsync();
            }
        }

        public async Task<bool> ValidateAccessTokenAsync()
        {
            var token = await GetAccessToken();
            if (string.IsNullOrEmpty(token)) return false;

            var expiration = await GetTokenExpiration();

            return expiration > DateTime.Now.AddMinutes(10);
        }

        public async Task<string> GetCustomerNumberAsync()
        {
            try
            {
                var token = await GetAccessToken();
                if (string.IsNullOrEmpty(token))
                    return string.Empty;

                var parts = token.Split('.');
                if (parts.Length < 2)
                    return string.Empty;

                var payload = parts[1];
                // 补齐 base64 padding
                payload = payload.Replace('-', '+').Replace('_', '/');
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                var jsonBytes = Convert.FromBase64String(payload);
                var json = Encoding.UTF8.GetString(jsonBytes);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // 后端使用 ClaimTypes.SerialNumber = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/serialnumber"
                var claimKey = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/serialnumber";
                if (root.TryGetProperty(claimKey, out var claimElement))
                    return claimElement.GetString() ?? string.Empty;

                // 备用：小写 key
                if (root.TryGetProperty("serialnumber", out var snElement))
                    return snElement.GetString() ?? string.Empty;

                return string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetCustomerNumberAsync Exception: {ex.Message}");
                return string.Empty;
            }
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

        public async Task<bool> IsBiometricEnabledAsync()
        {
            try
            {
                var enabled = await SecureStorage.GetAsync("BiometricEnabled");
                return enabled == "true";
            }
            catch
            {
                return false;
            }
        }

        public async Task SaveBiometricCredentialsAsync(string username, string password)
        {
            try
            {
                await SecureStorage.SetAsync("BiometricUsername", Obfuscate(username));
                await SecureStorage.SetAsync("BiometricPassword", Obfuscate(password));
                await SecureStorage.SetAsync("BiometricEnabled", "true");
                Debug.WriteLine("生物识别凭据已保存");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"保存生物识别凭据失败: {ex.Message}");
            }
        }

        public async Task<bool> LoginWithBiometricAsync()
        {
            try
            {
                if (!await IsBiometricsSupportedAsync())
                {
                    Debug.WriteLine("设备不支持生物识别");
                    return false;
                }

                var authResult = await CrossFingerprint.Current.AuthenticateAsync(
                    new AuthenticationRequestConfiguration("生物识别登录", "使用指纹或面容登录")
                    {
                        CancelTitle = "取消",
                        FallbackTitle = "使用密码"
                    });

                if (!authResult.Authenticated)
                {
                    Debug.WriteLine("生物识别验证失败");
                    return false;
                }

                var username = Deobfuscate(await SecureStorage.GetAsync("BiometricUsername"));
                var password = Deobfuscate(await SecureStorage.GetAsync("BiometricPassword"));

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    Debug.WriteLine("未找到保存的凭据");
                    return false;
                }

                return await LoginAsync(username, password);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"生物识别登录异常: {ex.Message}");
                return false;
            }
        }

        public async Task ClearBiometricCredentialsAsync()
        {
            try
            {
                SecureStorage.Remove("BiometricUsername");
                SecureStorage.Remove("BiometricPassword");
                SecureStorage.Remove("BiometricEnabled");
                Debug.WriteLine("生物识别凭据已清除");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"清除生物识别凭据失败: {ex.Message}");
            }
        }
    }
}
