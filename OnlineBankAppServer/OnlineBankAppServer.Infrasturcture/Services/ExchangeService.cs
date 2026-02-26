using Microsoft.Extensions.Configuration;
using OnlineBankAppServer.Application.Abstractions;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace OnlineBankAppServer.Infrastructure.Services;

public sealed class ExchangeService(HttpClient httpClient, IConfiguration configuration) : IExchangeService
{
    // --- AKILLI YÖNLENDİRİCİ METOT (TL KÖPRÜSÜ) ---
    public async Task<decimal> GetRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken)
    {
        fromCurrency = fromCurrency.ToUpper();
        toCurrency = toCurrency.ToUpper();

        if (fromCurrency == toCurrency) return 1;

        // SENARYO 1: Yabancı Döviz -> TL (Örn: USD -> TRY veya XAU -> TRY)
        if (toCurrency == "TRY" || toCurrency == "TL")
        {
            return await FetchRateFromVakifBankAsync(fromCurrency, "TL", cancellationToken);
        }

        // SENARYO 2: TL -> Yabancı Döviz (Örn: TRY -> USD)
        // Çözüm: USD -> TRY kurunu alırız ve 1'i o kura bölerek tersini buluruz.
        if (fromCurrency == "TRY" || fromCurrency == "TL")
        {
            decimal rate = await FetchRateFromVakifBankAsync(toCurrency, "TL", cancellationToken);
            return 1m / rate;
        }

        // SENARYO 3: Çapraz Kur (Örn: EUR -> USD veya XAU -> USD)
        // Çözüm: İki dövizin de TL karşılığını alırız ve birbirine böleriz.
        decimal rateFrom = await FetchRateFromVakifBankAsync(fromCurrency, "TL", cancellationToken);
        decimal rateTo = await FetchRateFromVakifBankAsync(toCurrency, "TL", cancellationToken);

        return rateFrom / rateTo;
    }

    // --- VAKIFBANK'A İSTEK ATAN GİZLİ İŞÇİ METOT ---
    private async Task<decimal> FetchRateFromVakifBankAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken)
    {
        // SADECE TRY -> TL çevirisi yapıyoruz. XAU, USD vs. KENDİ HALİYLE (XAU) GİDİYOR!
        string apiFrom = fromCurrency == "TRY" ? "TL" : fromCurrency;
        string apiTo = toCurrency == "TRY" ? "TL" : toCurrency;

        // 1. ADIM: Token Al
        string token = await GetAccessTokenAsync(cancellationToken);

        // 2. ADIM: İsteği Hazırla
        var requestModel = new VakifBankCalculationRequest
        {
            SourceCurrencyCode = apiFrom,
            SourceAmount = "1", // Tırnak içinde string
            TargetCurrencyCode = apiTo
        };

        // 3. ADIM: Serileştirme
        string jsonPayload = JsonSerializer.Serialize(requestModel);

        // 4. ADIM: HTTP İsteği
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, configuration["VakifBankApi:ApiUrl"]);

        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        requestMessage.Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

        // 5. ADIM: Gönder
        var response = await httpClient.SendAsync(requestMessage, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"VakıfBank Kur Hatası ({response.StatusCode}): {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<VakifBankCalculatorResponse>(responseContent);

        if (result?.Data?.Currency?.SaleAmount == null)
        {
            throw new Exception("VakıfBank'tan kur bilgisi okunamadı.");
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

        throw new Exception($"Kur format hatası: {result.Data.Currency.SaleAmount}");
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var clientId = configuration["VakifBankApi:ClientId"];
        var clientSecret = configuration["VakifBankApi:ClientSecret"];
        var scope = configuration["VakifBankApi:Scope"];
        var tokenUrl = configuration["VakifBankApi:TokenUrl"];

        // Basic Auth Header Oluşturma
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
            throw new Exception($"Token Hatası: {error}");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var tokenResult = JsonSerializer.Deserialize<VakifBankTokenResponse>(content);

        return tokenResult?.access_token ?? throw new Exception("Token boş.");
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
    public string? access_token { get; set; }
    public string? token_type { get; set; }
    public int expires_in { get; set; }
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