using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nugetSlackNotifications
{
    public class Program
    {
        private static readonly HttpClient client = new();

        private static string[] coreTechPackages = new string[] { "system.data.sqlclient", "microsoft.data.sqlclient", "mongocsharpdriver", "mongodb.driver", "mysql.data", "mysqlconnector", "stackexchange.redis", "rabbitmq.client", "microsoft.net.http", "restsharp", "serilog" };

        static async Task Main(string[] args)
        {
            List<Tuple<string, string>> newVersions = new();

            foreach (string package in coreTechPackages)
            {
                string response = await client.GetStringAsync($"https://api.nuget.org/v3/registration5-semver1/{package}/index.json");

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
                            newVersions.Add(new Tuple<string, string>(package, item.upper));
                    }
                }
            }

            foreach (var t in newVersions)
                Console.WriteLine($"{t.Item1}: {t.Item2}");
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
