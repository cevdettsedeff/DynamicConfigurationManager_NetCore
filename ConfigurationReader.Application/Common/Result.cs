namespace ConfigurationReader.Application.Common;

public class Result
{
    public bool IsSuccess { get; protected set; }
    public string Message { get; protected set; } = string.Empty;
    public List<string> Errors { get; protected set; } = new();

    protected Result(bool isSuccess, string message)
    {
        IsSuccess = isSuccess;
        Message = message;
    }

    protected Result(bool isSuccess, List<string> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public static Result Success(string message = "Operation successful")
        => new(true, message);

    public static Result Failure(string error)
        => new(false, new List<string> { error });

    public static Result Failure(List<string> errors)
        => new(false, errors);
}

public class Result<T> : Result
{
    public T? Data { get; private set; }

    private Result(bool isSuccess, string message, T? data = default)
        : base(isSuccess, message)
    {
        Data = data;
    }

    private Result(bool isSuccess, List<string> errors)
        : base(isSuccess, errors)
    {
    }

    public static Result<T> Success(T data, string message = "Operation successful")
        => new(true, message, data);

    public static new Result<T> Failure(string error)
        => new(false, new List<string> { error });

    public static new Result<T> Failure(List<string> errors)
        => new(false, errors);
}