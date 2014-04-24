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
    
    public partial class Ric
    {
        public Ric()
        {
            this.InstrumentBonds = new HashSet<InstrumentBond>();
            this.Chains = new HashSet<Chain>();
        }
    
        public long id { get; set; }
        public string Name { get; set; }
        public Nullable<long> Isin_id { get; set; }
        public Nullable<long> Feed_id { get; set; }
    
        public virtual Feed Feed { get; set; }
        public virtual ICollection<InstrumentBond> InstrumentBonds { get; set; }
        public virtual Isin Isin { get; set; }
        public virtual ICollection<Chain> Chains { get; set; }
    }
}