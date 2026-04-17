using Microsoft.Extensions.Hosting;

namespace CodeGator.Hosting.UnitTests;

/// <summary>
/// This class contains tests for single-instance RunOnce extension methods.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="HostExtensions.RunOnce{TProgram}(IHost, Action{IHost})"/> uses a process-wide mutex;
/// these tests use <see cref="DoNotParallelizeAttribute"/> to avoid conflicting runs.
/// </para>
/// </remarks>
[TestClass]
[DoNotParallelize]
public sealed class HostExtensionsRunOnceTests
{
    /// <summary>
    /// This method verifies RunOnce invokes the callback and returns true.
    /// </summary>
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
