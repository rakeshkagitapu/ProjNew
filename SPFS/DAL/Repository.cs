using SPFS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq.Expressions;
using System.Data.Entity;

namespace SPFS.DAL
{
    public class Repository : IRepository,IDisposable
    {
        private SPFSContext _context;
        public Repository()
        {
            _context = new SPFSContext();
        }

        public SPFSContext Context { get { return _context; } }
        

        /// <summary>
        /// Adds the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        public void Add<T>(T entity) where T : class
        {
            Context.Set<T>().Add(entity);
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        public void Delete<T>(T entity) where T : class
        {
            Context.Set<T>().Remove(entity);
        }

        public void Detach(object entity)
        {
            Context.Entry(entity).State = EntityState.Detached;
        }


        /// <summary>
        /// Executes SQL command.
        /// </summary>
        /// <param name="sql">The SQL statement.</param>
        /// <param name="parameters">The SQL parameters.</param>
        public void ExecuteSqlCommand(string sql, params object[] parameters)
        {
            Context.Database.ExecuteSqlCommand(sql, parameters);
        }

        /// <summary>
        /// Loads entity into context base on collection select.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="selector">The collection selector function.</param>
        public void LoadCollection<T, U>(T entity, Expression<Func<T, ICollection<U>>> selector)
            where T : class
            where U : class
        {
            Context.Entry(entity).Collection(selector).Load();
        }

        /// <summary>
        /// Loads entity into context base on reference select.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="selector">The reference selector function.</param>
        public void LoadEntity<T, U>(T entity, Expression<Func<T, U>> selector)
            where T : class
            where U : class
        {
            Context.Entry(entity).Reference(selector).Load();
        }

        /// <summary>
        /// Saves context changes.
        /// </summary>
        public void Save()
        {
            try
            {
                Context.SaveChanges();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        /// <summary>
        /// Gets the repository's specified set.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>IQueryable.</returns>
        public IQueryable Set(Type type)
        {
            return Context.Set(type);
        }

        /// <summary>
        /// Gets the repository's specified set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>IQueryable&lt;T&gt;.</returns>
        public IQueryable<T> Set<T>() where T : class
        {
            return Context.Set<T>();
        }

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="id">The identifier.</param>
        /// <exception cref="System.ArgumentException">Cannot add a null entity.</exception>
        public void Update<T>(T entity, int id) where T : class
        {
            if(entity==null)
            {
                throw new ArgumentException("Cannot add a null entity.");
            }
            var entry = Context.Entry<T>(entity);
            if(entry.State == EntityState.Detached)
            {
                var set = Context.Set<T>();
                T attachedEntity = set.Find(id);
                if(attachedEntity != null)
                {
                    var attachedEntry = Context.Entry(attachedEntity);
                    attachedEntry.CurrentValues.SetValues(entity);
                }
                else
                {
                    entry.State = EntityState.Modified;
                }
            }
        }

        public void Dispose()
        {
           Context.Dispose();
        }
    }
}