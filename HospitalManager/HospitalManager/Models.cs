namespace HospitalManager;

public class Patient
{
    public Name Name { get; set; }
    public Gender Gender { get; set; }
    public DateTime BirthDate { get; set; }
    public bool Active { get; set; }
}

public class Name
{
    public string Family { get; set; }
    public string Use { get; set; }
    public string[] Given { get; set; }
}

public enum Gender
{
    Male,
    Female,
    Other,
    Unknown
}