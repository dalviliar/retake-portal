namespace RetakePortal.Models;

public class Student
{
    public string IIN { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string Institute { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int Course { get; set; }
    public string EducationLevel { get; set; } = string.Empty;

    public string EducationLevelDisplay => EducationLevel switch
    {
        "bachelor"    => "Бакалавриат",
        "master_sci"  => "Магистратура (научно-педагогическая)",
        "master_prof" => "Магистратура (профильная)",
        "doctoral"    => "Докторантура",
        _             => EducationLevel
    };

    public decimal CreditCost => EducationLevel switch
    {
        "bachelor"    => 18523m,
        "master_sci"  => 25000m,
        "master_prof" => 20000m,
        "doctoral"    => 37060m,
        _             => 0m
    };
}
