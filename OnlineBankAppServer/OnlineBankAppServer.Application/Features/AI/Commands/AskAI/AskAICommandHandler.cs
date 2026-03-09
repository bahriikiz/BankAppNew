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

        // Gemini 1.5 Flash (Çok hızlı ve akıllıdır)
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

        // 2. Yapay Zekaya Karakter Yükleme (System Prompt)
        string prompt = $@"Sen 'İKİZ BANK' adlı dijital bir bankanın profesyonel, kibar ve yardımsever yapay zeka asistanısın. Adın 'İKİZ AI'. 
        Görevlerin: 
        1. Sadece finans, bankacılık, krediler, para transferleri, açık bankacılık ve hesap işlemleri hakkında konuş.
        2. Yemek tarifi, kod yazma, tarih vb. bankacılık dışı sorular sorulursa kibarca 'Ben bir bankacılık asistanıyım, sadece finansal konularda yardımcı olabilirim' diyerek reddet.
        3. Cevaplarını çok uzun tutma, net ve anlaşılır olsun.
        4. Markdown veya HTML kullanma, düz metin olarak cevap ver.
        
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