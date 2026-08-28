namespace Ivr.Api.Auth;

/// <summary>
/// W-0122. The destination reference a flag mutation is scoped to.
/// <para>
/// It used to arrive as a claim minted by the mock permission handler. With console sessions gone
/// there is no claim to mint, so Module 3 sends it as a header alongside the actor.
/// </para>
/// </summary>
public static class FeatureFlagClaims
{
    public const string DestinationRefHeaderName = "X-Destination-Ref";
}
