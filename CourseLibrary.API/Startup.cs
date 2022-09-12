using CourseLibrary.API.DbContexts;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;

namespace CourseLibrary.API
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
           services.AddControllers(setupAccept =>
           {
               setupAccept.ReturnHttpNotAcceptable = true;
           }).AddNewtonsoftJson(setupAction =>
           {
               setupAction.SerializerSettings.ContractResolver =
               new CamelCasePropertyNamesContractResolver();
           }).AddXmlDataContractSerializerFormatters()
           .ConfigureApiBehaviorOptions( setupAction =>
           {
               setupAction.InvalidModelStateResponseFactory = context =>
               {
                   //Create a problem details object
                   var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                   var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(
                       context.HttpContext,
                       context.ModelState);

                   // Add additional info not added by default
                   problemDetails.Detail = "See the errors field for details.";
                   problemDetails.Instance = context.HttpContext.Request.Path;

                   // find out which status code to use
                   var actionExecutingContext = context as Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext;

                   // if there are modelstate errors & all arguments were correctly
                   // found/parsed we're dealing with validation errors
                   if ((context.ModelState.ErrorCount > 0) &&
                   (actionExecutingContext?.ActionArguments.Count == context.ActionDescriptor.Parameters.Count))
                   {
                       problemDetails.Type = "modelvalidationproblem";
                       problemDetails.Status = StatusCodes.Status422UnprocessableEntity;
                       problemDetails.Title = "One or more validation errors occurred.";

                       return new UnprocessableEntityObjectResult(problemDetails)
                       {
                           ContentTypes = { "application/problem+json" }
                       };
                   }
                   // if one of the arguments wasn't correctly found / couldn't be parsed
                   // we are dealing with null/unparsable input
                   problemDetails.Status = StatusCodes.Status400BadRequest;
                   problemDetails.Title = "One or more errors on input occurred.";
                   return new BadRequestObjectResult(problemDetails)
                   {
                       ContentTypes = { "application/problem+json" }
                   };
               };

           });

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
             
            services.AddScoped<ICourseLibraryRepository, CourseLibraryRepository>();

            services.AddDbContext<CourseLibraryContext>(options =>
            {
                options.UseSqlServer(
                    Configuration["ConnectionString"]);
            }); 
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happend. Try again later");
                    });
                });
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
