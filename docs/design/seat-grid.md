# Seat Grid and Showings Design

Owner: Kushal C Bhagan  
Card: 309

## Purpose

This design explains how the cinema stores showings and manages seats.

Each showing has its own seat grid.  
Each screen has 8 rows and 12 seats per row.  
Seat codes run from A1 to H12.

## Seat Grid

The seat grid is a 2D array with 8 rows and 12 seats in each row.

Each seat can be:

- free
- taken

A seat code has a row letter and a seat number.

Examples:

- A1 is row A, seat 1.
- C5 is row C, seat 5.
- H12 is row H, seat 12.

## Pseudocode: Create Seat Grid

```text
CREATE_SEAT_GRID(rows, seats_per_row)
    CREATE grid WITH rows ROWS AND seats_per_row SEATS IN EACH ROW

    FOR row_number FROM 0 TO rows - 1
        FOR seat_number FROM 0 TO seats_per_row - 1
            SET grid[row_number][seat_number] TO FREE
        END FOR
    END FOR

    RETURN grid
END
```

## Pseudocode: Is Seat Free

This returns true only if the seat is free. It returns false for three reasons:

1. bad format (the code is not a row letter followed by a number)
2. outside the grid (the row letter or seat number is not on the screen)
3. already booked (the seat is taken)

```text
IS_SEAT_FREE(grid, seat_code)
    IF seat_code IS EMPTY OR seat_code IS NOT A ROW LETTER FOLLOWED BY A NUMBER
        RETURN FALSE                         // reason 1: bad format
    END IF

    row_letter = FIRST CHARACTER OF seat_code
    seat_number = NUMBER AFTER THE ROW LETTER

    row_number = CONVERT row_letter TO A NUMBER
    row_number = row_number - 1

    seat_index = seat_number - 1

    IF row_number IS OUTSIDE THE GRID
        RETURN FALSE                         // reason 2: outside the grid
    END IF

    IF seat_index IS OUTSIDE THE GRID
        RETURN FALSE                         // reason 2: outside the grid
    END IF

    IF grid[row_number][seat_index] IS TAKEN
        RETURN FALSE                         // reason 3: already booked
    END IF

    RETURN TRUE
END
```

## Pseudocode: Book Seats

Booking is all or nothing. Every seat is checked first. Seats are marked as taken only if all of them are free.

```text
BOOK_SEATS(grid, seat_codes)
    FOR EACH seat_code IN seat_codes
        IF IS_SEAT_FREE(grid, seat_code) IS FALSE
            RETURN FALSE
        END IF
    END FOR

    FOR EACH seat_code IN seat_codes
        row_letter = FIRST CHARACTER OF seat_code
        seat_number = NUMBER AFTER THE ROW LETTER

        row_number = CONVERT row_letter TO A NUMBER
        row_number = row_number - 1

        seat_index = seat_number - 1

        SET grid[row_number][seat_index] TO TAKEN
    END FOR

    RETURN TRUE
END
```

## Pseudocode: Free Seats

Freeing is all or nothing. Every seat code is checked first. If any seat code has a bad format or is outside the grid, nothing is freed and the result is false. Seats are set to free only after all codes pass the check, so the grid is never left half changed.

```text
FREE_SEATS(grid, seat_codes)
    FOR EACH seat_code IN seat_codes
        IF seat_code IS NOT A ROW LETTER FOLLOWED BY A NUMBER
            RETURN FALSE                     // nothing has been changed yet
        END IF

        row_letter = FIRST CHARACTER OF seat_code
        seat_number = NUMBER AFTER THE ROW LETTER

        row_number = CONVERT row_letter TO A NUMBER
        row_number = row_number - 1

        seat_index = seat_number - 1

        IF row_number OR seat_index IS OUTSIDE THE GRID
            RETURN FALSE                     // nothing has been changed yet
        END IF
    END FOR

    FOR EACH seat_code IN seat_codes
        row_letter = FIRST CHARACTER OF seat_code
        seat_number = NUMBER AFTER THE ROW LETTER

        row_number = CONVERT row_letter TO A NUMBER
        row_number = row_number - 1

        seat_index = seat_number - 1

        SET grid[row_number][seat_index] TO FREE
    END FOR

    RETURN TRUE
END
```

## Pseudocode: Create Showing

