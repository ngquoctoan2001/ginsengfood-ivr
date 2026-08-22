using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Ivr.Domain.Privacy;
using Ivr.Domain.Speech;

namespace Ivr.Domain.Scripts;

/// <summary>
/// One piece of an approved template before any order values are substituted.
/// </summary>
/// <param name="Text">
/// Literal prose for a fixed piece; empty for a placeholder, whose text only exists per order.
/// </param>
public sealed record SpeechSegmentTemplate(
    int Ordinal,
    SpeechSegmentKind Kind,
    string Text,
    string? PlaceholderName);

public static class TargetV1SpeechPolicy
{
    public const string MockTemplateId = "SCRIPT-ORDER-CONFIRM";
    public const string MockTemplateVersion = "v3-test-approved";
    public const string CanonicalVietnameseTemplate =
        "Xin chào Quý khách. "
        + "Đây là cuộc gọi tự động để xác nhận đơn hàng từ Ginsengfood. "
        + "Quý khách có đơn hàng gồm {{items_spoken}}, "
        + "tổng tiền {{total_amount_display}}, giao đến {{delivery_area_short}}. "
        + "Bấm phím một để xác nhận đơn hàng, hoặc bấm phím không để hủy đơn hàng.";

    public const string PreviousMockTemplateVersion = "v2-test-approved";
    public const string PreviousVietnameseTemplate =
        "Xin chào {{customer_display_name}}. "
        + "Đây là cuộc gọi tự động để xác nhận đơn hàng từ Ginsengfood. "
        + "Anh/chị có đơn hàng gồm {{items_spoken}}, "
        + "tổng tiền {{total_amount_display}}, giao đến {{delivery_area_short}}. "
        + "Bấm phím một để xác nhận đơn hàng, hoặc bấm phím không để hủy đơn hàng.";

    public const string LegacyMockTemplateVersion = "v1-test-approved";
    public const string LegacyVietnameseTemplate =
        "Xin chào anh/chị {{customer_display_name}}. "
        + "Anh/chị có đơn {{order_code_short}} gồm {{items_spoken}}, "
        + "tổng tiền {{total_amount_display}}, giao đến {{delivery_area_short}}. "
        + "Bấm phím 1 để xác nhận đơn hàng, bấm phím 0 để hủy.";

    private static readonly FrozenSet<string> AllowedPlaceholders = new[]
    {
        "customer_display_name",
        "order_code_short",
        "items_spoken",
        "total_amount_display",
        "delivery_area_short",
        "program_display_name",
    }.ToFrozenSet(StringComparer.Ordinal);

    private static readonly FrozenSet<string> RequiredPlaceholders = new[]
    {
        "items_spoken",
        "total_amount_display",
        "delivery_area_short",
    }.ToFrozenSet(StringComparer.Ordinal);

    private static readonly FrozenSet<string> ProductionDecisionPlaceholders = new[]
    {
        "items_spoken",
        "delivery_area_short",
    }.ToFrozenSet(StringComparer.Ordinal);

