using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace TamaGo.Models;

[Table("booking")]
public class Booking : BaseModel
{
    [PrimaryKey("id_booking", false)]
    public int IdBooking { get; set; }

    [Column("id_user")]
    public int IdUser { get; set; }

    [Column("id_tour")]
    public int IdTour { get; set; }

    [Column("reserve_date")]
    public DateTime ReserveDate { get; set; }

    [Column("tour_date")]
    public DateTime TourDate { get; set; }

    [Column("state")]
    public string State { get; set; } = "pending";

    [Column("ppl_quantity")]
    public int PplQuantity { get; set; }

    [Column("total_price")]
    public decimal TotalPrice { get; set; }
}