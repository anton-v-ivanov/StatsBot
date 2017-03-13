using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using TlenBot.Entities;

namespace TlenBot.Repos
{
	internal class StatsRepo : IStatsRepo
	{
		private readonly string _connectionString;

		public StatsRepo(string connectionString)
		{
			_connectionString = connectionString;
		}

		public async Task EnsureSenderExists(MessageInfo messageInfo)
		{
			using (var connection = new MySqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				await connection.ExecuteAsync(@"INSERT INTO participants (Id, UserName, FirstName, LastName) 
VALUES(@Id, @UserName, @FirstName, @LastName) 
ON DUPLICATE KEY UPDATE UserName = @UserName, FirstName = @FirstName, LastName = @LastName",
						new
						{
							Id = messageInfo.Id,
							UserName = messageInfo.UserName,
							FirstName = messageInfo.FirstName,
							LastName = messageInfo.LastName
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
							ParticipantId = messageInfo.Id,
							Date = date,
							MessageType = (int) messageInfo.MessageType
						});
			}
		}

		public async Task<IEnumerable<MessageInfo>> GetStats(long chatId, StatsCommand command)
		{
			var sql =@"SELECT p.*, m.Date, m.MessageType, m.Counter FROM messages m INNER JOIN participants p on m.ParticipantId = p.Id
WHERE m.ChatId = @ChatId AND (m.Date BETWEEN @From AND @To) AND Counter > 0";
			if (command.UserId != 0)
			{
				sql += " AND p.Id = " + command.UserId;
			}

			sql += " ORDER BY m.Counter DESC";

			using (var connection = new MySqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				return await connection.QueryAsync<MessageInfo>(sql, 
						new
						{
							ChatId = chatId,
							From = command.FromDate.Date,
							To = command.ToDate.Date,
						});
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
	}
}