# Logz.io NLog Target

- [Configuration](#configuration)
	- [XML](#xml)
	- [Code](#code)
- [Variables](#variables)
- [Extensibility](#extensibility)


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
                You can also add and edit this parameter to enable proxy routing:
                proxyAddress = "http://your.proxy.com:port"            
            -->
			<target name="logzio" type="Logzio" 
				token="DKJiomZjbFyVvssJDmUAWeEOSNnDARWz" 
				logzioType="nlog"
				listenerUrl="https://listener.logz.io:8071"
				bufferSize="100"
				bufferTimeout="00:00:05"
				retriesMaxAttempts="3"
				retriesInterval="00:00:02"
				includeEventProperties="true"
				useGzip="false"
				debug="false">
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

```C#			
	var config = new LoggingConfiguration();
	var logzioTarget = new LogzioTarget {
		Token = "DKJiomZjbFyVvssJDmUAWeEOSNnDARWz",
		// Uncomment and edit this line to add a proxy routing
		// ProxyAddress = "http://your.proxy.com:port"
	};
	config.AddTarget("Logzio", logzioTarget);
	config.AddRule(LogLevel.Debug, LogLevel.Fatal, "Logzio", "*");
	LogManager.Configuration = config;
```

## Context Properties

You can configure the target to include your own custom values when forwarding to Logzio. For example:

```xml
	<nlog>
		<variable name="site" value="New Zealand" />
		<variable name="rings" value="one" />
		<target name="logzio" type="Logzio" token="DKJiomZjbFyVvssJDmUAWeEOSNnDARWz" includeEventProperties="true" includeMdlc="false">
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
