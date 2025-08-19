using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Collections.ObjectModel;

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

    [Column("latitude")]
    public double? Latitude { get; set; }

    [Column("longitude")]
    public double? Longitude { get; set; }

    [Column("is_featured")]
    public bool IsFeatured { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public ObservableCollection<Tour> Tours { get; set; } = new();
    public ObservableCollection<Review> Reviews { get; set; } = new();
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsFavorite { get; set; }
    public string CategoryEmoji => Category?.ToLower() switch
    {
        "playa" => "🏖️",
        "naturaleza" => "🌴",
        "mirador" => "🏔️",
        "tour" => "🐴",
        "surf" => "🏄",
        "aventura" => "🚵",
        _ => "📍"
    };
}