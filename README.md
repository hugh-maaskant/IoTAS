# IoTAS
An **I**ntranet **o**perated **T**otal **A**udio **S**olution for airports.
Also a [Manning liveProject](https://liveproject.manning.com/) titled [Building **IoT A**pplications with **S**ignalR](https://www.manning.com/liveproject/building-iot-applications-with-signalr) exercise. 
## Functional Objectives
The ultimate goal of this project is to build an IoT type solution that distributes audi recordings in real-time to various
inter/intranet connected smart public address speakers in an airport.

The liveProject consists of 4 milestones:
1.	Creating a basic SignalR setup
2.	Enabling real-time audio transfer via SignalR
3.	Enabling IoT applications to run as a single cluster
4.	Enabling IoT application deployment via Docker

Currently this repository is at the first milestone.

## Technologies
The solution needs to be build with [.NET 5](https://docs.microsoft.com/en-us/dotnet/core/dotnet-five) and 
[SignalR](https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-5.0) technologies.
While the original assignment is to use Javascript for the management website with real-time information,
I have decided to use [Blazor](https://docs.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-5.0) WASM instead,
as learning this is one of my personal goals.
