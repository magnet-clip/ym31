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
    //using YieldMap.Database.Domains;
    public partial class Currency //: IObjectWithState
    {
        public Currency()
        {
            this.Legs = new HashSet<Leg>();
        }
    
        public long id { get; set; }
        public string Name { get; set; }
    	public Currency ToPocoSimple() {
    	    return new Currency {
    			id = this.id,
    			Name = this.Name,
    		};
    	}
    
    	//public State State {get;set;}
    		
        public virtual ICollection<Leg> Legs { get; set; }
    }
}
