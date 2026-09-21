using Ivr.Domain.Privacy;

namespace Ivr.Dsar;

public sealed record DsarRequest(string OrderCode, string RequestRef, bool Execute)
{
    public static DsarRequest Parse(IReadOnlyList<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        for (int index = 0; index < arguments.Count; index++)
        {
            string option = arguments[index];
            string value;
            switch (option)
            {
                case "--execute":
                case "--subject-verified":
                    value = "true";
                    break;
                case "--order-code":
                case "--request-ref":
                case "--confirm-order":
                    if (++index >= arguments.Count || arguments[index].StartsWith("--", StringComparison.Ordinal))
                    {
                        throw new ArgumentException("An option value is missing.", nameof(arguments));
                    }

                    value = arguments[index];
                    break;
                default:
                    throw new ArgumentException("Unknown option; use --help.", nameof(arguments));
            }

            if (!values.TryAdd(option, value))
            {
                throw new ArgumentException("Duplicate option.", nameof(arguments));
            }
        }

        string order = RequiredReference(values, "--order-code");
        string request = RequiredReference(values, "--request-ref");
        bool execute = values.ContainsKey("--execute");
        if (execute && (!values.ContainsKey("--subject-verified")
            || !values.TryGetValue("--confirm-order", out string? confirmed)
            || !string.Equals(order, confirmed, StringComparison.Ordinal)))
        {
            throw new ArgumentException("Execution requires --subject-verified and --confirm-order matching --order-code exactly.", nameof(arguments));
        }

        if (!execute && (values.ContainsKey("--confirm-order") || values.ContainsKey("--subject-verified")))
        {
            throw new ArgumentException("Confirmation flags require explicit --execute.", nameof(arguments));
        }

        return new DsarRequest(order, request, execute);
    }

    private static string RequiredReference(Dictionary<string, string> values, string key)
    {
        if (!values.TryGetValue(key, out string? value) || value.Length is < 1 or > 128
            || value.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not '-' and not '_' and not '.' and not ':'))
        {
            throw new ArgumentException("A single, non-empty order code and request reference are required.", nameof(values));
        }

        PiiGuard.EnsureSafeText(value);
        return value;
    }
}
