namespace Waybon.Domain.Exceptions;

public sealed class AccountLockedException(string message) : Exception(message)
{
    
}