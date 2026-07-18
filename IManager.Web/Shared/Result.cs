namespace IManager.Web.Shared;

public class Result
{
    public bool Succeeded { get; }
    public IEnumerable<string> Errors { get; }

    private Result(bool succeeded, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public Result(IEnumerable<string> errors)
    {
        Succeeded = !errors.Any();

        Errors = errors;
    }

    public static Result Ok() => new(true, []);
    public static Result Fail(string error) => new(false, [error]);
    public static Result Fail(IEnumerable<string> errors) => new(false, errors);
}