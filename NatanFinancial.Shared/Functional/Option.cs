namespace NatanFinancial.Shared.Functional;

/// <summary>
/// Represents an optional value that may or may not be present.
/// </summary>
public abstract class Option
{
  protected readonly bool HasValue;

  protected Option(bool hasValue)
  {
    HasValue = hasValue;
  }

  /// <summary>
  /// Returns true if the option contains a value, false otherwise.
  /// </summary>
  public bool IsSome => HasValue;

  /// <summary>
  /// Returns true if the option is empty, false otherwise.
  /// </summary>
  public bool IsNone => !HasValue;
}

/// <summary>
/// Represents an optional value of type T that may or may not be present.
/// </summary>
/// <typeparam name="T">The type of the optional value.</typeparam>
public class Option<T> : Option
{
  private readonly T? _value;

  private Option(T? value, bool hasValue) : base(hasValue)
  {
    _value = value;
  }

  /// <summary>
  /// Creates an Option containing the specified value.
  /// </summary>
  public static Option<T> Some(T value) =>
      value is null
          ? throw new ArgumentNullException(nameof(value), "Cannot create Some with null value. Use None instead.")
          : new Option<T>(value, true);

  /// <summary>
  /// Creates an empty Option.
  /// </summary>
  public static Option<T> None() => new Option<T>(default, false);

  /// <summary>
  /// Creates an Option from a nullable value.
  /// </summary>
  public static Option<T> FromNullable(T? value) =>
      value is null ? None() : Some(value);

  /// <summary>
  /// Returns the contained value if present, otherwise throws an exception.
  /// </summary>
  /// <exception cref="InvalidOperationException">Thrown when the option is empty.</exception>
  public T Unwrap()
  {
    if (!HasValue)
      throw new InvalidOperationException("Cannot unwrap None value");

    return _value!;
  }

  /// <summary>
  /// Returns the contained value if present, otherwise returns the provided default value.
  /// </summary>
  public T UnwrapOr(T defaultValue) =>
      HasValue ? _value! : defaultValue;

  /// <summary>
  /// Returns the contained value if present, otherwise returns the default value for type T.
  /// </summary>
  public T? UnwrapOrDefault() =>
      HasValue ? _value : default;

  /// <summary>
  /// Applies a mapping function to the contained value if present, otherwise returns None.
  /// </summary>
  public Option<U> Map<U>(Func<T, U> mapper) =>
      HasValue ? Option<U>.Some(mapper(_value!)) : Option<U>.None();

  /// <summary>
  /// Applies a function that returns an Option to the contained value if present, otherwise returns None.
  /// </summary>
  public Option<U> AndThen<U>(Func<T, Option<U>> mapper) =>
      HasValue ? mapper(_value!) : Option<U>.None();

  /// <summary>
  /// Returns the result of applying one of two functions depending on whether the option contains a value.
  /// </summary>
  public U Match<U>(Func<T, U> some, Func<U> none) =>
      HasValue ? some(_value!) : none();

  /// <summary>
  /// Returns the current option if it contains a value, otherwise returns the provided option.
  /// </summary>
  public Option<T> Or(Option<T> other) =>
      HasValue ? this : other;

  /// <summary>
  /// Returns the current option if it contains a value, otherwise returns the option produced by the provided function.
  /// </summary>
  public Option<T> OrElse(Func<Option<T>> otherProvider) =>
      HasValue ? this : otherProvider();

  /// <summary>
  /// Returns the current option if it contains a value and satisfies the predicate, otherwise returns None.
  /// </summary>
  public Option<T> Filter(Predicate<T> predicate) =>
      HasValue && predicate(_value!) ? this : None();

  /// <summary>
  /// Converts this Option to a Result with the specified error in case the Option is None.
  /// </summary>
  public Result<T, E> ToResult<E>(Func<E> errorProvider) where E : Exception =>
      HasValue ? Result<T, E>.Ok(_value!) : Result<T, E>.Err(errorProvider());

  /// <summary>
  /// Converts this Option to a Result with the specified error in case the Option is None.
  /// </summary>
  public Result<T, E> ToResult<E>(E error) where E : Exception =>
      HasValue ? Result<T, E>.Ok(_value!) : Result<T, E>.Err(error);
}