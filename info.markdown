
# ADO.NET w/Dapper

## Step 1 - Create Visual Sudio Project
1. I will be creating `Blazor Server App` project
1. The first thing we need to do is create two folders in `Object Explorer`:
    * Created folder named `Data`
    * Create folder named `Models`

## Step 2 - Add SqlClient & Dapper Nuget packages
1. Right click VS project > `Manage Nuget Packages`
1. Click `Browse`
1. Search for `dapper` then install `Dapper`
1. Search for `SqlClient` then install `System.Data.SqlClient`

## Step 3 - Create Database
1. Open `SQL Management Studio` & create new database `NunoSolutionsYoutube`
1. Create Employees table
```sql
CREATE TABLE dbo.Employees (
	emp_id int NOT NULL	IDENTITY (1,1)
		CONSTRAINT PK_Employees PRIMARY KEY CLUSTERED (emp_id),
	created_dt datetime2(7) NOT NULL
		CONSTRAINT DF_Employees_created_dt DEFAULT GetDate(),
	first_name varchar(50) NOT NULL,
	last_name varchar(50) NOT NULL,
	phone varchar(15) NOT NULL,
	modified_dt datetime2(7) NULL,
	active bit NOT NULL
		CONSTRAINT DF_Employees_active DEFAULT 1 
);
GO
-- Add test employee data
INSERT INTO [dbo].[Employees] (
	[first_name],[last_name],[phone]
)
SELECT [first_name] = 'Nuno', [last_name] = 'P', [phone] = '111-111-1111'
UNION
SELECT [first_name] = 'Andrew', [last_name] = 'Top G', [phone] = '111-111-1111'
UNION
SELECT [first_name] = 'Myron', [last_name] = 'G', [phone] = '111-111-1111'
GO	
-- View employees resultset to make sure we have data 
SELECT * FROM Employees
```
1. Here's a nice SQL snippet that will effortlessly export your `SQL table` to a `C# Class`. I don't remember where I found it but I use this all the time, it is extremley useful. Go ahead and run the following SQL:
```sql
declare @TableName sysname 
declare @Result varchar(max) 

set @TableName= 'Employees' -- CHANGE TABLE NAME

Set @Result =  'public class ' + @TableName + '
{'

select @Result = @Result + '
    public ' + ColumnType + NullableSign + ' ' + ColumnName + ' { get; set; }'
from
(
    select 
        replace(col.name, ' ', '_') ColumnName,
        column_id ColumnId,
        case typ.name 
            when 'bigint' then 'long'
            when 'binary' then 'byte[]'
            when 'bit' then 'bool'
            when 'char' then 'string'
            when 'date' then 'DateTime'
            when 'datetime' then 'DateTime'
            when 'datetime2' then 'DateTime'
            when 'datetimeoffset' then 'DateTimeOffset'
            when 'decimal' then 'decimal'
            when 'float' then 'float'
            when 'image' then 'byte[]'
            when 'int' then 'int'
            when 'money' then 'decimal'
            when 'nchar' then 'char'
            when 'ntext' then 'string'
            when 'numeric' then 'decimal'
            when 'nvarchar' then 'string'
            when 'real' then 'double'
            when 'smalldatetime' then 'DateTime'
            when 'smallint' then 'short'
            when 'smallmoney' then 'decimal'
            when 'text' then 'string'
            when 'time' then 'TimeSpan'
            when 'timestamp' then 'byte[]'
            when 'tinyint' then 'byte'
            when 'uniqueidentifier' then 'Guid'
            when 'varbinary' then 'byte[]'
            when 'varchar' then 'string'
            else 'UNKNOWN_' + typ.name
        end ColumnType,
        case 
            when col.is_nullable = 1 and typ.name in ('bigint', 'bit', 'date', 'datetime', 'datetime2', 'datetimeoffset', 'decimal', 'float', 'int', 'money', 'numeric', 'real', 'smalldatetime', 'smallint', 'smallmoney', 'time', 'tinyint', 'uniqueidentifier') 
            then '?' 
            else '' 
        end NullableSign
    from sys.columns col
        join sys.types typ on
            col.system_type_id = typ.system_type_id AND col.user_type_id = typ.user_type_id
    where object_id = object_id(@TableName)
) t
order by ColumnId

set @Result = @Result  + '
}'

print @Result
```
Alternatively, you can just copy the following:
```c#
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
```

