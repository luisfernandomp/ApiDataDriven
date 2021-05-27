using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shop.Data;

namespace Shop
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
    
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            services.AddControllers();

            services.AddResponseCompression(options =>
                {
                    options.Providers.Add<GzipCompressionProvider>();
                    options.MimeTypes =  
                        ResponseCompressionDefaults.MimeTypes.Concat(new[] {"application/json"});
                }
            );

            // services.AddResponseCaching() -> adiciona o header CACHE em toda a api
            
            // services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("Database")); -> Exemplo usando banco em memória

            var key = Encoding.ASCII.GetBytes(Settings.Secret);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddDbContext<DataContext>(opt => opt.UseSqlServer(Configuration.GetConnectionString("connectionString")));

            //Injeção de dependência 
            // AddScoped (garante um único datacontext por requisição), 
            // AddTransient (toda vez que se pede um datacontext ele entrega um novo)
            // e AddSingleton (cria uma instância por aplicação, a primeira vez que ela iniciar ele ira entregar o mesmo datacontext)
            // services.AddScoped<DataContext, DataContext>();

            // O AddDbContext já faz a função do AddScoped, não sendo mais necessário sua utilização

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Shop", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shop v1"));
            }

            app.UseHttpsRedirection();

            app.UseSwagger();
            
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json","Shop V1");
            });

            app.UseRouting();

            app.UseCors( x => x
                .AllowAnyHeader()
                .AllowAnyOrigin()
                .AllowAnyMethod());
            
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
