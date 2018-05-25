using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanesExample.Entities
{
    [Table("Cities")]
    public class City
    {
        [Key]
        public int CityId { get; set; }        
        public string CityName { get; set; }
        public override string ToString() => CityName;
    }
}
