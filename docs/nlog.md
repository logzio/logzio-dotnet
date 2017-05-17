# Logz.io NLog Target

- [Configuration](#configuration)
	- [XML](#xml)
	- [Code](#code)
- [Variables](#variables)
- [Extensibility](#extensibility)


Install the NLog target from the Package Manager Console:

    Install-Package Logzio.DotNet.NLog

If you prefer to install the library manually, download the latest version from the releases page.

##Configuration
### XML
If you configure your logging in an XML file, you need to register the assembly and then reference the target.

```xml
	<nlog>
		<extensions>
			<add assembly="Logzio.DotNet.NLog"/>
		</extensions>
		
		<targets>
			<!-- parameters are shown here with their default values. 
				Other than the token, all of the fields are optional and can be safely omitted. -->
			<target name="logzio" type="Logzio" 
				token="DKJiomZjbFyVvssJDmUAWeEOSNnDARWz" 
				logzioType="nlog"
				listenerUrl="https://listener.logz.io:8071"
				bufferSize="100"
				bufferTimeout="00:00:05"
				retriesMaxAttempts="3"
				retriesInterval="00:00:02"
				debug="false" />
		</targets>
		<rules>
				<logger name="*" minlevel="Info" writeTo="logzio" />
		</rules>
	</nlog>
```
###Code
To add the Logz.io target via code, add the following lines:

```C#			
	var config = new LoggingConfiguration();
	var logzioTarget = new LogzioTarget {
		Token = "DKJiomZjbFyVvssJDmUAWeEOSNnDARWz",
	};
	config.AddTarget("Logzio", logzioTarget);
	config.AddRule(LogLevel.Debug, LogLevel.Fatal, "Logzio", "*");
	LogManager.Configuration = config;
```

##Variables

Any variables defined in the NLog configuration will be forwarded to Logzio. For example:

```xml
	<nlog>
		<variable name="site" value="New Zealand" />
		<variable name="rings" value="one" />
	</nlog>
```

##Extensibility 

If you want to change some of the fields or add some of your own, inherit the target and override the `ExtendValues` method:

```C#
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
