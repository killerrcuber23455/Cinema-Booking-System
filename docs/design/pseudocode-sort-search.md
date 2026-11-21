\# Pseudocode: sort and search films (owner: kushal)



Week 8. Own arrays only. No List, no Array.Sort, no LINQ OrderBy, no built-in BinarySearch.



\## Merge sort (films by title, year or rating)



MERGE\_SORT(films, sort\_by)

&#x20;   IF films HAS 0 OR 1 ITEMS

&#x20;       RETURN A COPY OF films

&#x20;   END IF



&#x20;   middle = films.length / 2

&#x20;   left = COPY OF films FROM 0 TO middle - 1

&#x20;   right = COPY OF films FROM middle TO END



&#x20;   sorted\_left = MERGE\_SORT(left, sort\_by)

&#x20;   sorted\_right = MERGE\_SORT(right, sort\_by)



&#x20;   RETURN MERGE(sorted\_left, sorted\_right, sort\_by)

END



MERGE(left, right, sort\_by)

&#x20;   result = NEW ARRAY OF SIZE left.length + right.length

&#x20;   left\_index = 0

&#x20;   right\_index = 0

&#x20;   result\_index = 0



&#x20;   WHILE left\_index < left.length AND right\_index < right.length

&#x20;       // take from the left when equal, so equal films keep their order (stable)

&#x20;       IF COMPARE(left\[left\_index], right\[right\_index], sort\_by) <= 0

&#x20;           result\[result\_index] = left\[left\_index]

&#x20;           left\_index = left\_index + 1

&#x20;       ELSE

&#x20;           result\[result\_index] = right\[right\_index]

&#x20;           right\_index = right\_index + 1

&#x20;       END IF

&#x20;       result\_index = result\_index + 1

&#x20;   END WHILE



&#x20;   WHILE left\_index < left.length

&#x20;       COPY left\[left\_index] INTO result\[result\_index]

&#x20;       MOVE BOTH INDEXES ON BY 1

&#x20;   END WHILE



&#x20;   WHILE right\_index < right.length

&#x20;       COPY right\[right\_index] INTO result\[result\_index]

&#x20;       MOVE BOTH INDEXES ON BY 1

&#x20;   END WHILE



&#x20;   RETURN result

END



COMPARE(film\_a, film\_b, sort\_by)

&#x20;   IF sort\_by IS "title"

&#x20;       COMPARE film\_a.title WITH film\_b.title, IGNORING CASE

&#x20;   ELSE IF sort\_by IS "year"

&#x20;       COMPARE film\_a.year WITH film\_b.year

&#x20;   ELSE IF sort\_by IS "rating"

&#x20;       COMPARE film\_a.age\_rating WITH film\_b.age\_rating

&#x20;   END IF

&#x20;   RETURN A NEGATIVE NUMBER, 0, OR A POSITIVE NUMBER

END



\## Binary search (exact title on a array sorted by title)



BINARY\_SEARCH(sorted\_films, title)

&#x20;   low = 0

&#x20;   high = sorted\_films.length - 1



&#x20;   WHILE low <= high

&#x20;       middle = (low + high) / 2

&#x20;       result = COMPARE title WITH sorted\_films\[middle].title, IGNORING CASE



&#x20;       IF result IS 0

&#x20;           RETURN middle

&#x20;       ELSE IF result < 0

&#x20;           high = middle - 1

&#x20;       ELSE

&#x20;           low = middle + 1

&#x20;       END IF

&#x20;   END WHILE



&#x20;   RETURN -1

END



\## Linear contains search (partial title, any case)



LINEAR\_SEARCH(films, text)

&#x20;   matches = NEW ARRAY OF SIZE films.length

&#x20;   count = 0



&#x20;   FOR EACH film IN films

&#x20;       IF film.title CONTAINS text, IGNORING CASE

&#x20;           matches\[count] = film

&#x20;           count = count + 1

&#x20;       END IF

&#x20;   END FOR



&#x20;   RETURN THE FIRST count ITEMS OF matches

END

