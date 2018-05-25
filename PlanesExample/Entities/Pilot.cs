using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanesExample.Entities
{
    [Table("Pilots")]
    public class Pilot
    {
        [Key]
        public int PilotId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public override string ToString() => FirstName + " " + LastName;
    }
}
