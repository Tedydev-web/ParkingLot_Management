using System;
using System.Net;

namespace ParkingLotManagement.Exceptions
{
  public class GoongApiException : Exception
  {
    public HttpStatusCode StatusCode { get; }

    public GoongApiException(string message, HttpStatusCode statusCode)
        : base(message)
    {
      StatusCode = statusCode;
    }

    public GoongApiException(string message, HttpStatusCode statusCode, Exception inner)
        : base(message, inner)
    {
      StatusCode = statusCode;
    }
  }
}
