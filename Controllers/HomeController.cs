using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Data.SqlClient;

public class HomeController : Controller
{
    private SqlStore Db { get; }
    public HomeController(SqlStore db) => Db = db;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context,ActionExecutionDelegate next)
    {
        
        var result = await next();
        if (result.Exception is SqlException error)
        {
            result.ExceptionHandled = true;
            if (error.Number is >= 50000 and < 50100)
                result.Result = Feedback(error.Message);
            else if (error.Number is 2601 or 2627)
                result.Result = Feedback("A record with these values already exists.");
            else
            {
                Response.StatusCode = 503;
                result.Result = View("Unavailable");
            }
        }
    }

    private IActionResult Feedback(string message)
    {
        TempData["Feedback"] = message;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var customers = await Db.Read("SELECT CustomerId AS ID, FullName AS Customer, MeterNumber AS Meter, Address FROM Customer ORDER BY CustomerId DESC");
var bills = await Db.Read("SELECT TOP (100) * FROM dbo.BillingDashboard ORDER BY ReadingDate DESC, BillId DESC");
return View(new Dashboard(new[] { new Grid("Customers", customers), new Grid("Recent bills", bills) }));
    }

    

    [HttpPost]
public async Task<IActionResult> Customer([Required, StringLength(100)] string fullName,
    [Required, StringLength(30)] string meter, [Required, StringLength(200)] string address)
{
    if (!ModelState.IsValid) return Feedback("Enter a valid name, meter and address.");
    await Db.Run("INSERT Customer(FullName,MeterNumber,Address) VALUES(@name,@meter,@address)",
        ("@name", fullName.Trim()), ("@meter", meter.Trim()), ("@address", address.Trim()));
    return Feedback("Customer added.");
}
[HttpPost]
public async Task<IActionResult> Reading([Range(1,int.MaxValue)] int customerId, DateTime date,
    [Range(0,100000000)] int units)
{
    if (!ModelState.IsValid || date == default || date.Date > DateTime.Today) return Feedback("Enter a valid reading and date.");
    await Db.Procedure("dbo.RecordReading", ("@CustomerId",customerId), ("@ReadingDate",date.Date), ("@Units",units));
    return Feedback("Reading recorded and bill generated.");
}
}