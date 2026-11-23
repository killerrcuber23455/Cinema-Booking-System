// all sql server code for films, members, bookings, showings and seats (owner: kushal)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using Microsoft.Data.SqlClient;
using System;

namespace CinemaBooking.Data
{
    // the only class that talks to sql server. every query uses parameters, never text joining.
    public class Database : IDatabase
    {

        // the name of the unique rule that stops a seat being booked twice
        private const string SEAT_RULE_NAME = "uq_booked_seats_showing_seat";

        // the connection string read from appsettings.json
        private readonly string _connection_string;

        // reads the connection string from the config file
        public Database(AppConfig config)
        {
            _connection_string = config.GetConnectionString();
        }

        // uses a given connection string, so a test can use its own database
        public Database(string connection_string)
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

        // checks whether a sql error means a unique rule was broken
        private static bool IsUniqueError(SqlException error)
        {
            return SqlErrors.IsUniqueError(error);
        }

        // makes a bigger array and copies the old items into it
        private static T[] Grow<T>(T[] old_items)
        {
            T[] new_items = new T[old_items.Length * 2];

            for (int i = 0; i < old_items.Length; i++)
            {
                new_items[i] = old_items[i];
            }

            return new_items;
        }

        // makes an array with exactly count items
        private static T[] Trim<T>(T[] items, int count)
        {
            T[] result = new T[count];

            for (int i = 0; i < count; i++)
            {
                result[i] = items[i];
            }

            return result;
        }

        // turns the tier text from the database back into the Tier enum
        private static Tier ReadTier(string tier_text)
        {
            Tier tier;

            if (!Enum.TryParse<Tier>(tier_text, out tier))
            {
                tier = Tier.Standard;
            }

            return tier;
        }

        // reads every film from the database and adds it to the hash table.
        // returns how many films were added.
        public int LoadFilms(HashTable<int, Film> film_table)
        {
            int loaded = 0;

            using (SqlConnection connection = OpenConnection())
            using (SqlCommand command = new SqlCommand(
                "SELECT id, title, genre, [year], duration_minutes, age_rating, description, poster_file FROM films",
                connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string title = reader.GetString(1);
                    string genre = reader.GetString(2);
                    int year = reader.GetInt32(3);
                    int duration_minutes = reader.GetInt32(4);
                    int age_rating = reader.GetInt32(5);
                    string description = reader.IsDBNull(6) ? "" : reader.GetString(6);
                    string poster_file = reader.IsDBNull(7) ? "" : reader.GetString(7);

                    Film film = new Film(id, title, genre, year, duration_minutes, age_rating, description, poster_file);

                    if (film_table.Add(id, film))
                    {
                        loaded++;
                    }
                }
            }

            return loaded;
        }

        // saves a film with the id it already has (FilmService makes the next free id).
        // returns false if the id is already used.
        public bool SaveFilm(Film film)
        {
            using (SqlConnection connection = OpenConnection())
            using (SqlCommand command = new SqlCommand(
                "INSERT INTO films (id, title, genre, [year], duration_minutes, age_rating, description, poster_file) " +
                "VALUES (@id, @title, @genre, @year, @duration_minutes, @age_rating, @description, @poster_file)",
                connection))
            {
                command.Parameters.AddWithValue("@id", film.Id);
                command.Parameters.AddWithValue("@title", film.Title);
                command.Parameters.AddWithValue("@genre", film.Genre);
                command.Parameters.AddWithValue("@year", film.Year);
                command.Parameters.AddWithValue("@duration_minutes", film.DurationMinutes);
                command.Parameters.AddWithValue("@age_rating", film.AgeRating);
                command.Parameters.AddWithValue("@description", film.Description ?? "");
                command.Parameters.AddWithValue("@poster_file", film.PosterFile ?? "");

                try
                {
                    command.ExecuteNonQuery();
                    return true;
                }
                catch (SqlException error)
                {
                    if (IsUniqueError(error))
                    {
                        return false;
                    }

                    throw;
                }
            }
        }

