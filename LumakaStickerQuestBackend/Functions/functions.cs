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
					cmd.Parameters.AddWithValue("id", id);

					int rowsAffected = await cmd.ExecuteNonQueryAsync();
					return rowsAffected == 1; // i want to add validation here, but db connection currently fails; actually this might already be validation
				}
				catch
				{ 
					return false;
				}
			}

			public async Task<bool> UpdateUser(User user)
			{
				await using var conn = GetConnection();
				await conn.OpenAsync();

				var sql = @"
					UPDATE users
					SET username = @name email = @mail birth_date = @birthdate password_hash = @pwd points = @points sticker_id = @stickers
					WHERE user_id = @id
				";

				try
				{
					await using var cmd = new NpgsqlCommand(sql,conn);
					cmd.Parameters.AddWithValue("id", user.Id);
					cmd.Parameters.AddWithValue("name", user.Name);
					cmd.Parameters.AddWithValue("mail", user.Email);
					cmd.Parameters.AddWithValue("birthdate", user.Birthday);
					cmd.Parameters.AddWithValue("pwd", user.Password);
					cmd.Parameters.AddWithValue("points", user.Points);
					cmd.Parameters.AddWithValue("stickers", user.Stickers);

					int rowsAffected = await cmd.ExecuteNonQueryAsync();
					return rowsAffected == 1;
				}
				catch
				{
					return false;
				}
			}
		}
		
		// Functions for operations related to lists
		public class ListS
		{
			public async Task<bool> AddTask(ListItem task) //this should hand back the taks id so the frontend can call with that -> speak with maxi
			{
				await using var conn = GetConnection();
				await conn.OpenAsync();

				var sql = @"
					INSERT INTO user_tasks (user_id, task_description, category_id, points_reward, is_completed, position)
					VALUES (@userid, @desc, @catid, @pointrew, @complete, @pos);
				";

				try
				{
					await using var cmd = new NpgsqlCommand(sql, conn);
					cmd.Parameters.AddWithValue("userid", task.UserId);
					cmd.Parameters.AddWithValue("desc", task.Description);
					cmd.Parameters.AddWithValue("catid", task.Category);
					cmd.Parameters.AddWithValue("pointrew", task.Points);
					cmd.Parameters.AddWithValue("complete", false);
					cmd.Parameters.AddWithValue("pos", task.Position);

					int rowsAffected = await cmd.ExecuteNonQueryAsync();
					return rowsAffected == 1;
				}
				catch
				{
					return false;
				}
			}
		}
	}
}
