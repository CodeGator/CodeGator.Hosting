using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CodeGator.Hosting.UnitTests;

[TestClass]
public sealed class HostBuilderExtensionsTests
{
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

    [TestMethod]
    public void RunDelegate_Action_InvokesDelegate()
    {
        var invoked = false;

        Host.CreateDefaultBuilder()
            .RunDelegate(() => invoked = true);

        Assert.IsTrue(invoked);
    }

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

    private sealed class MyMarker;
}
