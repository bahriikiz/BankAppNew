using Microsoft.Extensions.Configuration;
using OnlineBankAppServer.Application.Abstractions;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace OnlineBankAppServer.Infrasturcture.Services;

public sealed class ExchangeService(HttpClient httpClient, IConfiguration configuration) : IExchangeService
{
    // TL karşısında diğer dövizlerin kurlarını VakıfBank API'si üzerinden alarak çapraz kur hesaplaması yapar.
    public async Task<decimal> GetRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken)
    {
        fromCurrency = fromCurrency.ToUpper();
        toCurrency = toCurrency.ToUpper();

        if (fromCurrency == toCurrency) return 1;

        if (toCurrency == "TRY" || toCurrency == "TL")
        {
            return await FetchRateFromVakifBankAsync(fromCurrency, "TL", cancellationToken);
        }

        if (fromCurrency == "TRY" || fromCurrency == "TL")
        {
            decimal rate = await FetchRateFromVakifBankAsync(toCurrency, "TL", cancellationToken);
            return 1m / rate;
        }

        decimal rateFrom = await FetchRateFromVakifBankAsync(fromCurrency, "TL", cancellationToken);
        decimal rateTo = await FetchRateFromVakifBankAsync(toCurrency, "TL", cancellationToken);

        return rateFrom / rateTo;
    }

    // --- VAKIFBANK'A İSTEK ATAN GİZLİ İŞÇİ METOT ---
    private async Task<decimal> FetchRateFromVakifBankAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken)
    {
        string apiFrom = fromCurrency == "TRY" ? "TL" : fromCurrency;
        string apiTo = toCurrency == "TRY" ? "TL" : toCurrency;
        string token = await GetAccessTokenAsync(cancellationToken);

        var requestModel = new VakifBankCalculationRequest
        {
            SourceCurrencyCode = apiFrom,
            SourceAmount = "1", 
            TargetCurrencyCode = apiTo
        };

 
        string jsonPayload = JsonSerializer.Serialize(requestModel);

        // istek oluşturma
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, configuration["VakifBankApi:ApiUrl"]);

        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        requestMessage.Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.SendAsync(requestMessage, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new KeyNotFoundException($"VakıfBank Kur Hatası ({response.StatusCode}): {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<VakifBankCalculatorResponse>(responseContent);

        if (result?.Data?.Currency?.SaleAmount == null)
        {
            throw new KeyNotFoundException("VakıfBank'tan kur bilgisi okunamadı.");
        }

        // Parse İşlemi (Nokta/Virgül kontrolü)
        if (decimal.TryParse(result.Data.Currency.SaleAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal rate))
        {
            return rate;
        }

        if (decimal.TryParse(result.Data.Currency.SaleAmount.Replace(".", ","), out decimal rateTr))
        {
            return rateTr;
        }

        throw new KeyNotFoundException($"Kur format hatası: {result.Data.Currency.SaleAmount}");
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var clientId = configuration["VakifBankApi:ClientId"];
        var clientSecret = configuration["VakifBankApi:ClientSecret"];
        var scope = configuration["VakifBankApi:Scope"];
        var tokenUrl = configuration["VakifBankApi:TokenUrl"];
        var authString = $"{clientId}:{clientSecret}";
        var base64Auth = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(authString));

        var dict = new Dictionary<string, string>
        {
            {"grant_type", "client_credentials"},
            {"scope", scope!}
        };

        var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
        {
            Content = new FormUrlEncodedContent(dict)
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);

        var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new KeyNotFoundException($"Token Hatası: {error}");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var tokenResult = JsonSerializer.Deserialize<VakifBankTokenResponse>(content);

        return tokenResult?.Access_token ?? throw new KeyNotFoundException("Token boş.");
    }
}

// --- MODELLER ---
public class VakifBankCalculationRequest
{
    [JsonPropertyName("SourceCurrencyCode")]
    public string? SourceCurrencyCode { get; set; }

    [JsonPropertyName("SourceAmount")]
    public string? SourceAmount { get; set; }

    [JsonPropertyName("TargetCurrencyCode")]
    public string? TargetCurrencyCode { get; set; }
}

public class VakifBankTokenResponse
{
    [JsonPropertyName("access_token")] 
    public string? Access_token { get; set; }

    [JsonPropertyName("token_type")] 
    public string? Token_type { get; set; }

    [JsonPropertyName("expires_in")] 
    public int Expires_in { get; set; }
}
public class VakifBankCalculatorResponse
{
    public VakifBankData? Data { get; set; }
    public VakifBankHeader? Header { get; set; }
}

public class VakifBankHeader
{
    public string? StatusCode { get; set; }
    public string? StatusDescription { get; set; }
}

public class VakifBankData
{
    public VakifBankCurrency? Currency { get; set; }
}

public class VakifBankCurrency
{
    public string? TargetCurrencyCode { get; set; }
    public string? SaleRate { get; set; }
    public string? SaleAmount { get; set; }
}