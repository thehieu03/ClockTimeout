using Catalog.Infrastructure;

namespace Catalog.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var app = builder.Build();

            app.Run();
        }
    }
}
