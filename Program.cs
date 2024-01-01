using Elasticsearch.Net;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ElasticSearch_Implementation
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await ElasticSearchQuery();
        }

        static async Task ElasticSearchQuery()
        {
            Console.WriteLine("Elastic Search Test\n");

            var settings = new ConnectionConfiguration(new Uri("http://localhost:9200"));
            var lowlevelClient = new ElasticLowLevelClient(settings);

            var person = new Person { FirstName = "John", LastName = "Doe" };
            var indexResponse = await lowlevelClient.IndexAsync<BytesResponse>("people", "1", PostData.Serializable(person));

            if (!indexResponse.Success)
            {
                Console.WriteLine($"Failed to index document: {indexResponse.DebugInformation}");
                return;
            }

            var searchResponse = await lowlevelClient.SearchAsync<StringResponse>("people", PostData.Serializable(new
            {
                query = new
                {
                    match = new
                    {
                        FirstName = "John"
                    }
                }
            }));

            if (!searchResponse.Success)
            {
                Console.WriteLine($"Failed to search documents: {searchResponse.DebugInformation}");
                return;
            }

            try
            {
                var searchResults = JsonSerializer.Deserialize<SearchResult>(searchResponse.Body);

                if (searchResults?.hits?.hits != null && searchResults.hits.hits.Any())
                {
                    var resultPerson = JsonSerializer.Deserialize<Person>(searchResults.hits.hits.First()._source.ToString());
                    if (resultPerson != null)
                    {
                        Console.WriteLine($"Found person: {resultPerson.FirstName} {resultPerson.LastName}");
                    }
                    else
                    {
                        Console.WriteLine("Deserialization of person failed.");
                    }
                }
                else
                {
                    Console.WriteLine("No hits or deserialization failed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializing response: {ex.Message}");
            }
        }
    }

    class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    class SearchResult
    {
        public Hits hits { get; set; }
    }

    class Hits
    {
        public Hit[] hits { get; set; }
    }

    class Hit
    {
        public JsonElement _source { get; set; }
    }
}