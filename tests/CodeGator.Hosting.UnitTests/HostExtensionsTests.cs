using Microsoft.Extensions.Hosting;

namespace CodeGator.Hosting.UnitTests;

/// <summary>
/// This class contains unit tests for <see cref="HostExtensions"/> methods.
/// </summary>
[TestClass]
public sealed class HostExtensionsTests
{
    /// <summary>
    /// This method verifies RunDelegate passes the host and stops it.
    /// </summary>
    [TestMethod]
    public void RunDelegate_ActionIHost_InvokesDelegateAndStopsHost()
    {
        using var host = Host.CreateDefaultBuilder().Build();

        IHost? captured = null;
        host.RunDelegate(h => captured = h);

        Assert.AreSame(host, captured);
    }

    /// <summary>
    /// This method verifies RunDelegate invokes a parameterless callback.
    /// </summary>
    [TestMethod]
    public void RunDelegate_Action_InvokesDelegateAndStopsHost()
    {
        using var host = Host.CreateDefaultBuilder().Build();
        var invoked = false;

        host.RunDelegate(() => invoked = true);

        Assert.IsTrue(invoked);
    }

    /// <summary>
    /// This method verifies RunDelegateAsync passes the host and stops it.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [TestMethod]
    public async Task RunDelegateAsync_ActionIHostCancellationToken_InvokesDelegateAndStopsHost()
    {
        using var host = Host.CreateDefaultBuilder().Build();

        IHost? captured = null;
        await host.RunDelegateAsync((h, _) => captured = h);

        Assert.AreSame(host, captured);
    }

    /// <summary>
    /// This method verifies RunDelegateAsync honors a canceled token.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [TestMethod]
    public async Task RunDelegateAsync_RespectsCancellation()
    {
        using var host = Host.CreateDefaultBuilder().Build();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () =>
        {
            await host.RunDelegateAsync((_, _) => { }, cts.Token);
        });
    }
}
