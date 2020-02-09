using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

public class sqlhelper
{
    public sqlhelper()
    {

    }
    /// <summary>
    /// 函数实现建立数据库连接，并返回该连接
    /// </summary>
    /// <returns>返回建立好的数据库连接</returns>
    public static SqlConnection createCon()
    {
        //获取配置文件中设置好的连接
        SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["CCCloudDataConnectionString"].ConnectionString);
        return con;
    }

    public static bool excuteBool(string sqlString)
    {
        SqlConnection con = createCon();
        con.Open();
        SqlCommand cmd = new SqlCommand(sqlString, con);
        bool result = true;
        try
        {
            cmd.ExecuteNonQuery();
        }
        catch
        {
            result = false;
        }
        con.Close();
        return result;
    }
}
