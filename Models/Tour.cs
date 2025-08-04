using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace TamaGo.Models;

[Table("tour")]
public class Tour : BaseModel
{
    [PrimaryKey("id_tour", false)]
    public int IdTour { get; set; }

    [Column("id_destination")]
    public int IdDestination { get; set; }

    [Column("title")]
    public string Title { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("price")]
    public decimal Price { get; set; }

    [Column("duration")]
    public int Duration { get; set; } // en minutos
}