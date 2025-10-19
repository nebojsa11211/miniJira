namespace MiniJira.Services.DTOs;

public class Result
{
    public bool IsSuccess { get; }
    public string ErrorMessage { get; }

    protected Result(bool isSuccess, string errorMessage = "")
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new Result(true);
    public static Result Failure(string errorMessage) => new Result(false, errorMessage);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string errorMessage = "")
        : base(isSuccess, errorMessage)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new Result<T>(true, value);
    public static new Result<T> Failure(string errorMessage) => new Result<T>(false, default, errorMessage);
}
