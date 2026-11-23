// sql for the users and audit_log tables (owner: kushal)
// the login service makes the salted hash and the Base64 salt, this class only stores and reads them
using CinemaBooking.Models;
using Microsoft.Data.SqlClient;
using System;

namespace CinemaBooking.Data
{
    // sql for the users and audit_log tables, used by the login service through IUserRepository
    public class UserRepository : IUserRepository
    {
        // sql server error numbers for a unique rule being broken
        private const int UNIQUE_KEY_ERROR = 2627;
        private const int UNIQUE_INDEX_ERROR = 2601;
        // more sql server error numbers: check rule or foreign key, text too long
        private const int CHECK_OR_FOREIGN_KEY_ERROR = 547;
        private const int TEXT_TOO_LONG_ERROR = 8152;
        private const int TEXT_TOO_LONG_ERROR_NEW = 2628;

        private readonly string _connection_string;

        // reads the connection string from the config file
        public UserRepository(AppConfig config)
        {
            _connection_string = config.GetConnectionString();
        }

        // uses a given connection string, so a test can use its own database
        public UserRepository(string connection_string)
        {
            _connection_string = connection_string;
        }

        // opens a connection, or throws a clear message if the database is down
        private SqlConnection OpenConnection()
        {
            SqlConnection connection = new SqlConnection(_connection_string);

            try
            {
                connection.Open();
            }
            catch (SqlException)
            {
                connection.Dispose();
                throw new InvalidOperationException(
                    "The database could not be reached. Check that SQL Server is running and appsettings.json is correct.");
            }

            return connection;
        }

        // we fold the email to lowercase, "Ann@x.com" and "ann@x.com" must be the same user
        private static string MakeEmailKey(string email)
        {
            return email.Trim().ToLowerInvariant();
        }

        // true when the database refused the data because of one of its rules
        private static bool IsRuleError(SqlException error)
        {
            return error.Number == UNIQUE_KEY_ERROR
                || error.Number == UNIQUE_INDEX_ERROR
                || error.Number == CHECK_OR_FOREIGN_KEY_ERROR
                || error.Number == TEXT_TOO_LONG_ERROR
                || error.Number == TEXT_TOO_LONG_ERROR_NEW;
        }

        // writes one audit row using a connection (and transaction) that is already open
        private static void InsertAudit(SqlConnection connection, SqlTransaction? transaction,
            int? user_id, string action, string details)
        {
            using (SqlCommand command = new SqlCommand(
                "INSERT INTO audit_log (user_id, action, details) VALUES (@user_id, @action, @details)",
                connection, transaction))
            {
                command.Parameters.AddWithValue("@user_id", user_id.HasValue ? user_id.Value : DBNull.Value);
                command.Parameters.AddWithValue("@action", action);
                command.Parameters.AddWithValue("@details", details);
                command.ExecuteNonQuery();
            }
        }

        // saves a user and an audit row in one transaction. returns the new id,
        // or -1 if the email is empty or the database refuses the data (duplicate email, bad role, text too long, unknown member)
        public int SaveUser(string email, string password_hash, string salt, Role role, int? member_id)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return -1;
            }

            string email_key = MakeEmailKey(email);

            using (SqlConnection connection = OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    int new_id;

                    using (SqlCommand command = new SqlCommand(
                        "INSERT INTO users (email, password_hash, salt, role, member_id) " +
                        "OUTPUT INSERTED.id " +
                        "VALUES (@email, @password_hash, @salt, @role, @member_id)",
                        connection, transaction))
                    {
                        command.Parameters.AddWithValue("@email", email_key);
                        command.Parameters.AddWithValue("@password_hash", password_hash);
                        command.Parameters.AddWithValue("@salt", salt);
                        command.Parameters.AddWithValue("@role", role.ToString());
                        command.Parameters.AddWithValue("@member_id", member_id.HasValue ? member_id.Value : DBNull.Value);

                        new_id = Convert.ToInt32(command.ExecuteScalar());
                    }

                    InsertAudit(connection, transaction, new_id, "user_created", email_key + " as " + role);

                    transaction.Commit();
                    return new_id;
                }
                catch (SqlException error)
                {
                    transaction.Rollback();

                    if (IsRuleError(error))
                    {
                        return -1;
                    }

                    throw;
                }
            }
        }

        // finds a user by email. returns null if there is none.
        public UserRecord? FindByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            using (SqlConnection connection = OpenConnection())
            using (SqlCommand command = new SqlCommand(
                "SELECT id, email, password_hash, salt, role, failed_attempts, locked_until, member_id " +
                "FROM users WHERE email = @email",
                connection))
            {
                command.Parameters.AddWithValue("@email", MakeEmailKey(email));

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    Role role;

                    if (!Enum.TryParse<Role>(reader.GetString(4), out role))
                    {
                        return null;
                    }

                    DateTime? locked_until = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6);
                    int? member_id = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7);

                    return new UserRecord(reader.GetInt32(0), reader.GetString(1), reader.GetString(2),
                        reader.GetString(3), role, reader.GetInt32(5), locked_until, member_id);
                }
            }
        }

        // saves the failed login count and the lock time in one call. pass null to clear the lock.
        // returns false if the user does not exist or the database refuses the numbers (for example a negative count).
        public bool SetFailedAttempts(int user_id, int failed_attempts, DateTime? locked_until)
        {
            using (SqlConnection connection = OpenConnection())
            using (SqlCommand command = new SqlCommand(
                "UPDATE users SET failed_attempts = @failed_attempts, locked_until = @locked_until WHERE id = @id",
                connection))
            {
                command.Parameters.AddWithValue("@failed_attempts", failed_attempts);
                command.Parameters.AddWithValue("@locked_until", locked_until.HasValue ? locked_until.Value : DBNull.Value);
                command.Parameters.AddWithValue("@id", user_id);

                try
                {
                    return command.ExecuteNonQuery() > 0;
                }
                catch (SqlException error)
                {
                    if (IsRuleError(error))
                    {
                        return false;
                    }

                    throw;
                }
            }
        }

        // writes one audit row. user_id is null when nobody is logged in (for example a failed login).
        public bool WriteAudit(int? user_id, string action, string details)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                return false;
            }

            using (SqlConnection connection = OpenConnection())
            {
                InsertAudit(connection, null, user_id, action, details);
            }

            return true;
        }

        // counts the audit rows with one action name, used by the tests and the admin screen
        public int CountAuditRows(string action)
        {
            using (SqlConnection connection = OpenConnection())
            using (SqlCommand command = new SqlCommand(
                "SELECT COUNT(*) FROM audit_log WHERE action = @action", connection))
            {
                command.Parameters.AddWithValue("@action", action);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        // returns true if at least one user has the role Admin
        public bool HasAnyAdmin()
        {
            using (SqlConnection connection = OpenConnection())
            using (SqlCommand command = new SqlCommand(
                "SELECT COUNT(*) FROM users WHERE role = @role", connection))
            {
                command.Parameters.AddWithValue("@role", Role.Admin.ToString());

                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }
    }
}