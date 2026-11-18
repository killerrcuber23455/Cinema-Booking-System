\# Film Hash Table Design

Owner: Kushal C Bhagan

Card: 303



\## Purpose

The film records are stored in our own hash table: HashTable<int, Film>.

No built-in collections such as List, Dictionary, HashSet, Queue or LinkedList are used for the main film storage.



\## Film record

Each film has 8 fields:

\- Id

\- Title

\- Genre

\- Year

\- DurationMinutes

\- AgeRating (0, 12, 15 or 18)

\- Description

\- PosterFile



\## Storage design

The hash table uses separate chaining.

Each slot contains our own linked list (ChainList) of entries (key and value).

The film Id is the key. The value is the Film object.

The table is generic, so it can store other types later.



\## Hash function

slot index = key hash code modulo number of slots

For an int key, the hash code is the number itself, so this is the film Id modulo the number of slots.

If the result is negative, the number of slots is added to it.



\## Adding a film

1\. If the key is null, return false.

2\. Calculate the slot from the key.

3\. Search that slot's linked list for the same key.

4\. If the key already exists, return false (duplicate).

5\. If it does not exist, add the new entry to the linked list and add 1 to the count.

6\. If the table is now too full, resize it.

7\. Return true.



\## Finding a film

1\. If the key is null, return the default value.

2\. Calculate the slot from the key.

3\. Search that slot's linked list for the matching key.

4\. Return the value if found.

5\. Return the default value if the end of the list is reached.



\## Removing a film

1\. If the key is null, return false.

2\. Calculate the slot from the key.

3\. Search the linked list for the matching key.

4\. If found, remove that node and link the previous node to the next node, then subtract 1 from the count and return true.

5\. If not found, return false.



\## Listing all films

ListAll returns every value in a plain array, going through each slot and each chain.



\## Resizing

The table starts with 16 slots.

When the load factor (count divided by number of slots) becomes greater than 0.75, the number of slots doubles.

Every entry is moved into its new slot using its key, because the slot number depends on the number of slots.

Duplicate checks are not repeated during resizing because the original table already contained unique keys.

