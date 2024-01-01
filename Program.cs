using Elasticsearch.Net;

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

            var person = new { FirstName = "John", LastName = "Doe" };
            var indexResponse = await lowlevelClient.IndexAsync<BytesResponse>("people", "1", PostData.Serializable(person));

            if (!indexResponse.Success)
            {
                Console.WriteLine($"Failed to index document: {indexResponse.DebugInformation}");
                return;
            }

            var searchResponse = await lowlevelClient.SearchAsync<BytesResponse>("people", PostData.Serializable(new
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

            var searchResult = System.Text.Json.JsonSerializer.Deserialize<SearchResult>(System.Text.Encoding.UTF8.GetString(searchResponse.Body));

            foreach (var hit in searchResult.hits.hits)
            {
                Console.WriteLine($"Found person: {hit._source.FirstName} {hit._source.LastName}");
            }
        }
    }

    public class SearchResult
    {
        public Hits? hits { get; set; }
    }

    public class Hits
    {
        public IEnumerable<Hit> hits { get; set; }
    }

    public class Hit
    {
        public Person _source { get; set; }
    }

    public class Person
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}