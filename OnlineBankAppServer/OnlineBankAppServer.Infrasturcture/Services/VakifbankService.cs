using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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

    // --- TOKEN ALMA ---
    private async Task<string> GetAccessTokenAsync(string rizaNo, CancellationToken cancellationToken)
    {
        var tokenUrl = _configuration["VakifBankB2B:TokenUrl"] ?? throw new ArgumentNullException("TokenUrl eksik!");

        var requestData = new Dictionary<string, string>
        {
            { "client_id", _configuration["VakifBankB2B:ClientId"]! },
            { "client_secret", _configuration["VakifBankB2B:ClientSecret"]! },
            { "grant_type", "b2b_credentials" },
            { "scope", _configuration["VakifBankB2B:Scope"]! },
            { "consentId", rizaNo },
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

    // --- HESAP LİSTESİ ÇEKME ---
    public async Task<VakifbankAccountListResponseDto?> GetAccountsAsync(string rizaNo, CancellationToken cancellationToken = default)
    {
        string accessToken = await GetAccessTokenAsync(rizaNo, cancellationToken);

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

        return await response.Content.ReadFromJsonAsync<VakifbankAccountListResponseDto>(cancellationToken: cancellationToken);
    }

    // --- HESAP HAREKETLERİ ÇEKME ---
    public async Task<VakifbankAccountTransactionsResponseDto?> GetAccountTransactionsAsync(string rizaNo, string accountNumber, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        string accessToken = await GetAccessTokenAsync(rizaNo, cancellationToken);

        var transactionsUrl = _configuration["VakifBankB2B:TransactionsUrl"]
            ?? throw new ArgumentNullException("TransactionsUrl appsettings.json dosyasında eksik!");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, transactionsUrl);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var requestModel = new
        {
            AccountNumber = accountNumber,
            StartDate = startDate.ToString("yyyy-MM-ddTHH:mm:ss+03:00"),
            EndDate = endDate.ToString("yyyy-MM-ddTHH:mm:ss+03:00")
        };

        requestMessage.Content = new StringContent(JsonSerializer.Serialize(requestModel), System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            if (responseContent.Contains("ACBH000202"))
                return new VakifbankAccountTransactionsResponseDto { Data = new VakifbankTransactionsDataDto { AccountTransactions = new List<VakifbankTransactionItemDto>() } };

            throw new Exception($"Vakıfbank Hesap Hareketleri Hatası: {response.StatusCode} - {responseContent}");
        }

        if (responseContent.Contains("\"AccountTransactions\":\"\"") || responseContent.Contains("\"AccountTransactions\": \"\""))
        {
            return new VakifbankAccountTransactionsResponseDto { Data = new VakifbankTransactionsDataDto { AccountTransactions = new List<VakifbankTransactionItemDto>() } };
        }

        try
        {
            return JsonSerializer.Deserialize<VakifbankAccountTransactionsResponseDto>(responseContent);
        }
        catch (System.Text.Json.JsonException)
        {
            return new VakifbankAccountTransactionsResponseDto { Data = new VakifbankTransactionsDataDto { AccountTransactions = new List<VakifbankTransactionItemDto>() } };
        }
    }

    // --- HESAP DETAYLARI ÇEKME ---
    public async Task<VakifbankAccountDetailResponseDto?> GetAccountDetailAsync(string rizaNo, string accountNumber, CancellationToken cancellationToken = default)
    {
        string accessToken = await GetAccessTokenAsync(rizaNo, cancellationToken);

        var detailUrl = _configuration["VakifBankB2B:AccountDetailUrl"]
                        ?? "https://inbound.apigatewaytest.vakifbank.com.tr:8443/accountDetail";

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, detailUrl);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var requestModel = new { AccountNumber = accountNumber };

        requestMessage.Content = new StringContent(JsonSerializer.Serialize(requestModel), System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Vakıfbank Hesap Detay Hatası: {response.StatusCode} - {responseContent}");
        }

        return JsonSerializer.Deserialize<VakifbankAccountDetailResponseDto>(responseContent);
    }

    // --- DEKONT SORGULAMA ---
    public async Task<VakifbankReceiptResponseDto?> GetReceiptAsync(string rizaNo, string accountNumber, string transactionId, string receiptFormat, CancellationToken cancellationToken = default)
    {
        string accessToken = await GetAccessTokenAsync(rizaNo, cancellationToken);

        var receiptUrl = _configuration["VakifBankB2B:ReceiptUrl"]
            ?? "https://inbound.apigatewaytest.vakifbank.com.tr:8443/getReceipt";

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, receiptUrl);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var requestModel = new
        {
            TransactionId = transactionId,
            AccountNumber = accountNumber,
            ReceiptFormat = receiptFormat
        };

        requestMessage.Content = new StringContent(JsonSerializer.Serialize(requestModel), System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Vakıfbank Dekont Hatası: {response.StatusCode} - {responseContent}");
        }

        return JsonSerializer.Deserialize<VakifbankReceiptResponseDto>(responseContent);
    }
}

// --- DTO MODELLERİ ---
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