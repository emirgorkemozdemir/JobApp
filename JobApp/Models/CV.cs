//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace JobApp.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class CV
    {
        public int CvID { get; set; }
        public string Link { get; set; }
        public int SelectedUser { get; set; }
    
        public virtual User User { get; set; }
    }
}
