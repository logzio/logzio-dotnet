using NUnit.Framework;

[assembly: Parallelizable(ParallelScope.None)]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config")]
