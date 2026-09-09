using System.Collections.Immutable;
using Ivr.Domain.Confirmation;

namespace Ivr.Domain.Speech;

/// <summary>
/// The recorded clips available for values that are not numbers: product names and unit labels.
/// <para>
/// Both arrive from Module 3 as free text — <c>public_name</c> is bounded at 160 characters and
/// <c>unit_label</c> at 40, and neither is checked against a catalogue anywhere. That is harmless
/// while a synthesizer reads whatever string it is handed; under a recorded bank it means a
/// product Sales adds tomorrow is a call that cannot be read aloud. This type is where that gap
/// becomes visible instead of becoming silence.
/// </para>
/// </summary>
public sealed class RecordedSpeechCatalog
{
    private readonly ImmutableDictionary<string, SpeechNumberClip> items;
    private readonly ImmutableDictionary<string, SpeechNumberClip> units;

    private RecordedSpeechCatalog(
        ImmutableDictionary<string, SpeechNumberClip> items,
        ImmutableDictionary<string, SpeechNumberClip> units)
    {
        this.items = items;
        this.units = units;
    }

    /// <summary>
    /// Nothing recorded. Deliberately the state a misconfigured deployment lands in, because a
    /// composer holding it speaks no product name at all and <c>TryCompose</c> refuses the call
    /// outright — louder than quietly reading "đơn hàng gồm một sản phẩm" to a customer.
    /// </summary>
    public static RecordedSpeechCatalog Empty { get; } = new(
        ImmutableDictionary<string, SpeechNumberClip>.Empty,
        ImmutableDictionary<string, SpeechNumberClip>.Empty
            .WithComparers(StringComparer.OrdinalIgnoreCase));

    /// <summary>
    /// Bank C as the owner settled it on 2026-09-09: one unit, "hộp". Not an estimate — the
    /// repository agrees, since every other unit string in it (gói, chai, kg, túi, thùng) appears
    /// only inside test fixtures and no specification declares one.
    /// <para>
    /// It is one entry rather than none because <c>unit_label</c> is optional and free text in the
    /// contract — <c>maxLength: 40</c>, no enum — so nothing stops a different unit arriving. When
    /// one does it folds into "và N sản phẩm khác" under 3b rather than being read wrong, which is
    /// the intended failure but a quiet one: an order shipped in chai would lose its unit without
    /// anything going red.
    /// </para>
    /// </summary>
    public static IReadOnlyDictionary<string, string> SettledUnitClipIds { get; } =
        new Dictionary<string, string> { ["hộp"] = "unit-hop" };

    public static RecordedSpeechCatalog Create(
        IReadOnlyDictionary<string, string> itemClipIds,
        IReadOnlyDictionary<string, string> unitClipIds)
    {
        ArgumentNullException.ThrowIfNull(itemClipIds);
        ArgumentNullException.ThrowIfNull(unitClipIds);
        // Product names match exactly; unit labels ignore case. The two are not the same kind of
        // string. A product name is a proper noun where case can carry meaning, and quietly
        // serving one recording for two spellings is the kind of wrong a customer notices and a
        // test does not. A unit is a common noun whose case is only field formatting: "Hộp" and
        // "hộp" are the same Vietnamese word, and the repository already carries both spellings.
        // Neither is accent-folded, in either direction -- hộp, hợp and họp are three words.
        return new RecordedSpeechCatalog(
            Build(itemClipIds, StringComparer.Ordinal),
            Build(unitClipIds, StringComparer.OrdinalIgnoreCase));
    }

    public bool TryGetItem(string publicName, out SpeechNumberClip clip) =>
        items.TryGetValue(Key(publicName), out clip);

    public bool TryGetUnit(string unitLabel, out SpeechNumberClip clip) =>
        units.TryGetValue(Key(unitLabel), out clip);

    private static ImmutableDictionary<string, SpeechNumberClip> Build(
        IReadOnlyDictionary<string, string> clipIds,
        StringComparer comparer)
    {
        ImmutableDictionary<string, SpeechNumberClip>.Builder builder =
            ImmutableDictionary.CreateBuilder<string, SpeechNumberClip>(comparer);
        foreach ((string spoken, string clipId) in clipIds)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(spoken);
            ArgumentException.ThrowIfNullOrWhiteSpace(clipId);
            builder[Key(spoken)] = new SpeechNumberClip(clipId, spoken.Trim());
        }

        return builder.ToImmutable();
    }

    private static string Key(string value) => (value ?? string.Empty).Trim();
}

/// <summary>
/// Builds the clip run for the spoken item list, under the rule the owner approved on 2026-09-08
/// for <c>m8-16</c> item <c>3b</c>.
/// <para>
/// Products that have a clip are named. Everything else — products with no recording, a recorded
/// product whose unit is not recorded, and anything past the spoken-item cap — folds into the
/// "và N sản phẩm khác" phrase the renderer has always used for overflow, so this reuses a
/// mechanism that already exists and already has tests rather than inventing a second one.
/// </para>
/// <para>
/// And a floor: if no item can be named, <see cref="TryCompose"/> returns <c>false</c> instead of
/// producing "đơn hàng gồm một sản phẩm". A call that reads that sentence confirms nothing, and
/// phoning a customer to say nothing is worse than not phoning. The caller routes those to admin
/// review rather than dialling.
/// </para>
/// </summary>
public static class RecordedSpeechComposer
{
    /// <summary>Joins the last two named products: "a và b".</summary>
    public static SpeechNumberClip AndClip { get; } = new("join-and", "và");

