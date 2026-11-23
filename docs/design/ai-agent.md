# AI agent plan (card 371, owner: shankar)
 
status: ready for team review, 23 Nov 2026. Pseudocode only, no C#.
Divyesh confirmed the points marked (D) on 23 Nov.
 
## 1. What the agent does and does not do
 
- It reads what the user types and suggests the next step. Our services do the work.
- It can: search films, book seats (as a proposal the user confirms), find a booking.
- It cannot: write to the database, skip the role check, save a booking without the user pressing Confirm, cancel a booking, change points, or see passwords or keys.
- We call an AI web API with HttpClient and System.Text.Json (lecturer, 8 Nov). No AI SDK package. The Week 12 slides use Semantic Kernel, so we write the same ideas ourselves: tools, a loop, memory, safety limits.
## 2. The arms
 
The agent always runs with the signed-in user's role and member id. It never gets more rights than the buttons. All four roles (Guest, Customer, Staff, Admin) may use arms 1 to 4 (D).
 
| Arm | Inputs | Output | Calls | Roles |
|---|---|---|---|---|
| search_films | title (partial, any case), genre; either may be empty | films (id, title, genre) | FilmService.SearchByTitle, SearchByGenre, Search(title, genre) | all four |
| list_showings | film_id | showings (id, screen, start time, price) | ShowingService.GetShowingsForFilm | all four |
| check_free_seats | showing_id | free seat codes (A1 to H12) | ShowingService.GetFreeSeats (new, 373). Reads the seat grid in the ShowingTable, the one source of truth (D) | all four |
| prepare_booking | showing_id, seat codes, guest name/email/phone (Guest only) | proposal (below). Nothing is saved | BookingService.PrepareBooking (new small method, 373). Same price rule as BookSeats, so proposal and real booking always match (D) | all four |
| find_booking | reference, email | booking or "not found" (same text for a wrong email) | BookingLookup.Find | Guest and Customer: email must match, like CancelBooking. Staff, Admin: any booking |
 
Proposal fields (D): showing_id, film title, start time, seat codes, total price after the tier discount, discount amount, points earned, points to use (always 0 until card 330). The agent never works out a price itself.
 
## 3. Flow (this is the think, act, observe loop from Week 12)
 
```
run_agent(user_text, session, cancel_token)    // session has role and member_id
    memory.Add("user", user_text)
    steps = 0
    start timer
    work_list = empty                          // tool results live here, not in memory
    while steps < MAX_STEPS and timer < TOTAL_TIMEOUT_SECONDS
        if cancel_token is cancelled
            audit("agent_cancelled")
            return message "stopped"
        reply = ai_client.AskAsync(memory.ListAll() + work_list, arm_list)
        if reply is a failure
            audit("agent_api_failed")
            return reply.error_message         // clear message, the app keeps working
        if reply is plain text
            memory.Add("assistant", reply.text)
            return reply.text
        steps = steps + 1
        if not arm_allowed(session.role, reply.arm_name)
            result = "this is not allowed for your role"
            audit("agent_denied", arm_name)
        else if inputs are bad (TryParse, empty, unknown arm)
            result = "bad input: " + reason
            audit("agent_bad_input", arm_name)
        else
            result = Tools.Run(reply.arm_name, reply.inputs, session)    // calls a service
            audit("agent_done", arm_name + " " + short result)
        if arm_name is prepare_booking and result is a proposal
            return proposal to the window      // loop stops here
        add result to work_list
    return message "I could not finish in MAX_STEPS steps, please use the screens"
 
window (Divyesh) shows the proposal with Confirm and Cancel
on Confirm
    BookingService.BookSeats(session.role, ...) // the same call as the book button
    audit("agent_booking_confirmed", booking reference)
on Cancel
    audit("agent_booking_declined", booking reference)
```
 
The window runs run_agent on a background task (BackgroundCall) and shows a Cancel button while it waits (D).
 
## 4. Safety limits
 
