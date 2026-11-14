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

```text
IS_SEAT_FREE(grid, seat_code)
    row_letter = FIRST CHARACTER OF seat_code
    seat_number = NUMBER AFTER THE ROW LETTER

    row_number = CONVERT row_letter TO A NUMBER
    row_number = row_number - 1

    seat_index = seat_number - 1

    IF row_number IS OUTSIDE THE GRID
        RETURN FALSE
    END IF

    IF seat_index IS OUTSIDE THE GRID
        RETURN FALSE
    END IF

    RETURN grid[row_number][seat_index] IS FREE
END
```

## Pseudocode: Book Seats

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

```text
FREE_SEATS(grid, seat_codes)
    FOR EACH seat_code IN seat_codes
        row_letter = FIRST CHARACTER OF seat_code
        seat_number = NUMBER AFTER THE ROW LETTER

        row_number = CONVERT row_letter TO A NUMBER
        row_number = row_number - 1

        seat_index = seat_number - 1

        IF row_number AND seat_index ARE INSIDE THE GRID
            SET grid[row_number][seat_index] TO FREE
        ELSE
            RETURN FALSE
        END IF
    END FOR

    RETURN TRUE
END
```

## Pseudocode: Create Showing

```text
CREATE_SHOWING(showing_id, film_id, screen_id, starts_at, price)
    CREATE showing
    SET showing.id TO showing_id
    SET showing.film_id TO film_id
    SET showing.screen_id TO screen_id
    SET showing.starts_at TO starts_at
    SET showing.price TO price

    CREATE seat_grid FOR THE SCREEN SIZE

    SET showing.seat_grid TO seat_grid

    ADD showing TO THE SHOWINGS HASH TABLE USING showing_id AS THE KEY
END
```

## Time Complexity

Let:

- `r` be the number of rows.
- `s` be the number of seats in each row.
- `n` be the number of seats selected.

| Operation | Best case | Average case | Worst case | Reason |
|---|---:|---:|---:|---|
| Create seat grid | O(r × s) | O(r × s) | O(r × s) | Every seat must be set to free |
| Is seat free | O(1) | O(1) | O(1) | The seat is found directly using its row and seat number |
| Book seats | O(n) | O(n) | O(n) | Each selected seat is checked and then marked as taken |
| Free seats | O(n) | O(n) | O(n) | Each selected seat is checked and then marked as free |
| Create showing | O(r × s) | O(r × s) | O(r × s) | A full seat grid is created for the showing |

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