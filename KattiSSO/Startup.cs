using KattiSSO.Models;
using KattiSSO.Services.PasswordHasher;
using KattiSSO.Services.TokenGenerator;
using KattiSSO.Services.UserRepositories;
using KattiSSO.Services.TokenValidators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KattiSSO.Services.RefreshTokenRepostitory;
using KattiSSO.Services.Authenticators;

namespace KattiSSO
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Cors for refresh token
            services.AddCors(options =>
            {
                options.AddPolicy("Service",
                    builder =>
                    {
                        builder
                            .SetIsOriginAllowedToAllowWildcardSubdomains()
                            .WithOrigins("https://*.kattiwae.com")
                            .AllowCredentials()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                           
                    });
            });

            services.AddControllers();


            //Authentication config
            AuthenticationConfiguration authConfig = new AuthenticationConfiguration();
            _configuration.Bind("Authentication", authConfig);
            services.AddSingleton(authConfig);

            //Authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {

                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authConfig.AccessTokenSecret)),
                    ValidIssuer = authConfig.Issuer,
                    ValidAudience = authConfig.Audience,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true
                };
            });

            //Login services
            services.AddSingleton<IPasswordHasher, BcryptHasher>();
            services.AddSingleton<IUserRepository, DbUserRepository>();
            services.AddSingleton<AccessTokenGenerator>();
            services.AddSingleton<RefreshTokenGenerator>();
            services.AddSingleton<RefreshTokenValidator>();
            services.AddSingleton<RefreshTokenRepository>();
            services.AddSingleton<TokenGenerator>();
            services.AddSingleton<RoleRepository>();
            services.AddSingleton<Authenticator>();
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "KattiSSO", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "KattiSSO v1"));

                // NOTE: DO NOT MOVE OUT OF DEV ENVIRONMENT, XSS ETC
                // Exception: Handle CORS with nginx or other ways
                app.UseCors(builder => builder
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader());
            }
            
            

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
