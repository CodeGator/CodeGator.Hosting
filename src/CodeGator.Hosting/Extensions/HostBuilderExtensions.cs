
#pragma warning disable IDE0130
namespace Microsoft.Extensions.Hosting;
#pragma warning restore IDE0130

/// <summary>
/// This class utility contains extension methods related to the <see cref="IHostBuilder"/>
/// type.
/// </summary>
public static partial class HostBuilderExtensions
{
    // *******************************************************************
    // Public methods.
    // *******************************************************************

    #region Public methods

    /// <summary>
    /// This method runs a delegate within the context of the specified <see cref="IHostBuilder"/> 
    /// object.
    /// </summary>
    /// <param name="hostBuilder">The host builder to use for the operation.</param>
    /// <param name="hostDelegate">The delegate to use for the operation.</param>
    /// <exception cref="ArgumentException">This exception is thrown whenever
    /// any of the arguments are missing, or NULL.</exception>
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

    // *******************************************************************

    /// <summary>
    /// This method runs a delegate within the context of the specified <see cref="IHostBuilder"/> 
    /// object.
    /// </summary>
    /// <param name="hostBuilder">The host builder to use for the operation.</param>
    /// <param name="action">The delegate to use for the operation.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="ArgumentException">This exception is thrown whenever
    /// any of the arguments are missing, or NULL.</exception>
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

    #endregion
}
