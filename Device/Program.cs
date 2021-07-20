using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR.Client;

using IoTAS.Shared.Hubs;

namespace IoTAS.Device
{
    class Program
    {
        //Hardcoded for now, possibly get from configuration lateron
        private static readonly string hubUrlHttps = "https://localhost:44388" + IDeviceHubServer.path;
        // private static readonly string hubUrlHttp  = "http://localhost:58939" + IDeviceHubServer.path;

        // Must be a field so the event handlers can access them :-(.
        private static readonly CancellationTokenSource tokenSource = new();
        private static HubConnection connection;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Device started");
            
            //  In future this might be passed in as a configuration item or a command argument
            string hubUrl = hubUrlHttps;

            Console.WriteLine($"Using server at {hubUrl}");
            Console.WriteLine();

            //  In future this might be passed in as a configuration item or a command argument
            int deviceId = GetDeviceId();
            if (deviceId != 0)
            {
                SetupCntrlcHandler();

                connection = new HubConnectionBuilder()
                    .WithUrl(hubUrl)
                    .Build();

                if (connection == null)
                {
                    Console.WriteLine("Error in connection builder");
                    // Allow user time to read the message in the console window before it closes
                    await Task.Delay(5000, tokenSource.Token);
                }

                bool started = await StartAndConfigureConnectionAsync(connection, tokenSource.Token);
                if (started)
                {
                    bool registered = await SendRegistrationAsync(connection, deviceId, tokenSource.Token);
                    if (registered)
                    {
                        await SendHeartbeatAsync(connection, deviceId, tokenSource.Token);
                    }
                }
                //
                // We only get here after a cancellation on the tokenSource (from Cntrl-C)
                //
                if (started)
                {
                    await TeardownConnectionAsync(connection);
                }

                tokenSource.Dispose();
            }

            Console.WriteLine("Bye ...");

            await Task.Delay(1000); // Allow user to see the Bye message
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
        /// Sets the Cntrl-C and Cntrl-Break handler to canvel the tokenSource, 
        /// which will then gracefully shut down the program.
        /// </summary>
        private static void SetupCntrlcHandler()
        {
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Console.WriteLine("Cancelling due to Cntrl-C ...");
                tokenSource.Cancel();
                eventArgs.Cancel = true;    // continue running
            };
        }

        #endregion  UserInteraction

        #region HubConnection Management

        /// <summary>
        /// Starts the HubConnection and configures it for lifecycle event handling, including automatic reconnection
        /// </summary>
        /// <param name="connection">The HubConnection that needs to be started and configured</param>
        /// <param name="token">Cancellation token to end the Task</param>
        /// <returns>
        /// <see langword="true"/> upon succesful connection startup, <see langword="false"/> otherwise
        /// </returns>
        private static async Task<bool> StartAndConfigureConnectionAsync(HubConnection connection, CancellationToken token)
        {
            bool connected = await ConnectWithRetryAsync(connection, token);

            if (connected)
            {
                connection.Closed += (e) => ConnectionClosedHandler(e);
                connection.Reconnecting += (e) => ConnectionReconnectingHandler(e);
                connection.Reconnected += (newId) => ConnectionReconnectedHandler(newId); ;
            }
            return connected;
        }

