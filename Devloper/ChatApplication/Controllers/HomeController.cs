using ChatApplication.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;

namespace ChatApplication.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        public ActionResult ChatBox()
        {
            string status = "";
            // Creating Connection  
            using (SqlConnection con = new SqlConnection("Data Source=.;Initial Catalog=Rahul;Integrated Security=True"))
            {
                SqlDataAdapter sde = new SqlDataAdapter("Select * from Users", con);
                DataTable ds = new DataTable();
                sde.Fill(ds);
                // List<UserModel> usr = new List<UserModel>();

                List<UserModel> usr = ds.AsEnumerable().Select(
                           dataRow => new UserModel
                           {
                               Id = dataRow.Field<int>("Id"),
                               Name = dataRow.Field<string>("Name")
                           }).ToList();

                ViewData.Model = usr;

            }
            return View();
        }
    

        [HttpPost]
        [WebMethod]
        public string MessageSend(string Message, string MsgTo, string MsgFrom,string MessageFrom)
        {
            DataTable ds = new DataTable();
             try
                {
                    string EncryptmsgsTo = Encrypt(Message, "sblw-3hn8-sqoy19");
                    string EncryptmsgsFrom = Encrypt(MessageFrom, "sblw-3hn8-sqoy19"); 

            using (SqlConnection con = new SqlConnection("Data Source=.;Initial Catalog=Rahul;Integrated Security=True"))
            {
                string sql = null;
               

                    sql = "insert into MessagesT (ToId,FromId,MessageTo,MessageFrom) values(@ToId,@FromId,@MessageTo,@MessageFrom)";
              
                con.Open();
                // Prepare the command to be executed on the db
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    // Create and set the parameters values 
                    cmd.Parameters.Add("@ToId", SqlDbType.NVarChar).Value = Convert.ToInt16(MsgTo);
                    cmd.Parameters.Add("@FromId", SqlDbType.NVarChar).Value = Convert.ToInt16(MsgFrom);
                    cmd.Parameters.Add("@MessageTo", SqlDbType.NVarChar).Value = EncryptmsgsTo;
                    cmd.Parameters.Add("@MessageFrom", SqlDbType.NVarChar).Value = EncryptmsgsFrom;
                    int rowsAdded = cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
          }
                catch (Exception ex) { }
                return GetMessage(MsgTo, MsgFrom);
      }




        [HttpPost]
        [WebMethod]
        public string GetMessage(string MsgTo, string MsgFrom)
        {
            DataTable _newDataTable = new DataTable();
            DataTable ds = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection("Data Source=.;Initial Catalog=Rahul;Integrated Security=True"))
                {
                    DataTable dt = new DataTable();
                    string sql = null;
                    SqlDataAdapter sde = new SqlDataAdapter("Select * from MessagesT ", con); //where ToId="+MsgTo+" and FromId="+MsgFrom
                    sde.Fill(_newDataTable);

                    string _sqlWhere = "ToId="+MsgTo+" or ToId="+MsgFrom;
                    string _sqlOrder = "";

                   ds = _newDataTable.Select(_sqlWhere, _sqlOrder).CopyToDataTable();
                    ds.Columns.Add("FromName", typeof(string));

                    if (ds.Rows.Count > 0)
                    {
                        foreach (DataRow row in ds.Rows)
                        {
                            dt = new DataTable();
                            SqlDataAdapter sda = new SqlDataAdapter("Select * from Users", con);
                            sda.Fill(dt);

                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                if (dt.Rows[i]["Id"].ToString().Contains(row["FromId"].ToString()))
                                {
                                row["FromName"] = dt.Rows[i]["Name"].ToString();
                                }
                            }

                           
                        row["MessageFrom"]= Decrypt(row["MessageFrom"].ToString(), "sblw-3hn8-sqoy19");
                        row["MessageTo"] = Decrypt(row["MessageTo"].ToString(), "sblw-3hn8-sqoy19");
                        }
                    }
                    if(ds.Rows.Count==0){
                        SqlDataAdapter sd = new SqlDataAdapter("Select * from MessagesT where FromId=" + MsgTo + " and ToId=" + MsgFrom, con);
                        sd.Fill(ds);
                        if (ds.Rows.Count > 0)
                        {
                            foreach (DataRow row in ds.Rows)
                            {
                                row["MessageFrom"] = Decrypt(row["MessageFrom"].ToString(), "sblw-3hn8-sqoy19");
                                row["MessageTo"] = Decrypt(row["MessageTo"].ToString(), "sblw-3hn8-sqoy19");
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { }
            return JsonConvert.SerializeObject(ds);
        }

        public string Encrypt(string input, string key)
        {
            byte[] inputArray = UTF8Encoding.UTF8.GetBytes(input);
            TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
            tripleDES.Key = UTF8Encoding.UTF8.GetBytes(key);
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tripleDES.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
            tripleDES.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
        public string Decrypt(string input, string key)
        {
            byte[] inputArray = Convert.FromBase64String(input);
            TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
            tripleDES.Key = UTF8Encoding.UTF8.GetBytes(key);
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tripleDES.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
            tripleDES.Clear();
            return UTF8Encoding.UTF8.GetString(resultArray);
        }  


    }

  }
