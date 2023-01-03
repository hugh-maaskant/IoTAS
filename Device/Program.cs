//
// Copyright (c) 2021 Hugh Maaskant
// MIT License
//

using System;
using System.Threading;
using System.Threading.Tasks;
using IoTAS.Shared.Hubs;
using Microsoft.AspNetCore.SignalR.Client;

namespace IoTAS.Device;

public sealed class Program
{
    // Hardcoded for now, possibly get from configuration lateron
    private static readonly string HubUrlHttps = "https://localhost:5001" + IDeviceHubServer.Path;
    // private static readonly string hubUrlHttps = "https://localhost:44388" + IDeviceHubServer.path;
    // private static readonly string hubUrlHttp  = "http://localhost:58939" + IDeviceHubServer.path;

    // Must be fields so the event handlers can access them.
    private static readonly CancellationTokenSource TokenSource = new();
    private static HubConnection _connection;
    private static int _deviceId;

    static async Task Main(string[] args)
    {
        Console.WriteLine("Device started");
            
        //  In future this might be passed in as a configuration item or a command argument
        string hubUrl = HubUrlHttps;

        Console.WriteLine($"Using server at {hubUrl}");
        Console.WriteLine();

        //  In future this might be passed in as a configuration item or a command argument
        _deviceId = GetDeviceId();
        if (_deviceId != 0)
        {
            // Set up CNTRL-C handling to cancel out of blocking async operations
            Console.CancelKeyPress += CntrlcHandler;

            _connection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .Build();

            // set connection life-cycle callbacks
            _connection.Closed += ConnectionClosedHandler;
            _connection.Reconnecting += ConnectionReconnectingHandler;
            _connection.Reconnected += ConnectionReconnectedHandler;

            bool operational = await StartConnectionAndRegisterAsync();
            if (operational)
            {
                await DoHeartbeatAsync();
            }
                
            // We only get here after a cancellation on the tokenSource (from CNTRL-C)
            await TeardownConnectionAsync();

            TokenSource.Dispose();
        }

        Console.WriteLine("Bye ...");

        await Task.Delay(1000); // Allow user to see the Bye message

        Environment.Exit(0);
    }

    #region UserInteraction

    /// <summary>
    /// Gets a DeviceId from user input
    /// </summary>
    /// <returns>A positive integer DeviceID or 0 to exit</returns>
    private static int GetDeviceId()
    {
        int deviceId;
        do
        {
            Console.Write("Please enter this device's DeviceId (or 0 to exit): ");
            string answer = Console.ReadLine()?.Trim() ?? string.Empty;

            bool valid = int.TryParse(answer, out deviceId);
            if (!valid || deviceId < 0)
            {
                Console.WriteLine("DeviceId must be a positive integer value - please try again");
                Console.WriteLine();
                deviceId = -1;
            }
        }
        while (deviceId < 0);

        return deviceId;
    }

    /// <summary>
    /// The Cntrl-C (and Cntrl-Break) handler cancels the tokenSource, 
    /// which will then gracefully shut down the program.
    /// </summary>
    private static readonly ConsoleCancelEventHandler CntrlcHandler = (sender, eventArgs) =>
    {
        Console.WriteLine("Cancelling due to Cntrl-C ...");
            
        try
        {
            // Remove ourselves ...
            Console.CancelKeyPress -= CntrlcHandler;

            TokenSource.Cancel();

            // continue running to clean up
            eventArgs.Cancel = true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception while cancelling: {e.Message}");
            throw;
        }
    };

    #endregion  UserInteraction

    #region HubConnection Management

