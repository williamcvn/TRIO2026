using Microsoft.Data.Sqlite;

var conn = new SqliteConnection("Data Source=D:\\TRIO2026\\Database\\system_event.db");
conn.Open();

var cmd = conn.CreateCommand();
cmd.CommandText = "SELECT COUNT(*) FROM SystemEvent";
Console.WriteLine($"Total events: {cmd.ExecuteScalar()}");

cmd.CommandText = "SELECT Id, substr(TimestampLocal,1,19) as Time, Level, Category, Source, Message, substr(Detail,1,60) as Detail FROM SystemEvent ORDER BY Id DESC LIMIT 20";
var reader = cmd.ExecuteReader();
Console.WriteLine($"{"Id",-4} {"Time",-20} {"Level",-7} {"Category",-12} {"Source",-16} {"Message",-25} {"Detail"}");
Console.WriteLine(new string('-', 130));
while (reader.Read())
{
    Console.WriteLine($"{reader.GetInt32(0),-4} {reader.GetString(1),-20} {reader.GetString(2),-7} {reader.GetString(3),-12} {reader.GetString(4),-16} {reader.GetString(5),-25} {(reader.IsDBNull(6) ? "" : reader.GetString(6))}");
}
conn.Close();