        // saves a member and returns the new id, or -1 if the email already exists
        public int SaveMember(Member member)
        {
            using (SqlConnection connection = OpenConnection())
            using (SqlCommand command = new SqlCommand(
                "INSERT INTO members (name, email, phone, tier, points, lifetime_points, joined_at) " +
                "OUTPUT INSERTED.id " +
                "VALUES (@name, @email, @phone, @tier, @points, @lifetime_points, @joined_at)",
                connection))
            {
                command.Parameters.AddWithValue("@name", member.Name);
                command.Parameters.AddWithValue("@email", member.Email);
                command.Parameters.AddWithValue("@phone", member.Phone);
                command.Parameters.AddWithValue("@tier", member.Tier.ToString());
                command.Parameters.AddWithValue("@points", member.Points);
                command.Parameters.AddWithValue("@lifetime_points", member.LifetimePoints);
                command.Parameters.AddWithValue("@joined_at", member.JoinedAt);

                try
                {
                    object? result = command.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
                catch (SqlException error)
                {
                    if (IsUniqueError(error))
                    {
                        return -1;
                    }

                    throw;
                }
            }
        }

        // returns every member, with tier, points, lifetime points and join date
        public Member[] GetAllMembers()
        {
            Member[] members = new Member[16];
            int count = 0;

            using (SqlConnection connection = OpenConnection())
            using (SqlCommand command = new SqlCommand(
                "SELECT id, name, email, phone, tier, points, lifetime_points, joined_at FROM members",
                connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (count == members.Length)
                    {
                        members = Grow(members);
                    }

                    members[count] = new Member(
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetString(3),
                        ReadTier(reader.GetString(4)),
                        reader.GetInt32(5),
                        reader.GetInt32(6),
                        reader.GetDateTime(7));
                    count++;
                }
            }

            return Trim(members, count);
        }

        // updates a member's points, lifetime points and tier. returns true if the member was found.
        public bool UpdateMemberPoints(int member_id, int points, int lifetime_points, Tier tier)
        {
            using (SqlConnection connection = OpenConnection())
            using (SqlCommand command = new SqlCommand(
                "UPDATE members SET points = @points, lifetime_points = @lifetime_points, tier = @tier WHERE id = @id",
                connection))
            {
                command.Parameters.AddWithValue("@points", points);
                command.Parameters.AddWithValue("@lifetime_points", lifetime_points);
                command.Parameters.AddWithValue("@tier", tier.ToString());
                command.Parameters.AddWithValue("@id", member_id);

                return command.ExecuteNonQuery() > 0;
            }
        }

        // saves a booking and its seats in one transaction and returns the new id.
        // if a seat is already taken, nothing is saved and -1 is returned.
        public int SaveBooking(Booking booking)
        {
            using (SqlConnection connection = OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    int new_id;

                    using (SqlCommand command = new SqlCommand(
                        "INSERT INTO bookings (reference, showing_id, member_id, guest_name, guest_email, guest_phone, " +
                        "total, points_earned, points_used, status, created_at) " +
                        "OUTPUT INSERTED.id " +
                        "VALUES (@reference, @showing_id, @member_id, @guest_name, @guest_email, @guest_phone, " +
                        "@total, @points_earned, @points_used, @status, @created_at)",
                        connection, transaction))
                    {
                        command.Parameters.AddWithValue("@reference", booking.Reference);
                        command.Parameters.AddWithValue("@showing_id", booking.ShowingId);
                        command.Parameters.AddWithValue("@member_id", booking.MemberId.HasValue ? booking.MemberId.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@guest_name", booking.MemberId.HasValue ? DBNull.Value : booking.GuestName);
                        command.Parameters.AddWithValue("@guest_email", booking.MemberId.HasValue ? DBNull.Value : booking.GuestEmail);
                        command.Parameters.AddWithValue("@guest_phone", booking.MemberId.HasValue ? DBNull.Value : booking.GuestPhone);
                        command.Parameters.AddWithValue("@total", booking.Total);
                        command.Parameters.AddWithValue("@points_earned", booking.PointsEarned);
                        command.Parameters.AddWithValue("@points_used", booking.PointsUsed);
                        command.Parameters.AddWithValue("@status", booking.Status.ToString());
                        command.Parameters.AddWithValue("@created_at", booking.CreatedAt);

                        new_id = Convert.ToInt32(command.ExecuteScalar());
                    }

                    for (int i = 0; i < booking.Seats.Length; i++)
                    {
                        using (SqlCommand seat_command = new SqlCommand(
                            "INSERT INTO booked_seats (booking_id, showing_id, seat_code) VALUES (@booking_id, @showing_id, @seat_code)",
                            connection, transaction))
                        {
                            seat_command.Parameters.AddWithValue("@booking_id", new_id);
                            seat_command.Parameters.AddWithValue("@showing_id", booking.ShowingId);
                            seat_command.Parameters.AddWithValue("@seat_code", booking.Seats[i]);
                            seat_command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    return new_id;
                }
                catch (SqlException error)
                {
                    transaction.Rollback();

                    // only a taken seat returns -1. any other error is passed on.
                    if (IsUniqueError(error) && error.Message.Contains(SEAT_RULE_NAME))
                    {
                        return -1;
                    }

                    throw;
                }
            }
        }

        // checks whether a booking reference is already used
        public bool ReferenceExists(string reference)
        {
            using (SqlConnection connection = OpenConnection())
            using (SqlCommand command = new SqlCommand(
                "SELECT COUNT(*) FROM bookings WHERE reference = @reference", connection))
            {
                command.Parameters.AddWithValue("@reference", reference);

                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        // returns the seat codes of one booking
        private string[] GetSeats(SqlConnection connection, int booking_id)
        {
            string[] seats = new string[8];
            int count = 0;

            using (SqlCommand command = new SqlCommand(
                "SELECT seat_code FROM booked_seats WHERE booking_id = @booking_id ORDER BY seat_code", connection))
            {
                command.Parameters.AddWithValue("@booking_id", booking_id);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (count == seats.Length)
                        {
                            seats = Grow(seats);
                        }

                        seats[count] = reader.GetString(0);
                        count++;
                    }
                }
            }

            return Trim(seats, count);
        }

        // the columns that every booking query reads, in this order
        private const string BOOKING_COLUMNS =
            "id, reference, showing_id, member_id, guest_name, guest_email, guest_phone, " +
            "total, points_earned, points_used, status, created_at";

        // turns the current row of a reader into a booking (seats are added later)
        private Booking ReadBooking(SqlDataReader reader)
        {
            BookingStatus status;

            if (!Enum.TryParse<BookingStatus>(reader.GetString(10), out status))
            {
                status = BookingStatus.Confirmed;
            }

            int? member_id = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3);
            string guest_name = reader.IsDBNull(4) ? "" : reader.GetString(4);
            string guest_email = reader.IsDBNull(5) ? "" : reader.GetString(5);
            string guest_phone = reader.IsDBNull(6) ? "" : reader.GetString(6);

            return new Booking(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetInt32(2),
                member_id,
                guest_name,
                guest_email,
                guest_phone,
                new string[0],
                reader.GetDecimal(7),
                reader.GetInt32(8),
                reader.GetInt32(9),
                status,
                reader.GetDateTime(11));
        }

        // returns every booking of one member, newest first
        public Booking[] GetBookingsForMember(int member_id)
        {
            Booking[] bookings = new Booking[8];
            int count = 0;

            using (SqlConnection connection = OpenConnection())
            {
                using (SqlCommand command = new SqlCommand(
                    "SELECT " + BOOKING_COLUMNS + " FROM bookings WHERE member_id = @member_id ORDER BY created_at DESC",
                    connection))
                {
                    command.Parameters.AddWithValue("@member_id", member_id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (count == bookings.Length)
                            {
                                bookings = Grow(bookings);
                            }

                            bookings[count] = ReadBooking(reader);
                            count++;
                        }
                    }
                }

                // the reader is closed now, so the seats can be read for each booking
                for (int i = 0; i < count; i++)
                {
                    bookings[i].Seats = GetSeats(connection, bookings[i].Id);
                }
            }

            return Trim(bookings, count);
        }

        // finds one booking by its reference. returns null if there is none.
        public Booking? FindBooking(string reference)
        {
            Booking? booking = null;

            using (SqlConnection connection = OpenConnection())
            {
                using (SqlCommand command = new SqlCommand(
                    "SELECT " + BOOKING_COLUMNS + " FROM bookings WHERE reference = @reference", connection))
                {
                    command.Parameters.AddWithValue("@reference", reference);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            booking = ReadBooking(reader);
                        }
                    }
                }

                if (booking != null)
                {
                    booking.Seats = GetSeats(connection, booking.Id);
                }
            }

