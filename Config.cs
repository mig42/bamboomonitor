using System.IO;

using log4net;

namespace BambooMonitor
{
    class Config
    {
        internal string TtsServer;
        internal string TtsUser;
        internal string TtsPassword;
        internal string PlasticRepo;
        internal string PlasticBranchPrefix;
        internal string BambooServer;
        internal string BambooPlan;
        internal string BambooUser;
        internal string BambooPassword;

        internal static Config ParseFromFile(string path)
        {
            if (!File.Exists(path))
                return null;

            Config result = new Config();
            foreach (string line in File.ReadAllLines(path))
                ParseLine(result, line.Trim());

            return result;
        }

        internal void Log()
        {
            mLog.InfoFormat("Running with settings:");
            mLog.InfoFormat(PARAM_LOG_FORMAT, PLASTIC_REPO_KEY, PlasticRepo);
            mLog.InfoFormat(PARAM_LOG_FORMAT, PLASTIC_BRANCH_PREFIX_KEY, PlasticBranchPrefix);
            mLog.InfoFormat(PARAM_LOG_FORMAT, TTS_SERVER_KEY, TtsServer);
            mLog.InfoFormat(PARAM_LOG_FORMAT, TTS_USER_KEY, TtsUser);
            mLog.InfoFormat(PARAM_LOG_FORMAT, TTS_PASSWORD_KEY, TtsPassword);
            mLog.InfoFormat(PARAM_LOG_FORMAT, BAMBOO_SERVER_KEY, BambooServer);
            mLog.InfoFormat(PARAM_LOG_FORMAT, BAMBOO_PLAN_KEY, BambooPlan);
            mLog.InfoFormat(PARAM_LOG_FORMAT, BAMBOO_USER_KEY, BambooUser);
            mLog.InfoFormat(PARAM_LOG_FORMAT, BAMBOO_PASSWORD_KEY, BambooPassword);
        }

        static void ParseLine(Config config, string line)
        {
            if (line.StartsWith("#"))
                return;

            string[] splitLine = line.Split('=');
            if (splitLine.Length != 2)
                return;

            string key = splitLine[0].Trim();
            string value = splitLine[1].Trim();

            switch (key)
            {
                case TTS_SERVER_KEY:
                    config.TtsServer = value;
                    return;
                case TTS_USER_KEY:
                    config.TtsUser = value;
                    return;
                case TTS_PASSWORD_KEY:
                    config.TtsPassword = value;
                    return;
                case PLASTIC_REPO_KEY:
                    config.PlasticRepo = value;
                    return;
                case PLASTIC_BRANCH_PREFIX_KEY:
                    config.PlasticBranchPrefix = value;
                    return;
                case BAMBOO_SERVER_KEY:
                    config.BambooServer = value;
                    return;
                case BAMBOO_PLAN_KEY:
                    config.BambooPlan = value;
                    return;
                case BAMBOO_USER_KEY:
                    config.BambooUser = value;
                    return;
                case BAMBOO_PASSWORD_KEY:
                    config.BambooPassword = value;
                    return;
            }
        }

        Config()
        {
        }

        static readonly ILog mLog = LogManager.GetLogger("bamboomonitor");

        const string TTS_SERVER_KEY = "tts.server";
        const string TTS_USER_KEY = "tts.user";
        const string TTS_PASSWORD_KEY = "tts.password";
        const string PLASTIC_REPO_KEY = "plastic.repo";
        const string PLASTIC_BRANCH_PREFIX_KEY = "plastic.branchPrefix";
        const string BAMBOO_SERVER_KEY = "bamboo.server";
        const string BAMBOO_PLAN_KEY = "bamboo.plan";
        const string BAMBOO_USER_KEY = "bamboo.user";
        const string BAMBOO_PASSWORD_KEY = "bamboo.password";

        const string PARAM_LOG_FORMAT = " {0}={1}";
    }
}
