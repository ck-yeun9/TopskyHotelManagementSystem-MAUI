using EOM.TSHotelManagementSystem.Mobile.Common.Utility;
using EOM.TSHotelManagementSystem.Mobile.Contract;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using RestSharp;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
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
                    return;
                }

                var expiration = await GetTokenExpiration();
                if (expiration > DateTime.Now)
                {
                    return;
                }

                var authResult = await AuthenticateWithBiometricsAsync();

                if (!authResult)
                {
                    await ClearTokenAsync();
                    return;
                }

                var refreshToken = await GetRefreshToken();

                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    await ClearTokenAsync();
                    return;
                }

                var json = JsonSerializer.Serialize(new { RefreshToken = refreshToken });
                var response = await _httpService.RequestAsync("Login/RefreshToken", json: json);

                if (string.IsNullOrWhiteSpace(response?.Message))
                {
                    await ClearTokenAsync();
                    return;
                }

                var result = HttpHelper.JsonToModel<SingleOutputDto<RefreshTokenResponseDto>>(response.Message);

                if (result?.Success == true && result.Data != null)
                {
                    if (!string.IsNullOrEmpty(result.Data.AccessToken))
                    {
                        await SaveAccessTokenAsync(result.Data.AccessToken, DateTime.Now.AddDays(7));
                    }

                    if (!string.IsNullOrEmpty(result.Data.RefreshToken))
                    {
                        await SaveRefreshTokenAsync(result.Data.RefreshToken);
                    }
                }
                else
                {
                    await ClearTokenAsync();
                }
            }
            catch (Exception ex)
            {
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
            var token = await GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("无法获取客户信息：Token 为空，请重新登录");
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var serialClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid");

                if (serialClaim == null)
                {
                    serialClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                }

                if (serialClaim != null && !string.IsNullOrEmpty(serialClaim.Value))
                {
                    return serialClaim.Value;
                }

                var allClaims = string.Join(", ", jwtToken.Claims.Select(c => $"{c.Type}={c.Value}"));
                throw new InvalidOperationException($"Token 中未找到客户编号(NameIdentifier)。可用 Claims: {allClaims}");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Token 解析失败: {ex.Message}", ex);
            }
        }

        public async Task<string> GetAccessToken()
        {
            try
            {
                var token = await SecureStorage.GetAsync(Constant.AccessTokenKey);
                if (!string.IsNullOrEmpty(token))
                {
                    return token;
                }

                token = Preferences.Get(Constant.AccessTokenKey, string.Empty);
                if (string.IsNullOrEmpty(token))
                {
                    return string.Empty;
                }

                var result = Deobfuscate(token);
                return result;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
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