            return booking;
        }

        // cancels a booking: sets the status to Cancelled and deletes its seats, in one transaction.
        // returns false if the booking does not exist or is already cancelled.
        public bool CancelBooking(int booking_id)
        {
            using (SqlConnection connection = OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    int changed;

                    using (SqlCommand command = new SqlCommand(
                        "UPDATE bookings SET status = 'Cancelled' WHERE id = @id AND status = 'Confirmed'",
                        connection, transaction))
                    {
                        command.Parameters.AddWithValue("@id", booking_id);
                        changed = command.ExecuteNonQuery();
                    }

                    if (changed == 0)
                    {
                        transaction.Rollback();
                        return false;
                    }

                    using (SqlCommand seat_command = new SqlCommand(
                        "DELETE FROM booked_seats WHERE booking_id = @id", connection, transaction))
                    {
                        seat_command.Parameters.AddWithValue("@id", booking_id);
                        seat_command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return true;
                }
                catch (SqlException)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        // returns the seat codes of the confirmed bookings of one showing
        public string[] GetTakenSeats(int showing_id)
        {
            using (SqlConnection connection = OpenConnection())
            {
                return ReadTakenSeats(connection, showing_id);
            }
        }

        // reads the taken seats using a connection that is already open
        private string[] ReadTakenSeats(SqlConnection connection, int showing_id)
        {
            string[] seats = new string[16];
            int count = 0;

            using (SqlCommand command = new SqlCommand(
                "SELECT bs.seat_code FROM booked_seats bs " +
                "INNER JOIN bookings b ON b.id = bs.booking_id " +
                "WHERE bs.showing_id = @showing_id AND b.status = 'Confirmed' " +
                "ORDER BY bs.seat_code",
                connection))
            {
                command.Parameters.AddWithValue("@showing_id", showing_id);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (count == seats.Length)
                        {
                            seats = Grow(seats);
                        }

                        seats[count] = reader.GetString(0);
                        count++;
                    }
                }
            }

            return Trim(seats, count);
        }

