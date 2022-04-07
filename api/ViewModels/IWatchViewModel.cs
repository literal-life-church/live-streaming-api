using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LiteralLifeChurch.LiveStreamingApi.ViewModels
{
    public interface IWatchViewModel
    {
        public Task<IActionResult> WatchDefault();
    }
}
