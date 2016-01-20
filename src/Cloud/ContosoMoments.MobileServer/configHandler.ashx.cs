using ContosoMoments.Web.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContosoMoments.Web
{
    /// <summary>
    /// Summary description for Handler1
    /// </summary>
    public class Handler1 : IHttpHandler
    {


        public void ProcessRequest(HttpContext context)
        {

            string jscontent = string.Format("var configJson={0}", JsonConvert.SerializeObject(new ConfigModel())); // This function will return my custom js string

            context.Response.ContentType = "application/javascript";

            context.Response.Write(jscontent);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}