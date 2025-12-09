using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.WebApi.Middleware;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using EFCore.DataClassification.WebApi.Mappings;

namespace EFCore.DataClassification.WebApi {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connectionString)
                        .UseDataClassificationSqlServer();

            });
            
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

         
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<UserMappingProfile>(); 
            });
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()) {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Exception handling middleware (must be first!)
            app.UseExceptionHandler();

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
