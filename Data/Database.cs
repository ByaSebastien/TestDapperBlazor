using TestDapperBlazor.Models;
using System.Data.SqlClient;
using Dapper;

namespace TestDapperBlazor.Data;

public class Database
{
    public static List<Employees> GetEmployeesUsingSqlDataReader()
    {
        var employees = new List<Employees>();
        var con = new SqlConnection("Data Source=.;Initial Catalog=NunoSolutionsYoutube;Integrated Security=SSPI");
        var cmd = con.CreateCommand();
        SqlDataReader rdr = null;
        try
        {
            cmd.CommandText = "SELECT emp_id, first_name, last_name, phone FROM Employees";
            con.Open();
            rdr = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
            while (rdr.Read())
            {
                employees.Add(new Employees()
                {
                    emp_id = Convert.ToInt32(rdr["emp_id"]),
                    first_name = Convert.ToString(rdr["first_name"]),
                    last_name = Convert.ToString(rdr["last_name"]),
                    phone = Convert.ToString(rdr["phone"])
                });
            }
        }
        catch (Exception)
        {
            // exception handling should be in production.
        }
        finally
        {
            if (rdr?.IsClosed == false) rdr.Close();
        }
        return employees;
    }
    
    public static List<Employees> GetEmployeesUsingDapper()
    {
        var con = new SqlConnection("Data Source=.;Initial Catalog=NunoSolutionsYoutube;Integrated Security=SSPI");
        return con.Query<Employees>("SELECT emp_id, first_name, last_name, phone FROM Employees").ToList();
    }

    public static void InsertEmployees()
    {
        var con = new SqlConnection("Data Source=.;Initial Catalog=NunoSolutionsYoutube;Integrated Security=SSPI");
        string sql = "INSERT INTO Employees (created_dt," +
                     "first_name," +
                     "last_name," +
                     "phone," +
                     "modified_dt," +
                     "active) OUTPUT INSERTED.* " +
                     "values(@created,@firstname,@lastname,@phone,@modified,@active)";
        object parameters = new
        {
            created = DateTime.Now,
            firstname = "New seb",
            lastname = "Bya",
            phone = "00000",
            modified = DateTime.Now,
            active = true
        };

        Employees emp = con.QuerySingle<Employees>(sql, parameters);
        Console.WriteLine($"id : {emp.emp_id} | nom : {emp.first_name}");
    }
    
    public static void DeleteEmployees()
    {
        var con = new SqlConnection("Data Source=.;Initial Catalog=NunoSolutionsYoutube;Integrated Security=SSPI");
        string sql = "DELETE FROM Employees where emp_id = @id";
        object parameters = new
        {
            id = 4
        };

        int nbRows = con.Execute(sql, parameters);
        Console.WriteLine(nbRows);
    }
    
    public static void UpdateEmployees()
    {
        var con = new SqlConnection("Data Source=.;Initial Catalog=NunoSolutionsYoutube;Integrated Security=SSPI");
        string sql = "UPDATE Employees set " +
                     "first_name = @firstname " +
                     "where emp_id = @id";
        object parameters = new
        {
            id = 5,
            firstname = "Toto",
        };

        int nbRows = con.Execute(sql, parameters);
        Console.WriteLine(nbRows);
    }
}