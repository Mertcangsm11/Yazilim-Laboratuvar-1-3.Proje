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

namespace TezYonetimi.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        
        [AllowAnonymous]
        public ActionResult Index(string error = "")
        {
            ViewBag.Error = error;
            return View();
        }

        [AllowAnonymous]
        public ActionResult Login(string username, string password, bool? remember)
        {
            if (GirisKontrol(username, password, remember))
                return RedirectToAction("Index","Home");
            else {
                return RedirectToAction("Index", "Account", new { error = "Hatalı Giriş" });
            }
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("/");
        }

        private bool GirisKontrol(string username, string password, bool? remember)
        {
            string uname = ConfigurationManager.AppSettings["adminUserName"].ToString();
            string pass = ConfigurationManager.AppSettings["adminPassword"].ToString();
            if (uname == username && pass == password) {
                LoginUserModel user = new LoginUserModel();
                user.ID = 0;
                user.Name = "Admin";
                user.Email = "";
                _main.loginUser = user;
                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, Newtonsoft.Json.JsonConvert.SerializeObject(user), 
                    DateTime.Now, DateTime.Now.AddHours(1), false, "admin");
                HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
                HttpContext.Response.SetCookie(cookie);
                return true;
            }
            else {
                DataTable dt_ret = new DataTable();
                string str_connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
                SqlConnection myconn = new SqlConnection(str_connection);
                try
                {
                    string q_sel = "SELECT TOP 1 ID, EMAIL, FULLNAME FROM USERS WHERE EMAIL = @EMAIL AND PASSWORD = @PASSWORD";
                    SqlCommand sql_cmd = new SqlCommand(q_sel, myconn);
                    sql_cmd.Parameters.Add(new SqlParameter("@EMAIL", username));
                    sql_cmd.Parameters.Add(new SqlParameter("@PASSWORD", _main.Encrypt(password, "951623")));
                    myconn.Open();
                    SqlDataReader dr = sql_cmd.ExecuteReader();
                    dt_ret.Load(dr);
                    myconn.Close();
                    if (dt_ret.Rows.Count > 0)
                    {
                        LoginUserModel user = new LoginUserModel();
                        user.ID = Convert.ToInt32(dt_ret.Rows[0]["ID"]);
                        user.Name = dt_ret.Rows[0]["FULLNAME"].ToString();
                        user.Email = dt_ret.Rows[0]["EMAIL"].ToString();
                        _main.loginUser = user;
                        FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, Newtonsoft.Json.JsonConvert.SerializeObject(user),
                   DateTime.Now, remember.HasValue && remember.Value ? DateTime.Now.AddHours(1) : DateTime.Now.AddMinutes(60), false, "user");
                        HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
                        if (remember.HasValue && remember.Value)
                            cookie.Expires = DateTime.Now.AddYears(1);
                        HttpContext.Response.SetCookie(cookie);
                        return true;
                    }
                }
                catch
                {
                    if (myconn.State == ConnectionState.Open)
                        myconn.Close();
                    return false;
                }
                return false;
            }
        }

        private List<GroupModel> GroupList()
        {
            string str_connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection myconn = new SqlConnection(str_connection);
            List<GroupModel> grouplist = new List<GroupModel>();
            try
            {
                string q_sel = "SELECT * FROM GROUPS";
                SqlCommand sql_cmd = new SqlCommand(q_sel, myconn);
                myconn.Open();
                SqlDataReader dr = sql_cmd.ExecuteReader();



                while (dr.Read())
                {
                    GroupModel group = new GroupModel();
                    group.ID = Convert.ToInt32(dr[0]);
                    group.GROUPID = Convert.ToInt32(dr[1]);
                    group.GROUPNAME = dr[2].ToString();
                    group.GROUPLIMIT =Convert.ToDecimal(dr[3]);
                    grouplist.Add(group);
                }

                dr.Close();
                myconn.Close();

            }
            catch (Exception ex)
            {
                if (myconn.State == ConnectionState.Open)
                    myconn.Close();
            }

           return grouplist;

        }

        public ActionResult Userlist()
        {
            string str_connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection myconn = new SqlConnection(str_connection);
            List<UserModel> userlist = new List<UserModel>();
            try
            {
                string q_sel = "SELECT * FROM USERS ";
                SqlCommand sql_cmd = new SqlCommand(q_sel, myconn);
                myconn.Open();
                SqlDataReader dr = sql_cmd.ExecuteReader();
                while (dr.Read())
                {
                    UserModel user = new UserModel();
                    user.ID = Convert.ToInt32(dr[0]);
                    user.GROUPID = Convert.ToInt32(dr[1]);
                    user.EMAIL = dr[2].ToString();
                    user.PASSWORD = dr[3].ToString();
                    user.FULLNAME = dr[4].ToString();
                    userlist.Add(user);
                }

                dr.Close();
                myconn.Close();

            }
            catch (Exception ex)
            {
                if (myconn.State == ConnectionState.Open)
                    myconn.Close();
            }

            return View(userlist);
        }

        [HttpGet]
        public ActionResult Adduser()
        {
            ViewBag.Action = Url.Action("Adduser", "Account");
            ViewBag.Title = "Add User";


            ViewBag.GroupList = GroupList().Select(c => new SelectListItem() { Text = c.GROUPNAME, Value = c.GROUPID.ToString() }).ToList();

            return PartialView("_Add", new UserModel());
        }

        [HttpPost]
        public ActionResult Adduser(UserModel model)
        {
            ReturnModel retVal = new ReturnModel();
            string str_connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection myconn = new SqlConnection(str_connection);
            
            try
            {
                string q_sel = "Insert Into USERS Values (1, @P1, @P2, @P3)";
                SqlCommand sql_cmd = new SqlCommand(q_sel, myconn);
                sql_cmd.Parameters.AddWithValue("@P1", model.EMAIL);
                sql_cmd.Parameters.AddWithValue("@P2", _main.Encrypt(model.PASSWORD, "951623") );
                sql_cmd.Parameters.AddWithValue("@P3", model.FULLNAME);
                myconn.Open();
                int result = sql_cmd.ExecuteNonQuery();
                myconn.Close();

                if (result > 0)
                {
                    retVal.IsSuccess = true;
                    retVal.Message = "User Added";
                } 
                else
                {
                    retVal.IsSuccess = false;
                    retVal.Message = "Add User Failed";
                }
               

            }
            catch (Exception ex)
            {
                if (myconn.State == ConnectionState.Open)
                    myconn.Close();

                retVal.IsSuccess = false;
                retVal.Message = "Add User Failed";
            }

            return Json(retVal);
        }

        [HttpGet]
        public ActionResult Edituser(int id)
        {
            ViewBag.Action = Url.Action("Edituser", "Account");
            ViewBag.Title = "Edit User";

            ViewBag.GroupList = GroupList().Select(c => new SelectListItem() { Text = c.GROUPNAME, Value = c.GROUPID.ToString() }).ToList();

            UserModel user = new UserModel();
            string str_connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection myconn = new SqlConnection(str_connection);
            try
            {
                string q_sel = "SELECT Top 1 * FROM USERS Where ID = @P0";
                SqlCommand sql_cmd = new SqlCommand(q_sel, myconn);
                sql_cmd.Parameters.AddWithValue("@P0", id);
                myconn.Open();
                SqlDataReader dr = sql_cmd.ExecuteReader();



                while (dr.Read())
                {
                    user.ID = Convert.ToInt32(dr[0]);
                    user.GROUPID = Convert.ToInt32(dr[1]);
                    user.EMAIL = dr[2].ToString();
                    user.PASSWORD = "";// dr[3].ToString();
                    user.FULLNAME = dr[4].ToString();
                }

                dr.Close();
                myconn.Close();

            }
            catch (Exception ex)
            {
                if (myconn.State == ConnectionState.Open)
                    myconn.Close();
            }
            return PartialView("_Add", user);
        }

        [HttpPost]
        public ActionResult Edituser(UserModel model)
        {
            ReturnModel retVal = new ReturnModel();
            string str_connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection myconn = new SqlConnection(str_connection);

            try
            {
                string q_sel = "Update USERS Set GROUPID = @P0, EMAIL = @P1, FULLNAME = @P2 ";
                if (model.PASSWORD != null)
                    q_sel += ",PASSWORD = @P4 ";
                q_sel+=" Where ID = @P3";
                SqlCommand sql_cmd = new SqlCommand(q_sel, myconn);
                sql_cmd.Parameters.AddWithValue("@P0", model.GROUPID);
                sql_cmd.Parameters.AddWithValue("@P1", model.EMAIL);
                sql_cmd.Parameters.AddWithValue("@P2", model.FULLNAME);
                sql_cmd.Parameters.AddWithValue("@P3", model.ID);
                if (model.PASSWORD != null)
                    sql_cmd.Parameters.AddWithValue("@P4", _main.Encrypt(model.PASSWORD, "951623"));
                
                myconn.Open();
                int result = sql_cmd.ExecuteNonQuery();
                myconn.Close();

                if (result > 0)
                {
                    retVal.IsSuccess = true;
                    retVal.Message = "User Info Updated";
                }
                else
                {
                    retVal.IsSuccess = false;
                    retVal.Message = "User Info Update Failed";
                }


            }
            catch (Exception ex)
            {
                if (myconn.State == ConnectionState.Open)
                    myconn.Close();

                retVal.IsSuccess = false;
                retVal.Message = "User Info Update Failed";
            }

            return Json(retVal);
        }

        public ActionResult Deleteuser(int ID)
        {
            ReturnModel retVal = new ReturnModel();
            string str_connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection myconn = new SqlConnection(str_connection);

            try
            {
                string q_sel = "DELETE FROM USERS  Where ID = @P0";
                SqlCommand sql_cmd = new SqlCommand(q_sel, myconn);
                sql_cmd.Parameters.AddWithValue("@P0", ID);
                myconn.Open();
                int result = sql_cmd.ExecuteNonQuery();
                myconn.Close();

                if (result > 0)
                {
                    retVal.IsSuccess = true;
                    retVal.Message = "User Deleted";
                }
                else
                {
                    retVal.IsSuccess = false;
                    retVal.Message = "User Delete Failed";
                }


            }
            catch (Exception ex)
            {
                if (myconn.State == ConnectionState.Open)
                    myconn.Close();

                retVal.IsSuccess = false;
                retVal.Message = "User Delete Failed";
            }

            return Json(retVal, JsonRequestBehavior.AllowGet);
        }

        //Gruplar 
        public ActionResult Group()
        {
           return  View(GroupList());
        }

        [HttpGet]
        public ActionResult EditGroup(int id)
        {
            ViewBag.Title = "Edit Group Limit";
            GroupModel group = new GroupModel();
            string str_connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection myconn = new SqlConnection(str_connection);
            try
            {
                string q_sel = "SELECT Top 1 * FROM GROUPS Where ID = @P0";
                SqlCommand sql_cmd = new SqlCommand(q_sel, myconn);
                sql_cmd.Parameters.AddWithValue("@P0", id);
                myconn.Open();
                SqlDataReader dr = sql_cmd.ExecuteReader();
                while (dr.Read())
                {
                    group.ID = Convert.ToInt32(dr["ID"]);
                    group.GROUPNAME = dr["GROUPNAME"].ToString();
                    group.GROUPLIMIT = Convert.ToDecimal(dr["GROUPLIMIT"].ToString());
                }

                dr.Close();
                myconn.Close();
            }
            catch (Exception ex)
            {
                if (myconn.State == ConnectionState.Open)
                    myconn.Close();
            }
            return PartialView("_EditGroup", group);
        }

        [HttpPost]
        public ActionResult EditGroup(GroupModel model)
        {
            ReturnModel retVal = new ReturnModel();
            string str_connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection myconn = new SqlConnection(str_connection);
            try
            {
                string q_sel = "Update GROUPS Set GROUPLIMIT = @P0 WHERE ID = @P1";
                SqlCommand sql_cmd = new SqlCommand(q_sel, myconn);
                sql_cmd.Parameters.AddWithValue("@P0", model.GROUPLIMIT);
                sql_cmd.Parameters.AddWithValue("@P1", model.ID);
                myconn.Open();
                int result = sql_cmd.ExecuteNonQuery();
                myconn.Close();

                if (result > 0)
                {
                    retVal.IsSuccess = true;
                    retVal.Message = "Group Info Updated";
                }
                else
                {
                    retVal.IsSuccess = false;
                    retVal.Message = "Group Info Update Failed";
                }
            }
            catch (Exception ex)
            {
                if (myconn.State == ConnectionState.Open)
                    myconn.Close();

                retVal.IsSuccess = false;
                retVal.Message = "Group Info Update Failed";
            }

            return Json(retVal);
        }

    }
}
