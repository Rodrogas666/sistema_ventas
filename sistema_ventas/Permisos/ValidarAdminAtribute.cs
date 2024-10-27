using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace sistema_ventas.Permisos
{
    public class ValidarAdminAtribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (HttpContext.Current.Session["rol"].ToString() !=  "admin")
            {
                filterContext.Result = new RedirectResult("~/Home/Index");
            }

            base.OnActionExecuting(filterContext);
        }
    }
}