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
    public partial class RicToChain //: IObjectWithState
    {
        public long id { get; set; }
        public Nullable<long> Ric_id { get; set; }
        public Nullable<long> Chain_id { get; set; }
    	public RicToChain ToPocoSimple() {
    	    return new RicToChain {
    			id = this.id,
    			Ric_id = this.Ric_id,
    			Chain_id = this.Chain_id,
    		};
    	}
    
    	//public State State {get;set;}
    		
        public virtual Chain Chain { get; set; }
        public virtual Ric Ric { get; set; }
    }
}
