using HrBackOffice.Helper.FileProcessingService;
using Microsoft.AspNetCore.SignalR;

namespace HrBackOffice.Hubs
{
    // SignalR Hub
    public class ProcessingHub : Hub
    {
        public async Task UpdateProcessingStatus(ProcessingStatus status)
        {
            await Clients.All.SendAsync("ReceiveProcessingUpdate", status);
        }
    }

}
