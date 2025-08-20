using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json;

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

    // Navigation property - not stored in database
    [JsonIgnore]
    public Tour? Tour { get; set; }

    // Computed properties - not stored in database
    [JsonIgnore]
    public string FormattedTourDate => TourDate.ToString("dddd, dd MMMM yyyy");
    [JsonIgnore]
    public string FormattedReserveDate => ReserveDate.ToString("dd/MM/yyyy");
    [JsonIgnore]
    public string FormattedPrice => TotalPrice.ToString("C0");
    [JsonIgnore]
    public string StatusText => State switch
    {
        "confirmed" => "Confirmada",
        "cancelled" => "Cancelada",
        "pending" => "Pendiente",
        _ => State
    };
    [JsonIgnore]
    public string StatusColor => State switch
    {
        "confirmed" => "#4CAF50",
        "cancelled" => "#F44336", 
        "pending" => "#FF9800",
        _ => "#666666"
    };
    [JsonIgnore]
    public string PeopleText => PplQuantity == 1 ? "1 persona" : $"{PplQuantity} personas";
    [JsonIgnore]
    public bool CanCancel => State == "confirmed" && TourDate >= DateTime.Now.Date;
    [JsonIgnore]
    public bool IsPast => TourDate < DateTime.Now.Date;
    [JsonIgnore]
    public bool IsUpcoming => TourDate >= DateTime.Now.Date && State == "confirmed";
}