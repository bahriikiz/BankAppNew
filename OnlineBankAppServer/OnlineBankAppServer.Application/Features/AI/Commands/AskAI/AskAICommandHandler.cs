using MediatR;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace OnlineBankAppServer.Application.Features.AI.Commands.AskAI;

internal sealed class AskAICommandHandler(IConfiguration configuration) : IRequestHandler<AskAICommand, string>
{
    public async Task<string> Handle(AskAICommand request, CancellationToken cancellationToken)
    {
        // 1. API Anahtarını al
        string? apiKey = configuration["Gemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey)) throw new Exception("Gemini API Key bulunamadı! Lütfen appsettings.json'u kontrol edin.");

        string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

        // 2. Yapay Zekaya Karakter Yükleme (System Prompt)
        string prompt = $@"Sen 'İKİZ BANK' adlı dijital bir bankanın profesyonel, kibar ve yardımsever yapay zeka asistanısın. Adın 'İKİZ AI'. 
        Görevlerin: 
        1. Uzmanlık Alanın: Kullanıcılara krediler, para transferleri (Havale/EFT/FAST), açık bankacılık, mevduat/katılım hesapları, kredi kartları ve tüm bankacılık işlemleri hakkında detaylı bilgi ver.
        2. Piyasa ve Yatırım: Döviz kurları (Dolar, Euro vb.), değerli madenler (Altın, Gümüş), borsa endeksleri, fonlar, kripto varlıklar ve küresel piyasalar hakkında profesyonel finansal yorumlar ve anlık bilgiler sun.
        3. Banka Dışı Konular: Yemek tarifi, kod yazma, tarih, oyun vb. bankacılık dışı sorular sorulursa kibarca 'Ben İKİZ BANK'ın finansal asistanıyım, sadece bankacılık, ekonomi ve piyasalar hakkında yardımcı olabilirim' diyerek reddet.
        4. Üslup: Cevapların kurumsal bir bankacı gibi güven verici, net, anlaşılır ve çok uzun olmayan bir dilde olsun.
        5. Format: Markdown (**, *, #) veya HTML etiketleri kullanma, tamamen düz ve temiz bir metin olarak cevap ver.
        
        Kullanıcının Sorusu: {request.Message}";

        // 3. Gemini'nin anladığı JSON yapısı
        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        using var httpClient = new HttpClient();
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        // 4. İsteği Gönder
        var response = await httpClient.PostAsync(url, content, cancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Yapay zeka servisi şu an meşgul, lütfen daha sonra tekrar deneyin.");
        }

        // 5. Cevabı Çözümle (Parse)
        using var jsonDocument = JsonDocument.Parse(responseString);
        var answer = jsonDocument.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return answer?.Trim() ?? "Özür dilerim, sizi anlayamadım.";
    }
}