
#pragma warning disable IDE0130
namespace Microsoft.Extensions.Hosting;
#pragma warning restore IDE0130

/// <summary>
/// This class provides extension methods for <see cref="IHostBuilder"/>.
/// </summary>
public static partial class HostBuilderExtensions
{
    /// <summary>
    /// This method builds a host and runs the supplied delegate on it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The host uses console lifetime. After the delegate returns, the host is stopped
    /// synchronously.
    /// </para>
    /// </remarks>
    /// <param name="hostBuilder">The builder used to create the host.</param>
    /// <param name="hostDelegate">The operation to run with the built host.</param>
    /// <example>
    /// This example demonstrates a typical use of the <see cref="RunDelegate(IHostBuilder, Action{IHost})"/>
    /// method:
    /// <code>
    /// public void ConfigureServices(IServiceCollection services)
    /// {
    ///     Host.CreateDefaultBuilder()
    ///         .RunDelegate((host) => 
    ///         {
    ///             Console.WriteLine("Hello World");
    ///         });
    /// }
    /// </code>
    /// </example>
    public static void RunDelegate(
       this IHostBuilder hostBuilder,
       Action<IHost> hostDelegate
       )
    {
        var host = hostBuilder.UseConsoleLifetime()
            .Build();

        try
        {
            hostDelegate(
                host
                );
        }
        finally
        {
            host.StopAsync().Wait();
        }
    }

    /// <summary>
    /// This method builds a host and runs a parameterless delegate on it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The host uses console lifetime. After the delegate returns, the host is stopped
    /// synchronously.
    /// </para>
    /// </remarks>
    /// <param name="hostBuilder">The builder used to create the host.</param>
    /// <param name="hostDelegate">The operation to run after the host is built.</param>
    /// <example>
    /// This example demonstrates a typical use of the <see cref="RunDelegate(IHostBuilder, Action)"/>
    /// method:
    /// <code>
    /// public void ConfigureServices(IServiceCollection services)
    /// {
    ///     Host.CreateDefaultBuilder()
    ///         .RunDelegate(() => 
    ///         {
    ///             Console.WriteLine("Hello World");
    ///         });
    /// }
    /// </code>
    /// </example>
    public static void RunDelegate(
       this IHostBuilder hostBuilder,
       Action hostDelegate
       )
    {
        var host = hostBuilder.UseConsoleLifetime()
            .Build();

        try
        {
            hostDelegate();
        }
        finally
        {
            host.StopAsync().Wait();
        }
    }

    /// <summary>
    /// This method builds a host and runs a cancellable delegate asynchronously.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The delegate runs via <see cref="Task.Run(Action, CancellationToken)"/>. The host is
    /// stopped after that work completes.
    /// </para>
    /// </remarks>
    /// <param name="hostBuilder">The builder used to create the host.</param>
    /// <param name="action">The operation to run with the host and cancellation token.</param>
    /// <param name="cancellationToken">A token that can cancel the asynchronous work.</param>
    /// <returns>A task that completes after the delegate and host shutdown finish.</returns>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    public static async Task RunDelegateAsync(
        this IHostBuilder hostBuilder,
        Action<IHost, CancellationToken> action,
        CancellationToken cancellationToken = default
        )
    {
        var host = hostBuilder.UseConsoleLifetime()
            .Build();

        try
        {
            await Task.Run(
                () => action(host, cancellationToken),
                cancellationToken
                ).ConfigureAwait(false);
        }
        finally
        {
            await host.StopAsync(
                cancellationToken
                ).ConfigureAwait(false);
        }
    }
}
