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
        // the work must not touch any control, the window reads the result after the await, back on its own thread
        public static async Task<CallResult<T>> RunAsync<T>(Func<T> work)
        {
            try
            {
                T value = await Task.Run(work);
                return new CallResult<T>(true, value, "");
            }
            catch (Exception exception)
            {
                return new CallResult<T>(false, default, ErrorTextFor(exception));
            }
        }

        // the sentence to show for this error
        // the data layer throws InvalidOperationException with a clear sentence when SQL Server cannot be reached,
        // that sentence is meant for the user. any other error shows the general text, so no technical detail leaks
        public static string ErrorTextFor(Exception exception)
        {
            if (exception is InvalidOperationException && !string.IsNullOrWhiteSpace(exception.Message))
            {
                return exception.Message;
            }

            return GENERIC_ERROR;
        }
    }
}
