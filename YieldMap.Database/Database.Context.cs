﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace YieldMap.Database
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class MainEntities : DbContext
    {
        public MainEntities()
            : base("name=MainEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Country> Countries { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Feed> Feeds { get; set; }
        public DbSet<Industry> Industries { get; set; }
        public DbSet<InstrumentBond> InstrumentBonds { get; set; }
        public DbSet<InstrumentCustomBond> InstrumentCustomBonds { get; set; }
        public DbSet<Isin> Isins { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<RatingAgency> RatingAgencies { get; set; }
        public DbSet<RawBondInfo> RawBondInfoes { get; set; }
        public DbSet<RawFrnData> RawFrnDatas { get; set; }
        public DbSet<RawRating> RawRatings { get; set; }
        public DbSet<Seniority> Seniorities { get; set; }
        public DbSet<SubIndustry> SubIndustries { get; set; }
        public DbSet<Ticker> Tickers { get; set; }
        public DbSet<Specimen> Specimens { get; set; }
        public DbSet<Chain> Chains { get; set; }
        public DbSet<Ric> Rics { get; set; }
        public DbSet<RicToChain> RicToChains { get; set; }
        public DbSet<Borrower> Borrowers { get; set; }
        public DbSet<Issuer> Issuers { get; set; }
    }
}
