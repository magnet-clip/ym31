﻿using System.Linq;
using YieldMap.Database;
using YieldMap.Transitive.Domains.Contexts;

namespace YieldMap.Transitive.Domains.Readers {
    public class FeedReader : ReadOnlyRepository<FeedsContext>, IFeedReader {
        public FeedReader() {}
        public FeedReader(FeedsContext context) : base(context) {}
        
        public IQueryable<Feed> Feeds {
            get {
                return Context.Feeds.AsNoTracking();
            }
        }
        //public IQueryable<Chain> Chains {
        //    get {
        //        return Context.Chains.AsNoTracking();
        //    }
        //}
        //public IQueryable<Ric> Rics {
        //    get {
        //        return Context.Rics.AsNoTracking();
        //    }
        //}
        //public IQueryable<Index> Indices {
        //    get {
        //        return Context.Indices.AsNoTracking();
        //    }
        //}
        //public IQueryable<Isin> Isins {
        //    get {
        //        return Context.Isins.AsNoTracking();
        //    }
        //}
    }
}