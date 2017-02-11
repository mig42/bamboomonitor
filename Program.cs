using System;
using System.Collections.Generic;

using Codice.CmdRunner;

namespace BambooMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            Config config = Config.ParseFromFile(DEFAULT_CONFIG_FILE);
            if (config == null)
            {
                Console.WriteLine("Missing configuration file " + DEFAULT_CONFIG_FILE);
                Environment.Exit(1);
            }

            List<string> resolvedBranches = GetResolvedBranches(config.PlasticRepo);

            foreach (string resolvedBranch in resolvedBranches)
                Console.WriteLine(resolvedBranch);

            List<string> integrableBranches = FilterIntegrableBranches(
                config, resolvedBranches);
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

        const string DEFAULT_CONFIG_FILE = "bamboomonitor.conf";
    }
}
