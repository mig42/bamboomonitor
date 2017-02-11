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

            List<string> branchesToIntegrate = RetrieveBranchesToIntegrate(config);
            if (branchesToIntegrate == null)
            {
                Console.WriteLine(
                    "Unable to retrieve the integrable branches. See log for further details.");
                Environment.Exit(1);
            }

            EnqueueBuildsInBamboo(config, branchesToIntegrate);
        }

        static List<string> RetrieveBranchesToIntegrate(Config config)
        {
            try
            {
                List<string> resolvedBranches = GetResolvedBranches(config);
                return FilterIntegrableBranches(config, resolvedBranches);
            }
            catch (Exception e)
            {
                mLog.ErrorFormat("Error retrieving integrable branches: {0}", e.Message);
                mLog.DebugFormat("Stack trace:{0}{1}", Environment.NewLine, e.StackTrace);
                return null;
            }
        }

        static List<string> GetResolvedBranches(Config config)
        {
            string cmdOutput = CmdRunner.ExecuteCommandWithStringResult(
                string.Format(
                    "cm find \"branch where name like '{0}%' and attribute='status' "
                    + "and attrvalue='RESOLVED' on repository '{1}'\" "
                    + "--format={{name}} --nototal ",
                    config.PlasticBranchPrefix,
                    config.PlasticRepo),
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

        static void EnqueueBuildsInBamboo(Config config, List<string> branchesToIntegrate)
        {
            if (branchesToIntegrate.Count == 0)
                return;

            BambooBuildEnqueuer enqueuer = new BambooBuildEnqueuer(config);
            foreach (string branch in branchesToIntegrate)
            {
                string planKey = enqueuer.GetBranchPlanKey(branch);
                if (string.IsNullOrEmpty(planKey))
                {
                    string message = string.Format(
                        "Couldn't find a build plan for branch {0}", branch);
                    Console.WriteLine(message);
                    mLog.ErrorFormat(message);
                    continue;
                }

                try
                {
                    enqueuer.EnqueueBuild(planKey);
                }
                catch (Exception e)
                {
                    Console.WriteLine(
                        "Unable to enqueue a build for branch {0}, plan {1} in "
                        + "Bamboo. See log for further details.", branch, planKey);
                    mLog.ErrorFormat(
                        "Error enqueuing branch {0} plan build {1}: {2}",
                        planKey,
                        branch,
                        e.Message);
                    mLog.DebugFormat("Stack trace:{0}{1}", Environment.NewLine, e.StackTrace);
                }
            }
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
