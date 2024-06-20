//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ConvenienceStore.Model
{
    using ConvenienceStore.Model.Admin;
    using System;
    using System.Collections.Generic;
    
    public partial class Consignment
    {
        public int InputInfoId { get; set; }
        public string ProductId { get; set; }
        public Nullable<int> Stock { get; set; }
        public Nullable<System.DateTime> ManufacturingDate { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }
        public Nullable<int> InputPrice { get; set; }
        public Nullable<int> OutputPrice { get; set; }
        public Nullable<int> Discount { get; set; }
    
        public virtual InputInfo InputInfo { get; set; }
        public virtual Product Product { get; set; }
    }
}
