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
            string bambooBranchName = ConvertBranchToBambooFormat(branch);

            try
            {
                string relUri = string.Join(
                    AuthenticatedWebClient.URI_SEPARATOR,
                    REST_API_BASE_URI,
                    string.Format(BRANCH_URI, mConfig.BambooPlan, bambooBranchName));

                string result = AuthenticatedWebClient.Get(
                    mConfig.BambooServer,
                    relUri,
                    mConfig.BambooUser,
                    mConfig.BambooPassword,
                    mQueryParams);

                if (string.IsNullOrEmpty(result))
                    return string.Empty;

                BambooBranch bambooBranch =
                    JsonConvert.DeserializeObject<BambooBranch>(result);

                return bambooBranch.Key;
            }
            catch (WebException e)
            {
                mLog.ErrorFormat(
                    "Unable to find the plan key for branch {0}: {1}",
                    bambooBranchName, e.Message);
                if (e.Response != null)
                    mLog.ErrorFormat("Queried URI: {0}", e.Response.ResponseUri);
                mLog.DebugFormat("Stack trace:{0}{1}", Environment.NewLine, e.StackTrace);
                return string.Empty;
            }
        }

        internal void EnqueueBuild(string branchPlanKey)
        {
            try
            {
                string relUri = string.Join(
                    AuthenticatedWebClient.URI_SEPARATOR,
                    REST_API_BASE_URI,
                    string.Format(QUEUE_URI, branchPlanKey));

                string result = AuthenticatedWebClient.Post(
                    mConfig.BambooServer,
                    relUri,
                    mConfig.BambooUser,
                    mConfig.BambooPassword,
                    mQueryParams);
            }
            catch (WebException e)
            {
                mLog.ErrorFormat(
                    "Unable to enqueue build for plan {0}: {1}", branchPlanKey, e.Message);
                mLog.DebugFormat("Stack trace:{0}{1}", Environment.NewLine, e.StackTrace);
                throw;
            }
        }

        string ConvertBranchToBambooFormat(string plasticBranch)
        {
            return plasticBranch.Replace(PLASTIC_BRANCH_SEPARATOR, BAMBOO_BRANCH_SEPARATOR);
        }

        Config mConfig;

        static readonly ILog mLog = LogManager.GetLogger("bamboomonitor");
        static readonly Dictionary<string, string> mQueryParams =
            new Dictionary<string, string> { {AUTH_PARAM, AUTH_TYPE} };

        const string REST_API_BASE_URI = "rest/api/latest";
        const string BRANCH_URI = "plan/{0}/branch/{1}.json";
        const string QUEUE_URI = "queue/{0}.json";

        const string AUTH_PARAM = "os_authType";
        const string AUTH_TYPE = "basic";

        const char PLASTIC_BRANCH_SEPARATOR = '/';
        const char BAMBOO_BRANCH_SEPARATOR = '-';
    }
}
