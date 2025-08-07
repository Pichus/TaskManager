namespace TaskManager.UseCases.Shared;

public class Result
{
    protected Result()
    {
    }

    protected Result(Error error)
    {
        Error = error;
    }

    public bool IsSuccess => !IsFailure;
    public bool IsFailure => Error is not null;
    public Error? Error { get; }

    public static Result Success()
    {
        return new Result();
    }

    public static Result Failure(Error error)
    {
        return new Result(error);
    }
}

public class Result<TSuccessValue> : Result
{
    protected Result(Error error) : base(error)
    {
    }

    protected Result(TSuccessValue value)
    {
        Value = value;
    }

    public TSuccessValue? Value { get; }

    public static Result<TSuccessValue> Success(TSuccessValue value)
    {
        return new Result<TSuccessValue>(value);
    }

    public new static Result<TSuccessValue> Failure(Error error)
    {
        return new Result<TSuccessValue>(error);
    }
}