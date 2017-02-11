using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BambooMonitor
{
    static class AuthenticatedWebClient
    {
        internal static string Get(
            string baseUri,
            string relativeUri,
            string user,
            string password,
            Dictionary<string, string> queryParams)
        {
            using (WebClient client = BuildClient(baseUri, user, password, queryParams))
            {
                return client.DownloadString(relativeUri);
            }
        }

        internal static string Post(
            string baseUri,
            string relativeUri,
            string user,
            string password,
            Dictionary<string, string> queryParams)
        {
            using (WebClient client = BuildClient(baseUri, user, password, queryParams))
            {
                return client.UploadString(relativeUri, string.Empty);
            }
        }

        static WebClient BuildClient(
            string baseUri,
            string user,
            string password,
            Dictionary<string, string> queryParams)
        {
            WebClient result = new WebClient();

            result.BaseAddress = GetValidBaseAddress(baseUri);

            byte[] authData = Encoding.ASCII.GetBytes(string.Concat(user, ":", password));
            result.Headers[HttpRequestHeader.Authorization] =
                string.Concat("Basic ", Convert.ToBase64String(authData));

            AddQueryParameters(result, queryParams);

            return result;
        }

        static string GetValidBaseAddress(string baseAddress)
        {
            if (!baseAddress.EndsWith(URI_SEPARATOR))
                baseAddress += URI_SEPARATOR;
            return baseAddress;
        }

        static void AddQueryParameters(
            WebClient client, Dictionary<string, string> queryParams)
        {
            if (queryParams == null)
                return;

            foreach (string key in queryParams.Keys)
                client.QueryString.Add(key, queryParams[key] ?? string.Empty);
        }

        internal const string URI_SEPARATOR = "/";
    }
}
