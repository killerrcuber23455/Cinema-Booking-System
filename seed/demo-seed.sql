-- demo data so a booking can be saved while the program does not save films and showings itself yet (owner: divyesh)
-- run once on the CinemaBooking database, it is safe to run again
-- the films are the first 5 films of data/films_sample.csv, the showings use the same numbers as DemoShowings.cs
USE CinemaBooking;
GO

IF NOT EXISTS (SELECT 1 FROM films WHERE id = 1)
    INSERT INTO films (id, title, genre, [year], duration_minutes, age_rating, description, poster_file)
    VALUES (1, N'The Last Voyage', N'Adventure', 2023, 128, 12, N'A crew sails to the edge of the map', N'last_voyage.jpg');
IF NOT EXISTS (SELECT 1 FROM films WHERE id = 2)
    INSERT INTO films (id, title, genre, [year], duration_minutes, age_rating, description, poster_file)
    VALUES (2, N'Midnight Kitchen', N'Comedy', 2022, 98, 0, N'A chef runs a secret late night diner', N'midnight_kitchen.jpg');
IF NOT EXISTS (SELECT 1 FROM films WHERE id = 3)
    INSERT INTO films (id, title, genre, [year], duration_minutes, age_rating, description, poster_file)
    VALUES (3, N'Iron Harbour', N'Action', 2024, 115, 15, N'A dockworker uncovers a smuggling ring', N'iron_harbour.jpg');
IF NOT EXISTS (SELECT 1 FROM films WHERE id = 4)
    INSERT INTO films (id, title, genre, [year], duration_minutes, age_rating, description, poster_file)
    VALUES (4, N'Paper Moon', N'Drama', 2021, 104, 12, N'Two siblings restore their late father''s theatre', N'paper_moon.jpg');
IF NOT EXISTS (SELECT 1 FROM films WHERE id = 5)
    INSERT INTO films (id, title, genre, [year], duration_minutes, age_rating, description, poster_file)
    VALUES (5, N'Silent Orbit', N'Sci-Fi', 2025, 132, 15, N'An astronaut hears a signal from deep space', N'silent_orbit.jpg');
GO

IF NOT EXISTS (SELECT 1 FROM showings WHERE id = 1)
    INSERT INTO showings (id, film_id, screen_id, starts_at, price)
    VALUES (1, 1, 1, DATEADD(MINUTE, 840, DATEADD(DAY, 1, CAST(CAST(GETDATE() AS DATE) AS DATETIME2))), 200);
IF NOT EXISTS (SELECT 1 FROM showings WHERE id = 2)
    INSERT INTO showings (id, film_id, screen_id, starts_at, price)
    VALUES (2, 1, 2, DATEADD(MINUTE, 1110, DATEADD(DAY, 1, CAST(CAST(GETDATE() AS DATE) AS DATETIME2))), 250);
IF NOT EXISTS (SELECT 1 FROM showings WHERE id = 3)
    INSERT INTO showings (id, film_id, screen_id, starts_at, price)
    VALUES (3, 1, 1, DATEADD(MINUTE, 1200, DATEADD(DAY, 2, CAST(CAST(GETDATE() AS DATE) AS DATETIME2))), 250);
IF NOT EXISTS (SELECT 1 FROM showings WHERE id = 4)
    INSERT INTO showings (id, film_id, screen_id, starts_at, price)
    VALUES (4, 2, 1, DATEADD(MINUTE, 960, DATEADD(DAY, 1, CAST(CAST(GETDATE() AS DATE) AS DATETIME2))), 200);
IF NOT EXISTS (SELECT 1 FROM showings WHERE id = 5)
    INSERT INTO showings (id, film_id, screen_id, starts_at, price)
    VALUES (5, 3, 2, DATEADD(MINUTE, 1260, DATEADD(DAY, 1, CAST(CAST(GETDATE() AS DATE) AS DATETIME2))), 250);
IF NOT EXISTS (SELECT 1 FROM showings WHERE id = 6)
    INSERT INTO showings (id, film_id, screen_id, starts_at, price)
    VALUES (6, 4, 1, DATEADD(MINUTE, 1140, DATEADD(DAY, 1, CAST(CAST(GETDATE() AS DATE) AS DATETIME2))), 250);
IF NOT EXISTS (SELECT 1 FROM showings WHERE id = 7)
    INSERT INTO showings (id, film_id, screen_id, starts_at, price)
    VALUES (7, 5, 2, DATEADD(MINUTE, 1200, DATEADD(DAY, 1, CAST(CAST(GETDATE() AS DATE) AS DATETIME2))), 300);
GO
