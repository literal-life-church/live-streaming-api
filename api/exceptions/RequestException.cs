using System.Net;

namespace LiteralLifeChurch.LiveStreamingApi.exceptions
{
    public abstract class RequestException : AppException
    {
        public RequestException()
        {
            Status = HttpStatusCode.BadRequest;
        }
    }
}
