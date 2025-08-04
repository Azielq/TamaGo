using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;   // ← añade esto

namespace TamaGo.Models;

[Table("users")]
public class User : BaseModel
{
    /* ---------- PK ---------- */
    [PrimaryKey("id_user", false)]
    public int IdUser { get; set; }

    /* ---------- Datos básicos ---------- */
    [Column("firstname")]  public string FirstName  { get; set; }
    [Column("lastname")]   public string LastName   { get; set; }
    [Column("username")]   public string Username   { get; set; }   // NUEVO
    [Column("email")]      public string Email      { get; set; }

    /* ---------- Seguridad ---------- */
    [Column("passwordhash")]   public string PasswordHash { get; set; }

    /* ---------- Perfil ---------- */
    [Column("profilepicture")] public string ProfilePicture { get; set; }

    [Column("country")]    public string  Country   { get; set; }   // NUEVO (nullable)
    [Column("phone")]      public string  Phone     { get; set; }   // NUEVO (nullable)
    [Column("birthdate")]  public DateTime? BirthDate { get; set; } // NUEVO (nullable)

    /* ---------- Timestamps ---------- */
    [Column("created_at")] public DateTime CreatedAt { get; set; }
    [Column("updated_at")] public DateTime UpdatedAt { get; set; }

    
    
    
}