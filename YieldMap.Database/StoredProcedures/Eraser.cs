﻿using System;
using System.Linq;
using YieldMap.Database.Access;

namespace YieldMap.Database.StoredProcedures {
    public class Eraser : AccessToDb {
        public void DeleteBonds(Func<InstrumentBond, bool> selector = null) {
            if (selector == null) selector = x => true;
            try {
                Context.Configuration.AutoDetectChangesEnabled = false;
                var bondsToDelete = Context.InstrumentBonds.ToList().Where(bond => selector(bond)).ToList();

                foreach (var bond in bondsToDelete) {
                    Context.InstrumentBonds.Remove(bond);
                }
                Context.SaveChanges();
            } finally {
                Context.Configuration.AutoDetectChangesEnabled = true;
            }
        }

        public void DeleteFeeds(Func<Feed, bool> selector = null) {
            if (selector == null)
                selector = x => true;

            try {
                Context.Configuration.AutoDetectChangesEnabled = false;
                var feeds = Context.Feeds.ToList().Where(feed => selector(feed)).ToList();
                foreach (var feed in feeds)  Context.Feeds.Remove(feed);
                Context.SaveChanges();
            } finally {
                Context.Configuration.AutoDetectChangesEnabled = true;
            }
        }

        public void DeleteChains(Func<Chain, bool> selector = null) {
            if (selector == null)
                selector = x => true;
            try {
                Context.Configuration.AutoDetectChangesEnabled = false;
                var chains = Context.Chains.ToList().Where(chain => selector(chain)).ToList();

                foreach (var chain in chains) {
                    var c = chain;
                    var links = Context.RicToChains.ToList().Where(link => link.Chain_id == c.id).ToList();
                    foreach (var link in links) Context.RicToChains.Remove(link);
                    Context.Chains.Remove(chain);
                }
                Context.SaveChanges();
            } finally {
                Context.Configuration.AutoDetectChangesEnabled = true;
            }
        }
    }
}