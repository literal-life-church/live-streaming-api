using System.Net;

namespace LiteralLifeChurch.LiveStreamingApi.exceptions
{
    public class WebhookResponseException : AppException
    {
        public WebhookResponseException()
        {
            Status = HttpStatusCode.InternalServerError;
        }
    }
}