    private static readonly Regex PlaceholderPattern = new(
        "\\{\\{(?<name>[a-z0-9_]+)\\}\\}",
        RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100));

    public static ImmutableArray<string> AllowedInputFields { get; } =
    [
        "customer_display_name",
        "order_code_short",
        "items[].public_name",
        "items[].quantity",
        "items[].unit_label",
        "total_amount",
        "currency",
        "delivery_area_short",
        "program_display_name",
        "locale",
        "pronunciation_hints",
    ];

    public static string ValidateTemplate(string templateText)
    {
        string normalized = ScriptTextGuard.Required(templateText, 2_000, nameof(templateText));
        if (normalized.Any(char.IsControl) || normalized.Contains('<') || normalized.Contains('>'))
        {
            throw new InvalidOperationException("Script templates cannot contain HTML or control characters.");
        }

        MatchCollection matches = PlaceholderPattern.Matches(normalized);
        FrozenSet<string> placeholders = matches
            .Select(match => match.Groups["name"].Value)
            .ToFrozenSet(StringComparer.Ordinal);

        if (matches.Count == 0
            || placeholders.Any(placeholder => !AllowedPlaceholders.Contains(placeholder))
            || RequiredPlaceholders.Any(required => !placeholders.Contains(required)))
        {
            throw new InvalidOperationException("Script template variables do not match the Target V1 whitelist.");
        }

        string remainder = PlaceholderPattern.Replace(normalized, string.Empty);
        if (remainder.Contains("{{", StringComparison.Ordinal)
            || remainder.Contains("}}", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Script template contains a malformed or unknown variable.");
        }

        bool containsConfirmInstruction =
            normalized.Contains("Bấm phím 1", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("Bấm phím một", StringComparison.OrdinalIgnoreCase);
        bool containsCancelInstruction =
            normalized.Contains("bấm phím 0", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("bấm phím không", StringComparison.OrdinalIgnoreCase);
        if (!containsConfirmInstruction
            || !containsCancelInstruction
            || normalized.Contains("phím 9", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Script template must preserve the fixed 1/0 confirmation instructions.");
        }

        PiiGuard.EnsureSafeText(remainder);
        return normalized;
    }

    /// <summary>
    /// Splits an approved template at its placeholder boundaries into the pieces a call is
    /// actually assembled from.
    /// <para>
    /// The prose between placeholders is identical for every order, so it can be recorded once
    /// per voice and replayed forever; only the placeholder values need a synthesizer. For the
    /// canonical v3 template that is four fixed pieces and three variable ones — the 68/32 split
    /// measured in W-0106 §4.6.
    /// </para>
    /// <para>
    /// Deriving the split here, from the same validated string the renderer substitutes into,
    /// is what keeps recorded audio and script wording from parting company: change one word of
    /// the template and every fixed piece downstream gets a new identity, so the old recording
    /// stops resolving instead of quietly playing the old wording.
    /// </para>
    /// </summary>
    public static ImmutableArray<SpeechSegmentTemplate> SegmentTemplate(string templateText)
    {
        string validTemplate = ValidateTemplate(templateText);
        var segments = ImmutableArray.CreateBuilder<SpeechSegmentTemplate>();
        int cursor = 0;
        int ordinal = 0;
        foreach (Match match in PlaceholderPattern.Matches(validTemplate))
        {
            if (match.Index > cursor)
            {
                string prose = validTemplate[cursor..match.Index];
                if (string.IsNullOrWhiteSpace(prose))
                {
                    // Two variables with only a space between them would produce a fixed piece
                    // that is a recording of a space. Rejecting the template is the honest
                    // answer; silently merging the space into a neighbour would move it into
                    // cached synthesized audio and change what that cache entry means.
                    throw new InvalidOperationException(
                        "Script template variables must be separated by spoken text, not whitespace alone.");
                }

                segments.Add(new SpeechSegmentTemplate(
                    ++ordinal,
                    SpeechSegmentKind.Fixed,
                    prose,
                    null));
            }

            segments.Add(new SpeechSegmentTemplate(
                ++ordinal,
                SpeechSegmentKind.Dynamic,
                string.Empty,
                match.Groups["name"].Value));
            cursor = match.Index + match.Length;
        }

        if (cursor < validTemplate.Length)
        {
            segments.Add(new SpeechSegmentTemplate(
                ++ordinal,
                SpeechSegmentKind.Fixed,
                validTemplate[cursor..],
                null));
        }

        return segments.ToImmutable();
    }

    /// <summary>
    /// Text hashes of every fixed piece of a template, in playback order. This is the exact list
    /// a media catalog has to cover, and the list a pre-render script has to produce.
    /// </summary>
    public static ImmutableArray<string> FixedSegmentHashes(string templateText) =>
    [
        .. SegmentTemplate(templateText)
            .Where(segment => segment.Kind == SpeechSegmentKind.Fixed)
            .Select(segment => SpeechSegment.ComputeTextHash(segment.Text)),
    ];

    public static bool UsesProductionDecisionFields(string templateText)
    {
        string validTemplate = ValidateTemplate(templateText);
        return PlaceholderPattern.Matches(validTemplate)
            .Select(match => match.Groups["name"].Value)
            .Any(ProductionDecisionPlaceholders.Contains);
    }
}
