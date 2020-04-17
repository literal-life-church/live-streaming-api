using LiteralLifeChurch.LiveStreamingApi.models.input;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi.services.validators
{
    public class ServiceValidator
    {
        private readonly AuthenticationService AuthService;
        private readonly ConfigurationService ConfigService;

        public ServiceValidator()
        {
            AuthService = new AuthenticationService();
            ConfigService = new ConfigurationService();
        }

        public async Task Validate(InputRequestModel input)
        {

        }
    }
}
