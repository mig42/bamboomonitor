using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

using log4net;

namespace BambooMonitor
{
    class IntegrationChecker
    {
        internal IntegrationChecker(Config config)
        {
            mConfig = config;
        }

        internal bool CanBeIntegrated(string branchName)
        {
            string taskNumber = GetTaskNumber(branchName);
            if (string.IsNullOrEmpty(taskNumber))
                return false;

            TaskInfo taskInfo = TaskInfoParser.FromHtml(RetrieveTaskPage(taskNumber));
            if (taskInfo == null)
                return false;

            return taskInfo.CanBeIntegrated();
        }

        string GetTaskNumber(string branchName)
        {
            int idIndex = branchName.ToLowerInvariant().LastIndexOf(
                mConfig.PlasticBranchPrefix);

            if (idIndex < 0)
                return string.Empty;

            return branchName.Substring(idIndex + mConfig.PlasticBranchPrefix.Length);
        }

        string RetrieveTaskPage(string taskNumber)
        {
            try 
            {
                return AuthenticatedWebClient.Get(
                    mConfig.TtsServer,
                    TTS_TASK_RELATIVE_URI,
                    mConfig.TtsUser,
                    mConfig.TtsPassword,
                    BuildQueryParams(taskNumber));
            }
            catch (WebException e)
            {
                mLog.ErrorFormat(
                    "Unable to query TTS for task {0}: {1}", taskNumber, e.Message);
                if (e.Response != null)
                    mLog.ErrorFormat("Queried URI: {0}", e.Response.ResponseUri);
                mLog.DebugFormat("Stack trace:{0}{1}", Environment.NewLine, e.StackTrace);
                return string.Empty;
            }
        }

        Dictionary<string, string> BuildQueryParams(string taskNumber)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add(TTS_TASK_ARGUMENT, taskNumber);

            return result;
        }

        Config mConfig;

        static readonly ILog mLog = LogManager.GetLogger("bamboomonitor");

        const string TTS_TASK_RELATIVE_URI = "visualize.php";
        const string TTS_TASK_ARGUMENT = "iddefect";
    }
}
