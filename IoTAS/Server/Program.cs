﻿//
// Copyright (c) 2021 Hugh Maaskant
// MIT License
//

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Templates;
using Serilog.Templates.Themes;

namespace IoTAS.Server;

public sealed class Program
{
    private static readonly string ConsoleLogFormat =
        "[{@t:HH:mm:ss} {@l:u3}] " +
        "{#if SourceContext is not null}" + 
        " {Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1), 30} : " +
        "{#else}" +
        " No known context ...           : " +
        "{#end}" +
        "{@m}\n{@x}";

    public static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(new ExpressionTemplate(ConsoleLogFormat, theme: TemplateTheme.Code))
            .CreateLogger();

        try
        {
            Log.Information("IoTAS Program started");
            CreateHostBuilder(args).Build().Run();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Program terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.Information("IoTAS Program stopped");
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();

                // Uncomment the following line to run on any local IP address, so the
                // server is accessible from other nodes (subject to firewall rules)
                // webBuilder.UseUrls("http://0.0.0.0:5000", "https://0.0.0.0:5001");
            });
}