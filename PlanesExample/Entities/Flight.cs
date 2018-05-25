using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanesExample.Entities
{
    [Table("Flights")]
    public class Flight
    {
        [Key]
        public int FlightId { get; set; }
        [ForeignKey("FK_Flight_Voyage")]
        public Voyage Voyage { get; set; }
        [ForeignKey("FK_Flight_Pilot")]
        public Pilot Pilot { get; set; }
        [ForeignKey("FK_Flight_Plane")]
        public Plane Plane { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int BoughtPlaces { get; set; }
    }
}
