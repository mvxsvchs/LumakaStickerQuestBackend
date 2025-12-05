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
			public async Task<FeUser?> GetById(int id)
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
					User tempUser = new User
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

					FeUser returnUser = new FeUser
					{
						UserId = tempUser.Id,
						Username = tempUser.Name,
						Points = tempUser.Points,
						StickerId = tempUser.Stickers
					};

					return returnUser;
				}
				else 
				{
					return null;
				}
			}

			public async Task<FeUser?> GetByMailAndPwd(string mail, string pwd)
			{
				await using var conn = GetConnection();
				await conn.OpenAsync();

				var sql = @"
					SELECT user_id, username, password_hash, email, points, birth_date, sticker_id
					FROM users
					WHERE email = @mail AND password_hash = @pwd
				";

				await using var cmd = new NpgsqlCommand(sql, conn);
				cmd.Parameters.AddWithValue("mail", mail.ToLower());
				cmd.Parameters.AddWithValue("pwd", pwd);

				await using var reader = await cmd.ExecuteReaderAsync();
				if (await reader.ReadAsync())
				{
					User tempUser = new User
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
					
					FeUser returnUser = new FeUser
					{
						UserId = tempUser.Id,
						Username = tempUser.Name,
						Points = tempUser.Points,
						StickerId = tempUser.Stickers
					};

					return returnUser;
				}
				else
				{
					return null;
				}
			}

			public async Task<bool> RegisterUser(FeRegister user)
			{
				await using var conn = GetConnection();
				await conn.OpenAsync();

				var sql = @"
					INSERT INTO users (username, email, password_hash)
					VALUES (@username, @email, @password_hash);
				";

				try
				{
					await using var cmd = new NpgsqlCommand(sql, conn);
					cmd.Parameters.AddWithValue("username", user.Username);
					cmd.Parameters.AddWithValue("email", user.Mail.ToLower());
					cmd.Parameters.AddWithValue("password_hash", user.Password);

					int rowsAffected = await cmd.ExecuteNonQueryAsync();
					return rowsAffected == 1;
				}
				catch
				{
					return false;
				}
			}

			public async Task<bool> DeleteUser(int id)
			{
				await using var conn = GetConnection();
				await conn.OpenAsync();

				var sql = @"
					DELETE FROM users
					WHERE user_id = @id
				";

				try
				{
					await using var cmd = new NpgsqlCommand(sql, conn);
					cmd.Parameters.AddWithValue("user_id", id);

					int rowsAffected = await cmd.ExecuteNonQueryAsync();
					return rowsAffected == 1; // i want to add validation here, but db connection currently fails
				}
				catch
				{ 
					return false;
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
