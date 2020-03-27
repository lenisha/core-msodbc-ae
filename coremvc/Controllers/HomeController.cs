using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using coremvc.Models;
using System.Data;
using System.Data.Odbc;
using Microsoft.Extensions.Configuration;

namespace coremvc.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration Configuration;

        public HomeController(IConfiguration Configuration)
        {
            this.Configuration = Configuration;
        }


        private string GetConnectionString() {
            string connectionString = "Driver={ODBC Driver 17 for SQL Server}; " +
               $"Server={Configuration["ODBC:Server"]};" +
               $"Database={Configuration["ODBC:Database"]};" +
               $"Uid={Configuration["ODBC:Uid"]};" +
               $"Pwd={Configuration["ODBC:Pwd"]};" +
               "Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;" +
               $"ColumnEncryption={Configuration["ODBC:ColumnEncryption"]};" +
               $"KeyStoreAuthentication={Configuration["ODBC:KeyStoreAuthentication"]};" +
               $"KeyStorePrincipalId={Configuration["ODBC:KeyStorePrincipalId"]};" +
               $"KeyStoreSecret={Configuration["ODBC:KeyStoreSecret"]};";

            Console.WriteLine(" ======== Conn string =====:" + connectionString);    
            return connectionString;
        }


        /*
          CREATE TABLE dbo.Persons  
               ( First_Name varchar(25) NOT NULL,  
                 Last_Name varchar(50) NULL)  
            GO 

            INSERT person Values ('Test', 'Blah blah');
            INSERT person Values ('Test2', 'mine blah');
            INSERT person Values ('Test3', 'ohhhh blah');
        */



        public IActionResult Index()
        {
            String header = "";

            Console.WriteLine(" --- Openning connection --- ");
            IDbConnection dbcon = new OdbcConnection(GetConnectionString());

            dbcon.Open();

            using (IDbCommand dbcmd = dbcon.CreateCommand())
            {
                dbcmd.CommandText = "SELECT * FROM Persons";

                IDataReader dbReader = dbcmd.ExecuteReader();
                int fCount = dbReader.FieldCount;

                Console.WriteLine(" --- Getting data --- ");
                // Read header - field definitions
                for (int i = 0; i < fCount; i++)
                {
                    String fName = dbReader.GetName(i);
                    header = header + fName + " | ";
                }

                // Read data
                int row = 0;
                while (dbReader.Read())
                {
                    String rowData = "";
                    for (int i = 0; i < fCount; i++)
                    {
                        object col = dbReader.GetValue(i);
                        

                        rowData = rowData + " | " + ConvertFromDBVal(col);
                    }
                    ViewData["Row" + row] = rowData;
                    row++;
                }
                ViewData["Rows"] = row;
            }
            dbcon.Close();
            dbcon = null;

           
            return View();
        }

        public static String ConvertFromDBVal(object obj)
        {
            if (obj == null || obj == DBNull.Value)
            {
                return string.Empty;  //return default(T); // returns the default value for the type
            }
            else
            {
                return obj.ToString();
            }
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
