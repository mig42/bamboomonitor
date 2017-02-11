using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BambooMonitor
{
    class BambooBuildEnqueuer
    {
        internal BambooBuildEnqueuer(Config config)
        {
            mConfig = config;
        }

        internal string GetBranchPlanKey(string branch)
        {
            string gitBranch = ConvertBranchToGitFormat(branch);

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.BaseAddress = mConfig.BambooServer;
                    byte[] authData = Encoding.ASCII.GetBytes(string.Concat(
                        mConfig.BambooUser, ":", mConfig.BambooPassword));
                    client.Headers[HttpRequestHeader.Authorization] =
                        string.Concat("Basic ", Convert.ToBase64String(authData));

                    client.QueryString.Add(AUTH_PARAM, AUTH_TYPE);

                    string relUri = string.Concat(
                        REST_API_BASE_URI,
                        string.Format(BRANCH_URI, mConfig.BambooPlan, gitBranch));

                    string result = client.DownloadString(relUri);

                    if (string.IsNullOrEmpty(result))
                        return string.Empty;

                    BambooBranch bambooBranch =
                        JsonConvert.DeserializeObject<BambooBranch>(result);

                    return bambooBranch.Key;
                }
            }
            catch (WebException e)
            {
                mLog.ErrorFormat(
                    "Unable to find the plan key for branch {0}: {1}", gitBranch, e.Message);
                mLog.DebugFormat("Stack trace:{0}{1}", Environment.NewLine, e.StackTrace);
                return string.Empty;
            }
        }

        internal void EnqueueBuild(string branchPlanKey)
        {

        }

        string ConvertBranchToGitFormat(string plasticBranch)
        {
            plasticBranch = plasticBranch.TrimStart(PLASTIC_BRANCH_SEPARATOR);

            return plasticBranch.Replace(PLASTIC_BRANCH_SEPARATOR, GIT_BRANCH_SEPARATOR);
        }

        Config mConfig;

        static readonly ILog mLog = LogManager.GetLogger("bamboomonitor");

        const string REST_API_BASE_URI = "/rest/api/latest";
        const string BRANCH_URI = "/plan/{0}/branch/{1}.json";

        const string AUTH_PARAM = "os_authType";
        const string AUTH_TYPE = "basic";

        const char PLASTIC_BRANCH_SEPARATOR = '/';
        const char GIT_BRANCH_SEPARATOR = '-';
    }
}