- MAX_STEPS = 5, 20 seconds per API call, 60 seconds for the whole request (D). Constants at the top of AgentLoop.
- Confirmation: only the window saves a booking, and only after Confirm.
- Audit: every agent action goes to audit_log with IUserRepository.WriteAudit(user_id, action, details). user_id is null for a Guest. action is at most 50 characters, details at most 500. Details never hold a password, a full email or a card number, only a booking reference (D).
- Data protection (CST2565): tool results sent to the AI never hold a member's or guest's email or phone number. Only what the answer needs: film titles, times, seat codes, prices, a booking reference. Keep tool descriptions and tool results short, because the tool list is sent on every call.
- Key: read from appsettings.json (git-ignored). Only appsettings.example.json is committed. A blank or placeholder key switches the agent off with a clear message.
- Failure: no network, timeout, bad status (including 429 rate limit) or bad json gives one plain message with the real reason. A 429 shows in the chat box as "the AI is busy, please wait a minute and try again". Services never show message boxes. The rest of the app keeps working.
## 5. Memory (card 374, Kushal)
 
- MemoryQueue, our own queue (array or ChainList, no List<T>). Last 10 messages, constant MEMORY_SIZE. When full, Add drops the oldest. This is the sliding window from Week 12.
- Per session, cleared on sign-out. In memory only, not in the database.
- Methods: Add(string role, string text), ListAll() oldest first, Clear(), Count.
- Only user and assistant text goes in. Tool results stay in work_list.
## 6. Tests (card 375, Akilesh)
 
- IAiClient is a small interface. AiClient is the real one. FakeAiClient is our own hand-written fake (no mock library) that returns scripted replies. Tests never call the real API.
- Cases: each arm allowed or denied for each of the four roles; bad input; stops at MAX_STEPS; stops at timeout; cancel stops the loop; API failure gives the message; blank key gives the message; prepare_booking saves nothing (booking count unchanged); confirm saves once; proposal price equals the BookSeats price; every action writes an audit row; audit details and tool results hold no email or phone number; a 429 gives the busy message; memory keeps 10 and drops the oldest; Clear on sign-out.
- AiClient itself is tested with a fake HttpMessageHandler for failures (card 372).
- Coding cards use red, green, refactor. This design card has no tests.
## 7. Who builds what
 
| Card | Part | Owner | When |
|---|---|---|---|
| 372 | IAiClient, AiClient (HttpClient, key from config) | Shankar | first |
| 373 | Tools.cs: five arms, role table, GetFreeSeats, PrepareBooking | Shankar | after 372 |
| 410 | AgentLoop.cs | Shankar | after 373 |
| 374 | MemoryQueue, KeywordTable | Kushal | now, standalone |
| 375 | AgentTests with the fake | Akilesh | after the IAiClient interface exists |
| 377 | show the agent in the video and report | Divyesh | Sprint 4 |
| - | agent chat box, proposal window, Cancel button | Divyesh | with the windows |
 
372, 373 and 410 will run into Sprint 4 (D). find_booking needs BookingLookup and ShowingService from feature/divyesh (commits b438f85 and 491be90). Shankar takes them from there, or merges after Divyesh says pushed.
 
## 8. Dates and open points
 
- Sprint 3 ends Sun 29 Nov. "Finish by Mon 23 Nov" in file 27 is old (D). Plan for submission on Thu 17 Dec. Divyesh will check the module brief for the official 17 or 18 Dec (D).
- AI provider (D): Groq (groq.com, not Grok from xAI) is the default. base_url, model_name and api_key all live in appsettings.json under "Ai", never in code, so changing one line changes the model or the provider. Everyone makes their own free key in their own appsettings.json, never in chat or Git.
- Limits: the free limit for the big Llama model is about 1,000 requests a day and 12,000 tokens a minute (third-party list, check console.groq.com limits for your own account). With up to 5 calls per request, one request can use most of a minute. For the demo video, run one request at a time.
- Before 372 goes green: Shankar tests one real tool call with his key on the chosen model, to prove tool calling works on it.
- Format: the Groq endpoint (https://api.groq.com/openai/v1) uses the same chat completions with tools format as the OpenAI API in the Week 12 slides (slides 5 to 7 and 19). Only AiClient knows the wire format. Choose a model that supports tool calling.
- KeywordTable: needs a job. Suggestion: the fake agent uses it to pick a scripted reply from the words in the user text. Kushal and Shankar to agree.
 