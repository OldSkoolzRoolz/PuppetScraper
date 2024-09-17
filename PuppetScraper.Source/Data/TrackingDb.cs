using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using KC.Dropins.PuppetScraper.Models;

using Microsoft.Data.Sqlite;

using PuppetScraper.Models;

namespace KC.Dropins.PuppetScraper.Data;

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
        ConcurrentDictionary<string, string> result = new ConcurrentDictionary<string, string>(5, 200);



        try
        {
            using var connection = GetConnection();
            if (connection != null)
            {
                using var command =
                    new SqliteCommand("SELECT article_id, post_url FROM Tracker where seen = 0 LIMIT 200", connection);
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
            using var command =
                new SqliteCommand("UPDATE Tracker SET seen = 1 WHERE article_id = @article_id", connection);
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
            using var command =
                new SqliteCommand(
                    "INSERT OR IGNORE INTO Tracker (article_id, post_type, post_url) VALUES (@article_id, @post_type, @post_url)",
                    connection);
            connection.Open();
            for (int i = 0; i < data.Length; i++)
            {
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@article_id", Path.GetFileNameWithoutExtension(data[i]));
                command.Parameters.AddWithValue("@post_type", "video");
                command.Parameters.AddWithValue("@post_url", data[i]);
                command.ExecuteNonQuery();
            }
        }
    }






    /// <summary>
    /// Inserts a record in the "site_profiles" table.
    /// </summary>
    /// <param name="profile_id">Profile ID</param>
    /// <param name="profile_name">Profile Name</param>
    /// <param name="profile_url">Profile URL</param>
    public static void InsertSiteProfile(string profile_id, string profile_name, string profile_url,
        string selector1, string selector2, string selector3, string target_element_type, bool use_pagination,
        string pagination_type, string pagination_next_selector)
    {
        if (string.IsNullOrEmpty(profile_id) || string.IsNullOrEmpty(profile_name) || string.IsNullOrEmpty(profile_url))
        {
            throw new ArgumentException("Profile details cannot be null or empty.");
        }

        var sql =
            "INSERT INTO site_profiles (profile_id, profile_name, profile_url, selector1, selector2, selector3, target_element_type, use_pagination, pagination_type, pagination_next_selector) VALUES (@profile_id, @profile_name, @profile_url, @selector1, @selector2, @selector3, @target_element_type, @use_pagination, @pagination_type, @pagination_next_selector)";

        try
        {
            using var connection = GetConnection();
            if (connection != null)
            {
                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@profile_id", profile_id);
                command.Parameters.AddWithValue("@profile_name", profile_name);
                command.Parameters.AddWithValue("@profile_url", profile_url);
                command.Parameters.AddWithValue("@selector1", selector1);
                command.Parameters.AddWithValue("@selector2", selector2);
                command.Parameters.AddWithValue("@selector3", selector3);
                command.Parameters.AddWithValue("@target_element_type", target_element_type);
                command.Parameters.AddWithValue("@use_pagination", use_pagination);
                command.Parameters.AddWithValue("@pagination_type", pagination_type);
                command.Parameters.AddWithValue("@pagination_next_selector", pagination_next_selector);


                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        catch (SqliteException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }






    /// <summary>
    /// Updates any values allowed to be changed in the "site_profiles" table.
    /// </summary>
    /// <param name="profile_id">Profile ID</param>
    /// <param name="new_profile_name">New Profile Name</param>
    /// <param name="new_profile_url">New Profile URL</param>
    public static void UpdateSiteProfile(string profile_id, string? new_profile_name = null,
        string? new_profile_url = null,
        string? new_selector1 = null, string? new_selector2 = null, string? new_selector3 = null,
        string? new_target_element_type = null,
        bool new_use_pagination = false, string? new_pagination_type = null,
        string? new_pagination_next_selector = null)
    {
        if (string.IsNullOrEmpty(profile_id))
        {
            throw new ArgumentException("Profile ID cannot be null or empty.");
        }

        var updates = new List<string>();
        if (!string.IsNullOrEmpty(new_profile_name)) updates.Add("profile_name = @new_profile_name");
        if (!string.IsNullOrEmpty(new_profile_url)) updates.Add("profile_url = @new_profile_url");
        if (!string.IsNullOrEmpty(new_selector1)) updates.Add("selector1 = @new_selector1");
        if (!string.IsNullOrEmpty(new_selector2)) updates.Add("selector2 = @new_selector2");
        if (!string.IsNullOrEmpty(new_selector3)) updates.Add("selector3 = @new_selector3");

        if (!string.IsNullOrEmpty(new_target_element_type))
            updates.Add("target_element_type = @new_target_element_type");
        if (new_use_pagination) updates.Add("use_pagination = @new_use_pagination");
        if (!string.IsNullOrEmpty(new_pagination_type)) updates.Add("pagination_type = @new_pagination_type");
        if (!string.IsNullOrEmpty(new_pagination_next_selector))
            updates.Add("pagination_next_selector = @new_pagination_next_selector");



        if (updates.Count == 0)
        {
            throw new ArgumentException("At least one field to update must be provided.");
        }

        var sql = $"UPDATE site_profiles SET {string.Join(", ", updates)} WHERE profile_id = @profile_id";

        try
        {
            using var connection = GetConnection();
            if (connection != null)
            {
                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@profile_id", profile_id);
                if (!string.IsNullOrEmpty(new_profile_name))
                    command.Parameters.AddWithValue("@new_profile_name", new_profile_name);
                if (!string.IsNullOrEmpty(new_profile_url))
                    command.Parameters.AddWithValue("@new_profile_url", new_profile_url);
                if (!string.IsNullOrEmpty(new_selector1)) command.Parameters.AddWithValue("@selector1", new_selector1);
                if (!string.IsNullOrEmpty(new_selector2)) command.Parameters.AddWithValue("@selector2", new_selector2);
                if (!string.IsNullOrEmpty(new_selector3)) command.Parameters.AddWithValue("@selector3", new_selector3);
                if (!string.IsNullOrEmpty(new_target_element_type))
                    command.Parameters.AddWithValue("@target_element_type", new_target_element_type);
                if (new_use_pagination) command.Parameters.AddWithValue("@use_pagination", new_use_pagination);
                if (!string.IsNullOrEmpty(new_pagination_type))
                    command.Parameters.AddWithValue("@pagination_type", new_pagination_type);
                if (!string.IsNullOrEmpty(new_pagination_next_selector))
                    command.Parameters.AddWithValue("@pagination_next_selector", new_pagination_next_selector);





                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        catch (SqliteException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }






    /// <summary>
    /// Selects a site profile by profile_id and returns a TargetProfile object.
    /// </summary>
    /// <param name="profileId">Profile ID</param>
    /// <returns>TargetProfile object</returns>
    public static TargetProfile? SelectSiteProfileById(int profileId)
    {
        if (profileId <= 0)
        {
            throw new ArgumentException("Profile ID must be greater than zero.");
        }

        var sql =
            "SELECT profile_id, profile_name, profile_url, selector1, selector2, selector3, target_element_type, use_pagination, pagination_type, pagination_next_selector FROM site_profiles WHERE profile_id = @profile_id";

        try
        {
            using var connection = GetConnection();
            if (connection != null)
            {
                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@profile_id", profileId);

                connection.Open();
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new TargetProfile
                    {
                        ProfileId = reader["profile_id"].ToString(),
                        ProfileName = reader["profilr_name"].ToString(),
                        ProfileUrl = reader["profile_url"].ToString(),
                        SelectorOne = reader["selector1"].ToString(),
                        SelectorTwo = reader["selector2"].ToString(),
                        SelectorThree = reader["selector3"].ToString(),
                        TargetType = reader["target_element_type"].ToString(),
                        UsePagination = Convert.ToBoolean(reader["use_pagination"]),
                        PaginationType = reader["pagination_type"].ToString(),
                        PaginationNextSelector = reader["pagination_next_selector"].ToString()
                    };
                }
            }
        }
        catch (SqliteException ex)
        {
            Console.WriteLine(ex.Message);
        }

        return null;
    }
}