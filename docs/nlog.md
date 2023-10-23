# Logz.io NLog Target

- [Configuration](#configuration)
	- [XML](#xml)
	- [Code](#code)
- [Variables](#variables)
- [Extensibility](#extensibility)
- [Trace Context](#trace-context)


Install the NLog target from the Package Manager Console:

    Install-Package Logzio.DotNet.NLog

If you prefer to install the library manually, download the latest version from the releases page.

## Configuration
### XML
If you configure your logging in an XML file, you need to register the assembly and then reference the target.

```xml
<nlog>
    <extensions>
	<add assembly="Logzio.DotNet.NLog"/>
    </extensions>
    <targets>
	<!-- parameters are shown here with their default values. 
	Other than the token, all of the fields are optional and can be safely omitted.            
        -->

	<target name="logzio" type="Logzio"
		token="<<SHIPPING-TOKEN>>"
		logzioType="nlog"
		listenerUrl="<<LISTENER-HOST>>:8071"
                <!--Optional proxy server address:
                proxyAddress = "http://your.proxy.com:port" -->
		bufferSize="100"
		bufferTimeout="00:00:05"
		retriesMaxAttempts="3"
		retriesInterval="00:00:02"
		includeEventProperties="true"
		useGzip="false"
		debug="false"
		debugLogFile="my_absolute_path\debug.txt"
		jsonKeysCamelCase="false"
		addTraceContext="false"
		<!-- parseJsonMessage="true"-->
        <!-- useStaticHttpClient="false"-->        
	>
		<contextproperty name="host" layout="${machinename}" />
		<contextproperty name="threadid" layout="${threadid}" />
	</target>
    </targets>
    <rules>
	<logger name="*" minlevel="Info" writeTo="logzio" />
    </rules>
</nlog>
```

### Code
To add the Logz.io target via code, add the following lines:

```c#
var config = new LoggingConfiguration();

// Replace these parameters with your configuration
var logzioTarget = new LogzioTarget {
    Name = "Logzio",
    Token = "<<SHIPPING-TOKEN>>",
    LogzioType = "nlog",
    ListenerUrl = "<<LISTENER-HOST>>:8071",
    BufferSize = 100,
    BufferTimeout = TimeSpan.Parse("00:00:05"),
    RetriesMaxAttempts = 3,
    RetriesInterval = TimeSpan.Parse("00:00:02"),
    Debug = false,
    DebugLogFile = "my_absolute_path_to_file",
    JsonKeysCamelCase = false,
    AddTraceContext = false,
    // ParseJsonMessage = true, 
    // ProxyAddress = "http://your.proxy.com:port",
    // UseStaticHttpClient = true,
};

config.AddRule(LogLevel.Debug, LogLevel.Fatal, logzioTarget);
LogManager.Configuration = config;
```

## Json Format

To parse your messages as Json add to the logger's configuration the field 'parseJsonMessage' with the value 'true' (or uncomment).  
When using 'JsonLayout' set the name of the attribute to **other than** 'message'. 
for example: 
```xml
<layout type="JsonLayout" includeAllProperties="true">
    <attribute name="msg"  layout="${message}" encode="false"/>
</layout>
````
Click here for more information about [JsonLayout](https://github.com/NLog/NLog/wiki/JsonLayout).

## Context Properties

You can configure the target to include your own custom values when forwarding to Logzio. For example:

```xml
<nlog>
    <variable name="site" value="New Zealand" />
    <variable name="rings" value="one" />
    <target name="logzio" type="Logzio" token="<<SHIPPING-TOKEN>>" includeEventProperties="true" includeMdlc="false">
	<contextproperty name="site" layout="${site}" />
	<contextproperty name="rings" layout="${rings}" />
    </target>
</nlog>
```

- includeEventProperties - Include NLog LogEvent Properties. Default=True
- includeMdlc - Include NLog MDLC properties by configuring. Default=False

Notice that the resulting messeage can grow in size to the point where it exceeds the endpoint's capacity. Changing to 'includeEventProperties="false"' will reduce the size of the message being shipped. Alternative you can enable `useGzip="true"`.

## Extensibility 

If you want to change some of the fields or add some of your own, inherit the target and override the `ExtendValues` method:

```C#
[Target("MyAppLogzio")]
public class MyAppLogzioTarget : LogzioTarget
{
    protected override void ExtendValues(LogEventInfo logEvent, Dictionary<string, string> values)
    {
	values["logger"] = "MyPrefix." + values["logger"];
	values["myAppClientId"] = new ClientIdProvider().Get();
    }
}
```

You will then have to change your configuration in order to use your own target.

## Trace Context

**WARNING**: Does not support .NET Standard 1.3

If you’re sending traces with OpenTelemetry instrumentation (auto or manual), you can correlate your logs with the trace context.
In this way, your logs will have traces data in it: span id and trace id.
To enable this feature, set `addTraceContext="true"` in your configuration or `AddTraceContext = true`
in your code (as shown in the previews sections).

## Serverless platforms
If you’re using a serverless function, you’ll need to call the appender's flush method at the end of the function run to make sure the logs are sent before the function finishes its execution. You’ll also need to create a static appender in the Startup.cs file so each invocation will use the same appender. The appender should have the `UseStaticHttpClient` flag set to `true`.

###### Azure serverless function code sample

*Startup.cs*

```csharp
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Logzio.DotNet.NLog;
using NLog;
using NLog.Config;
using System;

[assembly: FunctionsStartup(typeof(LogzioNLogSampleApplication.Startup))]

namespace LogzioNLogSampleApplication
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new LoggingConfiguration();

            // Replace these parameters with your configuration
            var logzioTarget = new LogzioTarget
            {
                Name = "Logzio",
                Token = "<<LOG-SHIPPING-TOKEN>>",
                LogzioType = "nlog",
                ListenerUrl = "https://<<LISTENER-HOST>>:8071",
                BufferSize = 100,
                BufferTimeout = TimeSpan.Parse("00:00:05"),
                RetriesMaxAttempts = 3,
                RetriesInterval = TimeSpan.Parse("00:00:02"),
                Debug = false,
                JsonKeysCamelCase = false,
                AddTraceContext = false,
                UseStaticHttpClient = true, 
                // ParseJsonMessage = true,
                // ProxyAddress = "http://your.proxy.com:port"
            };

            config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logzioTarget);
            LogManager.Configuration = config;
        }
    }
}
```

*FunctionApp.cs*

```csharp
using System;
using Microsoft.Azure.WebJobs;
using NLog;
using Microsoft.Extensions.Logging;
using MicrosoftLogger = Microsoft.Extensions.Logging.ILogger;

namespace LogzioNLogSampleApplication
{
    public class TimerTriggerCSharpNLog
    {
        private static readonly Logger nLog = LogManager.GetCurrentClassLogger();

        [FunctionName("TimerTriggerCSharpNLog")]
        public void Run([TimerTrigger("*/30 * * * * *")]TimerInfo myTimer, MicrosoftLogger msLog)
        {
            msLog.LogInformation($"NLogzio C# Timer trigger function executed at: {DateTime.Now}");

            nLog.WithProperty("iCanBe", "your long lost pal")
                .WithProperty("iCanCallYou", "Betty, and Betty when you call me")
                .WithProperty("youCanCallMe", "Al")
                .Info("If you'll be my bodyguard");
            // Call Flush method before function trigger finishes
            LogManager.Flush(5000);
        }
    }
}
```