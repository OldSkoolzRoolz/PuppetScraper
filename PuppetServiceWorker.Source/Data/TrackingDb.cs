

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Data.Sqlite;


namespace PuppetServiceWorker.Data;



public static class TrackingDb
{



    internal readonly static Dictionary<string, string> PostTracker = [];


    /// <summary>
    /// Loads all seen posts from the "Tracker" table into the "PostTracker" dictionary.
    /// </summary>
    public static void LoadSeenPosts()
    {

        try
        {
            using var connection = GetConnection();
            if (connection != null)
            {
                using var command = new SqliteCommand("SELECT article_id FROM Tracker", connection);
                connection.Open();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    PostTracker[reader.GetString(0)] = reader.GetString(0);
                }
            }
        }
        catch (SqliteException)
        {

            //ignore
        }

    }



/// <summary>
/// Loads a number of unseen posts from the "Tracker" table into a ConcurrentDictionary.
/// </summary>
/// <param name="count" ></param>
/// <returns>ConcurrentDictionary</returns>
    public static Task<ConcurrentDictionary<string, string>> LoadUnSeenPostsAsync(int count)
    {
        ConcurrentDictionary<string, string> result = new ConcurrentDictionary<string, string>(5,200);



        try
        {
            using var connection = GetConnection();
            if (connection != null)
            {
                using var command = new SqliteCommand("SELECT article_id, post_url FROM Tracker where seen = 0 LIMIT 200", connection);
                connection.Open();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
            
                    result.TryAdd(reader.GetString(0), reader.GetString(1));
                }
            }
        }
        catch (SqliteException)
        {

            //ignore
        }
        return Task.FromResult(result);





    }






    public static void InsertRecord([NotNull] string article_id, [NotNull] string post_type, [NotNull] string post_url)
    {
        if (string.IsNullOrEmpty(article_id))
        {
            throw new ArgumentException($"'{nameof(article_id)}' cannot be null or empty.", nameof(article_id));
        }

        if (string.IsNullOrEmpty(post_type))
        {
            throw new ArgumentException($"'{nameof(post_type)}' cannot be null or empty.", nameof(post_type));
        }

        if (string.IsNullOrEmpty(post_url))
        {
            throw new ArgumentException($"'{nameof(post_url)}' cannot be null or empty.", nameof(post_url));
        }


        var sql = "INSERT OR IGNORE INTO Tracker (article_id, post_type, post_url) " +
            "VALUES (@article_id, @post_type, @post_url)";

        try
        {
            using var connection = GetConnection();
            connection!.Open();
            // Bind parameters values
            using var command = new SqliteCommand(sql, connection);

            command.Parameters.AddWithValue("@article_id", article_id);
            command.Parameters.AddWithValue("@post_type", post_type);
            command.Parameters.AddWithValue("@post_url", post_url);



            // Execute the INSERT statement
            var rowInserted = command.ExecuteNonQuery();


            System.Console.WriteLine($"Row inserted: {rowInserted}");

        }
        catch (SqliteException ex)
        {
            Console.WriteLine(ex.Message);
        }

    }




    private static SqliteConnection? GetConnection()
    {

        try
        {
            // Open a new database connection
            using var connection = new SqliteConnection(@"Data Source=tracker.db");
            connection.Open();

            return connection;
        }
        catch (SqliteException ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }




    }



    /// <summary>
    /// Checks if the given ID is in the "PostTracker" dictionary.
    /// </summary>
    /// <param name="id">Article ID attribute on the "article" element</param>
    /// <returns>bool</returns>
    public static bool TrackerContains(string id)
    {
        if (string.IsNullOrEmpty(id)) { return false; }
        return PostTracker.ContainsKey(id);
    }





    private static void SelectRecord(int id)
    {
        var sql = @$"SELECT * FROM Tracker WHERE article_id = @article_id";
        try
        {
            // Open a new database connection
            using var connection = new SqliteConnection(@"Data Source=TrackingDb.db");
            connection.Open();

            // Bind parameters values
            using var command = new SqliteCommand(sql, connection);


        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex.Message);
        }

    }

















    private static void DeleteRecord()
    {



    }

    internal static void UpdateSeenStatus(string id)
    {
        using var connection = GetConnection();
        if (connection != null)
        {
            using var command = new SqliteCommand("UPDATE Tracker SET seen = 1 WHERE article_id = @article_id", connection);
            command.Parameters.AddWithValue("@article_id", id);
            connection.Open();
            command.ExecuteNonQuery();
        }
    }

    internal static void InsertRecords(string[] data)
    {
        using var connection = GetConnection();
        if (connection != null)
        {
            using var command = new SqliteCommand("INSERT OR IGNORE INTO Tracker (article_id, post_type, post_url) VALUES (@article_id, @post_type, @post_url)", connection);
            connection.Open();
            for (int i=0;i<data.Length;i++)
            {
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@article_id", Path.GetFileNameWithoutExtension(data[i]));
                command.Parameters.AddWithValue("@post_type", "video");
                command.Parameters.AddWithValue("@post_url", data[i]);
                command.ExecuteNonQuery();
            }
        }
    }
}