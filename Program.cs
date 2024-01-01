using Elasticsearch.Net;
using System.Text.Json;

namespace ElasticSearch_Implementation
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var lowlevelClient = CreateElasticClient();
            await IndexPersonAsync(lowlevelClient, new Person { FirstName = "John", LastName = "Doe" });
            var (person, docId) = await SearchPersonAsync(lowlevelClient, "John");
            if (person != null && docId != null)
            {
                await UpdatePersonAsync(lowlevelClient, docId, new Person { FirstName = "John", LastName = "Smith" });
                await DeletePersonAsync(lowlevelClient, docId);
            }
        }

        static ElasticLowLevelClient CreateElasticClient()
        {
            var settings = new ConnectionConfiguration(new Uri("http://localhost:9200"))
                .DisableDirectStreaming();
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

        static async Task<(Person, string)> SearchPersonAsync(ElasticLowLevelClient client, string firstName)
        {
            string documentId = null;
            Person foundPerson = null;

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
                return (null, null);
            }

            try
            {
                var searchResults = JsonSerializer.Deserialize<SearchResult>(searchResponse.Body);

                if (searchResults?.hits?.hits != null && searchResults.hits.hits.Any())
                {
                    var firstHit = searchResults.hits.hits.First();
                    documentId = firstHit._id;  // Retrieve the document ID
                    foundPerson = JsonSerializer.Deserialize<Person>(firstHit._source.ToString());
                    if (foundPerson != null)
                    {
                        Console.WriteLine($"Found person: {foundPerson.FirstName} {foundPerson.LastName}");
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

            return (foundPerson, documentId);  // Return the found person and document ID
        }

        static async Task UpdatePersonAsync(ElasticLowLevelClient client, string docId, Person updatedPerson)
        {
            if (string.IsNullOrEmpty(docId))
            {
                Console.WriteLine("Document ID is null or empty. Update operation cannot proceed.");
                return;
            }

            var updateResponse = await client.UpdateAsync<BytesResponse>("people", docId, PostData.Serializable(new
            {
                doc = updatedPerson
            }));

            if (!updateResponse.Success)
            {
                Console.WriteLine($"Failed to update document: {updateResponse.DebugInformation}");
            }
            else
            {
                Console.WriteLine($"Person with ID {docId} updated successfully.");
            }
        }

        static async Task DeletePersonAsync(ElasticLowLevelClient client, string docId)
        {
            if (string.IsNullOrEmpty(docId))
            {
                Console.WriteLine("Document ID is null or empty. Delete operation cannot proceed.");
                return;
            }

            var deleteResponse = await client.DeleteAsync<BytesResponse>("people", docId);

            if (!deleteResponse.Success)
            {
                Console.WriteLine($"Failed to delete document: {deleteResponse.DebugInformation}");
            }
            else
            {
                Console.WriteLine($"Person with ID {docId} deleted successfully.");
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
        public string _id { get; set; }
    }
}