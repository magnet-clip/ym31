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
    
    public partial class SubIndustry
    {
        public SubIndustry()
        {
            this.InstrumentBonds = new HashSet<InstrumentBond>();
        }
    
        public long id { get; set; }
        public string Name { get; set; }
        public long id_Industry { get; set; }
    	public SubIndustry ToPocoSimple() {
    	    return new SubIndustry {
    			id = this.id,
    			Name = this.Name,
    			id_Industry = this.id_Industry,
    		};
    	}
    		
        public virtual Industry Industry { get; set; }
        public virtual ICollection<InstrumentBond> InstrumentBonds { get; set; }
    }
}
