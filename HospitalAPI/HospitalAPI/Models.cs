using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalAPI;

public class Patient
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public Gender Gender { get; set; }
    [Required] public DateTime BirthDate { get; set; }
    public bool Active { get; set; }
    public Name Name { get; set; }
}

public class Name
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Use { get; set; }
    [Required] public string Family { get; set; }
    public string[] Given { get; set; }
    public int PatientId { get; set; }
}

public enum Gender
{
    Male,
    Female,
    Other,
    Unknown
}