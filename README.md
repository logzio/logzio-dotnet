# logzio-dotnet

This repository contains the Logz.io shippers for .NET frameworks, currently including [log4net](https://logging.apache.org/log4net/) and [NLog](http://nlog-project.org/).

- [log4net appender installation and configuration](docs/log4net.md)
- [NLog target installation and configuration](docs/nlog.md)

## Features
- Async, non-blocking and non-throwing logging to Logz.io
- Logs are uploaded in bulks of 100 messages (configurable) or a timeout of 5 seconds (configurable)
- Up to 3 retries (configurable) 2 seconds apart (configurable) in case the upload fails, for whatever reason
- Enable debug mode to see debug messages or errors in the console or trace log
- Provided with sample applications and configuration examples