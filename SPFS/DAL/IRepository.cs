using SPFS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace SPFS.DAL
{
    public interface IRepository
    {
        SPFS.Model.SPFSContext Context { get; }
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        void Detach(object entity);
        IQueryable Set(Type type);
        IQueryable<T> Set<T>() where T : class;
        void LoadEntity<T, U>(T entity, Expression<Func<T, U>> selector) where T : class where U : class;
        void LoadCollection<T, U>(T entity, Expression<Func<T, ICollection<U>>> selector) where T : class where U : class;
        void Save();       
        void ExecuteSqlCommand(string sql, params object[] parameters);
        void Update<T>(T entity, int id) where T : class;
        
    }   
}