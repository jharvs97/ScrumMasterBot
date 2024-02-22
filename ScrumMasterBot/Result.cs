using System;

namespace ScrumMasterBot;

public interface IResult<TResult>
{
    TResult Value { get; }
}

public interface IError
{
    public string Reason { get; }
}

public class None {}

public class Result<TResult> : IResult<TResult>
{
    public TResult Value => _value;

    public Result(TResult value)
    {
        _value = value;
    }

    TResult _value;
}

public class Error<TResult>(string reason) : IResult<TResult>, IError
{
    public TResult Value => throw new InvalidOperationException("Accessing value of Error result");
    public string Reason => reason;
}
