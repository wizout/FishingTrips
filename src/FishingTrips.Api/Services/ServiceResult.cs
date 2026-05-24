namespace FishingTrips.Api.Services;

public enum ResultStatus
{
    Ok,
    NotFound,
    Conflict,
    Invalid
}

public class ServiceResult<T>
{
    public ResultStatus Status { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }

    public bool IsSuccess => Status == ResultStatus.Ok;

    public static ServiceResult<T> Ok(T value) => new() { Status = ResultStatus.Ok, Value = value };
    public static ServiceResult<T> NotFound(string error) => new() { Status = ResultStatus.NotFound, Error = error };
    public static ServiceResult<T> Conflict(string error) => new() { Status = ResultStatus.Conflict, Error = error };
    public static ServiceResult<T> Invalid(string error) => new() { Status = ResultStatus.Invalid, Error = error };
}
