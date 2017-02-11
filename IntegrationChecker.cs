using System;
using System.Net;
using System.Text;

namespace BambooMonitor
{
    class IntegrationChecker
    {
        internal IntegrationChecker(Config config)
        {
            mConfig = config;
        }

        internal bool IsIntegrable(string branchName)
        {
            string taskNumber = GetTaskNumber(branchName);
            if (string.IsNullOrEmpty(taskNumber))
                return false;

            string taskPage = RetrieveTaskPage(taskNumber);
            if (string.IsNullOrEmpty(taskPage))
                return false;
            return true;
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
                // log
                return string.Empty;
            }
        }

        Config mConfig;

        const string TASK_IDENTIFIER = "scm";
        const string TTS_TASK_RELATIVE_URI = "/tts/visualize.php";
        const string TTS_TASK_ARGUMENT = "iddefect";
    }
}
