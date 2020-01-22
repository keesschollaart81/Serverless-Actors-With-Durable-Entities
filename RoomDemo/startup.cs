using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using RoomDemo.RoomBookingService;

[assembly: FunctionsStartup(typeof(RoomDemo.Startup))]
namespace RoomDemo
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped<IRoomBookingService, StubRoomBookingService>();
        }
    }
}