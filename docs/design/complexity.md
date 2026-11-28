# Hash Table and Search Time Complexity



Owner: Kushal C Bhagan  

Card: 301



## Purpose



This document explains the time complexity of adding, finding, removing and searching films in our custom hash table.



Films are stored using their `Id` as the key. Collisions are handled using our own linked lists. This is called separate chaining.



In this analysis:



- `n` means the number of films stored.

- `m` means the number of slots in the hash table.



## Find a film by ID



The hash table uses the film ID to calculate a slot index. It then checks the linked list at that slot for a matching ID.



| Case | Time complexity | Explanation |

|---|---:|---|

| Best | O(1) | The film is the first item checked, or the slot is empty. |

| Average | O(1) | Keys are spread evenly and the load factor stays low, so chains stay short. |

| Worst | O(n) | All films could be in one chain, so every item may need to be checked. |



## Add a film



The hash table uses the film ID to calculate a slot index. It checks that slot’s chain for a duplicate ID. If the ID is not present, a new node is added at the front.



| Case | Time complexity | Explanation |

|---|---:|---|

| Best | O(1) | The slot is empty, so the film can be added immediately. |

| Average | O(1) | With evenly spread keys and a low load factor, the chain checked for duplicates is short. |

| Worst | O(n) | All films could be in one chain, so every existing ID may need to be checked. |



## Remove a film by ID



The hash table uses the film ID to calculate a slot index. It searches that slot’s chain for the matching ID. If the film is found, the links are changed to remove its node while keeping the rest of the chain connected.



| Case | Time complexity | Explanation |

|---|---:|---|

| Best | O(1) | The film is the first node checked, or the slot is empty. |

| Average | O(1) | With evenly spread keys and a low load factor, only a short chain needs to be searched. |

| Worst | O(n) | All films could be in one chain, so every node may need to be checked. |



## Resize the hash table



The load factor is calculated as `n / m`: the number of films divided by the number of slots.



When the load factor becomes greater than 0.75, the table doubles its capacity. Each existing film must be placed into its new slot because the number of slots has changed.



| Case | Time complexity | Explanation |

|---|---:|---|

| Resize | O(n) | Every existing film must be moved into the larger table. |



Adding a film has an expected amortised time of O(1), assuming keys are spread evenly. Amortised means the occasional resize cost is spread across many additions.



## Search films by title



Title search checks every film because the user may search for part of a title, in any combination of upper-case and lower-case letters.



| Case | Time complexity | Explanation |

|---|---:|---|

| Best | O(n) | Every film must still be checked because the search text can match any part of a title. |

| Average | O(n) | Every film title is checked against the search text. |

| Worst | O(n) | Every film title is checked, including when no film matches. |



## Search films by genre



Genre search checks every film because films with the same genre may be stored in different hash-table slots.



| Case | Time complexity | Explanation |

|---|---:|---|

| Best | O(n) | Every film must be checked because matching films can be in any slot. |

| Average | O(n) | Every film genre is checked against the selected genre. |

| Worst | O(n) | Every film genre is checked, including when no film matches. |



## Summary



| Operation | Best | Average | Worst |

|---|---:|---:|---:|

| Find film by ID | O(1) | O(1) | O(n) |

| Add film | O(1) | O(1) | O(n) |

| Remove film by ID | O(1) | O(1) | O(n) |

| Resize hash table | O(n) | O(n) | O(n) |

| Search films by title | O(n) | O(n) | O(n) |

| Search films by genre | O(n) | O(n) | O(n) |



These average costs assume evenly spread keys and a low load factor. Film IDs are integers, so hashing and comparing each ID are treated as constant-time operations.