    /// <summary>Closes the fold: "và hai sản phẩm khác".</summary>
    public static SpeechNumberClip OtherProductsClip { get; } = new("join-other-products", "sản phẩm khác");

    /// <summary>
    /// The clip for a delivery area, or <c>false</c> when no provincial unit can be identified.
    /// <para>
    /// Bank E differs from the product bank in one way that matters: its contents are fully
    /// determined by code today. <see cref="AreaClipBank"/> enumerates every clip it will ever
    /// need, because the area is spoken as a province and the province table is compiled in — no
    /// list has to arrive from anywhere first.
    /// </para>
    /// <para>
    /// Returning <c>false</c> rather than an empty run is the point. The words around the value
    /// are recorded fixed segments — <c>", giao đến "</c> before it and <c>". Bấm phím một…"</c>
    /// after — so a missing area does not degrade quietly, it plays "giao đến. Bấm phím một để xác
    /// nhận" and asks a customer to confirm a delivery to nowhere. The caller has to decide, and
    /// what it should decide is still open: refuse the call the way an unnameable item list does,
    /// or substitute a generic value, which stays inside the approved template because it replaces
    /// the placeholder's value rather than the template itself. That choice is coupled to 4c —
    /// whether Sales still emits district-only areas — because refusing them all would drop every
    /// such order rather than a rare one.
    /// </para>
    /// </summary>
    public static bool TryComposeArea(
        string? deliveryAreaShort,
        out SpeechNumberClip clip)
    {
        string? province = DeliveryRegionResolver.TryResolveProvinceName(deliveryAreaShort);
        if (province is null)
        {
            clip = default;
            return false;
        }

        clip = AreaClip(province);
        return true;
    }

    /// <summary>
    /// Every clip bank E will ever hold: one per current provincial unit, in the voice that unit
    /// selects. Generated rather than written down, for the same reason the 0..99 script is —
    /// a hand-kept copy of a compiled table is a copy that can drift from it.
    /// </summary>
    public static ImmutableArray<SpeechNumberClip> AreaClipBank() =>
        DeliveryRegionResolver.ProvinceRegionTable.Keys
            .Select(DeliveryRegionResolver.TryResolveProvinceName)
            .Where(name => name is not null)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .Select(name => AreaClip(name!))
            .ToImmutableArray();

    private static SpeechNumberClip AreaClip(string province) =>
        new($"area-{VietnameseTextNormalizer.ToMatchKey(province).Replace(' ', '-')}", province);

    public static bool TryCompose(
        IReadOnlyList<SpeechItem> items,
        int maximumSpokenItems,
        VietnameseNumberStyle style,
        RecordedSpeechCatalog catalog,
        out ImmutableArray<SpeechNumberClip> clips)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(style);
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentOutOfRangeException.ThrowIfLessThan(maximumSpokenItems, 1);

        List<ImmutableArray<SpeechNumberClip>> named = [];
        foreach (SpeechItem item in items)
        {
            if (named.Count == maximumSpokenItems)
            {
                break;
            }

            if (TryComposeItem(item, style, catalog, out ImmutableArray<SpeechNumberClip> itemClips))
            {
                named.Add(itemClips);
            }
        }

        if (named.Count == 0)
        {
            clips = [];
            return false;
        }

        ImmutableArray<SpeechNumberClip>.Builder spoken =
            ImmutableArray.CreateBuilder<SpeechNumberClip>();
        for (int index = 0; index < named.Count; index++)
        {
            // The comma between products is silence, not a clip: a reader pauses there anyway,
            // and a recorded comma is a join that buys nothing.
            if (index > 0 && index == named.Count - 1)
            {
                spoken.Add(AndClip);
            }

            spoken.AddRange(named[index]);
        }

        int folded = items.Count - named.Count;
        if (folded > 0)
        {
            spoken.Add(AndClip);
            spoken.AddRange(VietnameseNumberSpeller.SpellClips(folded, style));
            spoken.Add(OtherProductsClip);
        }

        clips = spoken.ToImmutable();
        return true;
    }

    private static bool TryComposeItem(
        SpeechItem item,
        VietnameseNumberStyle style,
        RecordedSpeechCatalog catalog,
        out ImmutableArray<SpeechNumberClip> clips)
    {
        clips = [];
        if (!catalog.TryGetItem(item.PublicName, out SpeechNumberClip nameClip))
        {
            return false;
        }

        // A recorded product with an unrecorded unit is not speakable either. Dropping the unit
        // would read "hai sâm Ngọc Linh" for an order of two boxes — the customer is being asked
        // to confirm a quantity, so losing the unit loses the thing being confirmed.
        SpeechNumberClip unitClip = default;
        bool hasUnit = !string.IsNullOrWhiteSpace(item.UnitLabel);
        if (hasUnit && !catalog.TryGetUnit(item.UnitLabel!, out unitClip))
        {
            return false;
        }

        ImmutableArray<SpeechNumberClip>.Builder builder =
            ImmutableArray.CreateBuilder<SpeechNumberClip>();
        builder.AddRange(VietnameseNumberSpeller.SpellQuantityClips(item.Quantity, style));
        if (hasUnit)
        {
            builder.Add(unitClip);
        }

        builder.Add(nameClip);
        clips = builder.ToImmutable();
        return true;
    }
}
