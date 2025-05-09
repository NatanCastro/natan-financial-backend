namespace NatanFinancial.Shared.Functional;

public abstract class Result(bool isSuccess)
{
  protected readonly bool IsSuccess = isSuccess;
}

public class Result<T, E> : Result
 where E : Exception
{
  private readonly T? Data;
  private readonly E? Error;

  private Result(T? data, E? error, bool success = true) : base(success)
  {
    Data = data;
    Error = error;
  }


  public static Result<T, E> Ok(T data) => new(data, default);
  public static Result<T, E> Err(E error) => new(default, error, false);

  public bool Success => IsSuccess;
  public bool HasError => !IsSuccess;

  public R Match<R>(Func<T, R> success, Func<E, R> failure) =>
    IsSuccess ? success(Data!) : failure(Error!);

  public T Unwrap()
  {
    if (!IsSuccess || Data == null)
      throw new Exception("Trying to unwrap a failed result");
    return Data!;
  }
  public E UnwrapErr()
  {
    if (IsSuccess || Error != null)  // This condition is incorrect
      throw new Exception("Trying to unwrap an error from a successful result");
    return Error!;
  }

  public T UnwrapOr(T defaultValue)
  {
    if (!IsSuccess || Data == null)
      return defaultValue;
    return Data;
  }

  public T? UnwrapOrDefault()
  {
    if (!IsSuccess || Data == null)
      return default;
    return Data;
  }

  public Result<U, E> Map<U>(Func<T, U> mapper)
  {
    if (!IsSuccess || Data == null)
      return Result<U, E>.Err(Error!);

    return Result<U, E>.Ok(mapper(Data));
  }

  public Result<T, F> MapErr<F>(Func<E, F> mapper) where F : Exception
  {
    if (!IsSuccess)
      return Result<T, F>.Err(mapper(Error!));

    // If successful, create a new successful result with the same data
    return Result<T, F>.Ok(Data!);
  }
}