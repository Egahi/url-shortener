
using AutoMapper;
using MongoDbGenericRepository;
using url_shortener.AutoMapper;
using url_shortener.Repositories;
using url_shortener.Utilities;

namespace url_shortener
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // Auto Mapper
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            IMapper mapper = mappingConfig.CreateMapper();
            builder.Services.AddSingleton(mapper);

            // MongoDB
            // Connection string and database name hardcoded for simplicity
            var mongoDbContext = new MongoDbContext(AppConstants.MONGODB_CONNECTIONSTRING, AppConstants.URLS_REPOSITORY);
            builder.Services.AddSingleton(mongoDbContext);
            builder.Services.AddScoped<IUrlRepository, UrlRepository>();

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

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
