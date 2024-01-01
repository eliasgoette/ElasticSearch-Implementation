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
            var lowlevelClient = CreateElasticClient();
            await IndexPersonAsync(lowlevelClient, new Person { FirstName = "John", LastName = "Doe" });
            await SearchPersonAsync(lowlevelClient, "John");
        }

        static ElasticLowLevelClient CreateElasticClient()
        {
            var settings = new ConnectionConfiguration(new Uri("http://localhost:9200"));
            return new ElasticLowLevelClient(settings);
        }

        static async Task IndexPersonAsync(ElasticLowLevelClient client, Person person)
        {
            var indexResponse = await client.IndexAsync<BytesResponse>("people", "1", PostData.Serializable(person));
            if (!indexResponse.Success)
            {
                Console.WriteLine($"Failed to index document: {indexResponse.DebugInformation}");
            }
            else
            {
                Console.WriteLine("Person indexed successfully.");
            }
        }

        static async Task SearchPersonAsync(ElasticLowLevelClient client, string firstName)
        {
            var searchResponse = await client.SearchAsync<StringResponse>("people", PostData.Serializable(new
            {
                query = new
                {
                    match = new
                    {
                        FirstName = firstName
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