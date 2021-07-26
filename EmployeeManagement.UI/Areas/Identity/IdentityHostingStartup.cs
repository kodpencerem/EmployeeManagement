using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(EmployeeManagement.UI.Areas.Identity.IdentityHostingStartup))]
namespace EmployeeManagement.UI.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}