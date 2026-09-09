using System.Text.Json;

namespace Ivr.Infrastructure.Idempotency;

/// <summary>
/// The one serializer both idempotency stores replay responses through.
/// <para>
/// It is shared rather than configured twice because the two stores are meant to be
/// indistinguishable from the outside: a response replayed from Postgres and the same response
/// replayed from memory have to deserialize to the same object, or an idempotency key means
/// something different depending on which store is wired. They had drifted — the Postgres store
/// registered <see cref="ReadOnlySetJsonConverterFactory"/> and the in-memory store did not, so a
/// response carrying an <see cref="IReadOnlySet{T}"/> replayed in production and failed in MOCK.
/// Which is exactly backwards: MOCK is the mode used to rehearse the thing before it ships.
/// </para>
/// <para>
/// One instance, not one factory method called twice: sharing the object is what makes the two
/// stores unable to drift apart again, and a converter added below reaches both by construction.
/// <see cref="JsonSerializerOptions"/> is thread-safe for concurrent (de)serialization, and
/// nothing mutates this one after the initializer returns.
/// </para>
/// </summary>
internal static class IdempotencySerialization
{
    public static JsonSerializerOptions Options { get; } = Create();

    private static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new ReadOnlySetJsonConverterFactory());
        return options;
    }
}
