using Ivr.Domain.Ports;
using Ivr.Infrastructure.Persistence.Entities;

namespace Ivr.Infrastructure.Repositories;

public sealed record RawDispositionSignal(
    SimProviderDisposition? Disposition,
    string? DtmfKey,
    bool IsCapacity);

public interface IRawEventRepository
{
    public RawDispositionSignal Parse(RawCallEventEntity rawEvent);
}

public sealed class RawEventRepository : IRawEventRepository
{
    public RawDispositionSignal Parse(RawCallEventEntity rawEvent)
    {
        ArgumentNullException.ThrowIfNull(rawEvent);
        string status = new(
            rawEvent.RawCallStatus
                .Trim()
                .Where(char.IsAsciiLetterOrDigit)
                .Select(char.ToUpperInvariant)
                .ToArray());
        if (status is "CAPACITY" or "CAPACITYEXCEPTION")
        {
            return new RawDispositionSignal(null, null, true);
        }

        SimProviderDisposition? disposition = status switch
        {
            "ANSWERED" => SimProviderDisposition.Answered,
            "RINGTIMEOUT" => SimProviderDisposition.RingTimeout,
            "BUSY" => SimProviderDisposition.Busy,
            "REJECTED" => SimProviderDisposition.Rejected,
            "UNREACHABLE" => SimProviderDisposition.Unreachable,
            "INVALIDDESTINATION" or "INVALIDPHONE" =>
                SimProviderDisposition.InvalidDestination,
            "DROPPED" => SimProviderDisposition.Dropped,
            "NETWORKERROR" => SimProviderDisposition.NetworkError,
            "SIMERROR" => SimProviderDisposition.SimError,
            "AUDIOERROR" => SimProviderDisposition.AudioError,
            "DTMFERROR" => SimProviderDisposition.DtmfError,
            _ => null,
        };
        return new RawDispositionSignal(
            disposition,
            SanitizeDtmf(rawEvent.RawDtmf),
            false);
    }

    private static string? SanitizeDtmf(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim() switch
        {
            "1" => "1",
            "0" => "0",
            _ => "INVALID",
        };
    }
}
