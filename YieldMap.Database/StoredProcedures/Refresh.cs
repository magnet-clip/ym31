﻿using System;
using System.Linq;

namespace YieldMap.Database.StoredProcedures {
    public class Refresh : IDisposable {
        private readonly MainEntities _context = new MainEntities(DbConn.ConnectionString);
        
        private static bool NeedsRefresh(Chain chain, DateTime today) {
            return chain.Expanded.HasValue && chain.Expanded.Value < today || !chain.Expanded.HasValue;
        }

        private static bool NeedsRefresh(InstrumentBond bond, DateTime today) {
            if (!bond.NextCoupon.HasValue) return false;
            
            var nextCoupon = bond.NextCoupon.Value;
            return nextCoupon < today + TimeSpan.FromDays(7);
        }

        public Chain[] ChainsInNeed(DateTime dt) {
            return (from c in _context.Chains.ToList() 
                    where NeedsRefresh(c, dt)
                    select new Chain {
                        Name = c.Name, 
                        Expanded = c.Expanded, 
                        Feed = c.Feed == null ? null : new Feed {Name = c.Feed.Name}
                    }).ToArray();
        }

        public bool NeedsReload(DateTime dt) {
            return ChainsInNeed(dt).Any();
        }

        /// <summary>
        /// Enumerates rics of all EXISTING bonds that need to be refreshed
        /// Condition for refresh is presense of embedded option which is due in +/- 7 days from today
        /// irrespective to if these rics come from chains or they are standalone
        /// </summary>
        /// <param name="dt">Today's date</param>
        /// <returns>IEnumerable of Rics</returns>
        public Ric[] StaleBondRics(DateTime dt) {
                return _context.InstrumentBonds.ToList()
                    .Where(b => b.Ric != null && NeedsRefresh(b, dt))
                    .Select(b => b.Ric.ToPocoSimple())
                    .ToArray();
        }

        public Ric[] AllBondRics() {
            return _context.InstrumentBonds.ToList()
                    .Where(b => b.Ric != null)
                    .Select(b => b.Ric.ToPocoSimple())
                    .ToArray();
        }

        /// <summary> Enumerates rics which belong to matured bonds </summary>
        /// <param name="dt">Today's date</param>
        /// <returns>IEnumerable of Rics</returns>
        public Ric[] ObsoleteBondRics(DateTime dt) {
            return _context.InstrumentBonds.ToList()
                    .Where(b => b.Ric != null && b.Maturity.HasValue && b.Maturity.Value < dt)
                    .Select(b => b.Ric.ToPocoSimple())
                    .ToArray();
        }

        public void Dispose() {
            _context.Dispose();
        }
    }
}