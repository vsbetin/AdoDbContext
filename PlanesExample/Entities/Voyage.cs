using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanesExample.Entities
{
    [Table("Voyages")]
    public class Voyage
    {
        [Key]
        public int VoyageId { get; set; }
        [ForeignKey("FK_Voyage_StartCity")]
        public City StartCity { get; set; }
        [ForeignKey("FK_Voyage_EndCity")]
        public City EndCity { get; set; }
        public override string ToString() => StartCity.ToString() + "-" + EndCity.ToString();
    }
}