## Step 4 - Create Database Layer
1. Right click **Models** folder click `Add` > `Class` & name it `Employees.cs`
1. Replace the auto-generated `class`:
    ```c#
    public class Employees
    {
    }
    ```
1. With the following we copied from SSMS:
    ```c#
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
    ```
1. Right click *Data* folder > `Add` > `Class` & name it `Database.cs`
1. We need to add two namespaces to the top of the `Database.cs` file:
    ```c#
    using NunoSolutions.Dapper.Models;
    using System.Data.SqlClient;
    ```
1. Now let's add a C# method using the `old method` of data retrieval w/`SqlDataReader` to retrieve a list of `Employees`:
    ```c#
    public List<Employees> GetEmployeesUsingSqlDataReader()
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
    ```
1. Add a second method using Dapper to see how much simpler this code can be written:
    ```c#
    public List<Employees> GetEmployeesUsingDapper()
    {
        var con = new SqlConnection("Data Source=.;Initial Catalog=NunoSolutionsYoutube;Integrated Security=SSPI");
        var employees = con.Query<Employees>("SELECT emp_id, first_name, last_name, phone FROM Employees").ToList();
        return employees;
    }
    ```

## Let's use it
1. Open `Index.razor`
1. Now **delete** the boiler plate code:
    ```html
    <PageTitle>Index</PageTitle>

    <h1>Hello, world!</h1>

    Welcome to your new app.

    <SurveyPrompt Title="How is Blazor working for you?" />
    ```
1. Add a `@code { }` section
    ```
    @code {

    }
    ```
1. Add the following two namespaces to top of `Index.razor`:
    ```c#
    @using NunoSolutions.Dapper.Models;    
    @using NunoSolutions.Dapper.Data;
    ``` 
1. Add interpolated table to generate a list of employees on the UI
    ```html
    <table class="table table-bordered">
        <tr>
            <th>Emp ID</th>
            <th>First Name</th>
            <th>Last Name</th>
            <th>Phone</th>
        </tr>
        @foreach (var emp in emps)
        {
            <tr>
                <td>@emp.emp_id</td>
                <td>@emp.first_name</td>
                <td>@emp.last_name</td>
                <td>@emp.phone</td>
            </tr>
        }
    </table>
    ```
6. Add `OnInitializedAsync` override method and we are going to use the `Database.GetEmployeesUsingSqlDataReader()` method to fetch the employee data:
    ```c#
    @page "/"
    @using NunoSolutions.Dapper.Models;
    @using NunoSolutions.Dapper.Data;

    <table class="table table-bordered">
        <tr>
            <th>Emp ID</th>
            <th>First Name</th>
            <th>Last Name</th>
            <th>Phone</th>
        </tr>
        @foreach (var emp in emps)
        {
            <tr>
                <td>@emp.emp_id</td>
                <td>@emp.first_name</td>
                <td>@emp.last_name</td>
                <td>@emp.phone</td>
            </tr>
        }
    </table>

    @code {
        protected List<Employees> emps;

        protected override async Task OnInitializedAsync()
        {
            emps = Database.GetEmployeesUsingSqlDataReader();
        }
    }
    ```
1. Now let's swap that method call with our Dapper method `Database.GetEmployeesUsingDapper()` and run it again. You can see that we get the same exact result with a lot less lines of code.
    ```c#
    @page "/"
    @using NunoSolutions.Dapper.Models;
    @using NunoSolutions.Dapper.Data;

    <table class="table table-bordered">
        <tr>
            <th>Emp ID</th>
            <th>First Name</th>
            <th>Last Name</th>
            <th>Phone</th>
        </tr>
        @foreach (var emp in emps)
        {
            <tr>
                <td>@emp.emp_id</td>
                <td>@emp.first_name</td>
                <td>@emp.last_name</td>
                <td>@emp.phone</td>
            </tr>
        }
    </table>

    @code {
        protected List<Employees> emps;

        protected override async Task OnInitializedAsync()
        {
            //emps = Database.GetEmployeesUsingSqlDataReader();
            emps = Database.GetEmployeesUsingDapper();
        }
    }
    ```
   