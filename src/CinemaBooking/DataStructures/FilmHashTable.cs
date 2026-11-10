namespace CinemaBooking.DataStructures
{
    // Stores the details of one film.
    public class Film
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public int DurationMinutes { get; set; }
        public string AgeRating { get; set; }
        public string Description { get; set; }
    }

    // One item in the linked list used for separate chaining.
    public class FilmNode
    {
        public Film Film { get; set; }
        public FilmNode Next { get; set; }

        // Creates a node containing one film.
        public FilmNode(Film film)
        {
            Film = film;
            Next = null;
        }
    }

    // Custom hash table that stores films using separate chaining.
    public class FilmHashTable
    {
        // Each slot can contain a linked list of films.
        private FilmNode[] slots;

        // Tracks how many films are currently stored.
        private int filmCount;

        // When this load factor is passed, the table will resize.
        private const double LoadFactorLimit = 0.75;

        // Creates the hash table with an initial number of slots.
        public FilmHashTable(int initialSlots = 16)
        {
            slots = new FilmNode[initialSlots];
            filmCount = 0;
        }

        // Calculates which slot a film Id belongs in.
        private int CalculateSlot(int filmId)
        {
            return filmId % slots.Length;
        }

        // Adds a new film if its Id does not already exist.
        public bool AddFilm(Film film)
        {
            int slot = CalculateSlot(film.Id);

            // Check whether a film with the same Id already exists.
            FilmNode current = slots[slot];

            while (current != null)
            {
                if (current.Film.Id == film.Id)
                {
                    return false;
                }

                current = current.Next;
            }

            // Add the new film to the front of the linked list.
            FilmNode newNode = new FilmNode(film);
            newNode.Next = slots[slot];
            slots[slot] = newNode;

            filmCount++;

            if (NeedsResizing())
            {
                Resize();
            }

            return true;
        }

        // Finds a film using its Id.
        // Returns the film if it is found, or null if it is not found.
        public Film FindFilm(int filmId)
        {
            int slot = CalculateSlot(filmId);

            FilmNode current = slots[slot];

            while (current != null)
            {
                if (current.Film.Id == filmId)
                {
                    return current.Film;
                }

                current = current.Next;
            }

            return null;
        }

        // Removes a film using its Id.
        // Returns true if the film was removed, or false if it was not found.
        public bool RemoveFilm(int filmId)
        {
            int slot = CalculateSlot(filmId);

            FilmNode current = slots[slot];
            FilmNode previous = null;

            while (current != null)
            {
                if (current.Film.Id == filmId)
                {
                    if (previous == null)
                    {
                        // The film is the first item in the linked list.
                        slots[slot] = current.Next;
                    }
                    else
                    {
                        // Link the previous film to the next film.
                        previous.Next = current.Next;
                    }

                    filmCount--;
                    return true;
                }

                previous = current;
                current = current.Next;
            }

            return false;
        }
        // Checks whether the hash table needs resizing.
        private bool NeedsResizing()
        {
            double loadFactor = (double)filmCount / slots.Length;
            return loadFactor > LoadFactorLimit;
        }

        // Doubles the number of slots and moves every film into its new slot.
        private void Resize()
        {
            // Remember the old slots.
            FilmNode[] oldSlots = slots;

            // Create twice as many slots.
            slots = new FilmNode[oldSlots.Length * 2];
            filmCount = 0;

            // Move every film into the new slots.
            for (int slot = 0; slot < oldSlots.Length; slot++)
            {
                FilmNode current = oldSlots[slot];

                while (current != null)
                {
                    Film film = current.Film;

                    int newSlot = film.Id % slots.Length;

                    FilmNode newNode = new FilmNode(film);
                    newNode.Next = slots[newSlot];
                    slots[newSlot] = newNode;

                    filmCount++;

                    current = current.Next;
                }
            }
        }
    }
}