
using AvaloniaPlannerAPI.Managers;
using CSUtil.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using Swashbuckle.AspNetCore.Newtonsoft;
using Newtonsoft.Json.Converters;

namespace AvaloniaPlannerAPI
{
    public class Program
    {
        const string DatabaseConfigFile = "db.ini";

        // TODO: (critical) Fix keys being static and public
        public static byte[] Key = new byte[0];
        public static string Issuer = "";
        public static string Audience = "";

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
            builder.Services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.Converters.Add(new StringEnumConverter()));

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
            });
            builder.Services.AddSwaggerGenNewtonsoftSupport();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                IdentityModelEventSource.ShowPII = true;
            }

#if DEBUG
            app.UseSwagger();
            app.UseSwaggerUI();
#endif

            //app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}