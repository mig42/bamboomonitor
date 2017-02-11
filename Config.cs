using System.IO;

namespace BambooMonitor
{
    class Config
    {
        internal string TtsServer;
        internal string TtsUser;
        internal string TtsPassword;
        internal string PlasticRepo;

        internal static Config ParseFromFile(string path)
        {
            if (!File.Exists(path))
                return null;

            Config result = new Config();
            foreach (string line in File.ReadAllLines(path))
                ParseLine(result, line.Trim());

            return result;
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
            }
        }

        Config()
        {
        }

        const string TTS_SERVER_KEY = "tts.server";
        const string TTS_USER_KEY = "tts.user";
        const string TTS_PASSWORD_KEY = "tts.password";
        const string PLASTIC_REPO_KEY = "plastic.repo";
    }
}
