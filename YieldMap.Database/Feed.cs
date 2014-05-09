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
    
    public partial class Feed
    {
        public Feed()
        {
            this.Isins = new HashSet<Isin>();
            this.Chains = new HashSet<Chain>();
            this.Rics = new HashSet<Ric>();
        }
    
        public long id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    
        public virtual ICollection<Isin> Isins { get; set; }
        public virtual ICollection<Chain> Chains { get; set; }
        public virtual ICollection<Ric> Rics { get; set; }
    }
}
