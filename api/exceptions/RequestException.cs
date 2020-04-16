namespace LiteralLifeChurch.LiveStreamingApi.exceptions
{
    public abstract class RequestException : BaseException
    {
        public RequestException()
        {
            Status = 400;
        }
    }
}
