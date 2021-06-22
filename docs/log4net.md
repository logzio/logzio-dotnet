# Logz.io log4net Appender

- [Configuration](#configuration)
	- [XML](#xml)
	- [Code](#code)
- [Custom Fields](#custom-fields)
- [Extensibility](#extensibility)
- [Code Sample](#code-sample)


Install the log4net appender from the Package Manager Console:

    Install-Package Logzio.DotNet.Log4net

If you prefer to install the library manually, download the latest version from the releases page.

## Configuration
### XML
If you configure your logging in an XML file, simply add a reference to the Logz.io appender.

```xml
    <log4net>
    	<appender name="LogzioAppender" type="Logzio.DotNet.Log4net.LogzioAppender, Logzio.DotNet.Log4net">
    		<!-- 
				Required fields 
			-->
			<!-- Your Logz.io API token -->
			<token>DKJiomZjbFyVvssSZmWATeHAHAnDARWz</token>
			
			<!-- 
				Optional fields (with their default values) 
			-->
			<!-- The type field will be added to each log message, making it 
			easier for you to differ between different types of logs. -->
    		        <type>log4net</type>
			<!-- The URL of the Lgz.io listener -->
    		        <listenerUrl>https://listener.logz.io:8071</listenerUrl>
                        <!--Optional proxy server address:
                        proxyAddress = "http://your.proxy.com:port" -->
			<!-- The maximum number of log lines to send in each bulk -->
    		        <bufferSize>100</bufferSize>
			<!-- The maximum time to wait for more log lines, in a hh:mm:ss.fff format -->
    		        <bufferTimeout>00:00:05</bufferTimeout>
			<!-- If connection to Logz.io API fails, how many times to retry -->
    	         	<retriesMaxAttempts>3</retriesMaxAttempts>
    		        <!-- Time to wait between retries, in a hh:mm:ss.fff format -->
			<retriesInterval>00:00:02</retriesInterval>
			<!-- Set the appender to compress the message before sending it -->
		        <gzip>true</gzip>
			<!-- Uncomment this to enable sending logs in Json format -->
				<!--<parseJsonMessage>true</parseJsonMessage>-->
			<!-- Enable the appender's internal debug logger (sent to the console output and trace log) -->
			<debug>false</debug>
    	</appender>
    
    	<root>
    		<level value="INFO" />
    		<appender-ref ref="LogzioAppender" />
    	</root>
    </log4net>
```
Add a reference to the configuration file in your code, as shown in the example [here](https://github.com/logzio/logzio-dotnet/blob/master/sample-applications/LogzioLog4netSampleApplication/Program.cs).
### Code
To add the Logz.io appender via code, add the following lines:

```C#
	var hierarchy = (Hierarchy)LogManager.GetRepository();
	var logzioAppender = new LogzioAppender();
	logzioAppender.AddToken("DKJiomZjbFyVvssSZmWATeHAHAnDARWz");
	logzioAppender.AddListenerUrl("https://webhook.site/b229776a-3396-4c9d-9e05-b5350aeb98fa");
	 // <-- Uncomment and edit this line to enable proxy routing: --> 
	 // logzioAppender.AddProxyAddress("http://your.proxy.com:port");
	 // <-- Uncomment this to enable sending logs in Json format -->  
	 // logzioAppender.ParseJsonMessage(true);
	 // <-- Uncomment these lines to enable gzip compression --> 
	 // logzioAppender.AddGzip(true);
         // logzioAppender.ActivateOptions();
	hierarchy.Root.AddAppender(logzioAppender);
	hierarchy.Configured = true;
```

## Custom Fields

You can add static keys and values to be added to all log messages. For example:

```XML
    	<appender name="LogzioAppender" type="Logzio.DotNet.Log4net.LogzioAppender, Logzio.DotNet.Log4net">
			<token>DKJiomZjbFyVvssSZmWATeHAHAnDARWz</token>
			<customField>
				<key>Environment</key>
				<value>Production</value>
			</customField>
			<customField>
				<key>Location</key>
				<value>New Jerseay B1</value>
			</customField>
    	</appender>
```

## Extensibility 

If you want to change some of the fields or add some of your own, inherit the appender and override the `ExtendValues` method:

```C#
	public class MyAppLogzioAppender : LogzioAppender
	{
		protected override void ExtendValues(LoggingEvent loggingEvent, Dictionary<string, string> values)
		{
			values["logger"] = "MyPrefix." + values["logger"];
			values["myAppClientId"] = new ClientIdProvider().Get();
		}
	}
```

You will then have to change your configuration in order to use your own appender.

## Code Sample

```C#
using System.IO;
using log4net;
using log4net.Config;
using System.Reflection;

namespace dotnet_log4net
{
   class Program
   {
       static void Main(string[] args)
       {
          var logger = LogManager.GetLogger(typeof(Program));
          var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());

          // Replace "App.config" with the config file that holds your log4net configuration
          XmlConfigurator.Configure(logRepository, new FileInfo("App.config"));

          logger.Info("Now I don't blame him 'cause he run and hid");
          logger.Info("But the meanest thing he ever did");
          logger.Info("Before he left was he went and named me Sue");

          LogManager.Shutdown();
       }
   }
}
```