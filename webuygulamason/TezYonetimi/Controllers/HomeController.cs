using TezYonetimi.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.IO;

namespace TezYonetimi.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
       

        public ActionResult Index(string Message)
        {
            ViewBag.Message = Message;
            string str_connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection myconn = new SqlConnection(str_connection);
            List<ProjectModel> projectlist = new List<ProjectModel>();
            try
            {
                string q_sel = string.Format("SELECT * FROM PROJECTS WHERE KULLANICIADSOYAD = '{0}'", _main.loginUser.Name);
                if (_main.loginUser.ID == 0)
                    q_sel = "SELECT * FROM PROJECTS";
                SqlCommand sql_cmd = new SqlCommand(q_sel, myconn);
                myconn.Open();
                SqlDataReader dr = sql_cmd.ExecuteReader();
                while (dr.Read())
                {
                    ProjectModel project = new ProjectModel();
                    project.ID = Convert.ToInt32(dr["ID"]);
                    project.AD = dr["AD"].ToString();
                    project.DERSADI = dr["DERSADI"].ToString();
                    project.OZET = dr["OZET"].ToString();
                    project.DONEM = dr["DONEM"].ToString();
                    project.BASLIK = dr["BASLIK"].ToString();
                    project.ANAHTARKELIME = dr["ANAHTARKELIME"].ToString();
                    project.DANISMAN = dr["DANISMAN"].ToString();
                    project.JURI = dr["JURI"].ToString();
                    project.JURI2 = dr["JURI2"].ToString();
                    project.KULLANICIADSOYAD = dr["KULLANICIADSOYAD"].ToString();
                    projectlist.Add(project);
                }
                dr.Close();
                myconn.Close();
            }
            catch (Exception ex)
            {
                if (myconn.State == ConnectionState.Open)
                    myconn.Close();
            }
            return View(projectlist);
        }

        [HttpGet]
        public ActionResult AddProject()
        {
            ViewBag.Action = Url.Action("AddProject", "Home");
            ViewBag.Title = "Add Project";
            return PartialView("AddProject");
        }

        [HttpPost]
        public ActionResult AddProject(HttpPostedFileBase file)
        {
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    MemoryStream target = new MemoryStream();
                    file.InputStream.CopyTo(target);
                    byte[] FileByteArray = target.ToArray();
                    if (_main.AddPdfToDatabase(FileByteArray))
                        return RedirectToAction("Index", "Home", new { Message = "File Uploaded Successfully!!" });
                }
            }
            catch{}
            return RedirectToAction("Index", "Home", new { Message = "File upload failed!!" });
        }

        [HttpGet]
        public ActionResult DeleteProject(int id)
        {
            ViewBag.Action = Url.Action("DeleteProject", "Home");
            string q_del = $"DELETE FROM PROJECTS WHERE ID = {id}";
            string Msg = _main.komutcalistir(q_del) > 0 ? "Successfully deleted!!" : "Delete Failed!!";
            return RedirectToAction("Index", "Home");
        }
    }
}
