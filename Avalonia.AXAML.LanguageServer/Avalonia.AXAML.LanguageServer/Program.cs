using Avalonia.AXAML.LanguageServer;
using Avalonia.AXAML.LanguageServer.AssemblyMetadata;
using Avalonia.AXAML.LanguageServer.Document;
using Avalonia.AXAML.LanguageServer.Handlers;
using Avalonia.AXAML.LanguageServer.ProjectModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;
using Serilog;

namespace Avalonia.axamlLanguageServer
{
    internal class Program
     {
         private static void Main(string[] args) => MainAsync(args).Wait();

         private static async Task MainAsync(string[] args)
         {
             Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.File("Avalonia.AXAML.LanguageServer.Log.txt", rollingInterval: RollingInterval.Day)
                .MinimumLevel.Verbose()
                .CreateLogger();

            Log.Logger.Information("Starting log...");

            var server = await OmniSharp.Extensions.LanguageServer.Server.LanguageServer
                .From(
                    options => 
                        options
                            .WithInput(Console.OpenStandardInput())
                            .WithOutput(Console.OpenStandardOutput())
                            .ConfigureLogging(
                                x => x
                                    .AddSerilog(Log.Logger)
                                    .AddLanguageProtocolLogging()
                                    .SetMinimumLevel(LogLevel.Debug)
                            )
                            .WithServices(ConfigureServices)
                            .WithHandler<TextDocumentHandler>()
                            .WithHandler<FileChangeHandler>()
                            .WithHandler<AvaloniaCodeCompletionHandler>())
                .ConfigureAwait(false);

            await server
                .WaitForExit
                .ConfigureAwait(false);
         }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<TextDocumentBuffer>();
            serviceCollection.AddSingleton<ProjectShepard>();
            serviceCollection.AddSingleton<AvaloniaMetadataShepard>();
            serviceCollection.AddSingleton<AvaloniaMetadataLoader>();
            serviceCollection.AddSingleton<TextDocumentToProjectMapper>();
            serviceCollection.AddSingleton<DocumentMetadataProvider>();

            // serviceCollection.AddSingleton<ILanguageServer, LanguageServer>();
        }
    }
 }
