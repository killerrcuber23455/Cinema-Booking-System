-- creates all the cinema booking tables from an empty database (owner: kushal)

-- remove the tables if they exist, children first, so the script can run again
IF OBJECT_ID('audit_log', 'U') IS NOT NULL DROP TABLE audit_log;
IF OBJECT_ID('users', 'U') IS NOT NULL DROP TABLE users;
IF OBJECT_ID('booked_seats', 'U') IS NOT NULL DROP TABLE booked_seats;
IF OBJECT_ID('bookings', 'U') IS NOT NULL DROP TABLE bookings;
IF OBJECT_ID('members', 'U') IS NOT NULL DROP TABLE members;
IF OBJECT_ID('showings', 'U') IS NOT NULL DROP TABLE showings;
IF OBJECT_ID('screens', 'U') IS NOT NULL DROP TABLE screens;
IF OBJECT_ID('films', 'U') IS NOT NULL DROP TABLE films;
GO

-- stores the films, columns match the 8 final film fields
CREATE TABLE films (
    id INT NOT NULL,
    title NVARCHAR(100) NOT NULL,
    genre NVARCHAR(50) NOT NULL,
    [year] INT NOT NULL,
    duration_minutes INT NOT NULL,
    age_rating INT NOT NULL,
    description NVARCHAR(500) NULL,
    poster_file NVARCHAR(100) NULL,
    CONSTRAINT pk_films PRIMARY KEY (id),
    CONSTRAINT ck_films_id CHECK (id > 0),
    CONSTRAINT ck_films_duration CHECK (duration_minutes BETWEEN 1 AND 300),
    CONSTRAINT ck_films_age_rating CHECK (age_rating IN (0, 12, 15, 18))
);
GO

-- stores the two screens, each 8 rows by 12 seats
CREATE TABLE screens (
    id INT NOT NULL,
    name NVARCHAR(50) NOT NULL,
    row_count INT NOT NULL,
    seats_per_row INT NOT NULL,
    CONSTRAINT pk_screens PRIMARY KEY (id),
    CONSTRAINT uq_screens_name UNIQUE (name),
    CONSTRAINT ck_screens_size CHECK (row_count > 0 AND seats_per_row > 0)
);
GO

-- adds the two fixed screens
INSERT INTO screens (id, name, row_count, seats_per_row) VALUES (1, 'Screen 1', 8, 12);
INSERT INTO screens (id, name, row_count, seats_per_row) VALUES (2, 'Screen 2', 8, 12);
GO

-- stores one film on one screen at one time
CREATE TABLE showings (
    id INT NOT NULL,
    film_id INT NOT NULL,
    screen_id INT NOT NULL,
    starts_at DATETIME2 NOT NULL,
    price DECIMAL(8, 2) NOT NULL,
    CONSTRAINT pk_showings PRIMARY KEY (id),
    CONSTRAINT fk_showings_film FOREIGN KEY (film_id) REFERENCES films (id),
    CONSTRAINT fk_showings_screen FOREIGN KEY (screen_id) REFERENCES screens (id),
    CONSTRAINT uq_showings_screen_time UNIQUE (screen_id, starts_at),
    CONSTRAINT ck_showings_price CHECK (price > 0)
);
GO

-- stores cinema members, columns match Member.cs
-- points are spendable points, lifetime_points are all points ever earned
-- loyalty rules are checked in the services layer, not here, because Admin can change tier rules:
-- 1 point per Rs 10, silver from 100 lifetime points (5% off, 1.25x points), gold from 300 (10% off, 1.5x points)
-- 100 points = Rs 50 off, and a member cannot use more points than they have
CREATE TABLE members (
    id INT IDENTITY(1, 1) NOT NULL,
    name NVARCHAR(100) NOT NULL,
    email NVARCHAR(100) NOT NULL,
    phone NVARCHAR(30) NOT NULL,
    tier NVARCHAR(20) NOT NULL CONSTRAINT df_members_tier DEFAULT 'Standard',
    points INT NOT NULL CONSTRAINT df_members_points DEFAULT 0,
    lifetime_points INT NOT NULL CONSTRAINT df_members_lifetime DEFAULT 0,
    joined_at DATETIME2 NOT NULL CONSTRAINT df_members_joined DEFAULT SYSDATETIME(),
    CONSTRAINT pk_members PRIMARY KEY (id),
    CONSTRAINT uq_members_email UNIQUE (email),
    CONSTRAINT ck_members_tier CHECK (tier IN ('Standard', 'Silver', 'Gold')),
    CONSTRAINT ck_members_points CHECK (points >= 0 AND lifetime_points >= 0 AND points <= lifetime_points)
);
GO

