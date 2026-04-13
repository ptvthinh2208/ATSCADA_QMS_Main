using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using Microsoft.VisualBasic;
using ATSCADA_Library.Interfaces.Server;
using ATSCADA_Library.DTOs;

public class TokenApiService
{
    private readonly HttpClient _httpClient;
    private readonly IZnsConfigRepository _znsConfigRepository;

    public TokenApiService(HttpClient httpClient, IZnsConfigRepository znsConfigRepository)
    {
        _httpClient = httpClient;
        _znsConfigRepository = znsConfigRepository;
    }
    
    public async Task<string> RefreshAccessToken()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth.zaloapp.com/v4/oa/access_token");
        var znsInfo = await _znsConfigRepository.GetZnsInfoAsync();
        string appId = znsInfo[0].AppID!;
        string secretKey = znsInfo[0].SecretKey!;
        string refreshToken = znsInfo[0].RefreshToken!;


        // Set the headers
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
        request.Headers.Add("secret_key", secretKey);

        // Set the form content
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("refresh_token", refreshToken!),
            new KeyValuePair<string, string>("app_id", appId!),
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
        });
        request.Content = content;

        // Send the request and get the response
        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent);

            // Save the tokens
            if (responseData.TryGetValue("access_token", out var newAccessToken)! &&
                responseData.TryGetValue("refresh_token", out var newRefreshToken)!)
            {
                var znsInfoDto = new ZnsInfoDto();
                znsInfoDto.AccessToken = newAccessToken;
                znsInfoDto.RefreshToken = newRefreshToken;
                //await _tokenService.UpdateTokensAsync(newAccessToken, newRefreshToken);
                await _znsConfigRepository.UpdateZnsConfigAsync(znsInfoDto);
            }

            return responseContent;
        }
        else
        {
            return null!;
            throw new Exception($"Failed to refresh access token: {response.ReasonPhrase}");
        }
    }
}
