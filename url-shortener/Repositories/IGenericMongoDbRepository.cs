using System.Linq.Expressions;

namespace url_shortener.Repositories
{
    public interface IGenericMongoDbRepository<TEntity>
    {
        IEnumerable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "");

        TEntity GetOne(Expression<Func<TEntity, bool>> filter);

        void Insert(TEntity entity);

        void Delete(Expression<Func<TEntity, bool>> filter);

        void DeleteRange(Expression<Func<TEntity, bool>> filter);

        void Update(Expression<Func<TEntity, bool>> filter, TEntity entityToUpdate);
    }
}
