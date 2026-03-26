using Microsoft.Extensions.Configuration;
using OnlineBankAppServer.Application.Abstractions;
using OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;
using System.Net.Http.Headers;
using System.Text.Json;

namespace OnlineBankAppServer.Infrasturcture.Services;

public sealed class VakifbankService : IVakifbankService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    private const string AuthScheme = "Bearer";
    private const string EmptyJsonBody = "{}";
    private const string JsonMediaType = "application/json";

    private static readonly JsonSerializerOptions DefaultJsonOptions = new() { PropertyNameCaseInsensitive = true };
    private static readonly JsonSerializerOptions StrictJsonOptions = new() { PropertyNamingPolicy = null, PropertyNameCaseInsensitive = true };

    public VakifbankService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        string baseUrl = _configuration["VakifBankB2B:BaseUrl"] ?? throw new InvalidOperationException("BaseUrl eksik!");
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    #region Token İşlemleri

    private async Task<string> GetAccessTokenAsync(string rizaNo, CancellationToken cancellationToken)
    {
        var requestData = new Dictionary<string, string>
        {
            { "client_id", _configuration["VakifBankB2B:ClientId"]! },
            { "client_secret", _configuration["VakifBankB2B:ClientSecret"]! },
            { "grant_type", "b2b_credentials" },
            { "scope", _configuration["VakifBankB2B:Scope"]! },
            { "consentId", rizaNo },
            { "resource", _configuration["VakifBankB2B:Resource"]! }
        };

        return await RequestTokenInternalAsync(_configuration["VakifBankB2B:TokenUrl"], requestData, cancellationToken);
    }

    private async Task<string> GetPublicAccessTokenAsync(CancellationToken cancellationToken)
    {
        var requestBody = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", _configuration["VakifBankApi:ClientId"]! },
            { "client_secret", _configuration["VakifBankApi:ClientSecret"]! },
            { "scope", _configuration["VakifBankApi:Scope"]! }
        };

        return await RequestTokenInternalAsync(_configuration["VakifBankApi:TokenUrl"], requestBody, cancellationToken);
    }

    private async Task<string> RequestTokenInternalAsync(string? url, Dictionary<string, string> data, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(url)) throw new InvalidOperationException("Token URL eksik!");

        var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(data), cancellationToken);
        var result = await HandleResponseAsync<VakifbankTokenResponseDto>(response, cancellationToken);

        return result?.AccessToken ?? throw new InvalidOperationException("Token boş döndü!");
    }

    #endregion

    #region Genel API İstek Motoru

    private async Task<T?> SendVakifbankRequestAsync<T>(
        string? url,
        string accessToken,
        object? body = null,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(url)) throw new InvalidOperationException("API URL eksik!");

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue(AuthScheme, accessToken);

        string json = body != null ? JsonSerializer.Serialize(body, options ?? DefaultJsonOptions) : EmptyJsonBody;
        request.Content = new StringContent(json, System.Text.Encoding.UTF8, JsonMediaType);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        return await HandleResponseAsync<T>(response, cancellationToken, options);
    }

    private static async Task<T?> HandleResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken, JsonSerializerOptions? options = null)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"API Hatası: {response.StatusCode} - {content}");

        return JsonSerializer.Deserialize<T>(content, options ?? DefaultJsonOptions);
    }

    #endregion

    // --- Servis Metotları ---

    public async Task<VakifbankAccountListResponseDto?> GetAccountsAsync(string rizaNo, CancellationToken cancellationToken = default)
    {
        string token = await GetAccessTokenAsync(rizaNo, cancellationToken);
        return await SendVakifbankRequestAsync<VakifbankAccountListResponseDto>(_configuration["VakifBankB2B:ApiUrl"], token, null, null, cancellationToken);
    }

    public async Task<VakifbankAccountTransactionsResponseDto?> GetAccountTransactionsAsync(string rizaNo, string accountNumber, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        string token = await GetAccessTokenAsync(rizaNo, cancellationToken);
        var body = new { AccountNumber = accountNumber, StartDate = startDate.ToString("yyyy-MM-ddTHH:mm:ss+03:00"), EndDate = endDate.ToString("yyyy-MM-ddTHH:mm:ss+03:00") };
        return await SendVakifbankRequestAsync<VakifbankAccountTransactionsResponseDto>(_configuration["VakifBankB2B:TransactionsUrl"], token, body, null, cancellationToken);
    }

    public async Task<VakifbankAccountDetailResponseDto?> GetAccountDetailAsync(string rizaNo, string accountNumber, CancellationToken cancellationToken = default)
    {
        string token = await GetAccessTokenAsync(rizaNo, cancellationToken);
        return await SendVakifbankRequestAsync<VakifbankAccountDetailResponseDto>(_configuration["VakifBankB2B:AccountDetailUrl"], token, new { AccountNumber = accountNumber }, null, cancellationToken);
    }

    public async Task<VakifbankReceiptResponseDto?> GetReceiptAsync(string rizaNo, string accountNumber, string transactionId, string receiptFormat, CancellationToken cancellationToken = default)
    {
        string token = await GetAccessTokenAsync(rizaNo, cancellationToken);
        var body = new { TransactionId = transactionId, AccountNumber = accountNumber, ReceiptFormat = receiptFormat };
        return await SendVakifbankRequestAsync<VakifbankReceiptResponseDto>(_configuration["VakifBankB2B:ReceiptUrl"], token, body, null, cancellationToken);
    }

    public async Task<VakifbankCityResponseDto?> GetCitiesAsync(CancellationToken cancellationToken = default)
    {
        string token = await GetPublicAccessTokenAsync(cancellationToken);
        return await SendVakifbankRequestAsync<VakifbankCityResponseDto>(_configuration["VakifBankApi:CitiesUrl"], token, null, null, cancellationToken);
    }

    public async Task<VakifbankDistrictResponseDto?> GetDistrictsAsync(string cityCode, CancellationToken cancellationToken = default)
    {
        string token = await GetPublicAccessTokenAsync(cancellationToken);
        return await SendVakifbankRequestAsync<VakifbankDistrictResponseDto>(_configuration["VakifBankApi:DistrictsUrl"], token, new { CityCode = cityCode }, StrictJsonOptions, cancellationToken);
    }

    public async Task<VakifbankNeighborhoodResponseDto?> GetNeighborhoodsAsync(string districtCode, CancellationToken cancellationToken = default)
    {
        string token = await GetPublicAccessTokenAsync(cancellationToken);
        return await SendVakifbankRequestAsync<VakifbankNeighborhoodResponseDto>(_configuration["VakifBankApi:NeighborhoodsUrl"], token, new { DistrictCode = districtCode?.Trim() }, StrictJsonOptions, cancellationToken);
    }

    public async Task<VakifbankBranchResponseDto?> GetBranchesAsync(string cityCode, string bankDistrictCode, CancellationToken cancellationToken = default)
    {
        string token = await GetPublicAccessTokenAsync(cancellationToken);
        var result = await SendVakifbankRequestAsync<VakifbankBranchResponseDto>(_configuration["VakifBankApi:BranchesUrl"], token, null, null, cancellationToken);

        if (result?.Data?.Branch == null) throw new InvalidOperationException("Şube verisi dönmedi.");

        var filtered = result.Data.Branch.Where(x =>
            (string.IsNullOrEmpty(cityCode) || x.CityCode?.Trim() == cityCode.Trim()) &&
            (string.IsNullOrEmpty(bankDistrictCode) || x.DistrictCode?.Trim() == bankDistrictCode.Trim())).ToList();

        if (filtered.Count == 0) throw new KeyNotFoundException("Şube bulunamadı.");
        return new VakifbankBranchResponseDto(result.Header, new VakifbankBranchDataDto([.. filtered]));
    }

    public async Task<VakifbankBankListResponseDto?> GetBankListAsync(CancellationToken cancellationToken = default)
    {
        string token = await GetPublicAccessTokenAsync(cancellationToken);
        return await SendVakifbankRequestAsync<VakifbankBankListResponseDto>(_configuration["VakifBankApi:BankListUrl"], token, null, null, cancellationToken);
    }

    public async Task<VakifbankNearestResponseDto?> GetNearestBranchAndAtmAsync(string latitude, string longitude, int distanceLimit, CancellationToken cancellationToken = default)
    {
        string token = await GetPublicAccessTokenAsync(cancellationToken);
        var body = new { Latitude = latitude.Replace(".", ","), Longitude = longitude.Replace(".", ","), DistanceLimit = distanceLimit };
        var res = await SendVakifbankRequestAsync<VakifbankNearestResponseDto>(_configuration["VakifBankApi:NearestUrl"], token, body, StrictJsonOptions, cancellationToken);

        if (res?.Data?.BranchandATM == null || res.Data.BranchandATM.Count == 0) throw new KeyNotFoundException("ATM/Şube bulunamadı.");
        return res;
    }

    public async Task<VakifbankDepositResponseDto?> CalculateDepositAsync(decimal amount, string currencyCode, long depositType, long campaignId, int termDays, CancellationToken cancellationToken = default)
    {
        string token = await GetPublicAccessTokenAsync(cancellationToken);
        var body = new { Amount = amount, CurrencyCode = currencyCode, DepositType = depositType, CampaignId = campaignId, TermDays = termDays };
        var res = await SendVakifbankRequestAsync<VakifbankDepositResponseDto>(_configuration["VakifBankApi:DepositCalculatorUrl"], token, body, StrictJsonOptions, cancellationToken);

        if (res?.Data?.Deposit == null) throw new KeyNotFoundException("Mevduat bilgisi bulunamadı.");
        return res;
    }

    public async Task<VakifbankDepositProductResponseDto?> GetDepositProductsAsync(CancellationToken cancellationToken = default)
    {
        string token = await GetPublicAccessTokenAsync(cancellationToken);
        return await SendVakifbankRequestAsync<VakifbankDepositProductResponseDto>(_configuration["VakifBankApi:DepositProductsUrl"], token, null, null, cancellationToken);
    }
}