using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanesExample.Entities
{
    [Table("Planes")]
    public class Plane
    {
        [Key]
        public int PlaneId { get; set; }
        public string PlaneName { get; set; }
        public int Capacity { get; set; }
        public override string ToString() => PlaneName + ": " + Capacity;
    }
}
