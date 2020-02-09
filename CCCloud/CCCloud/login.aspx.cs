using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CCCloud
{
    public partial class login1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void submit_Click(object sender, EventArgs e)
        {
            if (PasswordTextbox.Text.Trim() == "" || UsernameTextbox.Text.Trim() == "")
                Response.Write("<script>alert('用户名或密码不能为空！')</script>");
            else
            {
                string sqlString = "select count(*) from t_user where userName='" + UsernameTextbox.Text.Trim() + "' and userPassword='" + PasswordTextbox.Text.Trim() + "'";
                if (SQLHelper.excuteInt(sqlString) == 1)
                {
                    Users u = new Users();
                    u.userName = UsernameTextbox.Text;
                    u.uid = Users.getuidByName(u.userName);
                    Session["user"] = u;
                    Response.Redirect("datadisplay.aspx?id=" + u.uid);
                }
                else
                {
                    Response.Write("<script>alert('用户名或密码不正确！')</script>");
                }
            }
        }

        protected void ok(object sender, EventArgs e)
        {
            Response.Redirect("datadisplay.aspx?id=0");
        }
    }
}