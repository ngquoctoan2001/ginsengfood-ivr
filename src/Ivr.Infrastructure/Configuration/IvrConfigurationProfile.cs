using Microsoft.Extensions.Configuration;

namespace Ivr.Infrastructure.Configuration;

/// <summary>
/// Resolves the named configuration profile a host was asked to layer on top of its
/// <c>appsettings.{Environment}.json</c> (W-0203).
/// <para>
/// The problem this solves is written down in <c>Invoke-LocalE2E.ps1</c>: the worker ships with
/// every loop disabled, which is the correct default, and so every rehearsal of the full pipeline
/// has re-derived the switch-on configuration by hand as a wall of environment variables. A wall
/// of environment variables cannot be reviewed, cannot be diffed, and cannot be pointed at in an
/// evidence pack. A file can.
/// </para>
/// <para>
/// A profile is a file, not an environment. The host environment stays whatever it was —
/// <c>Development</c>, normally — so everything that keys off the environment name, including the
/// non-production developer surface and the developer tokens, behaves exactly as it did before.
/// The profile only adds settings, and environment variables still sit above it.
/// </para>
/// </summary>
public static class IvrConfigurationProfile
{
    /// <summary>The setting, normally supplied as the environment variable of the same name.</summary>
    public const string SettingName = "IVR_CONFIG_PROFILE";

    private const int MaximumNameLength = 32;

    /// <summary>
    /// The file name for <paramref name="profileName"/>, or <see langword="null"/> when no profile
    /// was requested.
    /// </summary>
    /// <param name="profileName">The requested profile, e.g. <c>LocalMockE2E</c>.</param>
    /// <param name="environmentName">The host environment name, e.g. <c>Development</c>.</param>
    /// <exception cref="InvalidOperationException">
    /// The name is malformed, or the deployment is not one where a profile may be layered at all.
    /// Both throw rather than being ignored: a profile that silently fails to load produces a host
    /// that looks configured and is not, and the entire point of the file is that the configuration
    /// it carries is the configuration that ran.
    /// </exception>
    public static string? ResolveFileName(string? profileName, string? environmentName)
    {
        if (string.IsNullOrWhiteSpace(profileName))
        {
            return null;
        }

        string normalized = profileName.Trim();

        // Letters and digits only, and never a separator. The name becomes part of a file name, so
        // a name able to carry '/', '\' or ".." would be a way to make the host read a file the
        // deployment never intended to ship.
        if (normalized.Length > MaximumNameLength
            || !normalized.All(char.IsAsciiLetterOrDigit))
        {
            throw new InvalidOperationException(
                $"{SettingName} must be 1-{MaximumNameLength} ASCII letters or digits.");
        }

        // The same allowlist that guards the developer surface, for the same reason and with the
        // same failure mode: a deployment that invents an environment name is refused rather than
        // admitted. A profile exists to turn loops on, and the loops it turns on place calls.
        if (!NonProductionSurface.IsNonProductionEnvironment(environmentName))
        {
            throw new InvalidOperationException(
                $"{SettingName} may only be applied in a non-production environment.");
        }

        return $"appsettings.Profile.{normalized}.json";
    }

    /// <summary>
    /// The file name for the profile named by <see cref="SettingName"/> in
    /// <paramref name="configuration"/>, or <see langword="null"/> when none is requested.
    /// <para>
    /// A distinct name rather than an overload of <see cref="ResolveFileName(string?, string?)"/>:
    /// the two differ only in the first parameter, and both accept null there, so an overload
    /// pair would be ambiguous at every call site that passes one.
    /// </para>
    /// </summary>
    public static string? ResolveConfiguredFileName(
        IConfiguration configuration,
        string? environmentName)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return ResolveFileName(configuration[SettingName], environmentName);
    }
}