        /// <summary>
        /// (Re-)Starts the HubConnection untill succesfull
        /// </summary>
        /// <param name="connection">The HubConnection that needs to be started</param>
        /// <param name="token">Cancellation token to end the Task</param>
        /// <returns>
        /// <see langword="true"/> upon succesful connection startup, <see langword="false"/> otherwise
        /// </returns>
        private static async Task<bool> ConnectWithRetryAsync(HubConnection connection, CancellationToken token)
        {
            Random random = null;
            int delayIncrease = 0;

            // Keep trying to until we can start or the token is canceled.
            while (!token.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine("Trying to connect ...");
                    await connection.StartAsync(token);
                    Console.WriteLine("Connection has been established");
                    return true;
                }
                catch (TaskCanceledException)
                {
                    // fall-thru and return false
                }
                catch (Exception e)
                {
                    random ??= new Random();
                    int delay = random.Next(2500, 5000) + delayIncrease;
                    if (delayIncrease <= 12500) delayIncrease += 2500;

                    Console.WriteLine($"Error while connecting: {e.Message}; retrying in {delay} msec");
                    try
                    {
                        await Task.Delay(delay, token);
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

            // try re-connecting
            Console.WriteLine($"Connection to server has been lost: {e.Message}");
            return ConnectWithRetryAsync(connection, tokenSource.Token);
        }

        private static Task ConnectionReconnectingHandler(Exception e)
        {
            Console.WriteLine($"Reconnecting to server due to error: {e.Message}");
            return Task.CompletedTask;
        }

        private static Task ConnectionReconnectedHandler(string newId)
        {
            Console.WriteLine($"Reconnected to server, connectionId = \"{newId}\"");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Teardown and Dispose connection
        /// </summary>
        /// <param name="connection">The connection to teardown</param>
        /// <returns></returns>
        private static async Task TeardownConnectionAsync(HubConnection connection)
        {
            Console.WriteLine($"Tearing down connection ...");
            if (connection == null) return;

            Console.WriteLine($"Removing event handlers ...");
            connection.Closed -= ConnectionClosedHandler;
            connection.Reconnecting -= ConnectionReconnectingHandler;
            connection.Reconnected -= ConnectionReconnectedHandler;

            if (connection.State == HubConnectionState.Connected)
            {
                Console.WriteLine($"Stopping the connection ...");
                await connection.StopAsync();
                Console.WriteLine($"Connection has been stopped by Device");
            }

            await connection.DisposeAsync();
            Console.WriteLine($"Connection has been disposed");
        }

        # endregion HubConnection Management

        #region Registration

        /// <summary>
        /// Send a Device Registration message to the Server
        /// </summary>
        /// <param name="connection">The HubConnection on which the Registration is to be send</param>
        /// <param name="deviceId">The DeviceId for this Device</param>
        /// <param name="token">Cancellation token to end the Task</param>
        /// <returns><see langword="true"/> upon succesful registration, <see langword="false"/> otherwise</returns>
        private static async Task<bool> SendRegistrationAsync(HubConnection connection, int deviceId, CancellationToken token)
        {
            try
            {
                var dto = new DevToSrvDeviceRegistrationArgs(deviceId);
                Console.WriteLine($"Registering Device with Id {deviceId} ...");
                await connection.InvokeAsync(nameof(IDeviceHubServer.RegisterDeviceAsync), dto, token);
                Console.WriteLine("Registration succesfull");
                return true;
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("SendRegistrationAsync was cancelled");
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
        /// <param name="connection">The HubConnection on which the HandleHeartbeatAsync is to be send</param>
        /// <param name="deviceId">The DeviceId for this Device</param>
        /// <param name="token">Cancellation token to end the Task</param>
        /// <returns>A completed Task</returns>
        private static async Task SendHeartbeatAsync(HubConnection connection, int deviceId, CancellationToken token)
        {
            Console.WriteLine("Starting SendHeartbeatAsync task ...");
            var dto = new DevToSrvDeviceHeartbeatArgs(deviceId);
            int heartbeatNumber = 0;

            while (!token.IsCancellationRequested)
            {
                heartbeatNumber++;

                // Delay 15 seconds; in case of cancellation: end the Task
                try
                {
                    await Task.Delay(15 * 1000, token);
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("SendHeartbeatAsync cancelled while waiting to send heartbeat");
                    return;
                }

                // Send the heartbeat; in case of cancellation: end the Task
                try
                {
                    await connection.InvokeAsync(nameof(IDeviceHubServer.HandleHeartbeatAsync), dto, token);
                    Console.WriteLine($"SendHeartbeatAsync {heartbeatNumber} succeded");
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine($"SendHeartbeatAsync {heartbeatNumber} cancelled while sending heartbeat");
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"SendHeartbeatAsync {heartbeatNumber} error while sending heartbeat: {e.Message}");
                }
            }
        }

        #endregion Heartbeat
    }
}
