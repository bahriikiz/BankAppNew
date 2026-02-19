using System.Net;
using System.Text.Json;

namespace OnlineBankAppServer.WebApi.Middlewares;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            // İsteği bir sonraki adıma ilet (Controller'a git)
            await _next(context);
        }
        catch (Exception ex)
        {
            // Hata olursa yakala ve HandleExceptionAsync'e gönder
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Varsayılan olarak 500 (Sunucu Hatası) 
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        // Eğer hata bizim fırlattığımız "Bakiye Yetersiz" gibi bir hata ise 400 (Bad Request) 

        if (exception.Message == "Yetersiz bakiye." ||
            exception.Message.Contains("bulunamadı"))
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = exception.Message,
            Title = "Bir Hata Oluştu"
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}