using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using OnlineBankAppServer.Application.Abstractions;
using OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

namespace OnlineBankAppServer.Infrasturcture.Services;

public sealed class VakifbankService : IVakifbankService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private const string AuthScheme = "Bearer";
    private const string EmptyJsonBody = "{}";
    private const string JsonMediaType = "application/json";

    public VakifbankService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        string baseUrl = _configuration["VakifBankB2B:BaseUrl"] ?? throw new InvalidOperationException("BaseUrl eksik!");
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    private async Task<string> GetAccessTokenAsync(string rizaNo, CancellationToken cancellationToken)
    {
        var tokenUrl = _configuration["VakifBankB2B:TokenUrl"] ?? throw new InvalidOperationException("TokenUrl eksik!");

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
            throw new HttpRequestException($"Vakıfbank Token alınamadı! Status: {response.StatusCode}, Detay: {error}");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<VakifbankTokenResponseDto>(cancellationToken: cancellationToken);
        return tokenResponse?.AccessToken ?? throw new InvalidOperationException("Vakıfbank Token boş döndü!");
    }

    private async Task<string> GetPublicAccessTokenAsync(CancellationToken cancellationToken)
    {
        string tokenUrl = _configuration["VakifBankApi:TokenUrl"]
                       ?? throw new InvalidOperationException("VakifBankApi:TokenUrl ayarı eksik!");

        var requestBody = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", _configuration["VakifBankApi:ClientId"]! },
            { "client_secret", _configuration["VakifBankApi:ClientSecret"]! },
            { "scope", _configuration["VakifBankApi:Scope"]! }
        };

        var content = new FormUrlEncodedContent(requestBody);
        var response = await _httpClient.PostAsync(tokenUrl, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Vakıfbank Public Token alınamadı! Status: {response.StatusCode}, Detay: {error}");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<VakifbankTokenResponseDto>(cancellationToken: cancellationToken);
        return tokenResponse?.AccessToken ?? throw new InvalidOperationException("Vakıfbank Public Token boş döndü!");
    }

    public async Task<VakifbankAccountListResponseDto?> GetAccountsAsync(string rizaNo, CancellationToken cancellationToken = default)
    {
        string accessToken = await GetAccessTokenAsync(rizaNo, cancellationToken);
        var apiUrl = _configuration["VakifBankB2B:ApiUrl"] ?? throw new InvalidOperationException("ApiUrl eksik!");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue(AuthScheme, accessToken);
        requestMessage.Content = new StringContent(EmptyJsonBody, System.Text.Encoding.UTF8, JsonMediaType);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Vakıfbank Hesap Listesi API Hatası: {response.StatusCode} - Detay: {errorContent}");
        }

        return await response.Content.ReadFromJsonAsync<VakifbankAccountListResponseDto>(cancellationToken: cancellationToken);
    }

    public async Task<VakifbankAccountTransactionsResponseDto?> GetAccountTransactionsAsync(string rizaNo, string accountNumber, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        string accessToken = await GetAccessTokenAsync(rizaNo, cancellationToken);
        var transactionsUrl = _configuration["VakifBankB2B:TransactionsUrl"]
            ?? throw new InvalidOperationException("TransactionsUrl appsettings.json dosyasında eksik!");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, transactionsUrl);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue(AuthScheme, accessToken);

        var requestModel = new
        {
            AccountNumber = accountNumber,
            StartDate = startDate.ToString("yyyy-MM-ddTHH:mm:ss+03:00"),
            EndDate = endDate.ToString("yyyy-MM-ddTHH:mm:ss+03:00")
        };

        requestMessage.Content = new StringContent(JsonSerializer.Serialize(requestModel), System.Text.Encoding.UTF8, JsonMediaType);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Vakıfbank Hesap Hareketleri Hatası: {response.StatusCode} - {responseContent}");

        return JsonSerializer.Deserialize<VakifbankAccountTransactionsResponseDto>(responseContent);
    }

    public async Task<VakifbankAccountDetailResponseDto?> GetAccountDetailAsync(string rizaNo, string accountNumber, CancellationToken cancellationToken = default)
    {
        string accessToken = await GetAccessTokenAsync(rizaNo, cancellationToken);
        var detailUrl = _configuration["VakifBankB2B:AccountDetailUrl"]
                         ?? "https://inbound.apigatewaytest.vakifbank.com.tr:8443/accountDetail";

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, detailUrl);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue(AuthScheme, accessToken);

        var requestModel = new { AccountNumber = accountNumber };
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(requestModel), System.Text.Encoding.UTF8, JsonMediaType);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Vakıfbank Hesap Detayı Hatası: {response.StatusCode} - {responseContent}");

        return JsonSerializer.Deserialize<VakifbankAccountDetailResponseDto>(responseContent);
    }

    public async Task<VakifbankReceiptResponseDto?> GetTransactionReceiptAsync(string rizaNo, string transactionId, string accountNumber, string receiptFormat = "pdf", CancellationToken cancellationToken = default)
    {
        string accessToken = await GetAccessTokenAsync(rizaNo, cancellationToken);
        var receiptUrl = _configuration["VakifBankB2B:ReceiptUrl"]
            ?? "https://inbound.apigatewaytest.vakifbank.com.tr:8443/getReceipt";

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, receiptUrl);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue(AuthScheme, accessToken);

        var requestModel = new
        {
            TransactionId = transactionId,
            AccountNumber = accountNumber,
            ReceiptFormat = receiptFormat
        };

        requestMessage.Content = new StringContent(JsonSerializer.Serialize(requestModel), System.Text.Encoding.UTF8, JsonMediaType);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Vakıfbank Dekont Hatası: {response.StatusCode} - {responseContent}");

        return JsonSerializer.Deserialize<VakifbankReceiptResponseDto>(responseContent);
    }

    public async Task<VakifbankCityResponseDto?> GetCitiesAsync(CancellationToken cancellationToken = default)
    {
        string accessToken = await GetPublicAccessTokenAsync(cancellationToken);
        var apiUrl = _configuration["VakifBankApi:CitiesUrl"]
                     ?? throw new InvalidOperationException("VakifBankApi:CitiesUrl ayarı eksik!");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue(AuthScheme, accessToken);
        requestMessage.Content = new StringContent(EmptyJsonBody, System.Text.Encoding.UTF8, JsonMediaType);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Vakıfbank Şehir Listesi Hatası: {response.StatusCode} - {responseContent}");

        return JsonSerializer.Deserialize<VakifbankCityResponseDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<VakifbankDistrictResponseDto?> GetDistrictsAsync(string cityCode, CancellationToken cancellationToken = default)
    {
        string accessToken = await GetPublicAccessTokenAsync(cancellationToken);
        var apiUrl = _configuration["VakifBankApi:DistrictsUrl"]
                     ?? throw new InvalidOperationException("VakifBankApi:DistrictsUrl ayarı eksik!");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue(AuthScheme, accessToken);

        var requestModel = new { CityCode = cityCode };
        var json = JsonSerializer.Serialize(requestModel, new JsonSerializerOptions { PropertyNamingPolicy = null });
        requestMessage.Content = new StringContent(json, System.Text.Encoding.UTF8, JsonMediaType);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Vakıfbank İlçe Listesi Hatası: {response.StatusCode} - {responseContent}");

        return JsonSerializer.Deserialize<VakifbankDistrictResponseDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<VakifbankNeighborhoodResponseDto?> GetNeighborhoodsAsync(string districtCode, CancellationToken cancellationToken = default)
    {
        string accessToken = await GetPublicAccessTokenAsync(cancellationToken);
        var apiUrl = _configuration["VakifBankApi:NeighborhoodsUrl"]!;

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue(AuthScheme, accessToken);

        var requestModel = new { DistrictCode = districtCode?.Trim() };
        string jsonBody = JsonSerializer.Serialize(requestModel, new JsonSerializerOptions { PropertyNamingPolicy = null });

        var content = new StringContent(jsonBody, System.Text.Encoding.UTF8, JsonMediaType);
        content.Headers.ContentType!.CharSet = "";
        requestMessage.Content = content;

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Vakıfbank Mahalle Listesi Hatası: {response.StatusCode} - {responseContent}");

        return JsonSerializer.Deserialize<VakifbankNeighborhoodResponseDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<VakifbankBranchResponseDto?> GetBranchesAsync(string cityCode, string bankDistrictCode, CancellationToken cancellationToken = default)
    {
        string accessToken = await GetPublicAccessTokenAsync(cancellationToken);
        var apiUrl = _configuration["VakifBankApi:BranchesUrl"]
                     ?? throw new InvalidOperationException("VakifBankApi:BranchesUrl ayarı eksik!");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue(AuthScheme, accessToken);
        requestMessage.Content = new StringContent(EmptyJsonBody, System.Text.Encoding.UTF8, JsonMediaType);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Vakıfbank Şube Listesi Hatası: {response.StatusCode} - {responseContent}");

        var result = JsonSerializer.Deserialize<VakifbankBranchResponseDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (result?.Data?.Branch == null)
            throw new InvalidOperationException("Vakıfbank API'si şu anda şube verisi döndürmüyor.");

        var filteredBranches = result.Data.Branch.AsEnumerable();

        if (!string.IsNullOrEmpty(cityCode))
            filteredBranches = filteredBranches.Where(x => x.CityCode?.Trim() == cityCode.Trim());

        if (!string.IsNullOrEmpty(bankDistrictCode))
            filteredBranches = filteredBranches.Where(x => x.DistrictCode?.Trim() == bankDistrictCode.Trim());

        var finalBranchList = filteredBranches.ToList();

        if (finalBranchList.Count == 0)
            throw new KeyNotFoundException("Belirtilen kriterlere uygun herhangi bir şube bulunamadı.");

        return new VakifbankBranchResponseDto(result.Header, new VakifbankBranchDataDto([.. finalBranchList]));
    }

    public async Task<VakifbankBankListResponseDto?> GetBankListAsync(CancellationToken cancellationToken = default)
    {
        string accessToken = await GetPublicAccessTokenAsync(cancellationToken);
        var apiUrl = _configuration["VakifBankApi:BankListUrl"]
                     ?? throw new InvalidOperationException("VakifBankApi:BankListUrl ayarı eksik!");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue(AuthScheme, accessToken);
        requestMessage.Content = new StringContent(EmptyJsonBody, System.Text.Encoding.UTF8, JsonMediaType);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Vakıfbank Banka Listesi Hatası: {response.StatusCode} - {responseContent}");

        return JsonSerializer.Deserialize<VakifbankBankListResponseDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<VakifbankNearestResponseDto?> GetNearestBranchAndAtmAsync(string latitude, string longitude, int distanceLimit, CancellationToken cancellationToken = default)
    {
        string accessToken = await GetPublicAccessTokenAsync(cancellationToken);
        var apiUrl = _configuration["VakifBankApi:NearestUrl"]
                     ?? throw new InvalidOperationException("VakifBankApi:NearestUrl ayarı eksik!");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue(AuthScheme, accessToken);

        var safeLatitude = latitude.Replace(".", ",");
        var safeLongitude = longitude.Replace(".", ",");

        var requestModel = new
        {
            Latitude = safeLatitude,
            Longitude = safeLongitude,
            DistanceLimit = distanceLimit
        };

        string json = JsonSerializer.Serialize(requestModel, new JsonSerializerOptions { PropertyNamingPolicy = null });
        requestMessage.Content = new StringContent(json, System.Text.Encoding.UTF8, JsonMediaType);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Vakıfbank En Yakın Şube/ATM Hatası: {response.StatusCode} - {responseContent}");

        var result = JsonSerializer.Deserialize<VakifbankNearestResponseDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (result?.Data?.BranchandATM == null || result.Data.BranchandATM.Count == 0)
            throw new KeyNotFoundException("Belirtilen kriterlere uygun Şube veya ATM bulunamadı.");

        return result;
    }

    public async Task<VakifbankDepositResponseDto?> CalculateDepositAsync(decimal amount, string currencyCode, long depositType, long campaignId, int termDays, CancellationToken cancellationToken = default)
    {
        string accessToken = await GetPublicAccessTokenAsync(cancellationToken);
        var apiUrl = _configuration["VakifBankApi:DepositCalculatorUrl"]
                     ?? throw new InvalidOperationException("VakifBankApi:DepositCalculatorUrl ayarı eksik!");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue(AuthScheme, accessToken);

        var requestModel = new
        {
            Amount = amount,
            CurrencyCode = currencyCode,
            DepositType = depositType,
            CampaignId = campaignId,
            TermDays = termDays
        };

        string json = JsonSerializer.Serialize(requestModel, new JsonSerializerOptions { PropertyNamingPolicy = null });
        requestMessage.Content = new StringContent(json, System.Text.Encoding.UTF8, JsonMediaType);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Vakıfbank Mevduat Hesaplama Hatası: {response.StatusCode} - {responseContent}");

        var result = JsonSerializer.Deserialize<VakifbankDepositResponseDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (result?.Data?.Deposit == null)
            throw new KeyNotFoundException("Seçtiğiniz kriterlere uygun mevduat bilgisi bulunamadı.");

        return result;
    }

    public async Task<VakifbankDepositProductResponseDto?> GetDepositProductsAsync(CancellationToken cancellationToken = default)
    {
        string accessToken = await GetPublicAccessTokenAsync(cancellationToken);
        var apiUrl = _configuration["VakifBankApi:DepositProductsUrl"]
                     ?? throw new InvalidOperationException("VakifBankApi:DepositProductsUrl ayarı eksik!");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue(AuthScheme, accessToken);
        requestMessage.Content = new StringContent(EmptyJsonBody, System.Text.Encoding.UTF8, JsonMediaType);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Vakıfbank Mevduat Ürün Listesi Hatası: {response.StatusCode} - {responseContent}");

        return JsonSerializer.Deserialize<VakifbankDepositProductResponseDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<VakifbankReceiptResponseDto?> GetReceiptAsync(
    string rizaNo,
    string accountNumber,
    string transactionId,
    string receiptFormat = "pdf",
    CancellationToken cancellationToken = default)
    {
        string accessToken = await GetAccessTokenAsync(rizaNo, cancellationToken);

        var receiptUrl = _configuration["VakifBankB2B:ReceiptUrl"]
            ?? "https://inbound.apigatewaytest.vakifbank.com.tr:8443/getReceipt";

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, receiptUrl);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue(AuthScheme, accessToken);

        var requestModel = new
        {
            TransactionId = transactionId,
            AccountNumber = accountNumber,
            ReceiptFormat = receiptFormat
        };

        requestMessage.Content = new StringContent(JsonSerializer.Serialize(requestModel), System.Text.Encoding.UTF8, JsonMediaType);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Vakıfbank Dekont Hatası: {response.StatusCode} - {responseContent}");
        }

        return JsonSerializer.Deserialize<VakifbankReceiptResponseDto>(responseContent);
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