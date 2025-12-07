using LumakaStickerQuestBackend.Classes;
using Npgsql;
using System.Globalization;


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
			private static FeUser MapReaderToFeUser(NpgsqlDataReader reader)
			{
				var stickerOrdinal = reader.GetOrdinal("sticker_id");
				var stickerIds = reader.IsDBNull(stickerOrdinal)
					? Array.Empty<int>()
					: reader.GetFieldValue<int[]>(stickerOrdinal);

				return new FeUser
				{
					UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
					Username = reader.GetString(reader.GetOrdinal("username")),
					Points = reader.GetInt32(reader.GetOrdinal("points")),
					StickerId = stickerIds
				};
			}

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
				return await reader.ReadAsync()
					? MapReaderToFeUser(reader)
					: null;
			}

			public async Task<FeUser?> GetByMailAndPwd(string mail, string pwd)
			{
				if (string.IsNullOrWhiteSpace(mail) || string.IsNullOrWhiteSpace(pwd))
				{
					return null;
				}

				await using var conn = GetConnection();
				await conn.OpenAsync();

				var sql = @"
					SELECT user_id, username, password_hash, email, points, birth_date, sticker_id
					FROM users
					WHERE email = @mail
				";

				await using var cmd = new NpgsqlCommand(sql, conn);
				cmd.Parameters.AddWithValue("mail", mail.ToLower());

				await using var reader = await cmd.ExecuteReaderAsync();
				if (!await reader.ReadAsync())
				{
					return null;
				}

				var storedHash = reader.GetString(reader.GetOrdinal("password_hash"));
				var isValid = PasswordHasher.Verify(pwd, storedHash);

				if (!isValid)
				{
					return null;
				}

				return MapReaderToFeUser(reader);
			}

			public async Task<bool> RegisterUser(FeRegister user)
			{
				if (user == null
				    || string.IsNullOrWhiteSpace(user.Username)
				    || string.IsNullOrWhiteSpace(user.Mail)
				    || string.IsNullOrWhiteSpace(user.Password))
				{
					return false;
				}

				await using var conn = GetConnection();
				await conn.OpenAsync();

				var sql = @"
					INSERT INTO users (username, email, password_hash)
					VALUES (@username, @email, @password_hash);
				";

				try
				{
					var hashedPassword = PasswordHasher.Hash(user.Password);

					await using var cmd = new NpgsqlCommand(sql, conn);
					cmd.Parameters.AddWithValue("username", user.Username);
					cmd.Parameters.AddWithValue("email", user.Mail.ToLower());
					cmd.Parameters.AddWithValue("password_hash", hashedPassword);

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

				var updatePassword = !string.IsNullOrWhiteSpace(user.Password);
				var sql = updatePassword
					? @"
						UPDATE users
						SET username = @name, email = @mail, birth_date = @birthdate, points = @points, sticker_id = @stickers, password_hash = @pwd
						WHERE user_id = @id
					"
					: @"
						UPDATE users
						SET username = @name, email = @mail, birth_date = @birthdate, points = @points, sticker_id = @stickers
						WHERE user_id = @id
					";

				try
				{
					await using var cmd = new NpgsqlCommand(sql,conn);
					cmd.Parameters.AddWithValue("id", user.Id);
					cmd.Parameters.AddWithValue("name", user.Name);
					cmd.Parameters.AddWithValue("mail", user.Email);

					DateTime? birthdate = null;
					if (!string.IsNullOrWhiteSpace(user.Birthday) &&
					    DateTime.TryParse(user.Birthday, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
					{
						birthdate = parsedDate;
					}
					cmd.Parameters.AddWithValue("birthdate", birthdate.HasValue ? birthdate.Value : DBNull.Value);

					cmd.Parameters.AddWithValue("points", user.Points);
					cmd.Parameters.AddWithValue("stickers", user.Stickers ?? Array.Empty<int>());
					
					if (updatePassword)
					{
						var hashedPassword = PasswordHasher.Hash(user.Password);
						cmd.Parameters.AddWithValue("pwd", hashedPassword);
					}

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
