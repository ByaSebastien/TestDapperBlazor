namespace TestDapperBlazor.Models;

public class Employees
{
    public int emp_id { get; set; }
    public DateTime created_dt { get; set; }
    public string first_name { get; set; }
    public string last_name { get; set; }
    public string phone { get; set; }
    public DateTime? modified_dt { get; set; }
    public bool active { get; set; }
}