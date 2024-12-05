using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Upflux_WebService.Core.Configuration;
using Upflux_WebService.Services.Interfaces;
using Upflux_WebService.Services;
using Upflux_WebService.Data;
using Upflux_WebService.Repository.Interfaces;
using Upflux_WebService.Repository;
using Upflux_WebService.GrpcServices;
using Upflux_WebService.GrpcServices.Interfaces;

namespace Upflux_WebService
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container
			ConfigureServices(builder);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            var app = builder.Build();

			// Configure the middleware pipeline
			ConfigureMiddleware(app);


            app.Run();
		}

		private static void ConfigureServices(WebApplicationBuilder builder)
		{
			builder.Services.AddScoped<IAuthService, AuthService>()
				.AddScoped(typeof(IRepository<>), typeof(Repository<>))
				.AddScoped<ILicenceManagementService, LicenceManagementService>()
				.AddScoped<ILicenceRepository, LicenceRepository>()
				.AddScoped<IMachineRepository, MachineRepository>()
				.AddSingleton<LicenceCommunicationService>()
				.AddSingleton<ILicenceCommunicationService>(sp => sp.GetRequiredService<LicenceCommunicationService>());

			// Load JWT settings from configuration
			var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
			builder.Services.Configure<JwtSettings>(jwtSettingsSection);

			var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

			if (jwtSettings == null)
			{
				throw new InvalidOperationException("JWT settings are not configured properly.");
			}

			// Add controllers
			builder.Services.AddControllers();

			// Add Grpc Services
			builder.Services.AddGrpc();

			// Add Swagger for API documentation
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(options =>
			{
				options.SwaggerDoc("v1", new OpenApiInfo { Title = "Upflux Web Service API", Version = "v1" });

				// Add Bearer Authentication to Swagger
				options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					Name = "Authorization",
					Type = SecuritySchemeType.ApiKey,
					Scheme = "Bearer",
					BearerFormat = "JWT",
					In = ParameterLocation.Header,
					Description = "Enter 'Bearer' [space] and then your token in the text input below.\nExample: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9'"
				});

				// Set the comments path for the Swagger JSON and UI.
				var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
				options.IncludeXmlComments(xmlPath);

				options.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id = "Bearer"
							}
						},
						new string[] { }
					}
				});
			});

			// Add SignalR for real-time communication
			builder.Services.AddSignalR();

			// Configure JWT-based authentication
			builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = jwtSettings.Issuer,
					ValidAudience = jwtSettings.Audience,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
				};
			});

			// Add role-based authorization policies
			builder.Services.AddAuthorization(options =>
			{
				options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
				options.AddPolicy("EngineerOnly", policy => policy.RequireRole("Engineer"));
				options.AddPolicy("AdminOrEngineer", policy => policy.RequireRole("Admin", "Engineer"));
			});

			builder.Services.AddDbContext<ApplicationDbContext>(options =>
			options.UseMySql(
				builder.Configuration.GetConnectionString("DefaultConnection"),
				new MySqlServerVersion(new Version(8, 0, 39))
			));

			// Add any other required services here
			// Example: Dependency Injection for custom services
			// builder.Services.AddScoped<IMyService, MyService>();
		}

		private static void ConfigureMiddleware(WebApplication app)
		{
			// Enable Swagger in development environment
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

            // Use CORS
            app.UseCors("AllowAll");

            //Enforce HTTPS redirection
            app.UseHttpsRedirection();

			// Add authentication and authorization middleware
			app.UseAuthentication();
			app.UseAuthorization();

			// Map controllers
			app.MapControllers();

			// Map gRPC services
			app.MapGrpcService<LicenceCommunicationService>();

			// Map SignalR hubs (if applicable)
			// Example: app.MapHub<MyHub>("/myHub");
		}
	}
}
