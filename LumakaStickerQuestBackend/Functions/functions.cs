using LumakaStickerQuestBackend.Classes;
using Npgsql;


namespace LumakaStickerQuestBackend.Functions
{
	public class Services
	{
		// Overarching function to get a new connection to the DB
		public static NpgsqlConnection GetConnection()
		{
			string connStr = ConfigurationHelper.GetConnectionString("DefaultConnection");
			return new NpgsqlConnection(connStr);
		}

		// Functions for operations related to users
		public class UserS
		{
			public async Task<User> GetById(int id)
			{
				await using var conn = GetConnection();
				await conn.OpenAsync();

				var sql = @"
					SELECT user_id, username, password_hash, email, points, birth_date, sticker_id
					FROM users
					WHERE user_id = @id
				";

				await using var cmd = new NpgsqlCommand(sql, conn);
				cmd.Parameters.AddWithValue("id", id);

				await using var reader = await cmd.ExecuteReaderAsync();
				if (await reader.ReadAsync())
				{
					return new User
					{
						Id = reader.GetInt32(reader.GetOrdinal("user_id")),
						Name = reader.GetString(reader.GetOrdinal("username")),
						Password = reader.GetString(reader.GetOrdinal("password_hash")),
						Email = reader.GetString(reader.GetOrdinal("email")),
						Points = reader.GetInt32(reader.GetOrdinal("points")),
						Birthday = reader.IsDBNull(reader.GetOrdinal("birth_date"))
							? null
							: reader.GetDateTime(reader.GetOrdinal("birth_date")).ToString("yyyy-MM-dd"),
						Stickers = reader.IsDBNull(reader.GetOrdinal("sticker_id"))
							? new int[0]
							: reader.GetFieldValue<int[]>(reader.GetOrdinal("sticker_id"))
					};
				}
				else 
				{
					return null;
				}
			}

			public async Task<User> GetByMailAndPwd(string mail, string pwd)
			{
				await using var conn = GetConnection();
				await conn.OpenAsync();

				var sql = @"
					SELECT user_id, username, password_hash, email, points, birth_date, sticker_id
					FROM users
					WHERE email = @mail AND password_hash = @pwd
				";

				await using var cmd = new NpgsqlCommand(sql, conn);
				cmd.Parameters.AddWithValue("mail", mail);
				cmd.Parameters.AddWithValue("pwd", pwd);

				await using var reader = await cmd.ExecuteReaderAsync();
				if (await reader.ReadAsync())
				{
					return new User
					{
						Id = reader.GetInt32(reader.GetOrdinal("user_id")),
						Name = reader.GetString(reader.GetOrdinal("username")),
						Password = reader.GetString(reader.GetOrdinal("password_hash")),
						Email = reader.GetString(reader.GetOrdinal("email")),
						Points = reader.GetInt32(reader.GetOrdinal("points")),
						Birthday = reader.IsDBNull(reader.GetOrdinal("birth_date"))
							? null
							: reader.GetDateTime(reader.GetOrdinal("birth_date")).ToString("yyyy-MM-dd"),
						Stickers = reader.IsDBNull(reader.GetOrdinal("sticker_id"))
							? new int[0]
							: reader.GetFieldValue<int[]>(reader.GetOrdinal("sticker_id"))
					};
				}
				else
				{
					return null;
				}
			}
		}
		/*
		// Functions for operations related to lists
		public class ListS
		{
			public async Task<ListItem> GetListItem(int listId)
			{
				await Placeholder2;
			}
		}*/
	}
}