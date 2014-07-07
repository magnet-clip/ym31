﻿using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using YieldMap.Database;
using YieldMap.Transitive.Domains.Contexts;

namespace YieldMap.Transitive.Domains.Repositories {
    public class ChainRepository : IChainRepository {
        private readonly ChainRicContext _context;

        public ChainRepository(IUnitOfWork<ChainRicContext> uow) {
            _context = uow.Context;
        }

        public ChainRepository() {
            _context = new ChainRicContext();
        }
        
        public IQueryable<Chain> FindAll() {
            return _context.Chains;
        }

        public IQueryable<Chain> FindAllIncluding(params Expression<Func<Chain, object>>[] inc) {
            return inc.Aggregate<Expression<Func<Chain, object>>, IQueryable<Chain>>(
                _context.Chains, 
                (current, expression) => current.Include(expression));
        }

        public IQueryable<Chain> FindBy(Func<Chain, bool> predicate) {
            return _context.Chains.Where(predicate).AsQueryable();
        }

        public Chain FindById(long id) {
            return _context.Chains.Find(id);
        }

        public void Insert(Chain item) {
            _context.Chains.Add(item);
            if (item.State != State.Added) 
                _context.ApplyStateChanges();
        }

        public void Add(Chain item) {
            _context.Entry(item).State = item.id == default(long) ? 
                EntityState.Added : 
                EntityState.Modified;
        }

        public void Remove(Chain item) {
            _context.Chains.Remove(item);
        }

        public void Dispose() {
            _context.Dispose();
        }
    }
}