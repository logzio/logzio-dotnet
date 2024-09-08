[![Travis Build Status](https://travis-ci.org/logzio/logzio-dotnet.svg?branch=master)](https://travis-ci.org/logzio/logzio-dotnet)

# logzio-dotnet

This repository contains the [Logz.io](http://www.logz.io) shippers for .NET frameworks, currently including [log4net](https://logging.apache.org/log4net/) and [NLog](http://nlog-project.org/).

Prerequisites:
.Net Core SDK version 2.0 or above

- [log4net appender installation and configuration](docs/log4net.md)  [![NuGet](https://img.shields.io/nuget/v/Logzio.DotNet.Log4net.svg)](https://www.nuget.org/packages/Logzio.DotNet.Log4net)
- [NLog target installation and configuration](docs/nlog.md) [![NuGet](https://img.shields.io/nuget/v/Logzio.DotNet.NLog.svg)](https://www.nuget.org/packages/Logzio.DotNet.NLog)

## Features
- Async, non-blocking and non-throwing logging to [Logz.io](http://www.logz.io)
- Logs are uploaded in bulks of 100 messages (configurable) or a timeout of 5 seconds (configurable)
- Up to 3 retries (configurable) 2 seconds apart (configurable) in case the upload fails, for whatever reason
- On console applications, logs are flushed before the app exits
- Enable debug mode to see debug messages and errors in the console output and trace log
- Provided with sample applications and configuration examples

## Build and Test Locally

This project uses .NET 8.0 and can be built and tested locally. Follow the steps below to do so:

1. Clone the repository

   ```bash
   git clone https://github.com/logzio/logzio-dotnet.git
   ```

2. go to `./src` directory

  ```bash
  cd logzio-dotnet/src
  ```

3. build and run tests

  ```bash
  dotnet restore logzio-dotnet.sln
  dotnet build logzio-dotnet.sln /p:Configuration=Release
  dotnet test ./UnitTests/UnitTests.csproj
  dotnet test ./IntegrationTests/IntegrationTests.csproj
  ```