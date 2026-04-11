using Microsoft.Extensions.Hosting;

namespace CodeGator.Hosting.UnitTests;

/// <summary>
/// <see cref="HostExtensions.RunOnce{TProgram}"/> uses a process-wide mutex; keep this fixture from running alongside itself.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class HostExtensionsRunOnceTests
{
    [TestMethod]
    public void RunOnce_InvokesActionAndReturnsTrue()
    {
        using var host = Host.CreateDefaultBuilder().Build();
        var invoked = false;

        var result = host.RunOnce<HostExtensionsRunOnceTests>(h =>
        {
            invoked = true;
            Assert.AreSame(host, h);
        });

        Assert.IsTrue(result);
        Assert.IsTrue(invoked);
    }

}
