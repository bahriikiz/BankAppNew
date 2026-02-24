using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

namespace OnlineBankAppServer.Application.Integration.Vakifbank;

public sealed class VakifbankService : IVakifbankService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public VakifbankService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;

        string baseUrl = _configuration["VakifBankB2B:BaseUrl"] ?? throw new ArgumentNullException("BaseUrl eksik!");
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var tokenUrl = _configuration["VakifBankB2B:TokenUrl"] ?? throw new ArgumentNullException("TokenUrl eksik!");

        var requestData = new Dictionary<string, string>
        {
            { "client_id", _configuration["VakifBankB2B:ClientId"]! },
            { "client_secret", _configuration["VakifBankB2B:ClientSecret"]! },
            { "grant_type", "b2b_credentials" },
            { "scope", _configuration["VakifBankB2B:Scope"]! },
            { "consentId", _configuration["VakifBankB2B:ConsentId"]! },
            { "resource", _configuration["VakifBankB2B:Resource"]! }
        };

        var content = new FormUrlEncodedContent(requestData);

        var response = await _httpClient.PostAsync(tokenUrl, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Vakıfbank Token alınamadı! Status: {response.StatusCode}, Detay: {error}");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<VakifbankTokenResponseDto>(cancellationToken: cancellationToken);

        return tokenResponse?.AccessToken ?? throw new Exception("Vakıfbank Token boş döndü!");
    }

    public async Task<VakifbankAccountListResponseDto?> GetAccountsAsync(CancellationToken cancellationToken = default)
    {
        string accessToken = await GetAccessTokenAsync(cancellationToken);

        var apiUrl = _configuration["VakifBankB2B:ApiUrl"] ?? throw new ArgumentNullException("ApiUrl eksik!");
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl);

        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        requestMessage.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Vakıfbank Hesap Listesi API Hatası: {response.StatusCode} - Detay: {errorContent}");
        }

        var result = await response.Content.ReadFromJsonAsync<VakifbankAccountListResponseDto>(cancellationToken: cancellationToken);

        return result;
    }
}

public class VakifbankTokenResponseDto
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;
}