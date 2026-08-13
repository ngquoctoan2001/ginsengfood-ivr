using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Domain.Scripts;

namespace Ivr.Infrastructure.Telephony;

public sealed class ApprovedVietnameseSpeechRenderer(
    IScriptRegistry scriptRegistry,
    IScriptPreviewRenderer previewRenderer) : ISpeechRenderer
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

        ScriptPreview preview = previewRenderer.Render(approved, summary);
        int collapsed = Math.Max(
            0,
            summary.Items.Length - ScriptRenderOptions.Default.MaximumSpokenItems);
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
