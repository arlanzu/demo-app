namespace DemoApp.Application.Contracts;

/// <summary>
/// Represents the outcome of a contact submission request.
/// </summary>
public sealed record ContactSubmissionResult(bool IsSuccess, bool IsDuplicate)
{
    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful submission result.</returns>
    public static ContactSubmissionResult Success()
    {
        return new ContactSubmissionResult(true, false);
    }

    /// <summary>
    /// Creates a duplicate result.
    /// </summary>
    /// <returns>A duplicate submission result.</returns>
    public static ContactSubmissionResult Duplicate()
    {
        return new ContactSubmissionResult(false, true);
    }
}
