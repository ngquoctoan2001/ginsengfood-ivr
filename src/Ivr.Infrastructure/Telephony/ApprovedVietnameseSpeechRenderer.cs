using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Domain.Scripts;
using Ivr.Infrastructure.Speech;

namespace Ivr.Infrastructure.Telephony;

public sealed class ApprovedVietnameseSpeechRenderer(
    IScriptRegistry scriptRegistry,
    IScriptPreviewRenderer previewRenderer,
    RegionalVoiceMap regionalVoices) : ISpeechRenderer
{
    public async ValueTask<RenderedSpeech> RenderAsync(
        PrivacySafeOrderSummary summary,
        string scriptTemplateId,
        string scriptVersion,
        ExecutionMode executionMode,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(summary);
        ApprovedScript? approved = await scriptRegistry.TryGetApproved(
            scriptTemplateId,
            scriptVersion,
            executionMode,
            cancellationToken).ConfigureAwait(false);
        if (approved is null)
        {
            throw new InvalidOperationException(
                "The requested script version is not approved for the execution mode.");
        }

        // The renderer resolves the region itself, so only the FALLBACK has to be shared: if the
        // two fell back independently, an unrecognised delivery area could get a Southern voice
        // reading a Northern "nghìn", and nothing would fail to reveal it.
        ScriptRenderOptions renderOptions = ScriptRenderOptions.Create(
            ScriptRenderOptions.Default.MaximumSpokenItems,
            ScriptRenderOptions.Default.MaximumCharacters,
            ScriptRenderOptions.Default.WordsPerMinute,
            regionalVoices.FallbackRegion);

        ScriptPreview preview = previewRenderer.Render(approved, summary, renderOptions);
        int collapsed = Math.Max(
            0,
            summary.Items.Length - renderOptions.MaximumSpokenItems);
        return new RenderedSpeech(
            preview.ScriptReference,
            preview.ExactText,
            preview.ContentHash,
            summary.Locale,
            preview.EstimatedDuration,
            collapsed,
            "FAKE_TEXT_ONLY");
    }
}
