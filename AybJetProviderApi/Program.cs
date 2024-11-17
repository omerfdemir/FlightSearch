using AybJetProviderApi.Services;
using AybJetProviderApi.Services.Cache;

namespace AybJetProviderApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            builder.Services.AddControllers();
            builder.Services.AddScoped<IAybJetService, AybJetService>();
            builder.Services.AddSingleton<ICache, MemoryCache>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            var app = builder.Build();
            
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            
            app.UseRouting();

            app.MapControllers();

            app.Run();
        }
    }
}