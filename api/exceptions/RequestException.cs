using System.Net;

namespace LiteralLifeChurch.LiveStreamingApi.Exceptions
{
    public abstract class RequestException : AppException
    {
        protected RequestException()
        {
            Status = HttpStatusCode.BadRequest;
        }
    }
}
