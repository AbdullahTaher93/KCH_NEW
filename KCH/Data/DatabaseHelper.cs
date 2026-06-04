using Microsoft.Data.Sqlite;

namespace KCH.Data;

public static class DatabaseHelper
{
    private static string _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "KCH.db");

    public static string ConnectionString => $"Data Source={_dbPath}";

    public static void Initialize()
    {
        var dir = Path.GetDirectoryName(_dbPath)!;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Login (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL,
                Password TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Info_Cost (
                ID_C INTEGER PRIMARY KEY AUTOINCREMENT,
                Name_C TEXT,
                Da TEXT,
                Address_C TEXT,
                Discount REAL DEFAULT 0,
                Pay REAL DEFAULT 0,
                Bro REAL DEFAULT 0,
                Final_price REAL DEFAULT 0,
                S_P REAL DEFAULT 0,
                Nodes TEXT
            );

            CREATE TABLE IF NOT EXISTS Menu_Cost (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ID_C INTEGER,
                ID_O INTEGER,
                Name_Object TEXT,
                No_Object INTEGER,
                Price_Object REAL,
                Total_price REAL,
                FOREIGN KEY (ID_C) REFERENCES Info_Cost(ID_C)
            );
        ";
        cmd.ExecuteNonQuery();

        // Insert default admin user if table is empty
        cmd.CommandText = "SELECT COUNT(*) FROM Login";
        var count = Convert.ToInt64(cmd.ExecuteScalar());
        if (count == 0)
        {
            cmd.CommandText = "INSERT INTO Login (Username, Password) VALUES (@u, @p)";
            cmd.Parameters.AddWithValue("@u", "admin");
            cmd.Parameters.AddWithValue("@p", "admin");
            cmd.ExecuteNonQuery();
        }
    }

    public static SqliteConnection GetConnection()
    {
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        return connection;
    }
}
