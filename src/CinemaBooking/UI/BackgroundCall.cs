// runs a slow call (the database, later the AI) on a background thread so the window stays responsive (owner: divyesh)
// and turns an error into one clear sentence for the user. the window shows a loading text while it waits
using System;
using System.Threading.Tasks;

namespace CinemaBooking.UI
{
    // what came back from a background call: the value, or the text to show when it failed
    public class CallResult<T>
    {
        public bool IsOk { get; }
        public T? Value { get; }
        public string ErrorText { get; }

        public CallResult(bool is_ok, T? value, string error_text)
        {
            IsOk = is_ok;
            Value = value;
            ErrorText = error_text;
        }
    }

    public static class BackgroundCall
    {
        // shown when something unexpected fails, it never shows the technical message to the user
        public const string GENERIC_ERROR = "Something went wrong. Please try again.";

        // runs the work on another thread and gives back the value, or the error text if it failed
        // this is only a stub for the red step, it does not run the work
        public static Task<CallResult<T>> RunAsync<T>(Func<T> work)
        {
            return Task.FromResult(new CallResult<T>(true, default, ""));
        }

        // the sentence to show for this error
        // this is only a stub for the red step
        public static string ErrorTextFor(Exception exception)
        {
            return "";
        }
    }
}
