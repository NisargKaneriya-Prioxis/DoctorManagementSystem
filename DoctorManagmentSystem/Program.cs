using DMS.Models.Models.MyDoctorsDB;
using DMS.Models.Validator;
using DMS.Service.Repository.Implementation;
using DMS.Service.Repository.Interface;
using DoctorManagmentSystem.Helper;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DoctorManagmentSystem;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        
        //Code for adding the log to the new file 
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
            .Enrich.FromLogContext()
            .CreateLogger();

        builder.Host.UseSerilog();
        
        var connectionString = builder.Configuration.GetConnectionString("DBConnection");
        builder.Services.AddScoped<IDoctorRepository , DoctorRepository>();
        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        builder.Services.AddControllers()
            .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<DoctorRequestModelValidator>());
        builder.Services.AddControllers()
            .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<DoctorRequestWithoutSidModelValidator>());
        // Add services to the container.
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        builder.Services.AddDbContext<DoctorsDbContext>(options => options.UseSqlServer(connectionString));
        
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
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.Run();
    }
}