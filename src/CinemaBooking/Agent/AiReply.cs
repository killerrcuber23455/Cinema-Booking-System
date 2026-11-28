// the answer from the ai client: the reply text, or a plain error message (owner: shankar)
namespace CinemaBooking.Agent
{
    public class AiReply
    {
        public bool IsOk { get; }
        public string Text { get; }
        public string ErrorMessage { get; }

        private AiReply(bool is_ok, string text, string error_message)
        {
            IsOk = is_ok;
            Text = text;
            ErrorMessage = error_message;
        }

        // good answer from the AI
        public static AiReply Ok(string text)
        {
            return new AiReply(true, text, "");
        }

        //something went wrong and message says what
        public static AiReply Fail(string error_message)
        {
            return new AiReply(false, "", error_message);
        }
    }
}