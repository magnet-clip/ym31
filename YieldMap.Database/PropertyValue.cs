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
    public partial class PropertyValue //: IObjectWithState
    {
        public long id { get; set; }
        public long id_Property { get; set; }
        public long id_Instrument { get; set; }
        public string Value { get; set; }
    	public PropertyValue ToPocoSimple() {
    	    return new PropertyValue {
    			id = this.id,
    			id_Property = this.id_Property,
    			id_Instrument = this.id_Instrument,
    			Value = this.Value,
    		};
    	}
    
    	//public State State {get;set;}
    		
        public virtual Instrument Instrument { get; set; }
        public virtual Property Property { get; set; }
    }
}
