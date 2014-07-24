﻿using System.Data.Entity;
using YieldMap.Database;
using YieldMap.Tools.Logging;

namespace YieldMap.Transitive.Domains.Contexts {
    public class SaverContext : BaseContext<SaverContext> {
        private static readonly Logging.Logger Logger = Logging.LogFactory.create("YieldMap.Transitive.Domains.Contexts.SaverContext");
        public SaverContext() {
            Logger.Debug("SaverContext()");
        }  
        
        
        public DbSet<Description> Descriptions { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<LegalEntity> LegalEntities { get; set; }
        public DbSet<Ticker> Tickers { get; set; }
        public DbSet<Industry> Industries { get; set; }
        public DbSet<SubIndustry> SubIndustries { get; set; }
        public DbSet<Specimen> Specimens { get; set; }
        public DbSet<Seniority> Seniorities { get; set; }
        public DbSet<Instrument> Instruments { get; set; }
        public DbSet<InstrumentType> InstrumentTypes { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Index> Indices { get; set; } // todo ??
        public DbSet<Ric> Rics { get; set; } // todo ??
        public DbSet<RicToChain> RicToChains { get; set; } // todo ??
        public DbSet<Feed> Feeds { get; set; } // todo ??
        public DbSet<Chain> Chains { get; set; } // todo ??
        public DbSet<Isin> Isins { get; set; } // todo ??
    }
}