using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Text.Json;

namespace fn_votecounter
{
    public class FnVoteCounter
    {
        [FunctionName("FnVoteCounter")]
        public void Run([ServiceBusTrigger("sbq-voting", Connection = "SBConnection")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
            
            var vote = JsonSerializer.Deserialize<Vote>(myQueueItem);

            try
            {
                using var conn = new SqlConnection(Environment.GetEnvironmentVariable("sqldb_connection"));
                conn.Open();

                using var cmd = new SqlCommand("UPDATE dbo.Counts SET Count = Count + 1 WHERE ID = @ID;", conn);
                cmd.Parameters.AddWithValue("@ID", vote.Id);

                var rows = cmd.ExecuteNonQuery();
                if (rows == 0)
                {
                    log.LogError("Entry not found on the database for ID: {id}", vote.Id);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
            }
        }

        private class Vote
        {
            public int Id { get; set; }
        }
    }
}
