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
    
    public partial class Posting
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Posting()
        {
            this.Application = new HashSet<Application>();
        }
    
        public int PostingID { get; set; }
        public string Title { get; set; }
        public int Company { get; set; }
        public string Description { get; set; }
        public string Skills { get; set; }
        public string Location { get; set; }

        public List<Skill> SkillsList { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Application> Application { get; set; }
        public virtual Company Company1 { get; set; }
    }
}
