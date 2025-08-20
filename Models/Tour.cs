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
    public int Duration { get; set; }

    // Note: max_capacity column doesn't exist in DB, using computed property
    public int MaxCapacity => IdTour switch
    {
        1 => 8,  // Clases de Surf para Principiantes
        2 => 12, // Tour de Snorkel  
        3 => 6,  // Tour Nocturno de Tortugas
        4 => 10, // Kayak en los Manglares
        5 => 8,  // Sunset Sailing
        6 => 6,  // Stand Up Paddle
        _ => 8   // Default capacity
    };

    [Column("difficulty_level")]
    public string DifficultyLevel { get; set; }

    [Column("includes")]
    public string Includes { get; set; }

    [Column("requirements")]
    public string Requirements { get; set; }

    [Column("tour_picture")]
    public string TourPicture { get; set; }

    [Column("is_available")]
    public bool IsAvailable { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public Destination Destination { get; set; }
    public string FormattedPrice => $"${Price:F0}"; 
    public string FormattedDuration => Duration >= 60 
        ? $"{Duration / 60}h {Duration % 60}min" 
        : $"{Duration}min";
    public string DifficultyEmoji => DifficultyLevel?.ToLower() switch
    {
        "facil" => "🟢",
        "medio" => "🟡", 
        "dificil" => "🔴",
        _ => "🟢"
    };
}