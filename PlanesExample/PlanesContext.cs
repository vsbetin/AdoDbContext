using AdoDbContext;
using PlanesExample.Entities;

namespace PlanesExample
{
    public class PlanesContext : DbContext
    {
        public DbList<City> Cities { get; set; }
        public DbList<Flight> Flights { get; set; }
        public DbList<Passenger> Passengers { get; set; }
        public DbList<Pilot> Pilots { get; set; }
        public DbList<Plane> Planes { get; set; }
        public DbList<Voyage> Voyages { get; set; }

        // Create your MS SQL DB or local DB and pass connection string
        public PlanesContext() : base(@"Data Source=.\SQLEXPRESS;Initial Catalog=Planes;Integrated Security=True")
        {            
        }
    }
}
