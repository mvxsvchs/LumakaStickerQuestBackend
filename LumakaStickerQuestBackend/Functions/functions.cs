using LumakaStickerQuestBackend.Classes;
using Npgsql;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using static LumakaStickerQuestBackend.Functions.Services;


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
            private static UserDto MapReaderToUserDto(NpgsqlDataReader reader)
            {
                var stickerOrdinal = reader.GetOrdinal("sticker_id");
                var stickerIds = reader.IsDBNull(stickerOrdinal)
                    ? Array.Empty<int>()
                    : reader.GetFieldValue<int[]>(stickerOrdinal);

				return new UserDto
				{
					UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
					Username = reader.GetString(reader.GetOrdinal("username")),
					Points = reader.GetInt32(reader.GetOrdinal("points")),
					Stickers = stickerIds
				};
			}

            public async Task<UserDto?> GetById(int id)
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
                    ? MapReaderToUserDto(reader)
                    : null;
            }

            public async Task<UserDto?> GetByMailAndPwd(string mail, string pwd)
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

                return MapReaderToUserDto(reader);
            }

            public async Task<bool> RegisterUser(RegisterDto user)
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
                    return
                        rowsAffected == 1;
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
                    await using var cmd = new NpgsqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("id", user.Id);
                    cmd.Parameters.AddWithValue("name", user.Name);
                    cmd.Parameters.AddWithValue("mail", user.Email);

                    DateTime? birthdate = null;
                    if (!string.IsNullOrWhiteSpace(user.Birthday) &&
                        DateTime.TryParse(user.Birthday, CultureInfo.InvariantCulture, DateTimeStyles.None,
                            out var parsedDate))
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

			public async Task<bool> UpdateStickers(int userId, int[] stickers)
			{
				await using var conn = GetConnection();
				await conn.OpenAsync();

				var sql = @"
					UPDATE users
					SET sticker_id = @stickers,
						points = GREATEST(points - 10, 0)
					WHERE user_id = @id
					RETURNING points
				";

				try
				{
					await using var cmd = new NpgsqlCommand(sql, conn);
					cmd.Parameters.AddWithValue("id", userId);
					cmd.Parameters.AddWithValue("stickers", stickers ?? Array.Empty<int>());

					var result = await cmd.ExecuteScalarAsync();
					return result != null;
				}
				catch
				{
					return false;
				}
			}

			public async Task<int?> UpdateUserPoints(int userId, int delta)
			{
				await using var conn = GetConnection();
				await conn.OpenAsync();

				var sql = @"
					UPDATE users
					SET points = GREATEST(points + @delta, 0)
					WHERE user_id = @id
					RETURNING points
				";

				try
				{
					await using var cmd = new NpgsqlCommand(sql, conn);
					cmd.Parameters.AddWithValue("id", userId);
					cmd.Parameters.AddWithValue("delta", delta);

					var result = await cmd.ExecuteScalarAsync();
					return result is int points ? points : null;
				}
				catch
				{
					return null;
				}
			}
		}
		
		// Functions for operations related to lists
        public class ListS
        {
            public async Task<int?> AddTask(TaskCreateRequest task)
            {
                await using var conn = GetConnection();
                await conn.OpenAsync();

                var sql = @"
					INSERT INTO user_tasks (user_id, task_description, category_id, points_reward, is_completed, position)
					VALUES (@userid, @desc, @catid, @pointrew, @complete, @pos)
					RETURNING task_id;
				";

                try
                {
                    await using var cmd = new NpgsqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("userid", task.UserId);
                    cmd.Parameters.AddWithValue("desc", task.TaskDescription);
                    cmd.Parameters.AddWithValue("catid", task.CategoryId);
					cmd.Parameters.AddWithValue("pointrew", task.PointsReward);
                    cmd.Parameters.AddWithValue("complete", false);
                    cmd.Parameters.AddWithValue("pos", task.Position);

                    var result = await cmd.ExecuteScalarAsync();
                    return result is int taskId ? taskId : null;
                }
                catch
                {
                    return null;
                }
            }

            public async Task<IReadOnlyCollection<TaskResponse>> GetTasksByUserId(int userId)
            {
                await using var conn = GetConnection();
                await conn.OpenAsync();

                var sql = @"
					SELECT task_id, task_description, category_id, is_completed, position, points_reward
					FROM user_tasks
					WHERE user_id = @userid
					ORDER BY position NULLS LAST, task_id;
				";

                var tasks = new List<TaskResponse>();
                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("userid", userId);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tasks.Add(new TaskResponse
                    {
                        TaskId = reader.GetInt32(reader.GetOrdinal("task_id")),
                        TaskDescription = reader.IsDBNull(reader.GetOrdinal("task_description"))
                            ? string.Empty
                            : reader.GetString(reader.GetOrdinal("task_description")),
                        CategoryId = reader.GetInt32(reader.GetOrdinal("category_id")),
                        IsCompleted = reader.GetBoolean(reader.GetOrdinal("is_completed")),
                        Position = reader.IsDBNull(reader.GetOrdinal("position"))
                            ? 0
                            : reader.GetInt32(reader.GetOrdinal("position")),
                        PointsReward = reader.GetInt32(reader.GetOrdinal("points_reward"))
                    });
                }

                return tasks;
            }

            public async Task<bool> DeleteTask(int taskId)
            {
                await using var conn = GetConnection();
                await conn.OpenAsync();

                var sql = @"
					DELETE FROM user_tasks
					WHERE task_id = @taskid
				";

                try
                {
                    await using var cmd = new NpgsqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("taskid", taskId);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected == 1;
                }
                catch
                {
                    return false;
                }
            }

            public async Task<int?> UpdateTaskCompletion(int taskId, bool isCompleted)
            {
                await using var conn = GetConnection();
                await conn.OpenAsync();
                await using var transaction = await conn.BeginTransactionAsync();

				try
				{
					// Load task info and current completion flag
					var selectSql = @"
						SELECT user_id, is_completed
						FROM user_tasks
						WHERE task_id = @taskid
						FOR UPDATE
					";
					int userId;
					bool previousCompleted;

					await using (var selectCmd = new NpgsqlCommand(selectSql, conn, transaction))
					{
						selectCmd.Parameters.AddWithValue("taskid", taskId);
						await using var reader = await selectCmd.ExecuteReaderAsync();
						if (!await reader.ReadAsync())
						{
							return null;
						}

						userId = reader.GetInt32(reader.GetOrdinal("user_id"));
						previousCompleted = reader.GetBoolean(reader.GetOrdinal("is_completed"));
					}

					// Update completion state
					var updateTaskSql = @"
						UPDATE user_tasks
						SET is_completed = @completed
						WHERE task_id = @taskid
					";
					await using (var updateTaskCmd = new NpgsqlCommand(updateTaskSql, conn, transaction))
					{
						updateTaskCmd.Parameters.AddWithValue("completed", isCompleted);
						updateTaskCmd.Parameters.AddWithValue("taskid", taskId);
						await updateTaskCmd.ExecuteNonQueryAsync();
					}

					// Adjust user points when the completion flag changes (fixed at +/-5)
					int delta = 0;
					const int reward = 5;
					if (isCompleted && !previousCompleted)
					{
						delta = reward;
					}
					else if (!isCompleted && previousCompleted)
					{
						delta = -reward;
					}

					int newPoints;
					var updateUserSql = @"
						UPDATE users
						SET points = GREATEST(points + @delta, 0)
						WHERE user_id = @userid
						RETURNING points
					";

					await using (var updateUserCmd = new NpgsqlCommand(updateUserSql, conn, transaction))
					{
						updateUserCmd.Parameters.AddWithValue("delta", delta);
						updateUserCmd.Parameters.AddWithValue("userid", userId);
						var result = await updateUserCmd.ExecuteScalarAsync();
						newPoints = result is int points ? points : 0;
					}

					await transaction.CommitAsync();
					return newPoints;
				}
				catch
				{
					await transaction.RollbackAsync();
					return null;
				}
			}
        }

        // Function for operations related to boards
        public class BoardS
        {
            public async Task<Board?> GetBoard(int userId)
            {
				await using var conn = GetConnection();
				await conn.OpenAsync();

				var sql = @"
					SELECT board_id, user_id, is_completed, created_at
					FROM bingo_boards
					WHERE user_id = @userId
				";

				try
				{
					await using var cmd = new NpgsqlCommand(sql, conn);
					cmd.Parameters.AddWithValue("userId", userId);

					await using var reader = await cmd.ExecuteReaderAsync();
					
					Field[] tempFields = new Field[9];
                    tempFields = await GetFields(reader.GetInt32(reader.GetOrdinal("board_id")));

                    return new Board
					{
						Id = reader.GetInt32(reader.GetOrdinal("board_id")),
						UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
						IsCompleted = reader.GetBoolean(reader.GetOrdinal("is_completed")),
						Fields = tempFields
					};
				}
				catch
				{
					return null;
				}
			}

			public async Task<Field[]> GetFields(int boardId)
			{
				await using var conn = GetConnection();
				await conn.OpenAsync();

				var sql = @"
					SELECT field_id, field_name, sticker, board_id
					FROM bingo_fields
					WHERE board_id = @boardId
				";

				try
				{
					await using var cmd = new NpgsqlCommand(sql, conn);
					cmd.Parameters.AddWithValue("boardId", boardId);

					await using var reader = await cmd.ExecuteReaderAsync();
					
					var fields = new List<Field>();

					while (await reader.ReadAsync())
					{
						fields.Add(new Field
						{
							Id = reader.GetInt32(reader.GetOrdinal("field_id")),
							Name = reader.GetString(reader.GetOrdinal("field_name")),
							StickerId = reader.GetInt32(reader.GetOrdinal("sticker"))
						});
					}

					return fields.ToArray();
				}
				catch
				{
					return null;
				}
			}

			public async Task<bool> AddBoard(int userId)
			{
				if(userId == null || userId < 1)
				{
					return false;
				}

				await using var conn = GetConnection();
				await conn.OpenAsync();

				var sql = @"
					INSERT INTO bingo_boards (user_id, is_completed)
					VALUES (@userId, @completed);
				";

				try
				{
					await using var cmd = new NpgsqlCommand(sql, conn);
					cmd.Parameters.AddWithValue("userId", userId);
					cmd.Parameters.AddWithValue("completed", false);

					int rowsAffected = await cmd.ExecuteNonQueryAsync();
					if(rowsAffected == 1)
					{
						var tempBoard = GetBoard(userId);
						if (await AddField(tempBoard.Id) != false) 
						{
							return true;
						}
					}
					return false;
				}
				catch
				{
					return false;
				}
			}

			public async Task<bool> AddField(int boardId)
			{
				if(boardId == null || boardId < 1)
				{
					return false;
				}

				await using var conn = GetConnection();
				await conn.OpenAsync();

				var sql = @"
					INSERT INTO bingo_fields (board_id)
					VALUES
						(@boardId), (@boardId), (@boardId),
						(@boardId), (@boardId), (@boardId),
						(@boardId), (@boardId), (@boardId);
				";

				try
				{
					await using var cmd = new NpgsqlCommand(sql, conn);
					cmd.Parameters.AddWithValue("boardId", boardId);

					int rowsAffected = await cmd.ExecuteNonQueryAsync();
					return rowsAffected == 9;
				}
				catch
				{
					return false;
				}
			}

			public async Task<bool> UpdateBoard(Board board)
			{
				await using var conn = GetConnection();
				await conn.OpenAsync();

				var sql = @"
					UPDATE users
					SET is_completed = @completed
					WHERE user_id = @userId AND board_id = @boardId;
				";

				try
				{
					await using var cmd = new NpgsqlCommand(sql, conn);
					cmd.Parameters.AddWithValue("user_id", board.UserId);
					cmd.Parameters.AddWithValue("board_id", board.Id);
					cmd.Parameters.AddWithValue("completed", board.IsCompleted);

					int rowsAffected = await cmd.ExecuteNonQueryAsync();
					return rowsAffected == 1;
				}
				catch
				{
					return false;
				}
			}

			public async Task<bool> UpdateField(Field field)
			{
				await using var conn = GetConnection();
				await conn.OpenAsync();

				var sql = @"
					UPDATE users
					SET field_name = @fieldName, sticker = @sticker
					WHERE field_id = @fieldId;
				";

				try
				{
					await using var cmd = new NpgsqlCommand(sql, conn);
					cmd.Parameters.AddWithValue("fieldId", field.Id);
					cmd.Parameters.AddWithValue("fieldName", field.Name);
					cmd.Parameters.AddWithValue("sticker", field.StickerId);

					int rowsAffected = await cmd.ExecuteNonQueryAsync();
					return rowsAffected == 1;
				}
				catch
				{
					return false;
				}
			}

			public async Task<bool> DeleteBoard(int userId)
			{
				await using var conn = GetConnection();
				await conn.OpenAsync();

				var sql = @"
					DELETE FROM bingo_boards
					WHERE user_id = @userId
				";

				try
				{
					await using var cmd = new NpgsqlCommand(sql, conn);
					cmd.Parameters.AddWithValue("userId", userId);

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
