//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class InstrumentBond
    {
        public long id { get; set; }
        public Nullable<long> id_Issuer { get; set; }
        public Nullable<long> id_Borrower { get; set; }
        public Nullable<long> id_Currency { get; set; }
        public string BondStructure { get; set; }
        public string RateStructure { get; set; }
        public Nullable<long> IssueSize { get; set; }
        public string Name { get; set; }
        public Nullable<bool> IsCallable { get; set; }
        public Nullable<bool> IsPutable { get; set; }
        public string Series { get; set; }
        public Nullable<long> id_Isin { get; set; }
        public Nullable<long> id_Ric { get; set; }
        public Nullable<long> id_Ticker { get; set; }
        public Nullable<long> id_SubIndustry { get; set; }
        public Nullable<long> id_Type { get; set; }
        public Nullable<System.DateTime> Issue { get; set; }
        public Nullable<System.DateTime> Maturity { get; set; }
    
        public virtual RefCurrency RefCurrency { get; set; }
        public virtual RefBorrower RefBorrower { get; set; }
        public virtual RefInstrument RefInstrument { get; set; }
        public virtual RefSubIndustry RefSubIndustry { get; set; }
        public virtual RefTicker RefTicker { get; set; }
        public virtual RefRic RefRic { get; set; }
        public virtual RefIsin RefIsin { get; set; }
        public virtual RefIssuer RefIssuer { get; set; }
    }
}
