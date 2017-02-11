using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using log4net;
using log4net.Config;

using Codice.CmdRunner;

namespace BambooMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigureLogging();

            string configFile = Path.Combine(GetConfigPath(), CONFIG_FILE);

            Config config = Config.ParseFromFile(CONFIG_FILE);
            if (config == null)
            {
                Console.WriteLine("Unable to get configuration from " + configFile);
                Environment.Exit(1);
            }
            
            try
            {
                List<string> resolvedBranches = GetResolvedBranches(config.PlasticRepo);

                List<string> integrableBranches = FilterIntegrableBranches(
                    config, resolvedBranches);
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Unable to retrieve the integrable branches. See log for further details.");
                mLog.ErrorFormat("Error retrieving integrable branches: {0}", e.Message);
                mLog.DebugFormat("Stack trace:{0}{1}", Environment.NewLine, e.StackTrace);
                Environment.Exit(1);
            }
        }

        static List<string> GetResolvedBranches(string repo)
        {
            string cmdOutput = CmdRunner.ExecuteCommandWithStringResult(
                "cm find \"branch where attribute='status' and attrvalue='RESOLVED' " +
                "on repository '" + repo + "'\" --format={name} --nototal ",
                Environment.CurrentDirectory);

            return new List<string>(cmdOutput.Split(
                new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
        }

        static List<string> FilterIntegrableBranches(
            Config config, List<string> resolvedBranches)
        {
            IntegrationChecker checker = new IntegrationChecker(config);

            List<string> result = new List<string>();
            foreach (string resolvedBranch in resolvedBranches)
            {
                if (checker.CanBeIntegrated(resolvedBranch))
                    result.Add(resolvedBranch);
            }
            return result;
        }

        static void ConfigureLogging()
        {
            string log4netPath = Path.Combine(GetConfigPath(), LOG_CONFIG_FILE);

            if (!File.Exists(log4netPath))
                return;

            XmlConfigurator.ConfigureAndWatch(new FileInfo(log4netPath));
        }

        static string GetConfigPath()
        {
            return Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);
        }

        static readonly ILog mLog = LogManager.GetLogger("bamboomonitor");

        const string LOG_CONFIG_FILE = "bamboomonitor.log.conf";
        const string CONFIG_FILE = "bamboomonitor.conf";
    }
}
