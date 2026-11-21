# Pseudocode: sort and search films

Owner: Kushal. Week 8. Own arrays only: no List, no Array.Sort, no LINQ OrderBy, no built-in BinarySearch.

## Merge sort (films by title, year or rating)

```text
MERGE_SORT(films, sort_by)
    IF films HAS 0 OR 1 ITEMS
        RETURN A COPY OF films
    END IF

    middle = films.length / 2
    left = COPY OF films FROM 0 TO middle - 1
    right = COPY OF films FROM middle TO THE END

    sorted_left = MERGE_SORT(left, sort_by)
    sorted_right = MERGE_SORT(right, sort_by)

    RETURN MERGE(sorted_left, sorted_right, sort_by)
END

MERGE(left, right, sort_by)
    result = NEW ARRAY OF SIZE left.length + right.length
    left_index = 0
    right_index = 0
    result_index = 0

    WHILE left_index < left.length AND right_index < right.length
        // take from the left when equal, so equal films keep their order (stable)
        IF COMPARE(left[left_index], right[right_index], sort_by) <= 0
            result[result_index] = left[left_index]
            left_index = left_index + 1
        ELSE
            result[result_index] = right[right_index]
            right_index = right_index + 1
        END IF
        result_index = result_index + 1
    END WHILE

    WHILE left_index < left.length
        result[result_index] = left[left_index]
        left_index = left_index + 1
        result_index = result_index + 1
    END WHILE

    WHILE right_index < right.length
        result[result_index] = right[right_index]
        right_index = right_index + 1
        result_index = result_index + 1
    END WHILE

    RETURN result
END

COMPARE(film_a, film_b, sort_by)
    IF sort_by IS "title"
        RETURN COMPARISON OF film_a.title AND film_b.title, IGNORING CASE
    ELSE IF sort_by IS "year"
        RETURN COMPARISON OF film_a.year AND film_b.year
    ELSE IF sort_by IS "rating"
        RETURN COMPARISON OF film_a.age_rating AND film_b.age_rating
    END IF
END
```

The comparison gives a negative number, 0, or a positive number.

## Binary search (exact title on an array sorted by title)

If two films have the same title, binary search returns the index of one of them, not always the first. Films are unique by id, so the Films screen uses the id when it needs one exact film.

```text
BINARY_SEARCH(sorted_films, title)
    low = 0
    high = sorted_films.length - 1

    WHILE low <= high
        middle = (low + high) / 2
        result = COMPARISON OF title AND sorted_films[middle].title, IGNORING CASE

        IF result IS 0
            RETURN middle
        ELSE IF result < 0
            high = middle - 1
        ELSE
            low = middle + 1
        END IF
    END WHILE

    RETURN -1
END
```

## Linear contains search (partial title, any case)

```text
LINEAR_SEARCH(films, text)
    matches = NEW ARRAY OF SIZE films.length
    count = 0

    FOR EACH film IN films
        IF film.title CONTAINS text, IGNORING CASE
            matches[count] = film
            count = count + 1
        END IF
    END FOR

    RETURN THE FIRST count ITEMS OF matches
END
```
