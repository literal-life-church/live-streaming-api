using System.Net;

namespace LiteralLifeChurch.LiveStreamingApi.exceptions
{
    public abstract class RequestException : AppException
    {
        protected RequestException()
        {
            Status = HttpStatusCode.BadRequest;
        }
    }
}
