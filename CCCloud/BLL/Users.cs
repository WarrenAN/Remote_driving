using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;

public class Users
{
    public int uid;
    public string userName;
    public string userPassword;

    public static Users validateUser()
    {
        return new Users();//用于判断用户登陆，返回true或false
    }


    public static Users getUserById(string id)
    {
        Users users = new Users();
        string sqlString = "select * from t_user where uid=" + id;
        SqlConnection con = SQLHelper.createCon();
        con.Open();
        SqlCommand cmd = new SqlCommand(sqlString, con);

        SqlDataReader sdr = cmd.ExecuteReader();
        while (sdr.Read())
        {
            users.uid = Convert.ToInt32(sdr[0]);
            users.userName = Convert.ToString(sdr[1]);
            users.userPassword = Convert.ToString(sdr[2]);
        }
            con.Close();
            return users;
        }

        public static int getuidByName(string userName)
        {
            string sqlString = "select uid from t_user where userName='" + userName + "'";
            SqlConnection con = SQLHelper.createCon();
            con.Open();
            SqlCommand cmd = new SqlCommand(sqlString, con);
            int uid = Convert.ToInt32(cmd.ExecuteScalar());
            con.Close();
            return uid;
        }
}

