using System;
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
            int idIndex = branchName.LastIndexOf(TASK_IDENTIFIER);
            if (idIndex < 0)
                return string.Empty;

            return branchName.Substring(idIndex + TASK_IDENTIFIER.Length);
        }

        string RetrieveTaskPage(string taskNumber)
        {
            try 
            {
                using (WebClient client = new WebClient())
                {
                    client.BaseAddress = mConfig.TtsServer;
                    byte[] authData = Encoding.ASCII.GetBytes(string.Concat(
                        mConfig.TtsUser, ":", mConfig.TtsPassword));
                    client.Headers[HttpRequestHeader.Authorization] =
                        string.Concat("Basic ", Convert.ToBase64String(authData));

                    client.QueryString.Add(TTS_TASK_ARGUMENT, taskNumber);

                    return client.DownloadString(TTS_TASK_RELATIVE_URI);
                }
            }
            catch (WebException e)
            {
                mLog.ErrorFormat(
                    "Unable to query TTS for task {0}: {1}", taskNumber, e.Message);
                mLog.DebugFormat("Stack trace:{0}{1}", Environment.NewLine, e.StackTrace);
                return string.Empty;
            }
        }

        Config mConfig;

        static readonly ILog mLog = LogManager.GetLogger("bamboomonitor");

        const string TASK_IDENTIFIER = "scm";
        const string TTS_TASK_RELATIVE_URI = "visualize.php";
        const string TTS_TASK_ARGUMENT = "iddefect";
    }
}
