using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDbGenericRepository;

namespace url_shortener.Repositories
{
    public class GenericMongoDbRepository<TEntity> : IGenericMongoDbRepository<TEntity> where TEntity : class
    {
        internal MongoDbContext context;
        internal IMongoCollection<TEntity> dbSet;

        public GenericMongoDbRepository(MongoDbContext context, string collection)
        {
            this.context = context;
            dbSet = context.Database.GetCollection<TEntity>(collection);
        }

        public virtual IEnumerable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet.AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }
            else
            {
                return query.ToList();
            }
        }

        public virtual TEntity GetOne(Expression<Func<TEntity, bool>> filter)
        {
            return dbSet.Find(filter).FirstOrDefault();
        }

        public virtual void Insert(TEntity entity)
        {
            dbSet.InsertOne(entity);
        }

        public virtual void Delete(Expression<Func<TEntity, bool>> filter)
        {
            dbSet.FindOneAndDelete(filter);
        }

        public virtual void DeleteRange(Expression<Func<TEntity, bool>> filter)
        {
            dbSet.DeleteMany(filter);
        }

        public virtual void Update(Expression<Func<TEntity, bool>> filter, TEntity entityToUpdate)
        {
            dbSet.ReplaceOne(filter, entityToUpdate);
        }
    }
}