The screen size is read from the screen (8 rows by 12 seats). A showing with an Id that already exists is refused, so the existing showing is never replaced.

```text
CREATE_SHOWING(showings_table, showing_id, film_id, screen_id, starts_at, price)
    IF showings_table ALREADY CONTAINS showing_id
        RETURN FALSE                         // duplicate Id is refused
    END IF

    READ rows AND seats_per_row FROM THE SCREEN screen_id     // 8 rows x 12 seats

    CREATE showing
    SET showing.id TO showing_id
    SET showing.film_id TO film_id
    SET showing.screen_id TO screen_id
    SET showing.starts_at TO starts_at
    SET showing.price TO price

    seat_grid = CREATE_SEAT_GRID(rows, seats_per_row)

    SET showing.seat_grid TO seat_grid

    ADD showing TO showings_table USING showing_id AS THE KEY

    RETURN TRUE
END
```

## Pseudocode: Find Showing

```text
FIND_SHOWING(showings_table, showing_id)
    SLOT = CALCULATE THE SLOT FROM showing_id
    SEARCH THE CHAIN IN THAT SLOT FOR showing_id

    IF FOUND
        RETURN THE SHOWING
    END IF

    RETURN NOT FOUND
END
```

## Rebuilding the Seat Grid from the Database

The seat grids live in memory, so they are lost when the program closes. The database is the permanent record of which seats are booked. When the program starts (or when a showing is loaded), the seat grid of each showing is rebuilt from the database.

```text
REBUILD_SEAT_GRID(showing, booked_seat_codes)
    READ rows AND seats_per_row FROM THE SCREEN OF showing

    grid = CREATE_SEAT_GRID(rows, seats_per_row)        // every seat starts free

    FOR EACH seat_code IN booked_seat_codes
        row_letter = FIRST CHARACTER OF seat_code
        seat_number = NUMBER AFTER THE ROW LETTER

        row_number = CONVERT row_letter TO A NUMBER
        row_number = row_number - 1

        seat_index = seat_number - 1

        IF row_number AND seat_index ARE INSIDE THE GRID
            SET grid[row_number][seat_index] TO TAKEN
        END IF
    END FOR

    SET showing.seat_grid TO grid
END
```

Notes:

- booked_seat_codes is the list of seats booked for this showing, read from the database by the Data layer. Cancelled bookings are not included, so their seats stay free.
- The database is always the truth. If the grid and the database ever differ, the grid is rebuilt from the database.
- A seat code in the database that is outside the grid is ignored, so bad data cannot crash the rebuild.

## Time Complexity

Let:

- `r` be the number of rows.
- `s` be the number of seats in each row.
- `n` be the number of seats selected.
- `b` be the number of booked seats read from the database.

| Operation | Best case | Average case | Worst case | Reason |
|---|---:|---:|---:|---|
| Create seat grid | O(r × s) | O(r × s) | O(r × s) | Every seat must be set to free |
| Is seat free | O(1) | O(1) | O(1) | The seat is found directly using its row and seat number |
| Book seats | O(n) | O(n) | O(n) | Each selected seat is checked and then marked as taken |
| Free seats | O(n) | O(n) | O(n) | Each selected seat is checked and then marked as free |
| Find showing | O(1) | O(1) | O(k) | The hash table goes straight to one slot. In the worst case all k showings are in the same chain |
| Create showing | O(r × s) | O(r × s) | O(r × s + k) | A full seat grid is created. The duplicate check is O(1) on average and O(k) in the worst case |
| Rebuild seat grid | O(r × s + b) | O(r × s + b) | O(r × s + b) | The grid is created, then each booked seat is marked as taken |

For this cinema, the grid is always 8 rows by 12 seats, so it has 96 seats.

Creating one seat grid takes:

```text
O(8 × 12) = O(96)
```

This is treated as constant time because the screen size does not change.

Checking whether one seat is free takes:

```text
O(1)
```

Booking or freeing `n` seats takes:

```text
O(n)
```

For example, booking 3 seats takes:

```text
O(3)
```

## Why a 2D Array Is Suitable

A 2D array matches the cinema seat layout.

Rows are letters A to H.  
Seats are numbers 1 to 12.

The program can find a seat immediately from its row and seat number. It does not need to search through every seat.

This makes checking a single seat fast:

```text
O(1)
```