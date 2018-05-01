[![Travis Build Status](https://travis-ci.org/logzio/logzio-dotnet.svg?branch=master)](https://travis-ci.org/logzio/logzio-dotnet)

# logzio-dotnet

This is a fork of the official .NET log shippers of [Logz.io](http://www.logz.io).

This repository contains the [Logz.io](http://www.logz.io) shippers for .NET frameworks, currently including [log4net](https://logging.apache.org/log4net/) and [NLog](http://nlog-project.org/).

- [log4net appender installation and configuration](docs/log4net.md)
- [NLog target installation and configuration](docs/nlog.md)

## Features
- Async, non-blocking and non-throwing logging to [Logz.io](http://www.logz.io)
- Logs are uploaded in bulks of 100 messages (configurable) or a timeout of 5 seconds (configurable)
- Up to 3 retries (configurable) 2 seconds apart (configurable) in case the upload fails, for whatever reason
- On console applications, logs are flushed before the app exits
- Enable debug mode to see debug messages and errors in the console output and trace log
- Provided with sample applications and configuration examples
