using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace TamaGo.Models;

[Table("favorite")]
public class Favorite : BaseModel
{
    [PrimaryKey("id_favorite", false)]
    public int IdFavorite { get; set; }

    [Column("id_user")]
    public int IdUser { get; set; }

    [Column("id_destination")]
    public int IdDestination { get; set; }

    [Column("added_date")]
    public DateTime AddedDate { get; set; }
}