using System.Collections.ObjectModel;
using Ivr.Domain.Privacy;

namespace Ivr.Domain.Errors;

/// <summary>
/// A known, privacy-safe failure that the API may expose through its stable
/// error envelope.
/// </summary>
public sealed class IvrFailureException : Exception
{
    public IvrFailureException(
        string errorCode,
        string safeMessage,
        IReadOnlyDictionary<string, string>? details = null)
        : base(safeMessage)
    {
        if (!IvrErrorCodes.IsDefined(errorCode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(errorCode),
                errorCode,
                "The code is not part of the API-06 stable catalog.");
        }

        PiiGuard.EnsureSafeText(safeMessage);
        Dictionary<string, string> safeDetails = new(StringComparer.Ordinal);

        foreach ((string field, string value) in
                 details ?? new Dictionary<string, string>())
        {
            PiiGuard.EnsureSafeField(field);
            PiiGuard.EnsureSafeText(value);
            safeDetails.Add(field, value);
        }

        ErrorCode = errorCode;
        Details = new ReadOnlyDictionary<string, string>(safeDetails);
    }

    public string ErrorCode { get; }

    public IReadOnlyDictionary<string, string> Details { get; }
}
