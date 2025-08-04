using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace TamaGo.Models;

[Table("review")]
public class Review : BaseModel
{
    [PrimaryKey("id_review", false)]
    public int IdReview { get; set; }

    [Column("id_user")]
    public int IdUser { get; set; }

    [Column("id_destination")]
    public int IdDestination { get; set; }

    [Column("review")]
    public string ReviewText { get; set; }

    [Column("rating")]
    public int Rating { get; set; }

    [Column("review_date")]
    public DateTime ReviewDate { get; set; }
}