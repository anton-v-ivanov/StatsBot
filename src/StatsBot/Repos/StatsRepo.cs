using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using Telegram.Bot.Types.Enums;
using TlenBot.Entities;

namespace TlenBot.Repos
{
	public class StatsRepo : IStatsRepo
	{
		private readonly string _connectionString;

		public StatsRepo(string connectionString)
		{
			_connectionString = connectionString;
		}

		public async Task EnsureSenderExists(UserInfo user)
		{
			using (var connection = new MySqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				await connection.ExecuteAsync(@"INSERT INTO participants (Id, UserName, FirstName, LastName) 
VALUES(@Id, @UserName, @FirstName, @LastName) 
ON DUPLICATE KEY UPDATE UserName = @UserName, FirstName = @FirstName, LastName = @LastName",
						new
						{
							Id = user.Id,
							UserName = user.UserName,
							FirstName = user.FirstName,
							LastName = user.LastName
						});
			}
		}

		public async Task AddMessage(long chatId, MessageInfo messageInfo, DateTime date)
		{
			using (var connection = new MySqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				await connection.ExecuteAsync(@"INSERT INTO messages (ChatId, ParticipantId, Date, MessageType) 
VALUES(@ChatId, @ParticipantId, @Date, @MessageType) 
ON DUPLICATE KEY UPDATE Counter = Counter+1",
						new
						{
							ChatId = chatId,
							ParticipantId = messageInfo.User.Id,
							Date = date,
							MessageType = (int) messageInfo.MessageType
						});
			}
		}

		public async Task<Stats> GetStats(long chatId, StatsCommand command)
		{
			var sql = new StringBuilder();
		    sql.Append(@"SELECT p.*, sum(m.Counter) as Counter FROM messages m INNER JOIN participants p on m.ParticipantId = p.Id ");
            sql.Append("WHERE m.ChatId = @ChatId AND (m.Date BETWEEN @From AND @To)");
			if (command.UserId != 0)
				sql.Append($" AND p.Id = {command.UserId}");
			sql.AppendLine(" GROUP BY p.Id ORDER BY sum(m.Counter) DESC;");

            sql.Append("SELECT m.MessageType, sum(m.Counter) as Counter FROM messages as m ");
            sql.Append("WHERE m.ChatId = @ChatId AND (m.Date BETWEEN @From AND @To)");
            if (command.UserId != 0)
                sql.Append($" AND p.Id = {command.UserId}");
		    sql.Append(" GROUP BY m.MessageType ORDER BY m.MessageType ASC;");

            using (var connection = new MySqlConnection(_connectionString))
			{
				await connection.OpenAsync();

			    using (var multi = await connection.QueryMultipleAsync(sql.ToString(),
			        new
			        {
			            ChatId = chatId,
			            From = command.FromDate.Date,
			            To = command.ToDate.Date,
			        }))
			    {
			        var stats = new Stats();
			        var rows = multi.Read();
			        foreach (var row in rows)
			        {
			            stats.UserStats.Add(new UserInfo((int) row.Id, row.UserName, row.FirstName, row.LastName),
			                (int) row.Counter);
			        }
			        rows = multi.Read();
			        foreach (var row in rows)
			        {
			            stats.ChatStats.Add((MessageType)row.MessageType, (int)row.Counter);
			        }
                    return stats;
                }
			}
		}

		public async Task<IEnumerable<long>> GetChatsIds()
		{
			using (var connection = new MySqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				return await connection.QueryAsync<long>(@"SELECT DISTINCT ChatId FROM messages");
			}
		}

		public async Task<int> GetUserId(string userName)
		{
			using (var connection = new MySqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				return await connection.QueryFirstOrDefaultAsync<int>(@"SELECT Id FROM participants WHERE UserName = @UserName", new { UserName = userName });
			}
		}

	    public async Task<Dictionary<DateTime, int>> GetPerDayStats(long chatId, StatsCommand command)
	    {
            var sql = new StringBuilder();
            sql.Append(@"SELECT m.Date, sum(m.Counter) as Counter FROM messages m ");
            sql.Append("WHERE m.ChatId = @ChatId AND (m.Date BETWEEN @From AND @To) ");
            sql.Append("GROUP BY m.Date ORDER BY m.Date ASC");
	        using (var connection = new MySqlConnection(_connectionString))
	        {
	            await connection.OpenAsync();
                var rows = await connection.QueryAsync(sql.ToString(), new
                {
                    ChatId = chatId,
                    From = command.FromDate.Date,
                    To = command.ToDate.Date,
                });
	            return rows.ToDictionary(row => (DateTime) row.Date, row => (int) row.Counter);
	        }
	    }
    }
}