    /// <summary>
    /// (Re-)Starts the HubConnection and (re-)registers untill succesful or cancelled
    /// </summary>
    /// <returns>
    /// <see langword="true"/> upon succesful connection startup and registration, <see langword="false"/> otherwise
    /// </returns>
    private static async Task<bool> StartConnectionAndRegisterAsync()
    {
        Random random = null;
        int delayIncrease = 0;

        bool started = false;
        bool registered = false;

        // Keep trying until we can start and register or the token source is canceled.
        while (!TokenSource.IsCancellationRequested && !registered)
        {
            if (!started)
            {
                started = await TryToStartConnectionAsync();
            }

            if (started)
            {
                registered = await TryToRegisterDeviceAsync();
            }

            if (registered) return true;
            if (TokenSource.IsCancellationRequested) return false;

            // Either Start or Registration failed, backoff and try again ...
            random ??= new Random();
            int delay = random.Next(2500, 5000) + delayIncrease;
            if (delayIncrease <= 20000) delayIncrease += 5000;

            Console.WriteLine($"Retrying in {delay} msec");
            try
            {
                await Task.Delay(delay, TokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("ConnectWithRetryAsync cancelled while waiting to retry connection");
            }
        }

        // We only get here due to cancellation, all other conditions retry the operation(s)
        return false;
    }

    private static async Task<bool> TryToStartConnectionAsync()
    {
        try
        {
            Console.WriteLine("Trying to start the connection ...");
            await _connection.StartAsync(TokenSource.Token);
            Console.WriteLine("Connection has been started");

            return true;
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("TryToStartConnectionAsync was cancelled");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while starting the connection: {e.Message}");
        }

        return false;
    }

    // Called by SignalR runtime on cancellation, error or keep-alive time-out
    private static async Task ConnectionClosedHandler(Exception e)
    {
        if (TokenSource.IsCancellationRequested)
        {
            Console.WriteLine("Connection to server closed due to cancellation - exiting");
            return;
        }

        if (e is null)
        {
            Console.WriteLine("Connection closed by Server");
        }
        else
        {
            Console.WriteLine($"Connection to Server has been lost: {e.Message}");
        }

        _ = await StartConnectionAndRegisterAsync();
    }

    // Currently we do not use SignalR's auto-reconnect, so we should never get here ...
    private static Task ConnectionReconnectingHandler(Exception e)
    {
        Console.WriteLine($"Reconnecting to server due to error: {e.Message}");
        return Task.CompletedTask;
    }

    // Currently we do not use SignalR's auto-reconnect, so we should never get here ...
    private static Task ConnectionReconnectedHandler(string newId)
    {
        Console.WriteLine($"Reconnected to server, connectionId = \"{newId}\"");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Teardown and Dispose connection
    /// </summary>
    /// <returns>A Task</returns>
    private static async Task TeardownConnectionAsync()
    {
        Console.WriteLine("Tearing down connection ...");
        if (_connection is null) return;

        Console.WriteLine("Removing life-cycle event handlers ...");
        _connection.Closed -= ConnectionClosedHandler;
        _connection.Reconnecting -= ConnectionReconnectingHandler;
        _connection.Reconnected -= ConnectionReconnectedHandler;

        if (_connection.State == HubConnectionState.Connected)
        {
            Console.WriteLine("Stopping the connection ...");
            await _connection.StopAsync();
            Console.WriteLine("Connection has been stopped by Device");
        }
        else
        {
            Console.WriteLine("Connection was not connected, no need to stop it.");
        }

        await _connection.DisposeAsync();
        Console.WriteLine("Connection has been disposed");
    }

    # endregion HubConnection Management

    #region Registration

    /// <summary>
    /// Try to send a Device Registration message to the Server
    /// </summary>
    /// <returns><see langword="true"/> upon succesful registration, <see langword="false"/> otherwise</returns>
    private static async Task<bool> TryToRegisterDeviceAsync()
    {
        try
        {
            var dto = new DevToSrvDeviceRegistrationDto(_deviceId);
            Console.WriteLine($"Registering Device with Id {_deviceId} ...");
            await _connection.InvokeAsync(nameof(IDeviceHubServer.RegisterDeviceClient), dto, TokenSource.Token);
            Console.WriteLine("Registration succesful");
            return true;
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("TryToRegisterDeviceAsync was cancelled");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while registering: {e.Message}");
        }
        return false;
    }

    #endregion Registration

    #region Heartbeat

    /// <summary>
    /// Sends Hearbeat messages with 15 second interval
    /// </summary>
    /// <returns>A completed Task when cancelled</returns>
    private static async Task DoHeartbeatAsync()
    {
        Console.WriteLine($"{nameof(DoHeartbeatAsync)} Task started");
        var dto = new DevToSrvDeviceHeartbeatDto(_deviceId);
        int heartbeatNumber = 0;

        while (!TokenSource.IsCancellationRequested)
        {
            heartbeatNumber++;

            // Delay 15 seconds; in case of cancellation: end the Task
            try
            {
                await Task.Delay(15 * 1000, TokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"{nameof(DoHeartbeatAsync)} cancelled while waiting to send heartbeat");
                return;
            }

            if (_connection.State == HubConnectionState.Connected)
            {
                // Send the heartbeat; in case of cancellation: end the Task
                try
                {
                    await _connection.InvokeAsync(nameof(IDeviceHubServer.ReceiveDeviceHeartbeat), dto, TokenSource.Token);
                    Console.WriteLine($"{nameof(DoHeartbeatAsync)} {heartbeatNumber} succeded");
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine($"{nameof(DoHeartbeatAsync)} {heartbeatNumber} cancelled while sending heartbeat");
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{nameof(DoHeartbeatAsync)} {heartbeatNumber} error while sending heartbeat: {e.Message}");
                }
            }
            else
            {
                Console.WriteLine($"{nameof(DoHeartbeatAsync)} {heartbeatNumber} skipped (no connection)");
            }
        }
    }

    #endregion Heartbeat
}