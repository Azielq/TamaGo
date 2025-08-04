using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace TamaGo.Models;

[Table("destination")]
public class Destination : BaseModel
{
    [PrimaryKey("id_destination", false)]
    public int IdDestination { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("location")]
    public string Location { get; set; }

    [Column("destPicture")]
    public string DestPicture { get; set; }

    [Column("category")]
    public string Category { get; set; }
}