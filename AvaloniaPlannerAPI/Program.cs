
using AvaloniaPlannerAPI.Managers;
using CSUtil.Logging;

namespace AvaloniaPlannerAPI
{
    public class Program
    {
        const string DatabaseConfigFile = "db.ini";

        public static void Main(string[] args)
        {
            if(!File.Exists(DatabaseConfigFile))
            {
                Log.FatalError("Database config file not found in " + DatabaseConfigFile);
                throw new FileNotFoundException("Database config file not found in " + DatabaseConfigFile);
            }
            DbManager.Initialize(File.ReadAllText(DatabaseConfigFile));

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}