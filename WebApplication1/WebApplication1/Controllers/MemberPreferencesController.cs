using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using System.Text;
using WebApplication1.Services;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
namespace WebApplication1.Controllers
{
    public class MemberPreferencesController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index(String id)
        {
            SalesforceTokenManager.AccessToken response = SalesforceTokenManager.getAccessToken(
                "3MVG92u_V3UMpV.ipXxNgIhH.2g.BQNULCFtTO8ExktgDJi4lK6pYhPxlFoT1QvwHO6lLruMFw_1ApAPjdOEb",
                "amir.hafeez@quintessentially.com.devamir").Result;

            Debug.WriteLine("RESPONSE: ", response.access_token);

            ViewData["id"] = id;
            return View();
        }
    }
}
