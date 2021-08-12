using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR.Client;

using IoTAS.Shared.Hubs;

namespace IoTAS.Device
{
    public sealed class Program
    {
        // Hardcoded for now, possibly get from configuration lateron
        private static readonly string hubUrlHttps = "https://localhost:5001" + IDeviceHubServer.path;
        // private static readonly string hubUrlHttps = "https://localhost:44388" + IDeviceHubServer.path;
        // private static readonly string hubUrlHttp  = "http://localhost:58939" + IDeviceHubServer.path;

        // Must be fields so the event handlers can access them :-(.
        private static readonly CancellationTokenSource tokenSource = new();
        private static HubConnection connection;
        private static int deviceId;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Device started");
            
            //  In future this might be passed in as a configuration item or a command argument
            string hubUrl = hubUrlHttps;

            Console.WriteLine($"Using server at {hubUrl}");
            Console.WriteLine();

            //  In future this might be passed in as a configuration item or a command argument
            deviceId = GetDeviceId();
            if (deviceId != 0)
            {
                // Set up CNTRL-C handling to cancel out of blocking async operations
                Console.CancelKeyPress += CntrlcHandler;

                connection = new HubConnectionBuilder()
                    .WithUrl(hubUrl)
                    .Build();

                if (connection is null)
                {
                    Console.WriteLine("Fatal error in connection builder");
                    // Allow user time to read the message in the console window before it closes
                    await Task.Delay(5000, tokenSource.Token);
                    Environment.Exit(-1);
                }

                // set connection life-cycle callbacks
                connection.Closed += ConnectionClosedHandler;
                connection.Reconnecting += ConnectionReconnectingHandler;
                connection.Reconnected += ConnectionReconnectedHandler;

                bool started = await StartAndRegisterAsync();
                if (started)
                {
                    await StartHeartbeatAsync();
                }
                
                // We only get here after a cancellation on the tokenSource (from Cntrl-C)
                await TeardownConnectionAsync();

                tokenSource.Dispose();
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
                string answer = Console.ReadLine().Trim();

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

                tokenSource.Cancel();

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
        /// (Re-)Starts the HubConnection untill succesfull
        /// </summary>
        /// <returns>
        /// <see langword="true"/> upon succesful connection startup, <see langword="false"/> otherwise
        /// </returns>
        private static async Task<bool> StartAndRegisterAsync()
        {
            Random random = null;
            int delayIncrease = 0;

            // Keep trying to until we can start or the token source is canceled.
            while (!tokenSource.IsCancellationRequested)
            {
                try
                {
                    if (connection.State != HubConnectionState.Connected)
                    {
                        Console.WriteLine("Trying to connect ...");
                        await connection.StartAsync(tokenSource.Token);
                        Console.WriteLine("Connection has been established");
                    }

                    return await RegisterDeviceAsync();
                }
                catch (TaskCanceledException)
                {
                    // fall-thru and return false
                }
                catch (Exception e)
                {
                    random ??= new Random();
                    int delay = random.Next(2500, 5000) + delayIncrease;
                    if (delayIncrease <= 20000) delayIncrease += 5000;

                    Console.WriteLine($"Error while connecting: {e.Message}; retrying in {delay} msec");
                    try
                    {
                        await Task.Delay(delay, tokenSource.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        Console.WriteLine("ConnectWithRetryAsync cancelled while waiting to retry connection");
                    }
                }
            }

            return false;
        }

        private static Task ConnectionClosedHandler(Exception e)
        {
            if (tokenSource.IsCancellationRequested)
            {
                Console.WriteLine("Connection to server closed due to cancellation");
                return Task.CompletedTask;
            }

            if (e == null)
            {
                Console.WriteLine("Connection to server closed by Server - exiting");
                return Task.CompletedTask;
            }

            Console.WriteLine($"Connection to server has been lost: {e.Message}");

            // try re-connecting and re-registering
            return StartAndRegisterAsync();
        }

        // Currently we do not use SignalR's auto-reconnect, so we shoeldd never get here ...
        private static Task ConnectionReconnectingHandler(Exception e)
        {
            Console.WriteLine($"Reconnecting to server due to error: {e.Message}");
            return Task.CompletedTask;
        }

        // Currently we do not use SignalR's auto-reconnect, so we shoeldd never get here ...
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
            Console.WriteLine($"Tearing down connection ...");
            if (connection == null) return;

            Console.WriteLine($"Removing life-cycle event handlers ...");
            connection.Closed -= ConnectionClosedHandler;
            connection.Reconnecting -= ConnectionReconnectingHandler;
            connection.Reconnected -= ConnectionReconnectedHandler;

            if (connection.State == HubConnectionState.Connected)
            {
                Console.WriteLine($"Stopping the connection ...");
                await connection.StopAsync();
                Console.WriteLine($"Connection has been stopped by Device");
            }
            else
            {
                Console.WriteLine($"Connection was not connected, no need to stop it.");
            }

            await connection.DisposeAsync();
            Console.WriteLine($"Connection has been disposed");
        }

        # endregion HubConnection Management

        #region Registration

        /// <summary>
        /// Send a Device Registration message to the Server
        /// </summary>
        /// <returns><see langword="true"/> upon succesful registration, <see langword="false"/> otherwise</returns>
        private static async Task<bool> RegisterDeviceAsync()
        {
            try
            {
                var dto = new DevToSrvDeviceRegistrationDto(deviceId);
                Console.WriteLine($"Registering Device with Id {deviceId} ...");
                await connection.InvokeAsync(nameof(IDeviceHubServer.RegisterDeviceClient), dto, tokenSource.Token);
                Console.WriteLine("Registration succesfull");
                return true;
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("RegisterDeviceAsync was cancelled");
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while registering: {e.Message}");
                return false;
            }
        }

    #endregion Registration

        #region Heartbeat

        /// <summary>
        /// Sends Hearbeat messages with 15 second interval
        /// </summary>
        /// <returns>A completed Task</returns>
        private static async Task StartHeartbeatAsync()
        {
            Console.WriteLine($"{nameof(StartHeartbeatAsync)} Task started");
            var dto = new DevToSrvDeviceHeartbeatDto(deviceId);
            int heartbeatNumber = 0;

            while (!tokenSource.IsCancellationRequested)
            {
                heartbeatNumber++;

                // Delay 15 seconds; in case of cancellation: end the Task
                try
                {
                    await Task.Delay(15 * 1000, tokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine($"{nameof(StartHeartbeatAsync)} cancelled while waiting to send heartbeat");
                    return;
                }

                // Send the heartbeat; in case of cancellation: end the Task
                try
                {
                    if (connection.State == HubConnectionState.Connected)
                    {
                        await connection.InvokeAsync(nameof(IDeviceHubServer.ReceiveDeviceHeartbeat), dto, tokenSource.Token);
                        Console.WriteLine($"{nameof(StartHeartbeatAsync)} {heartbeatNumber} succeded");
                    }
                    else
                    {
                        Console.WriteLine($"{nameof(StartHeartbeatAsync)} {heartbeatNumber} skipped (no connection)");
                    }
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine($"{nameof(StartHeartbeatAsync)} {heartbeatNumber} cancelled while sending heartbeat");
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{nameof(StartHeartbeatAsync)} {heartbeatNumber} error while sending heartbeat: {e.Message}");
                }
            }
        }

        #endregion Heartbeat
    }
}
