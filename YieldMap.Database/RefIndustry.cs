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
    
    public partial class RefIndustry
    {
        public RefIndustry()
        {
            this.RefSubIndustries = new HashSet<RefSubIndustry>();
        }
    
        public long id { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<RefSubIndustry> RefSubIndustries { get; set; }
    }
}