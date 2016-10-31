namespace Cake.Pact
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using Cake.Core;
    using Cake.Core.Annotations;
    using Cake.Core.Diagnostics;
    using Flurl;
    using Flurl.Http;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [CakeAliasCategory("Testing")]
    [CakeAliasCategory("Consumer-Driven Contracts")]
    [CakeAliasCategory("Pact")]
    public static class PactAliases
    {
        [CakeMethodAlias]
        public static bool PactPublishToBroker(this ICakeContext ctx, string server, string pactFilePath, string version)
        {
            ctx.Log.Information("Reading pact from {0} ...", pactFilePath);

            string pactJson;

            try
            {
                pactJson = System.IO.File.ReadAllText(pactFilePath);
            }
            catch (Exception ex)
            {
                ctx.Log.Error("Could not load pact from {0}: {1}", pactFilePath, ex.Message);
                return false;
            }

            ctx.Log.Information("Deserializing pact into JObject ...");
            JObject pactJObject = null;

            try
            {
                pactJObject = JsonConvert.DeserializeObject<JObject>(pactJson);
            }
            catch (Exception)
            {
                ctx.Log.Error("Could not convert the pact to a JObject. Check that the pact contains valid json.");
                ctx.Log.Error(pactJson);
            }

            return PactPublishToBroker(ctx, server, pactJObject, version);
        }

        [CakeMethodAlias]
        public static bool PactPublishToBroker(this ICakeContext ctx, string server, JObject pact, string version)
        {
            var consumer = (string)pact["consumer"]["name"];
            var provider = (string)pact["provider"]["name"];

            return Publish(ctx, server, version, provider, consumer, pact).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private static async Task<bool> Publish(ICakeContext ctx, string server, string version, string provider, string consumer, JObject pactJson)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(consumer))
            {
                errors.Add("Consumer not set");
            }

            if (string.IsNullOrEmpty(provider))
            {
                errors.Add("Provider not set");
            }

            if (string.IsNullOrEmpty(version))
            {
                errors.Add("Version not set");
            }

            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    ctx.Log.Error(error);
                }

                return false;
            }

            var uri = server
                .AppendPathSegments("pacts/provider", provider, "consumer", consumer, "versions", version);

            ctx.Log.Information("Pact between provider {0} and consumer {1} version {2} will be published to {3} using URI {4}", provider, consumer, version, server, uri);

            try
            {
                var resultFromBroker = await uri
                    .WithHeader("Accept", "application/json")
                    .PutJsonAsync(pactJson);

                var result = resultFromBroker.StatusCode == HttpStatusCode.OK ||
                             resultFromBroker.StatusCode == HttpStatusCode.Created;

                return result;
            }
            catch (FlurlHttpTimeoutException ex)
            {
                ctx.Log.Error(ex.Message);
            }
            catch (FlurlHttpException ex)
            {
                ctx.Log.Error(ex.Message);
            }
            catch (Exception ex)
            {
                ctx.Log.Error("Failed to publish the pact: {0}", ex.Message);
            }

            return false;
        }
    }
}
