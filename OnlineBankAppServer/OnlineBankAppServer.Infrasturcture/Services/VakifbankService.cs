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

    // JSON seçeneklerini merkezi bir yerden yönetiyoruz
    private static readonly JsonSerializerOptions DefaultJsonOptions = new() { PropertyNameCaseInsensitive = true };
    private static readonly JsonSerializerOptions StrictJsonOptions = new() { PropertyNamingPolicy = null, PropertyNameCaseInsensitive = true };

    public VakifbankService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        string baseUrl = _configuration["VakifBankB2B:BaseUrl"] ?? throw new InvalidOperationException("BaseUrl eksik!");
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    #region Token İşlemleri (Tekilleştirildi)

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

    private async Task<string> RequestTokenInternalAsync(string? url, Dictionary<string, string> data, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(url)) throw new InvalidOperationException("Token URL eksik!");

        var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(data), ct);
        var result = await HandleResponseAsync<VakifbankTokenResponseDto>(response, ct);

        return result?.AccessToken ?? throw new InvalidOperationException("Token boş döndü!");
    }

    #endregion

    #region Genel API İstek Motoru (Tekrarı Bitiren Kısım)

    private async Task<T?> SendVakifbankRequestAsync<T>(
        string? url,
        string accessToken,
        object? body = null,
        JsonSerializerOptions? options = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(url)) throw new InvalidOperationException("API URL eksik!");

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue(AuthScheme, accessToken);

        string json = body != null ? JsonSerializer.Serialize(body, options ?? DefaultJsonOptions) : EmptyJsonBody;
        request.Content = new StringContent(json, System.Text.Encoding.UTF8, JsonMediaType);

        var response = await _httpClient.SendAsync(request, ct);
        return await HandleResponseAsync<T>(response, ct, options);
    }

    private static async Task<T?> HandleResponseAsync<T>(HttpResponseMessage response, CancellationToken ct, JsonSerializerOptions? options = null)
    {
        var content = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"API Hatası: {response.StatusCode} - {content}");

        return JsonSerializer.Deserialize<T>(content, options ?? DefaultJsonOptions);
    }

    #endregion

    // --- Servis Metotları ---

    public async Task<VakifbankAccountListResponseDto?> GetAccountsAsync(string rizaNo, CancellationToken ct = default)
    {
        string token = await GetAccessTokenAsync(rizaNo, ct);
        return await SendVakifbankRequestAsync<VakifbankAccountListResponseDto>(_configuration["VakifBankB2B:ApiUrl"], token, null, null, ct);
    }

    public async Task<VakifbankAccountTransactionsResponseDto?> GetAccountTransactionsAsync(string rizaNo, string accountNumber, DateTime start, DateTime end, CancellationToken ct = default)
    {
        string token = await GetAccessTokenAsync(rizaNo, ct);
        var body = new { AccountNumber = accountNumber, StartDate = start.ToString("yyyy-MM-ddTHH:mm:ss+03:00"), EndDate = end.ToString("yyyy-MM-ddTHH:mm:ss+03:00") };
        return await SendVakifbankRequestAsync<VakifbankAccountTransactionsResponseDto>(_configuration["VakifBankB2B:TransactionsUrl"], token, body, null, ct);
    }

    public async Task<VakifbankAccountDetailResponseDto?> GetAccountDetailAsync(string rizaNo, string accNo, CancellationToken ct = default)
    {
        string token = await GetAccessTokenAsync(rizaNo, ct);
        return await SendVakifbankRequestAsync<VakifbankAccountDetailResponseDto>(_configuration["VakifBankB2B:AccountDetailUrl"], token, new { AccountNumber = accNo }, null, ct);
    }

    public async Task<VakifbankReceiptResponseDto?> GetReceiptAsync(string riza, string accNo, string txId, string format = "pdf", CancellationToken ct = default)
    {
        string token = await GetAccessTokenAsync(riza, ct);
        var body = new { TransactionId = txId, AccountNumber = accNo, ReceiptFormat = format };
        return await SendVakifbankRequestAsync<VakifbankReceiptResponseDto>(_configuration["VakifBankB2B:ReceiptUrl"], token, body, null, ct);
    }

    public async Task<VakifbankCityResponseDto?> GetCitiesAsync(CancellationToken ct = default)
    {
        string token = await GetPublicAccessTokenAsync(ct);
        return await SendVakifbankRequestAsync<VakifbankCityResponseDto>(_configuration["VakifBankApi:CitiesUrl"], token, null, null, ct);
    }

    public async Task<VakifbankDistrictResponseDto?> GetDistrictsAsync(string cityCode, CancellationToken ct = default)
    {
        string token = await GetPublicAccessTokenAsync(ct);
        return await SendVakifbankRequestAsync<VakifbankDistrictResponseDto>(_configuration["VakifBankApi:DistrictsUrl"], token, new { CityCode = cityCode }, StrictJsonOptions, ct);
    }

    public async Task<VakifbankNeighborhoodResponseDto?> GetNeighborhoodsAsync(string districtCode, CancellationToken ct = default)
    {
        string token = await GetPublicAccessTokenAsync(ct);
        return await SendVakifbankRequestAsync<VakifbankNeighborhoodResponseDto>(_configuration["VakifBankApi:NeighborhoodsUrl"], token, new { DistrictCode = districtCode?.Trim() }, StrictJsonOptions, ct);
    }

    public async Task<VakifbankBranchResponseDto?> GetBranchesAsync(string city, string dist, CancellationToken ct = default)
    {
        string token = await GetPublicAccessTokenAsync(ct);
        var result = await SendVakifbankRequestAsync<VakifbankBranchResponseDto>(_configuration["VakifBankApi:BranchesUrl"], token, null, null, ct);

        if (result?.Data?.Branch == null) throw new InvalidOperationException("Şube verisi dönmedi.");

        var filtered = result.Data.Branch.Where(x =>
            (string.IsNullOrEmpty(city) || x.CityCode?.Trim() == city.Trim()) &&
            (string.IsNullOrEmpty(dist) || x.DistrictCode?.Trim() == dist.Trim())).ToList();

        if (filtered.Count == 0) throw new KeyNotFoundException("Şube bulunamadı.");
        return new VakifbankBranchResponseDto(result.Header, new VakifbankBranchDataDto([.. filtered]));
    }

    public async Task<VakifbankBankListResponseDto?> GetBankListAsync(CancellationToken ct = default)
    {
        string token = await GetPublicAccessTokenAsync(ct);
        return await SendVakifbankRequestAsync<VakifbankBankListResponseDto>(_configuration["VakifBankApi:BankListUrl"], token, null, null, ct);
    }

    public async Task<VakifbankNearestResponseDto?> GetNearestBranchAndAtmAsync(string lat, string lon, int dist, CancellationToken ct = default)
    {
        string token = await GetPublicAccessTokenAsync(ct);
        var body = new { Latitude = lat.Replace(".", ","), Longitude = lon.Replace(".", ","), DistanceLimit = dist };
        var res = await SendVakifbankRequestAsync<VakifbankNearestResponseDto>(_configuration["VakifBankApi:NearestUrl"], token, body, StrictJsonOptions, ct);

        if (res?.Data?.BranchandATM == null || res.Data.BranchandATM.Count == 0) throw new KeyNotFoundException("ATM/Şube bulunamadı.");
        return res;
    }

    public async Task<VakifbankDepositResponseDto?> CalculateDepositAsync(decimal amt, string cur, long type, long camp, int days, CancellationToken ct = default)
    {
        string token = await GetPublicAccessTokenAsync(ct);
        var body = new { Amount = amt, CurrencyCode = cur, DepositType = type, CampaignId = camp, TermDays = days };
        var res = await SendVakifbankRequestAsync<VakifbankDepositResponseDto>(_configuration["VakifBankApi:DepositCalculatorUrl"], token, body, StrictJsonOptions, ct);

        if (res?.Data?.Deposit == null) throw new KeyNotFoundException("Mevduat bilgisi bulunamadı.");
        return res;
    }

    public async Task<VakifbankDepositProductResponseDto?> GetDepositProductsAsync(CancellationToken ct = default)
    {
        string token = await GetPublicAccessTokenAsync(ct);
        return await SendVakifbankRequestAsync<VakifbankDepositProductResponseDto>(_configuration["VakifBankApi:DepositProductsUrl"], token, null, null, ct);
    }

    // Interface için yedek isim yönlendirmesi
    public Task<VakifbankReceiptResponseDto?> GetTransactionReceiptAsync(string r, string t, string a, string f = "pdf", CancellationToken c = default)
        => GetReceiptAsync(r, a, t, f, c);
}