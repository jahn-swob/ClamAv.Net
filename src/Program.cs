using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ClamAv.Net
{
    public static class Program
    {
        [ExcludeFromCodeCoverage]
        private static void Main(string[] args)
        {
            Host
                .CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(web =>
                {
                    web.UseKestrel(c => c.AddServerHeader = false);
                    web.UseStartup<Startup>();
                })
                .Build()
                .Run();
        }
    }
}
