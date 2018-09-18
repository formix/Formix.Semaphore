using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Formix.Semaphore.Sql
{
    public class SqlSemaphore : AbstractSemaphore
    {
        public string ConnectionString { get; set; }

        public override IEnumerable<Token> Tokens { get { return GetTokens(); } }

        public SqlSemaphore(string connectionString, string name, int value)
        {
            ConnectionString = connectionString;
            Delay = 1000;
            Name = name;
            Value = value;
        }

        protected override void Enqueue(Token token)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = 
                        "INSERT SemaphoreTokens (Id, Name, [TimeStamp], Usage) " +
                        "VALUES (@id, @name, @timeStamp, @usage)";
                    cmd.Parameters.AddWithValue("@id", token.Id);
                    cmd.Parameters.AddWithValue("@name", Name);
                    cmd.Parameters.AddWithValue("@timeStamp", token.TimeStamp);
                    cmd.Parameters.AddWithValue("@usage", token.Usage);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        protected override void Dequeue(Token token)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM SemaphoreTokens WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@id", token.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private IEnumerable<Token> GetTokens()
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = 
                        "SELECT Id, Usage, [TimeStamp] " +
                        "FROM SemaphoreTokens WHERE Name = @name " +
                        "ORDER BY [TimeStamp] ASC";
                    cmd.Parameters.AddWithValue("@name", Name);

                    ICollection<Token> tokens = new LinkedList<Token>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tokens.Add(new Token(
                                (Guid)reader["Id"],
                                (int)reader["Usage"],
                                (long)reader["TimeStamp"]));
                        }
                    }

                    return tokens;
                }
            }
        }
    }
}
