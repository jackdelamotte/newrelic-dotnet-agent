using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

#pragma warning disable CS8618

namespace nugetSlackNotifications
{
    public class Program
    {
        private static readonly HttpClient client = new();

        static async Task Main(string[] args)
        {
            List<Tuple<string, string>> newVersions = new();

            for (int i = 0; i < args.Length - 1; i++)
            {
                string response = await client.GetStringAsync($"https://api.nuget.org/v3/registration5-semver1/{args[i]}/index.json");

                SearchResult? searchResult = JsonSerializer.Deserialize<SearchResult>(response);
                if (searchResult is null) continue;

                foreach (Item item in searchResult.items)
                {
                    if (item.items is not null)
                    {
                        Catalogentry latestCatalogEntry = item.items[^1].catalogEntry;
                        if (latestCatalogEntry.published > DateTime.Now.AddDays(-10))
                            newVersions.Add(new Tuple<string, string>(latestCatalogEntry.id, latestCatalogEntry.version));
                    }
                    else // if item.items is null the json structure is weird and we have to use different properties
                    {
                        if (item.commitTimeStamp > DateTime.Now.AddDays(-10))
                            newVersions.Add(new Tuple<string, string>(args[i], item.upper));
                    }
                }
            }

            string msg = "Hi team! Dotty here :technologist::pager:\nThere's some new NuGet releases you should know about :arrow_heading_down::sparkles:";
            foreach (var t in newVersions)
                msg += $"\n\t:package: {char.ToUpper(t.Item1[0]) + t.Item1.Substring(1)} version {t.Item2}";
            msg += $"\nThanks and have a wonderful {DateTime.Now.DayOfWeek}.";

            StringContent jsonContent = new(
                JsonSerializer.Serialize(new
                {
                    text = msg
                }),
                Encoding.UTF8,
                "application/json");

            // Environment.GetEnvironmentVariable("SLACK_NUGET_NOTIFICATIONS_WEBHOOK")
            await client.PostAsync(args[^1], jsonContent);

        }
    }

    public class SearchResult
    {
        public int count { get; set; }
        public Item[] items { get; set; }
    }

    public class Item
    {
        public DateTime commitTimeStamp { get; set; }
        public int count { get; set; }
        public Release[] items { get; set; }
        // some packages like MySqlConnector and Serilog return json that will have this and not items
        public string upper { get; set; }
    }

    public class Release
    {
        public string id { get; set; }
        public Catalogentry catalogEntry { get; set; }
    }

    public class Catalogentry
    {
        public string id { get; set; }
        public DateTime published { get; set; }
        public string version { get; set; }
    }
}

#pragma warning restore CS8618
