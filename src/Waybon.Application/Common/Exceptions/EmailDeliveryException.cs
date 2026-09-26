namespace Waybon.Application.Common.Exceptions;

public sealed class EmailDeliveryException(string message) : Exception(message)
{

}