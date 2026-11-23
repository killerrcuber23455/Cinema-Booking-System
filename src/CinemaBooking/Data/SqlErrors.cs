// the sql server error numbers and the checks that Database and UserRepository share (owner: kushal)
using Microsoft.Data.SqlClient;

namespace CinemaBooking.Data
{
    internal static class SqlErrors
    {
        // a unique rule was broken
        public const int UNIQUE_KEY = 2627;
        public const int UNIQUE_INDEX = 2601;

        // a check rule or a foreign key rule refused the data
        public const int CHECK_OR_FOREIGN_KEY = 547;

        // text was longer than the column
        public const int TEXT_TOO_LONG = 8152;
        public const int TEXT_TOO_LONG_NEW = 2628;

        // true when a unique rule was broken
        public static bool IsUniqueError(SqlException error)
        {
            return error.Number == UNIQUE_KEY || error.Number == UNIQUE_INDEX;
        }

        // true when the database refused the data because of one of its rules
        public static bool IsRuleError(SqlException error)
        {
            return IsUniqueError(error)
                || error.Number == CHECK_OR_FOREIGN_KEY
                || error.Number == TEXT_TOO_LONG
                || error.Number == TEXT_TOO_LONG_NEW;
        }
    }
}