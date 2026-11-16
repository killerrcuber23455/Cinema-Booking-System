// pseudocode for our own hash table and the film searches (owner: shankar)
// FilmService.AddFilm and RemoveFilm check the role (and the film, for add), then call this table's Add and Remove (card 304)

Constant RESIZE_AT ← 0.75
Constant INITIAL_SLOTS ← 16

// the table stores key and value pairs where the key is the film id and the value is the Film
// each slot holds a chain (ChainList, our own linked list) and stays null until its first entry
// the chain has Add (puts the new item at the front), Find, Remove and ListAll

Declare slots : array of ChainList
Declare count : integer //count gives back the number of items stored in the table

Procedure CreateTable(ByVal initial_slots : integer)    // starts as INITIAL_SLOTS unless caller gives another number
    slots ← new array of ChainList with initial_slots slots, all null
    count ← 0
end procedure

// hash function
Procedure CalculateSlot(ByVal key : TKey, ByRef slot : integer)
    Declare hash_code : integer
    hash_code ← HASHCODE(key)                // for an integer key this is the number itself
    slot ← hash_code MOD LENGTH(slots)
    If slot < 0 then
        slot ← slot + LENGTH(slots)          // MOD can give a negative number, a slot cannot be negative
    endif
end procedure

Procedure NeedsResizing(ByRef needs_resizing : boolean)
    Declare load_factor : real
    load_factor ← count / LENGTH(slots)
    If load_factor > RESIZE_AT then
        needs_resizing ← true
    else
        needs_resizing ← false
    endif
end procedure

Procedure Add(ByVal key : TKey, ByVal value : TValue, ByRef is_added : boolean)
    Declare slot : integer
    Declare existing_entry, new_entry : HashTableEntry
    Declare needs_resizing : boolean
    call CalculateSlot(key, slot)
    If slots[slot] = null then
        slots[slot] ← new ChainList()
    endif
    // the chain returns the first entry that matches the rule, or null
    call slots[slot].Find(entry.Key = key, existing_entry)
    If existing_entry <> null then
        is_added ← false
    else
        // collision: entries already in this slot stay, the new entry goes in front of them
        new_entry ← new HashTableEntry(key, value)
        call slots[slot].Add(new_entry)
        count ← count + 1
        call NeedsResizing(needs_resizing)
        If needs_resizing = true then
            call Resize()
        endif
        is_added ← true
    endif
end procedure

Procedure Remove(ByVal key : TKey, ByRef is_removed : boolean)
    Declare slot : integer
    call CalculateSlot(key, slot)
    is_removed ← false
    If slots[slot] <> null then
        // the chain removes the first entry that matches the rule and says whether it found one
        call slots[slot].Remove(entry.Key = key, is_removed)
        If is_removed = true then
            count ← count - 1
        endif
    endif
end procedure

Procedure Find(ByVal key : TKey, ByRef found_value : TValue)
    Declare slot : integer
    Declare found_entry : HashTableEntry
    call CalculateSlot(key, slot)
    found_value ← null     // stays null if the slot is empty or the key is not found
    If slots[slot] <> null then
        call slots[slot].Find(entry.Key = key, found_entry)
        If found_entry <> null then
            found_value ← found_entry.Value
        endif
    endif
end procedure

Procedure ListAll(ByRef values : array of TValue)
    Declare slot, entry_index, value_index : integer
    Declare entries : array of HashTableEntry
    values ← new array of TValue with count items
    value_index ← 0
    For slot ← 0 to LENGTH(slots) - 1
        If slots[slot] <> null then
            call slots[slot].ListAll(entries)
            For entry_index ← 0 to LENGTH(entries) - 1
                values[value_index] ← entries[entry_index].Value
                value_index ← value_index + 1
            Next entry_index
        endif
    Next slot
end procedure

Procedure Resize()
    Declare old_slots : array of ChainList
    Declare entries : array of HashTableEntry
    Declare entry : HashTableEntry
    Declare old_slot, entry_index, new_slot : integer
    old_slots ← slots
    slots ← new array of ChainList with LENGTH(old_slots) * 2 slots, all null
    count ← 0
    // every entry is placed again, because the slot depends on the array length
    For old_slot ← 0 to LENGTH(old_slots) - 1
        If old_slots[old_slot] <> null then
            call old_slots[old_slot].ListAll(entries)
            For entry_index ← 0 to LENGTH(entries) - 1
                entry ← entries[entry_index]
                call CalculateSlot(entry.Key, new_slot)
                If slots[new_slot] = null then
                    slots[new_slot] ← new ChainList()
                endif
                call slots[new_slot].Add(entry)
                count ← count + 1
            Next entry_index
        endif
    Next old_slot
end procedure

// film searches (in FilmService, they use the table's ListAll)
// films is the HashTable with the film id as key

Procedure SearchByTitle(ByVal films : HashTable, ByVal search_text : string, ByRef result_array : array of Film)
    Declare all_films : array of Film
    Declare text_lower : string
    Declare film_index : integer
    Declare results : ChainList
    If search_text = null then
        search_text ← ""
    endif
    text_lower ← LOWER(search_text)
    results ← new ChainList()
    // the hash only works with an id, so every film is checked
    call films.ListAll(all_films)
    For film_index ← 0 to LENGTH(all_films) - 1
        If CONTAINS(LOWER(all_films[film_index].Title), text_lower) then
            call results.Add(all_films[film_index])
        endif
    Next film_index
    call results.ListAll(result_array)
end procedure

Procedure SearchByGenre(ByVal films : HashTable, ByVal genre : string, ByRef result_array : array of Film)
    Declare all_films : array of Film
    Declare genre_lower : string
    Declare film_index : integer
    Declare results : ChainList
    If genre = null then
        genre ← ""
    endif
    genre_lower ← LOWER(genre)
    results ← new ChainList()
    call films.ListAll(all_films)
    For film_index ← 0 to LENGTH(all_films) - 1
        If CONTAINS(LOWER(all_films[film_index].Genre), genre_lower) then
            call results.Add(all_films[film_index])
        endif
    Next film_index
    call results.ListAll(result_array)
end procedure
