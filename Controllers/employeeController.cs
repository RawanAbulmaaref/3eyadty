using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace Mvc.Controllers
{
    public class employeeController : Controller
    {
       public string rawan (string n,int a)
        {
            return $"Name: {n}, Age: {a}";
        }


        public ViewResult GetView()
        {
            ViewResult v=new ViewResult();
            v.ViewName="Index";
            return v;
        }
    }


}
