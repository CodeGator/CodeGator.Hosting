using Microsoft.Extensions.Hosting;

namespace CodeGator.Hosting.UnitTests;

[TestClass]
public sealed class HostExtensionsTests
{
    [TestMethod]
    public void RunDelegate_ActionIHost_InvokesDelegateAndStopsHost()
    {
        using var host = Host.CreateDefaultBuilder().Build();

        IHost? captured = null;
        host.RunDelegate(h => captured = h);

        Assert.AreSame(host, captured);
    }

    [TestMethod]
    public void RunDelegate_Action_InvokesDelegateAndStopsHost()
    {
        using var host = Host.CreateDefaultBuilder().Build();
        var invoked = false;

        host.RunDelegate(() => invoked = true);

        Assert.IsTrue(invoked);
    }

    [TestMethod]
    public async Task RunDelegateAsync_ActionIHostCancellationToken_InvokesDelegateAndStopsHost()
    {
        using var host = Host.CreateDefaultBuilder().Build();

        IHost? captured = null;
        await host.RunDelegateAsync((h, _) => captured = h);

        Assert.AreSame(host, captured);
    }

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
