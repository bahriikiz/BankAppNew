using MediatR;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace OnlineBankAppServer.Application.Features.AI.Commands.AskAI;

internal sealed class AskAICommandHandler(IConfiguration configuration) : IRequestHandler<AskAICommand, string>
{
    public async Task<string> Handle(AskAICommand request, CancellationToken cancellationToken)
    {
        // 1. Ayarları doğrudan IConfiguration üzerinden alıyoruz (Hardcoded değerler kaldırıldı)
        string? url = configuration["Ollama:Url"];
        string? modelName = configuration["Ollama:Model"];

        // Eğer appsettings.json'da bu ayarlar unutulmuşsa uygulamanın sessizce patlamaması için hata fırlatıyoruz
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(modelName))
        {
            throw new InvalidOperationException("Ollama yapılandırması (Url veya Model) bulunamadı! Lütfen appsettings.json dosyanızı kontrol edin.");
        }

        // 2. Yapay Zekaya Karakter Yükleme (System Prompt)
        string systemPrompt = $@"Sen 'İKİZ BANK' adlı dijital bir bankanın profesyonel, kibar ve yardımsever yapay zeka asistanısın. Adın 'İKİZ AI'. 
        Görevlerin: 
        1. Uzmanlık Alanın: Kullanıcılara krediler, para transferleri (Havale/EFT/FAST), açık bankacılık, mevduat/katılım hesapları, kredi kartları ve tüm bankacılık işlemleri hakkında detaylı bilgi ver.
        2. Piyasa ve Yatırım: Döviz kurları (Dolar, Euro vb.), değerli madenler (Altın, Gümüş), borsa endeksleri, fonlar, kripto varlıklar ve küresel piyasalar hakkında profesyonel finansal yorumlar ve anlık bilgiler sun.
        3. Banka Dışı Konular: Yemek tarifi, kod yazma, tarih, oyun vb. bankacılık dışı sorular sorulursa kibarca 'Ben İKİZ BANK'ın finansal asistanıyım, sadece bankacılık, ekonomi ve piyasalar hakkında yardımcı olabilirim' diyerek reddet.
        4. Üslup: Cevapların kurumsal bir bankacı gibi güven verici, net, anlaşılır ve çok uzun olmayan bir dilde olsun.
        5. Format: Markdown (**, *, #) veya HTML etiketleri kullanma, tamamen düz ve temiz bir metin olarak cevap ver.";

        string fullPrompt = $"{systemPrompt}\n\nKullanıcının Sorusu: {request.Message}";

        // 3. Ollama'nın anladığı JSON yapısı
        var requestBody = new
        {
            model = modelName,
            prompt = fullPrompt,
            stream = false
        };

        // Not: İlerleyen süreçte 'using var httpClient = new HttpClient()' yerine 
        // Dependency Injection üzerinden IHttpClientFactory kullanmak performansı daha da artıracaktır.
        using var httpClient = new HttpClient();
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        try
        {
            // 4. İsteği Gönder
            var response = await httpClient.PostAsync(url, content, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Ollama servisi hata döndürdü (Kod: {response.StatusCode}).");
            }

            // 5. Cevabı Çözümle (Parse)
            using var jsonDocument = JsonDocument.Parse(responseString);
            var answer = jsonDocument.RootElement
                .GetProperty("response")
                .GetString();

            return answer?.Trim() ?? "Özür dilerim, sizi anlayamadım.";
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Yapay zeka asistanına şu an ulaşılamıyor.");
        }
    }
}