        // saves a showing. returns false if the id already exists or the screen is already
        // used at that time. the film and screen must exist.
        public bool SaveShowing(Showing showing)
        {
            using (SqlConnection connection = OpenConnection())
            using (SqlCommand command = new SqlCommand(
                "INSERT INTO showings (id, film_id, screen_id, starts_at, price) " +
                "VALUES (@id, @film_id, @screen_id, @starts_at, @price)",
                connection))
            {
                command.Parameters.AddWithValue("@id", showing.Id);
                command.Parameters.AddWithValue("@film_id", showing.FilmId);
                command.Parameters.AddWithValue("@screen_id", showing.ScreenId);
                command.Parameters.AddWithValue("@starts_at", showing.StartsAt);
                command.Parameters.AddWithValue("@price", showing.Price);

                try
                {
                    command.ExecuteNonQuery();
                    return true;
                }
                catch (SqlException error)
                {
                    if (IsUniqueError(error))
                    {
                        return false;
                    }

                    throw;
                }
            }
        }

        // loads every showing into the table, each with a seat grid that already
        // has its booked seats marked as taken. returns how many showings were added.
        public int LoadShowings(ShowingTable showing_table)
        {
            Showing[] showings = new Showing[8];
            int count = 0;
            int added = 0;

            using (SqlConnection connection = OpenConnection())
            {
                // read all the showings first, because one connection cannot run a second query while a reader is open
                using (SqlCommand command = new SqlCommand(
                    "SELECT s.id, s.film_id, s.screen_id, s.starts_at, s.price, sc.row_count, sc.seats_per_row " +
                    "FROM showings s INNER JOIN screens sc ON sc.id = s.screen_id",
                    connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (count == showings.Length)
                        {
                            showings = Grow(showings);
                        }

                        SeatGrid grid = new SeatGrid(reader.GetInt32(5), reader.GetInt32(6));

                        showings[count] = new Showing(
                            reader.GetInt32(0),
                            reader.GetInt32(1),
                            reader.GetInt32(2),
                            reader.GetDateTime(3),
                            reader.GetDecimal(4),
                            grid);
                        count++;
                    }
                }

                // now mark the taken seats in each grid
                for (int i = 0; i < count; i++)
                {
                    string[] taken = ReadTakenSeats(connection, showings[i].Id);

                    if (taken.Length > 0)
                    {
                        showings[i].SeatGrid.BookSeats(taken);
                    }

                    if (showing_table.AddShowing(showings[i]))
                    {
                        added++;
                    }
                }
            }

            return added;
        }
    }
}