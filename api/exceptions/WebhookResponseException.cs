using System.Net;

namespace LiteralLifeChurch.LiveStreamingApi.Exceptions
{
    public class WebhookResponseException : AppException
    {
        public WebhookResponseException()
        {
            Status = HttpStatusCode.InternalServerError;
        }
    }
}
