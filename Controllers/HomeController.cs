using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyPharmacy.Controllers;

[Authorize]
public class HomeController : Controller
{
    public IActionResult Index() => View();
    [AllowAnonymous]
    public IActionResult Error() => View();
}
