using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Persistence.Security;
using Ivr.Infrastructure.Scheduling;
using Ivr.Infrastructure.Telephony;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests.Telephony;

/// <summary>
/// PD-01.1 through PD-01.5. The production dial path: the carrier-facing options, the vault that
/// turns an authorised token into a destination, and the composition root that picks between the
/// lab and the trunk.
/// <para>
/// The property worth pinning hardest is the ordering one. A customer number exists in the clear
/// for exactly one dial, so a token the ledger already refused must never be decrypted at all -
/// otherwise every refusal path becomes another place a number briefly lives.
/// </para>
/// </summary>
public sealed class SipTrunkProductionDialTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 16, 9, 0, 0, TimeSpan.Zero);

    private const string CarrierHost = "sip.carrier.example.vn";

    private static SipTrunkOptions ValidOptions(
        SipTrunkNumberFormat format = SipTrunkNumberFormat.E164Plus) => new()
        {
            Enabled = true,
            ExecutionMode = ExecutionModes.ProductionReal,
            ProviderName = "CARRIER",
            Environment = "production",
            TrunkEndpoint = "carrier-trunk",
            CarrierSipHost = CarrierHost,
            OutboundCallerId = "02873001234",
            NumberFormat = format,
            ContractedChannels = 8,
            MaxCallStartsPerSecond = 2,
            DtmfMode = SipTrunkDtmfModes.Rfc2833,
        };

    private static DialTokenResolutionRequest Request(
        string attemptId = "attempt-1",
        int maxResolves = 3,
        string token = "enc:prod-aes:QUJDRA",
        string? directPhoneE164 = null) =>
        new(
            DialTokenReference.Create(token, Now.AddMinutes(30)),
            AttemptId.Create(attemptId),
            TaskId.Create("TASK-297-0001"),
            maxResolves,
            directPhoneE164);

    /// <summary>Records whether it was asked, and what it was asked for.</summary>
    private sealed class RecordingProtector(string reveals) : IOpaqueValueProtector
    {
        public int UnprotectCalls { get; private set; }

        public string Protect(string purpose, string plaintext) => plaintext;

        public string Unprotect(string purpose, string ciphertext)
        {
            UnprotectCalls++;
            return reveals;
        }
    }

    private sealed class ThrowingProtector : IOpaqueValueProtector
    {
        internal const string SecretBearingMessage =
            "key kms://ring/key-7 failed decrypting enc:prod-aes:QUJDRA";

        public string Protect(string purpose, string plaintext) => plaintext;

        public string Unprotect(string purpose, string ciphertext) =>
            throw new InvalidOperationException(SecretBearingMessage);
    }

    private static ProductionDialTokenVault Vault(
        IOpaqueValueProtector protector,
        SipTrunkOptions? options = null,
        IDialTokenResolveLedger? ledger = null) =>
        new(
            Options.Create(options ?? ValidOptions()),
            protector,
            ledger ?? new DialTokenResolveLedger());

    // ---------------------------------------------------------------- vault

    /// <summary>
    /// The ordering property. A refused token is refused before the protector is consulted, so a
    /// dial that will not happen never materialises a number to not happen with.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TRUNK-DIAL-01")]
    public async Task ARefusedTokenIsNeverDecrypted()
    {
        RecordingProtector protector = new("+84912345678");
        ProductionDialTokenVault vault = Vault(protector);

        // maxResolves 0 states no ceiling, which the ledger refuses rather than treats as unlimited.
        DialTokenRefusedException refused = await Assert.ThrowsAsync<DialTokenRefusedException>(
            async () => await vault.ResolveAsync(Request(maxResolves: 0), Now, CancellationToken.None));

        Assert.Equal(DialTokenRefusalCodes.CeilingMissing, refused.RefusalCode);
        Assert.Equal(0, protector.UnprotectCalls);
    }

    /// <summary>An authorised token becomes a dial address the trunk can route.</summary>
    [Fact]
    [Trait("TestId", "UT-TRUNK-DIAL-02")]
    public async Task AnAuthorisedTokenResolvesToACarrierDestination()
    {
        RecordingProtector protector = new("+84912345678");
        ProductionDialTokenVault vault = Vault(protector);

        DialAuthorization authorization = await vault.ResolveAsync(
            Request(),
            Now,
            CancellationToken.None);

        Assert.Equal(
            $"sip:+84912345678@{CarrierHost}",
            authorization.RevealToTrustedGateway());
        Assert.Equal(1, protector.UnprotectCalls);
    }

    /// <summary>
    /// Every spelling Module 3 might have protected reaches the same dialled number. The three are
    /// all in use in Vietnamese order data and none of them is wrong to receive.
    /// </summary>
    [Theory]
    [InlineData("+84912345678")]
    [InlineData("84912345678")]
    [InlineData("0912345678")]
    [InlineData("091 234 5678")]
    [Trait("TestId", "UT-TRUNK-DIAL-03")]
    public async Task EveryAcceptedSpellingDialsTheSameNumber(string revealed)
    {
        ProductionDialTokenVault vault = Vault(new RecordingProtector(revealed));

        DialAuthorization authorization = await vault.ResolveAsync(
            Request(),
            Now,
            CancellationToken.None);

        Assert.Equal(
            $"sip:+84912345678@{CarrierHost}",
            authorization.RevealToTrustedGateway());
    }

    /// <summary>
    /// The carrier decides the wire format, so the same number leaves three ways. A mismatch here
    /// fails as "calls never connect", which is why it is configuration and why it is asserted.
    /// </summary>
    [Theory]
    [InlineData(SipTrunkNumberFormat.E164Plus, "+84912345678")]
    [InlineData(SipTrunkNumberFormat.E164NoPlus, "84912345678")]
    [InlineData(SipTrunkNumberFormat.National, "0912345678")]
    [Trait("TestId", "UT-TRUNK-DIAL-04")]
    public async Task TheConfiguredFormatDecidesWhatGoesOnTheWire(
        SipTrunkNumberFormat format,
        string expected)
    {
        ProductionDialTokenVault vault = Vault(
            new RecordingProtector("0912345678"),
            ValidOptions(format));

        DialAuthorization authorization = await vault.ResolveAsync(
            Request(),
            Now,
            CancellationToken.None);

        Assert.Equal($"sip:{expected}@{CarrierHost}", authorization.RevealToTrustedGateway());
    }

    /// <summary>
    /// A protector failure surfaces as a refusal to dial, and carries none of what the protector
    /// said. Key identifiers and ciphertext both turn up in provider exception messages, and this
    /// one travels up into dispatch logging.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TRUNK-DIAL-05")]
    public async Task AProtectorFailureDoesNotCarryTheProtectorsMessage()
    {
        ProductionDialTokenVault vault = Vault(new ThrowingProtector());

        InvalidOperationException failure = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await vault.ResolveAsync(Request(), Now, CancellationToken.None));

        Assert.DoesNotContain("kms://", failure.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("enc:prod-aes", failure.Message, StringComparison.Ordinal);
        Assert.Null(failure.InnerException);
    }

    /// <summary>
    /// A decrypted value that is not a usable number is refused rather than dialled. Dialling a
    /// mangled token bills a call to whoever the surviving digits happen to reach.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("849123456789012")]
    [InlineData("84 91234 +5678")]
    [InlineData("not-a-number")]
    [Trait("TestId", "UT-TRUNK-DIAL-06")]
    public async Task AnUnusableDestinationIsRefusedRatherThanDialled(string revealed)
    {
        ProductionDialTokenVault vault = Vault(new RecordingProtector(revealed));

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await vault.ResolveAsync(Request(), Now, CancellationToken.None));
    }

    /// <summary>A disabled trunk does not dial, whatever the token says.</summary>
    [Fact]
    [Trait("TestId", "UT-TRUNK-DIAL-07")]
    public async Task ADisabledTrunkRefusesBeforeAnythingElse()
    {
        SipTrunkOptions disabled = ValidOptions();
        disabled.Enabled = false;
        RecordingProtector protector = new("+84912345678");

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await Vault(protector, disabled)
                .ResolveAsync(Request(), Now, CancellationToken.None));

        Assert.Equal(0, protector.UnprotectCalls);
    }

    // -------------------------------------------------------------- options

    private static SipTrunkOptionsValidator Validator() => new();

    /// <summary>
    /// An absent section asserts nothing. The production trunk is configured after the carrier
    /// document arrives, and until then a deployment must still start.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TRUNK-OPT-01")]
    public void ADisabledTrunkValidatesWithoutAssertingAnything()
    {
        SipTrunkOptions blank = new();

        Assert.True(Validator().Validate(null, blank).Succeeded);
    }

    /// <summary>A complete carrier configuration is accepted.</summary>
    [Fact]
    [Trait("TestId", "UT-TRUNK-OPT-02")]
    public void ACompleteCarrierConfigurationIsAccepted()
    {
        Assert.True(Validator().Validate(null, ValidOptions()).Succeeded);
    }

    /// <summary>
    /// The trunk serves the production mode alone. Answering to MOCK or the softphone lab would put
    /// a billable carrier route behind guards written for a destination that cannot be billed.
    /// </summary>
    [Theory]
    [InlineData(ExecutionModes.Mock)]
    [InlineData(ExecutionModes.LabRealSim)]
    [Trait("TestId", "UT-TRUNK-OPT-03")]
    public void TheTrunkRefusesEveryModeButProduction(string mode)
    {
        SipTrunkOptions options = ValidOptions();
        options.ExecutionMode = mode;

        Assert.True(Validator().Validate(null, options).Failed);
    }

    /// <summary>
    /// The endpoint name and carrier host are concatenated into a dial string, so configuration
    /// that carries routing punctuation is refused at startup rather than at originate.
    /// </summary>
    [Theory]
    [InlineData("carrier/trunk", CarrierHost)]
    [InlineData("carrier trunk", CarrierHost)]
    [InlineData("carrier-trunk", "sip:sip.carrier.example.vn")]
    [InlineData("carrier-trunk", "sip.carrier.example.vn:5060")]
    [InlineData("carrier-trunk", "sip.carrier.example.vn/route")]
    [InlineData("carrier-trunk", "user@sip.carrier.example.vn")]
    [Trait("TestId", "UT-TRUNK-OPT-04")]
    public void RoutingPunctuationInTheEndpointOrHostIsRefused(string endpoint, string host)
    {
        SipTrunkOptions options = ValidOptions();
        options.TrunkEndpoint = endpoint;
        options.CarrierSipHost = host;

        Assert.True(Validator().Validate(null, options).Failed);
    }

    /// <summary>
    /// Opening calls faster than the trunk can hold them only buys carrier-side rejections, which
    /// arrive looking like network faults.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TRUNK-OPT-05")]
    public void CallsPerSecondCannotExceedTheChannelsContracted()
    {
        SipTrunkOptions options = ValidOptions();
        options.ContractedChannels = 4;
        options.MaxCallStartsPerSecond = 8;

        Assert.True(Validator().Validate(null, options).Failed);
    }

    /// <summary>
    /// A DTMF mode outside the three this adapter knows is refused. The failure it prevents is the
    /// quietest one on the path: keypresses that are simply never delivered.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TRUNK-OPT-06")]
    public void AnUnknownDtmfModeIsRefused()
    {
        SipTrunkOptions options = ValidOptions();
        options.DtmfMode = "auto";

        Assert.True(Validator().Validate(null, options).Failed);
    }

    /// <summary>An enabled trunk that grants no channels cannot place a call.</summary>
    [Fact]
    [Trait("TestId", "UT-TRUNK-OPT-07")]
    public void AnEnabledTrunkMustGrantAtLeastOneChannel()
    {
        SipTrunkOptions options = ValidOptions();
        options.ContractedChannels = 0;

        Assert.True(Validator().Validate(null, options).Failed);
    }

    /// <summary>Options never print their contents, whatever a logger is given.</summary>
    [Fact]
    [Trait("TestId", "UT-TRUNK-OPT-08")]
    public void OptionsDoNotPrintTheirContents()
    {
        string printed = ValidOptions().ToString();

        Assert.Equal("[REDACTED_SIP_TRUNK_OPTIONS]", printed);
        Assert.DoesNotContain(CarrierHost, printed, StringComparison.Ordinal);
    }


    // ------------------------------------- W-0310 option B: the number, sent outright

    /// <summary>
    /// Module 3 sent the number, so nothing is decrypted and the protector is never asked.
    /// </summary>
    /// <remarks>
    /// The protector here is the one whose every method throws with a secret-bearing message. If
    /// the vault touched it at all this test would fail loudly rather than quietly taking the old
    /// path, which is the failure worth catching: a deployment that has moved to option B but is
    /// still decrypting is one that still needs the key store option B was chosen to remove.
    /// </remarks>
    [Fact]
    [Trait("TestId", "UT-TRUNK-DIAL-08")]
    public async Task ANumberSentOutrightIsDialledWithoutTouchingTheProtector()
    {
        ProductionDialTokenVault vault = Vault(new ThrowingProtector());

        DialAuthorization authorization = await vault.ResolveAsync(
            Request(directPhoneE164: "+84912345678"),
            Now,
            CancellationToken.None);

        Assert.Equal(
            $"sip:+84912345678@{CarrierHost}",
            authorization.RevealToTrustedGateway());
    }

    /// <summary>
    /// The ordering property holds for option B too: the ledger refuses before the number is read.
    /// </summary>
    /// <remarks>
    /// <c>UT-TRUNK-DIAL-01</c> proves this for the token path, where it is obvious - not decrypting
    /// is visibly cheaper. Here there is nothing to decrypt, so reading the number first would cost
    /// nothing and look harmless, and the property would rot. It is the same property either way:
    /// a dial that will not happen never materialises a customer number in this process.
    /// </remarks>
    [Fact]
    [Trait("TestId", "UT-TRUNK-DIAL-09")]
    public async Task ARefusedTaskNeverReadsTheNumberEither()
    {
        ProductionDialTokenVault vault = Vault(new ThrowingProtector());

        DialTokenRefusedException refused = await Assert.ThrowsAsync<DialTokenRefusedException>(
            async () => await vault.ResolveAsync(
                Request(maxResolves: 0, directPhoneE164: "+84912345678"),
                Now,
                CancellationToken.None));

        Assert.Equal(DialTokenRefusalCodes.CeilingMissing, refused.RefusalCode);

        // The refusal names the ceiling and nothing else. A message carrying the number would put
        // it into dispatch logging, which is the one place this design spends effort keeping clear.
        Assert.DoesNotContain("84912345678", refused.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// A task sent the old way still dials the old way, with no branch and no configuration.
    /// </summary>
    /// <remarks>
    /// This is the cutover property. Module 3 moves when Module 3 is ready, and in between the two
    /// shapes arrive mixed - a task accepted yesterday with a token may be dialled today by a
    /// worker that already understands numbers. Both have to work in the same process at the same
    /// time, which is why the choice is made per task at dial time rather than per deployment.
    /// </remarks>
    [Fact]
    [Trait("TestId", "UT-TRUNK-DIAL-10")]
    public async Task WithoutANumberTheTokenPathIsUnchanged()
    {
        RecordingProtector protector = new("+84987654321");
        ProductionDialTokenVault vault = Vault(protector);

        DialAuthorization authorization = await vault.ResolveAsync(
            Request(directPhoneE164: null),
            Now,
            CancellationToken.None);

        Assert.Equal(
            $"sip:+84987654321@{CarrierHost}",
            authorization.RevealToTrustedGateway());
        Assert.Equal(1, protector.UnprotectCalls);
    }

    /// <summary>
    /// An empty or blank number is not a number, and falls through to the token rather than
    /// becoming a dial to nowhere.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [Trait("TestId", "UT-TRUNK-DIAL-11")]
    public async Task ABlankNumberIsNotTreatedAsOneSent(string blank)
    {
        RecordingProtector protector = new("+84987654321");
        ProductionDialTokenVault vault = Vault(protector);

        DialAuthorization authorization = await vault.ResolveAsync(
            Request(directPhoneE164: blank),
            Now,
            CancellationToken.None);

        Assert.Equal(
            $"sip:+84987654321@{CarrierHost}",
            authorization.RevealToTrustedGateway());
        Assert.Equal(1, protector.UnprotectCalls);
    }

    /// <summary>
    /// A number that is not a usable Vietnamese destination is refused, whichever way it arrived.
    /// </summary>
    /// <remarks>
    /// The schema already rejects these at intake, so reaching here means a row written before the
    /// pattern existed or a producer bypassing the contract. The vault refuses rather than trusting
    /// the layer above: dialling a mangled number bills a call to whoever the digits reach.
    /// </remarks>
    /// <para>
    /// <c>"0912345678x"</c> is deliberately absent: <c>VietnameseDestinationNumber.TryParse</c>
    /// strips formatting characters, so it dials <c>912345678</c> rather than refusing. That is
    /// right for the token path - <c>UT-TRUNK-DIAL-03</c> pins it, because Module 3 may have
    /// protected <c>"0912 345 678"</c> - and harmless for option B, where the schema pattern
    /// <c>^\+84[0-9]{9}$</c> rejects the trailing character at intake. Worth knowing if a repair
    /// script ever writes the column directly: the parser will not catch junk the schema would.
    /// </para>
    [Theory]
    [InlineData("+8491234")]
    [InlineData("not-a-number")]
    [InlineData("+841234567890123")]
    [Trait("TestId", "UT-TRUNK-DIAL-12")]
    public async Task AnUnusableNumberSentOutrightIsRefused(string unusable)
    {
        ProductionDialTokenVault vault = Vault(new ThrowingProtector());

        InvalidOperationException refused =
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await vault.ResolveAsync(
                    Request(directPhoneE164: unusable),
                    Now,
                    CancellationToken.None));

        Assert.DoesNotContain(unusable, refused.Message, StringComparison.Ordinal);
    }

    // ------------------------------------------------------- composition root

    private static ServiceCollection Compose(
        string executionMode,
        bool asteriskEnabled,
        bool trunkEnabled,
        bool useMockCapacity = false)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{AsteriskAriOptions.SectionName}:Enabled"] =
                    asteriskEnabled ? "true" : "false",
                [$"{SipTrunkOptions.SectionName}:Enabled"] = trunkEnabled ? "true" : "false",
            })
            .Build();

        ServiceCollection services = new();
        services.AddIvrScheduling(configuration, executionMode, useMockCapacity);
        return services;
    }

    private static Type? ImplementationFor<TService>(IServiceCollection services) =>
        services.LastOrDefault(descriptor => descriptor.ServiceType == typeof(TService))
            ?.ImplementationType;

    /// <summary>
    /// PD-01.4. The production branch selects the ARI dispatch gateway rather than the one that
    /// refuses to dial. Asserted on the registration rather than a built provider, because
    /// resolving it would need a database and the question here is only which type was chosen.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TRUNK-DI-01")]
    public void AConfiguredProductionTrunkSelectsTheAriDispatchGateway()
    {
        ServiceCollection services = Compose(
            IvrOptions.ProductionRealExecutionMode,
            asteriskEnabled: true,
            trunkEnabled: true);

        Assert.Equal(
            typeof(AsteriskSchedulerDispatchGateway),
            ImplementationFor<ISchedulerDispatchGateway>(services));
    }

    /// <summary>
    /// Both halves are required. A trunk without Asterisk, or Asterisk without a trunk, is a
    /// half-configured deployment, and the safe landing for one of those is the gateway that
    /// refuses rather than a dial path missing a piece.
    /// </summary>
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    [Trait("TestId", "UT-TRUNK-DI-02")]
    public void HalfAConfigurationFallsBackToTheGatewayThatRefuses(
        bool asteriskEnabled,
        bool trunkEnabled)
    {
        ServiceCollection services = Compose(
            IvrOptions.ProductionRealExecutionMode,
            asteriskEnabled,
            trunkEnabled);

        Assert.Equal(
            typeof(UnavailableSchedulerDispatchGateway),
            ImplementationFor<ISchedulerDispatchGateway>(services));
    }

    /// <summary>
    /// The lab is untouched by the production branch: a lab-mode deployment still lands on the ARI
    /// gateway through its own branch, and enabling a trunk section cannot divert it.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TRUNK-DI-03")]
    public void TheLabBranchStillWinsInLabMode()
    {
        ServiceCollection services = Compose(
            IvrOptions.LabRealSimExecutionMode,
            asteriskEnabled: true,
            trunkEnabled: true);

        Assert.Equal(
            typeof(AsteriskSchedulerDispatchGateway),
            ImplementationFor<ISchedulerDispatchGateway>(services));

        // The lab substitutes its own fingerprinting vault for the protector. Production must not,
        // and the next test is the other half of that pair.
        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IOpaqueValueProtector));
    }

    /// <summary>
    /// Production does not substitute the protector. The lab replaces it with its own irreversible
    /// fingerprinting vault; production takes whatever Platform registered, so that a deployment
    /// still holding <see cref="UnavailableOpaqueValueProtector"/> fails closed at the first
    /// resolve rather than dialling a number it could not protect.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TRUNK-DI-04")]
    public void ProductionDoesNotSubstituteThePlatformProtector()
    {
        ServiceCollection production = Compose(
            IvrOptions.ProductionRealExecutionMode,
            asteriskEnabled: true,
            trunkEnabled: true);

        Assert.DoesNotContain(
            production,
            descriptor => descriptor.ServiceType == typeof(LabDialTokenVault));
    }
    // --------------------------------- PD-02: the ceiling against the contract

    /// <summary>
    /// No carrier, nothing asserted. The same first rule the trunk validator has, for the same
    /// reason: a lab or MOCK deployment never configured a contract, and a check that compared
    /// against the resulting zero would refuse to start every developer machine in the project.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SCH-TRUNKCAP-01")]
    public void WithNoCarrierConfiguredTheCeilingIsNotComparedToAnything()
    {
        SipTrunkOptions absent = new();
        SchedulerOptions greedy = new() { MaxConcurrentDispatches = 256 };

        Assert.Empty(SchedulerTrunkCapacity.Failures(greedy, absent));
    }

    /// <summary>
    /// A worker told to hold more calls than the contract grants does not start.
    /// <para>
    /// This is the misconfiguration the runbook warns about twice and nothing prevented. Its
    /// symptom is the reason it is worth a startup refusal rather than a log line: the calls above
    /// the contract are rejected by the carrier, and carrier-side rejection presents as an
    /// intermittent network fault on the route - hardest to reproduce exactly when load is highest,
    /// and invisible to every test that does not involve a carrier, which is all of them.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SCH-TRUNKCAP-02")]
    public void AWorkerMayNotBeToldToHoldMoreCallsThanWereBought()
    {
        SipTrunkOptions trunk = ValidOptions();
        SchedulerOptions scheduler = new()
        {
            MaxConcurrentDispatches = 32,
            MaxCallStartsPerSecond = 1,
        };

        string failure = Assert.Single(SchedulerTrunkCapacity.Failures(scheduler, trunk));

        // Both numbers in the message. An operator reading this at three in the morning needs to
        // know which of the two to change, and a message naming only the one that was too large
        // invites them to raise the contract figure to match.
        Assert.Contains("32", failure, StringComparison.Ordinal);
        Assert.Contains("8", failure, StringComparison.Ordinal);
    }

    /// <summary>
    /// Equal is allowed, and one above is not. The boundary asserted from both sides, because a
    /// contract for eight channels is a contract for eight calls - refusing the eighth would make
    /// every deployment quietly buy one more than it uses.
    /// </summary>
    [Theory]
    [InlineData(8, 0)]
    [InlineData(9, 1)]
    [Trait("TestId", "UT-SCH-TRUNKCAP-03")]
    public void TheLastContractedChannelIsUsable(int ceiling, int expectedFailures)
    {
        SchedulerOptions scheduler = new()
        {
            MaxConcurrentDispatches = ceiling,
            MaxCallStartsPerSecond = 1,
        };

        Assert.Equal(
            expectedFailures,
            SchedulerTrunkCapacity.Failures(scheduler, ValidOptions()).Count);
    }

    /// <summary>
    /// The start rate is policed against the carrier's own rate and not against the channel count.
    /// <para>
    /// Separate limits, separately enforced by the carrier. A deployment can sit inside the
    /// channel ceiling all day and still be refused on the burst, and that failure only appears
    /// under load - it passes every quiet test, which is the shape of defect that reaches
    /// production.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SCH-TRUNKCAP-04")]
    public void HoldingEightChannelsIsNotPermissionToOpenEightInOneSecond()
    {
        SipTrunkOptions trunk = ValidOptions();
        SchedulerOptions scheduler = new()
        {
            MaxConcurrentDispatches = 8,
            MaxCallStartsPerSecond = 8,
        };

        string failure = Assert.Single(SchedulerTrunkCapacity.Failures(scheduler, trunk));

        Assert.Contains(
            nameof(SchedulerOptions.MaxCallStartsPerSecond),
            failure,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// The rule is wired, not merely written. Resolving the scheduler options runs it and the
    /// process refuses to come up.
    /// <para>
    /// Worth its own test because the wiring is the part that could silently not happen: this
    /// validator reads a second options object through <c>IOptionsMonitor</c>, and a validator
    /// that fails to resolve its dependency, or is never registered, is indistinguishable from one
    /// that passes. Built from the two sections alone rather than through
    /// <c>AddIvrScheduling</c>, which would want a database to resolve.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SCH-TRUNKCAP-05")]
    public void TheProcessRefusesToStartOnACeilingAboveTheContract()
    {
        ServiceCollection services = new();
        services.AddOptions<SipTrunkOptions>().Configure(options =>
        {
            SipTrunkOptions valid = ValidOptions();
            options.Enabled = valid.Enabled;
            options.ContractedChannels = valid.ContractedChannels;
            options.MaxCallStartsPerSecond = valid.MaxCallStartsPerSecond;
        });
        services.AddOptions<SchedulerOptions>().Configure(options =>
        {
            options.MaxConcurrentDispatches = 32;
            options.MaxCallStartsPerSecond = 1;
        });
        services.AddSingleton<IValidateOptions<SchedulerOptions>,
            SchedulerTrunkCapacityValidator>();

        using ServiceProvider provider = services.BuildServiceProvider();

        OptionsValidationException refused = Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<SchedulerOptions>>().Value);

        Assert.Contains(
            nameof(SchedulerOptions.MaxConcurrentDispatches),
            string.Join(' ', refused.Failures),
            StringComparison.Ordinal);
    }

    /// <summary>
    /// And the real composition root registers it, so the test above is about the deployment and
    /// not only about a service collection built inside a test.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SCH-TRUNKCAP-06")]
    public void TheSchedulerCompositionRegistersTheCapacityCheck()
    {
        ServiceCollection services = Compose(
            IvrOptions.ProductionRealExecutionMode,
            asteriskEnabled: true,
            trunkEnabled: true);

        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IValidateOptions<SchedulerOptions>)
                && descriptor.ImplementationType == typeof(SchedulerTrunkCapacityValidator));
    }
    /// <summary>
    /// PD-02. Raising the ceiling is done through configuration, so the configuration path is what
    /// has to carry the raised number.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every other test of the ceiling constructs <see cref="SchedulerOptions"/> directly, which
    /// proves the pump and proves nothing about the route the value actually travels in a
    /// deployment. The four SIP-05 options are bound one at a time by hand rather than by
    /// <c>Bind</c>, and a key that never reaches the object would look exactly like a deployment
    /// that had not been reconfigured yet: the worker starts, reports healthy, and holds one call.
    /// </para>
    /// <para>
    /// All four together, because they were added in one pass and a missing line in that pass is
    /// the likely defect. Values are deliberately unlike each other and unlike the defaults, so a
    /// key wired to the wrong field is a mismatch rather than a coincidence.
    /// </para>
    /// </remarks>
    [Fact]
    [Trait("TestId", "UT-SCH-CFG-01")]
    public void TheSipOptionsArriveFromConfiguration()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{SchedulerOptions.SectionName}:MaxConcurrentDispatches"] = "16",
                [$"{SchedulerOptions.SectionName}:MaxCallStartsPerSecond"] = "3",
                [$"{SchedulerOptions.SectionName}:DispatchDrainSeconds"] = "200",
                [$"{SchedulerOptions.SectionName}:ControllerLeaseSeconds"] = "300",
            })
            .Build();

        ServiceCollection services = new();
        services.AddIvrScheduling(
            configuration,
            IvrOptions.MockExecutionMode,
            useMockCapacity: true);

        using ServiceProvider provider = services.BuildServiceProvider();
        SchedulerOptions bound = provider.GetRequiredService<IOptions<SchedulerOptions>>().Value;

        Assert.Equal(16, bound.MaxConcurrentDispatches);
        Assert.Equal(3, bound.MaxCallStartsPerSecond);
        Assert.Equal(200, bound.DispatchDrainSeconds);
        Assert.Equal(300, bound.ControllerLeaseSeconds);
    }
}
