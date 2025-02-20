using url_shortener.Models;

namespace url_shortener.Repositories
{
    public interface IUrlRepository : IGenericMongoDbRepository<Url>
    {
    }
}
