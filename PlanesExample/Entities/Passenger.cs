using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanesExample.Entities
{
    [Table("Passengers")]
    public class Passenger
    {
        [Key]
        public int PassengerId { get; set; }
        public string PassengerFirstName { get; set; }
        public string PassengerLastName { get; set; }
        [ForeignKey("FK_Passenger_Flight")]
        public Flight Flight { get; set; }
    }
}
