using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace Analyzers.Tests.Helpers;

internal sealed class TUnitVerifier : IVerifier
{
    public void Empty<T>(string collectionName, IEnumerable<T> collection) =>
        AsyncHelper.RunSync(async () => await Assert.That(collection).IsEmpty());

    public void Equal<T>(T expected, T actual, string? message = null) =>
        AsyncHelper.RunSync(async () => await Assert.That(actual).IsEqualTo(expected));

    public void True([DoesNotReturnIf(false)] bool assert, string? message = null) =>
        AsyncHelper.RunSync(async () => await Assert.That(assert).IsTrue());

    public void False([DoesNotReturnIf(true)] bool assert, string? message = null) =>
        AsyncHelper.RunSync(async () => await Assert.That(assert).IsFalse());

    [DoesNotReturn]
    public void Fail(string? message = null)
    {
        Assert.Fail(message ?? "Verification failed.");
        throw new InvalidOperationException("Unreachable");
    }

    public void LanguageIsSupported(string language) { }

    public void NotEmpty<T>(string collectionName, IEnumerable<T> collection) =>
        AsyncHelper.RunSync(async () => await Assert.That(collection).IsNotEmpty());

    public void SequenceEqual<T>(
        IEnumerable<T> expected,
        IEnumerable<T> actual,
        IEqualityComparer<T>? equalityComparer = null,
        string? message = null
    ) =>
        AsyncHelper.RunSync(async () =>
            await Assert
                .That(
                    actual.SequenceEqual(expected, equalityComparer ?? EqualityComparer<T>.Default)
                )
                .IsTrue()
        );

    public IVerifier PushContext(string context) => new TUnitVerifier();
}