-- stores bookings, columns match Booking.cs
-- member_id is null for a guest booking, the seats are in booked_seats
CREATE TABLE bookings (
    id INT IDENTITY(1, 1) NOT NULL,
    reference NVARCHAR(30) NOT NULL,
    showing_id INT NOT NULL,
    member_id INT NULL,
    guest_name NVARCHAR(100) NULL,
    guest_email NVARCHAR(100) NULL,
    guest_phone NVARCHAR(30) NULL,
    total DECIMAL(8, 2) NOT NULL,
    points_earned INT NOT NULL CONSTRAINT df_bookings_earned DEFAULT 0,
    points_used INT NOT NULL CONSTRAINT df_bookings_used DEFAULT 0,
    status NVARCHAR(20) NOT NULL CONSTRAINT df_bookings_status DEFAULT 'Confirmed',
    created_at DATETIME2 NOT NULL CONSTRAINT df_bookings_created DEFAULT SYSDATETIME(),
    CONSTRAINT pk_bookings PRIMARY KEY (id),
    CONSTRAINT uq_bookings_reference UNIQUE (reference),
    CONSTRAINT fk_bookings_showing FOREIGN KEY (showing_id) REFERENCES showings (id),
    CONSTRAINT fk_bookings_member FOREIGN KEY (member_id) REFERENCES members (id),
    CONSTRAINT ck_bookings_status CHECK (status IN ('Confirmed', 'Cancelled')),
    CONSTRAINT ck_bookings_total CHECK (total >= 0),
    CONSTRAINT ck_bookings_points CHECK (points_earned >= 0 AND points_used >= 0),
    -- a guest needs name, email and phone, and never earns or uses points
    CONSTRAINT ck_bookings_guest CHECK (
        member_id IS NOT NULL
        OR (guest_name IS NOT NULL AND guest_email IS NOT NULL AND guest_phone IS NOT NULL
            AND points_earned = 0 AND points_used = 0)
    )
);
GO

-- stores every seat booked for a showing
-- the unique rule stops the same seat being booked twice for one showing
-- cancelling a booking deletes its rows from this table, so the seat can be booked again
-- (the unique rule would block a new booking of that seat if the old rows stayed)
CREATE TABLE booked_seats (
    id INT IDENTITY(1, 1) NOT NULL,
    booking_id INT NOT NULL,
    showing_id INT NOT NULL,
    seat_code NVARCHAR(3) NOT NULL,
    CONSTRAINT pk_booked_seats PRIMARY KEY (id),
    CONSTRAINT fk_booked_seats_booking FOREIGN KEY (booking_id) REFERENCES bookings (id),
    CONSTRAINT fk_booked_seats_showing FOREIGN KEY (showing_id) REFERENCES showings (id),
    CONSTRAINT uq_booked_seats_showing_seat UNIQUE (showing_id, seat_code)
);
GO

-- stores the people who log in. the login service makes the hash and the salt,
-- this table only stores them: salt is Base64 text in its own column, password_hash is the hash as text.
-- failed_attempts and locked_until are used for the login lock.
-- email is always saved trimmed and lowercase. Guest is never stored.
CREATE TABLE users (
    id INT IDENTITY(1, 1) NOT NULL,
    email NVARCHAR(100) NOT NULL,
    password_hash NVARCHAR(200) NOT NULL,
    salt NVARCHAR(100) NOT NULL,
    role NVARCHAR(20) NOT NULL,
    failed_attempts INT NOT NULL CONSTRAINT df_users_failed DEFAULT 0,
    locked_until DATETIME2 NULL,
    member_id INT NULL,
    CONSTRAINT pk_users PRIMARY KEY (id),
    CONSTRAINT uq_users_email UNIQUE (email),
    CONSTRAINT fk_users_member FOREIGN KEY (member_id) REFERENCES members (id),
    CONSTRAINT ck_users_role CHECK (role IN ('Customer', 'Staff', 'Admin')),
    CONSTRAINT ck_users_failed CHECK (failed_attempts >= 0)
);
GO

-- stores who did what and when
CREATE TABLE audit_log (
    id INT IDENTITY(1, 1) NOT NULL,
    user_id INT NULL,
    action NVARCHAR(50) NOT NULL,
    details NVARCHAR(500) NULL,
    created_at DATETIME2 NOT NULL CONSTRAINT df_audit_log_created DEFAULT SYSDATETIME(),
    CONSTRAINT pk_audit_log PRIMARY KEY (id),
    CONSTRAINT fk_audit_log_user FOREIGN KEY (user_id) REFERENCES users (id)
);
GO