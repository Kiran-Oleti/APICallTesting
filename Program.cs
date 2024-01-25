using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Npgsql;

class CatFact
{
    public string Fact { get; set; }
}

class Program
{
    static async Task Main()
    {
        string apiUrl = "https://catfact.ninja/fact";
        string connectionString = "Host=localhost; Username=postgres; Password=saijanu@123; Database=catsdb";

        CatFact catFact = await MakeHttpRequest(apiUrl);

        if (catFact != null)
        {
            StoreInPostgres(catFact, connectionString);
        }
        Console.ReadLine();
    }

    static async Task<CatFact> MakeHttpRequest(string apiUrl)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    // Deserialize JSON string into a C# object
                    return JsonConvert.DeserializeObject<CatFact>(responseData);
                }
                else
                {
                    Console.WriteLine($"Negative Response:\nStatus Code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return null;
        }
    }

    static void StoreInPostgres(CatFact catFact, string connectionString)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();

            using (NpgsqlCommand cmd = new NpgsqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = "INSERT INTO catfacts (fact) VALUES (@fact)";
                cmd.Parameters.AddWithValue("@fact", catFact.Fact);

                int rowsAffected = cmd.ExecuteNonQuery();
                Console.WriteLine($"{rowsAffected} row(s) inserted.");
            }
        }
    }
}
