using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Waybon.Infrastructure.Persistence;

public static class DbUpdateExceptionExtensions
{
    // ===================================
    // IsUniqueViolation
    // ===================================

    public static bool IsUniqueViolation(this DbUpdateException exception)
    {
        return exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation
        };
    }

    public static bool IsUniqueViolation(this DbUpdateException exception, string constraintName)
    {
        return exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation
        } postgresException && postgresException.ConstraintName == constraintName;
    }


    // ===================================
    // IsForeignKeyViolation
    // ===================================

    public static bool IsForeignKeyViolation(this DbUpdateException exception)
    {
        return exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ForeignKeyViolation
        };
    }
}