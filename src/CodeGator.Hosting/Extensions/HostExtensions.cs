
#pragma warning disable IDE0130
namespace Microsoft.Extensions.Hosting;
#pragma warning restore IDE0130

/// <summary>
/// This class provides extension methods for <see cref="IHost"/>.
/// </summary>
public static partial class HostExtensions
{
    /// <summary>
    /// This method runs a delegate on the host asynchronously with a token.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The delegate executes inside <see cref="Task.Run(Action, CancellationToken)"/>. The host
    /// is stopped after that work completes.
    /// </para>
    /// </remarks>
    /// <param name="host">The host used as the execution context.</param>
    /// <param name="action">The operation to run with the host and cancellation token.</param>
    /// <param name="token">A token that can cancel the asynchronous work.</param>
    /// <returns>A task that completes after the delegate and host shutdown finish.</returns>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    /// <example>
    /// This example demonstrates a typical use of the <see cref="RunDelegateAsync(IHost, Action{IHost, CancellationToken}, CancellationToken)"/>
    /// method:
    /// <code>
    /// public void ConfigureServices(IServiceCollection services)
    /// {
    ///     Host.CreateDefaultBuilder()
    ///         .Build()
    ///         .RunDelegateAsync((host, token) => 
    ///         {
    ///             Console.WriteLine("Hello World");
    ///         }).Result;
    /// }
    /// </code>
    /// </example>
    public static async Task RunDelegateAsync(
        this IHost host,
        Action<IHost, CancellationToken> action,
        CancellationToken token = default
        )
    {
        try
        {
            await Task.Run(
                () => action(host, token),
                token
                ).ConfigureAwait(false);
        }
        finally
        {
            await host.StopAsync(
                token
                ).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// This method runs a delegate that receives the host, then stops it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// After the delegate returns, <see cref="IHost.StopAsync(CancellationToken)"/> is awaited
    /// synchronously via <see cref="Task.Wait()"/>.
    /// </para>
    /// </remarks>
    /// <param name="host">The host used as the execution context.</param>
    /// <param name="action">The operation to run with the host instance.</param>
    /// <example>
    /// This example demonstrates a typical use of the <see cref="RunDelegate(IHost, Action{IHost})"/>
    /// method:
    /// <code>
    /// public void ConfigureServices(IServiceCollection services)
    /// {
    ///     Host.CreateDefaultBuilder()
    ///         .Build()
    ///         .RunDelegate((host) => 
    ///         {
    ///             Console.WriteLine("Hello World");
    ///         });
    /// }
    /// </code>
    /// </example>
    public static void RunDelegate(
        this IHost host,
        Action<IHost> action
        )
    {
        try
        {
            action(host);
        }
        finally
        {
            host.StopAsync().Wait();
        }
    }

    /// <summary>
    /// This method runs a parameterless delegate on the host, then stops it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// After the delegate returns, <see cref="IHost.StopAsync(CancellationToken)"/> is awaited
    /// synchronously via <see cref="Task.Wait()"/>.
    /// </para>
    /// </remarks>
    /// <param name="host">The host used as the execution context.</param>
    /// <param name="action">The operation to run after the host is ready.</param>
    /// <example>
    /// This example demonstrates a typical use of the <see cref="RunDelegate(IHost, Action)"/>
    /// method:
    /// <code>
    /// public void ConfigureServices(IServiceCollection services)
    /// {
    ///     Host.CreateDefaultBuilder()
    ///         .Build()
    ///         .RunDelegate(() => 
    ///         {
    ///             Console.WriteLine("Hello World");
    ///         });
    /// }
    /// </code>
    /// </example>
    public static void RunDelegate(
        this IHost host,
        Action action
        )
    {
        try
        {
            action();
        }
        finally
        {
            host.StopAsync().Wait();
        }
    }

    /// <summary>
    /// This method runs the action when the process acquires the app mutex.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A named system mutex limits one concurrent run per application. When the mutex is
    /// acquired within one second, <paramref name="action"/> runs and the method returns
    /// <see langword="true"/>; otherwise it returns <see langword="false"/>. An abandoned mutex
    /// is treated as recoverable so the action may still run.
    /// </para>
    /// </remarks>
    /// <typeparam name="TProgram">A type token used to distinguish mutex scope between programs.</typeparam>
    /// <param name="host">The host passed to the action when it runs.</param>
    /// <param name="action">The operation to run at most once per successful mutex acquisition.</param>
    /// <returns><see langword="true"/> if the action ran; otherwise <see langword="false"/>.</returns>
    /// <example>
    /// This example demonstrates how to use the <see cref="RunOnce{TProgram}(IHost, Action{IHost})"/> 
    /// method:
    /// <code>
    /// class TestProgram
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///        Host.CreateDefaultBuilder()
    ///            .Build()
    ///           .RunOnce(host => 
    ///           {
    ///               Console.WriteLine("Hello World");
    ///           });
    ///     }
    /// }
    /// </code>
    /// </example>
    public static bool RunOnce<TProgram>(
        this IHost host,
        Action<IHost> action
        ) where TProgram : class
    {
        var appName = AppDomain.CurrentDomain.FriendlyName;
        var mutexName = $"Global\\{{{appName.Replace('\\', '_').Replace(':', '_')}}}";

#pragma warning disable IDE0063 // Use simple 'using' statement
        using (var mutex = new Mutex(false, mutexName))
        {
            try
            {
                if (mutex.WaitOne(TimeSpan.FromSeconds(1), false))
                {
                    try
                    {
                        action.Invoke(host);
                        return true;
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                    }
                }
            }
            catch (AbandonedMutexException ex)
            {
                // If we get here then, likely, whatever application/thread owned the mutex at the time
                //   we tried to wait for it crashed. So now the O/S considers the mutex "abandoned". 

                ex.Mutex?.ReleaseMutex();
                action.Invoke(host);
            }
            finally
            {
                host.StopAsync().Wait();
            }
        }
#pragma warning restore IDE0063 // Use simple 'using' statement

        return false;
    }

    /// <summary>
    /// This method runs the action asynchronously after taking the app mutex.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A named system mutex limits one concurrent run per application. When the mutex is
    /// acquired within one second, <paramref name="action"/> is scheduled with
    /// <see cref="Task.Run(Action, CancellationToken)"/> and the method returns
    /// <see langword="true"/>; otherwise it returns <see langword="false"/>. An abandoned mutex
    /// is treated as recoverable so the action may still run.
    /// </para>
    /// <para>
    /// This method is intended to wrap logic in a process main entry point so it cannot run
    /// more than once at the same time.
    /// </para>
    /// </remarks>
    /// <typeparam name="TProgram">A type token used to distinguish mutex scope between programs.</typeparam>
    /// <param name="host">The host passed to the action when it runs.</param>
    /// <param name="action">The operation to run at most once per successful mutex acquisition.</param>
    /// <param name="cancellationToken">A token that can cancel asynchronous work.</param>
    /// <returns>
    /// A task whose result is <see langword="true"/> if the action ran; otherwise
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    /// <example>
    /// This example demonstrates how to use the <see cref="RunOnceAsync{TProgram}(IHost, Action{IHost}, CancellationToken)"/> 
    /// method:
    /// <code>
    /// class TestProgram
    /// {
    ///     static async Task Main(string[] args)
    ///     {
    ///        await Host.CreateDefaultBuilder()
    ///            .Build()
    ///           .RunOnceAsync((host) => 
    ///           {
    ///               Console.WriteLine("Hello World");
    ///           });
    ///     }
    /// }
    /// </code>
    /// </example>
    public static async Task<bool> RunOnceAsync<TProgram>(
        this IHost host,
        Action<IHost> action,
        CancellationToken cancellationToken = default
        ) where TProgram : class
    {
        // Get a friendly name for the application.
        var appName = AppDomain.CurrentDomain.FriendlyName;

        // Create a safe mutex name.
        var mutexName = $"Global\\{{{appName.Replace('\\', '_').Replace(':', '_')}}}";

        // Create a mutex to control access to the delegate.
#pragma warning disable IDE0063 // Use simple 'using' statement
        using (var mutex = new Mutex(false, mutexName))
        {
            try
            {
                if (mutex.WaitOne(TimeSpan.FromSeconds(1), false))
                {
                    try
                    {
                        await Task.Run(
                            () => action(host),
                            cancellationToken
                            ).ConfigureAwait(false);

                        return true;
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                    }
                }
            }
            catch (AbandonedMutexException ex)
            {
                // If we get here then, likely, whatever application/thread owned the mutex at the time
                //   we tried to wait for it, crashed. So now the O/S considers the mutex "abandoned". 

                ex.Mutex?.ReleaseMutex();

                await Task.Run(
                    () => action(host),
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
#pragma warning restore IDE0063 // Use simple 'using' statement

        return false;
    }
}
