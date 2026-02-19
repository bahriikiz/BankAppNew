namespace OnlineBankAppServer.Domain.Abstractions;

public sealed class Result<T>
{
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsSuccess { get; set; } = true;

    // Başarılı olduğunda
    public static Result<T> Succeed(T data)
    {
        return new Result<T>
        {
            Data = data,
            IsSuccess = true
        };
    }

    // Başarısız olduğunda
    public static Result<T> Failure(string errorMessage)
    {
        return new Result<T>
        {
            ErrorMessage = errorMessage,
            IsSuccess = false
        };
    }
}