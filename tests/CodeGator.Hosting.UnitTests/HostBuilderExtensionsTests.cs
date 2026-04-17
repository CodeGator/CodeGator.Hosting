using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CodeGator.Hosting.UnitTests;

/// <summary>
/// This class contains unit tests for <see cref="HostBuilderExtensions"/> methods.
/// </summary>
[TestClass]
public sealed class HostBuilderExtensionsTests
{
    /// <summary>
    /// This method verifies RunDelegate receives services from the built host.
    /// </summary>
    [TestMethod]
    public void RunDelegate_ActionIHost_InvokesDelegateWithBuiltHost()
    {
        IHost? captured = null;

        Host.CreateDefaultBuilder()
            .ConfigureServices(services => services.AddSingleton<MyMarker>())
            .RunDelegate(host =>
            {
                captured = host;
                Assert.IsNotNull(host.Services.GetService(typeof(MyMarker)));
            });

        Assert.IsNotNull(captured);
    }

    /// <summary>
    /// This method verifies RunDelegate runs a parameterless callback.
    /// </summary>
    [TestMethod]
    public void RunDelegate_Action_InvokesDelegate()
    {
        var invoked = false;

        Host.CreateDefaultBuilder()
            .RunDelegate(() => invoked = true);

        Assert.IsTrue(invoked);
    }

    /// <summary>
    /// This method verifies RunDelegateAsync runs with the built host.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [TestMethod]
    public async Task RunDelegateAsync_ActionIHostCancellationToken_InvokesDelegate()
    {
        IHost? captured = null;

        await Host.CreateDefaultBuilder()
            .RunDelegateAsync((host, ct) =>
            {
                captured = host;
            });

        Assert.IsNotNull(captured);
    }

    /// <summary>
    /// This method verifies RunDelegateAsync honors a canceled token.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [TestMethod]
    public async Task RunDelegateAsync_RespectsCancellation()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () =>
        {
            await Host.CreateDefaultBuilder()
                .RunDelegateAsync(
                    (_, ct) => { },
                    cts.Token);
        });
    }

    /// <summary>
    /// This class is a DI marker type used only by these tests.
    /// </summary>
    private sealed class MyMarker;
}
