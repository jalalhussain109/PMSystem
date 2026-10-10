namespace PMS.Application.Common;

public class ServiceResult<T>
{
    public bool Succeeded { get; init; }
    public bool NotFound { get; init; }
    public T? Data { get; init; }
    public List<string> Errors { get; init; } = [];

    public static ServiceResult<T> Success(T? data = default) =>
        new()
        {
            Succeeded = true,
            Data = data
        };

    public static ServiceResult<T> Failure(params string[] errors) =>
        new()
        {
            Succeeded = false,
            Errors = errors.ToList()
        };

    public static ServiceResult<T> Missing() =>
        new()
        {
            Succeeded = false,
            NotFound = true
        };
}