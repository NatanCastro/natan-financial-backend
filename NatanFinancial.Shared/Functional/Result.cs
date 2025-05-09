namespace NatanFinancial.Shared.Functional;

/// <summary>
/// Base class for representing the result of an operation that may succeed or fail.
/// </summary>
public abstract class Result
{
  /// <summary>
  /// Indicates whether the operation was successful.
  /// </summary>
  protected readonly bool IsSuccess;

  /// <summary>
  /// Creates a new instance of the Result class.
  /// </summary>
  /// <param name="isSuccess">Whether the operation was successful.</param>
  protected Result(bool isSuccess)
  {
    IsSuccess = isSuccess;
  }

  /// <summary>
  /// Gets whether the result represents a successful operation.
  /// </summary>
  public bool Success => IsSuccess;

  /// <summary>
  /// Gets whether the result represents a failed operation.
  /// </summary>
  public bool Failure => !IsSuccess;
}

/// <summary>
/// Represents the result of an operation that may succeed with a value of type T or fail with an error of type E.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
/// <typeparam name="E">The type of the error. Must be an Exception.</typeparam>
public class Result<T, E> : Result
    where E : Exception
{
  private T? Data { get; }
  private E? Error { get; }

  /// <summary>
  /// Creates a new instance of the Result class.
  /// </summary>
  /// <param name="data">The success value.</param>
  /// <param name="error">The error value.</param>
  /// <param name="success">Whether the operation was successful.</param>
  private Result(T? data, E? error, bool success = true) : base(success)
  {
    Data = data;
    Error = error;
  }

  /// <summary>
  /// Creates a successful result containing the specified value.
  /// </summary>
  /// <param name="data">The success value.</param>
  /// <returns>A successful result containing the specified value.</returns>
  public static Result<T, E> Ok(T data) => new(data, default);

  /// <summary>
  /// Creates a failed result containing the specified error.
  /// </summary>
  /// <param name="error">The error.</param>
  /// <returns>A failed result containing the specified error.</returns>
  public static Result<T, E> Err(E error) => new(default, error, false);

  /// <summary>
  /// Returns the success value if the result is successful; otherwise throws an exception.
  /// </summary>
  /// <returns>The success value.</returns>
  /// <exception cref="Exception">Thrown when the result is not successful.</exception>
  public T Unwrap()
  {
    if (!IsSuccess || Data == null)
      throw new Exception("Trying to unwrap a failed result");
    return Data;
  }

  /// <summary>
  /// Returns the error if the result is not successful; otherwise throws an exception.
  /// </summary>
  /// <returns>The error.</returns>
  /// <exception cref="Exception">Thrown when the result is successful.</exception>
  public E UnwrapErr()
  {
    if (IsSuccess || Error == null)
      throw new Exception("Trying to unwrap an error from a successful result");
    return Error;
  }

  /// <summary>
  /// Returns the success value if the result is successful; otherwise returns the specified default value.
  /// </summary>
  /// <param name="defaultValue">The default value to return if the result is not successful.</param>
  /// <returns>The success value or the default value.</returns>
  public T UnwrapOr(T defaultValue)
  {
    if (!IsSuccess || Data == null)
      return defaultValue;
    return Data;
  }

  /// <summary>
  /// Returns the success value if the result is successful; otherwise returns the default value for type T.
  /// </summary>
  /// <returns>The success value or the default value for type T.</returns>
  public T? UnwrapOrDefault()
  {
    if (!IsSuccess || Data == null)
      return default;
    return Data;
  }

  /// <summary>
  /// Applies a mapping function to the success value if the result is successful; otherwise returns a new failed result with the same error.
  /// </summary>
  /// <typeparam name="U">The type of the mapped success value.</typeparam>
  /// <param name="mapper">The mapping function to apply to the success value.</param>
  /// <returns>A new result with the mapped success value or the same error.</returns>
  public Result<U, E> Map<U>(Func<T, U> mapper)
  {
    if (!IsSuccess || Data == null)
      return Result<U, E>.Err(Error!);
    return Result<U, E>.Ok(mapper(Data));
  }

  /// <summary>
  /// Applies a mapping function to the error if the result is not successful; otherwise returns a new successful result with the same value.
  /// </summary>
  /// <typeparam name="F">The type of the mapped error.</typeparam>
  /// <param name="mapper">The mapping function to apply to the error.</param>
  /// <returns>A new result with the same success value or the mapped error.</returns>
  public Result<T, F> MapErr<F>(Func<E, F> mapper) where F : Exception
  {
    if (!IsSuccess)
      return Result<T, F>.Err(mapper(Error!));
    return Result<T, F>.Ok(Data!);
  }

  /// <summary>
  /// Applies a function that returns a result to the success value if the result is successful; otherwise returns a new failed result with the same error.
  /// </summary>
  /// <typeparam name="U">The type of the success value of the returned result.</typeparam>
  /// <param name="func">The function to apply to the success value.</param>
  /// <returns>The result of applying the function to the success value or a new failed result with the same error.</returns>
  public Result<U, E> AndThen<U>(Func<T, Result<U, E>> func)
  {
    if (!IsSuccess)
      return Result<U, E>.Err(Error!);
    return func(Data!);
  }

  /// <summary>
  /// Returns the result of applying one of two functions depending on whether the result is successful.
  /// </summary>
  /// <typeparam name="R">The return type of both functions.</typeparam>
  /// <param name="success">The function to apply if the result is successful.</param>
  /// <param name="failure">The function to apply if the result is not successful.</param>
  /// <returns>The result of applying the appropriate function.</returns>
  public R Match<R>(Func<T, R> success, Func<E, R> failure) =>
      IsSuccess ? success(Data!) : failure(Error!);

  /// <summary>
  /// Converts this result to an Option, discarding any error information.
  /// </summary>
  /// <returns>An Option containing the success value if the result is successful; otherwise an empty Option.</returns>
  public Option<T> ToOption() =>
      IsSuccess && Data != null ? Option<T>.Some(Data) : Option<T>.None();
}