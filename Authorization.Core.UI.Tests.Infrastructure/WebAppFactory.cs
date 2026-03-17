using Authorization.Core.UI.Test.Web.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Data.Common;
using System.Net;
using System.Net.Sockets;

namespace Authorization.Core.UI.Tests.Infrastructure;

public class WebAppFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebAppFactory"/> class.
    /// This constructor ensures that the web application can start without port conflicts, which is 
    /// especially useful in testing scenarios where multiple instances may be running concurrently.
    /// </summary>
    /// <remarks>
    /// Configures the web application to run on an available port by dynamically assigning a free TCP 
    /// port and setting the ASPNETCORE_URLS environment variable.
    /// </remarks>
    public WebAppFactory()
    {
        int port = GetAvailablePort();
        Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://AuthCoreUi.dev.localhost:{port}");
    }

    /// <summary>
    /// Configures the web host for integration testing by setting up an in-memory SQLite database and adjusting 
    /// service registrations.
    /// </summary>
    /// <remarks>
    /// Removes existing database context and connection registrations, then adds an open in-memory SQLite 
    /// connection to ensure consistent database state across tests. Sets the environment to "IntegrationTests" 
    /// for test-specific configuration.
    /// </remarks>
    /// <param name="builder">
    /// The web host builder used to configure services and environment settings for the test host.
    /// </param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");

        // Ensure static web assets are available during testing.
        // They are not automatically included in the "IntegrationTests" environment.
        builder.ConfigureAppConfiguration((context, builder) =>
        {
            StaticWebAssetsLoader.UseStaticWebAssets(context.HostingEnvironment, context.Configuration);
        });

        // Replace existing DbContext and DbConnection registrations with test-specific implementations.
        builder.ConfigureTestServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                );
            if (dbContextDescriptor is not null)
                services.Remove(dbContextDescriptor);

            var dbConnectionDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbConnection)
                );
            if (dbConnectionDescriptor is not null)
                services.Remove(dbConnectionDescriptor);

            // Create open SqliteConnection so EF won't automatically close it.
            services.AddSingleton<DbConnection>(container =>
            {
                var connection = new SqliteConnection("DataSource=:memory:");
                connection.Open();

                return connection;
            });

            services.AddDbContext<ApplicationDbContext>((container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);
            });
        });
    }

    /// <summary>
    /// Finds an available TCP port on the local machine.
    /// </summary>
    /// <remarks>
    /// The returned port is available at the time of the call, but may be claimed by another process
    /// before it is used. This method is useful for scenarios where a free port is needed for temporary network
    /// operations, such as testing or dynamic service allocation.
    /// </remarks>
    /// <returns>
    /// A port number that is currently available for TCP connections on the local loopback interface.
    /// </returns>
    private static int GetAvailablePort()
    {
        // Create a TCP listener on port 0 (OS will choose a free port)
        var listener = new TcpListener(IPAddress.Loopback, 0);
        try
        {
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            return port;
        }
        finally
        {
            // Always stop the listener to free the port
            listener.Stop();
        }
    }
}
