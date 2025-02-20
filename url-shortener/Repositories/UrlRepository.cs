using MongoDbGenericRepository;
using url_shortener.Models;
using url_shortener.Utilities;

namespace url_shortener.Repositories
{
    public class UrlRepository : GenericMongoDbRepository<Url>, IUrlRepository
    {
        public UrlRepository(MongoDbContext context) : base(context, AppConstants.URLS_REPOSITORY)
        {
        }
    }
}
