using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;


public class SQLHelper
{
    public static SqlConnection createCon()
    {
        SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["CCCloudDataConnectionString"].ConnectionString);
        return con;
    }
    public static void excuteSql(string sqlString)
    {
        SqlConnection con = createCon();
        con.Open();
        SqlCommand cmd =new SqlCommand(sqlString,con);
        cmd.ExecuteNonQuery();
        con.Close();
    }

    public static DataSet excuteDataSet(string sqlString)
    {
        SqlConnection con = createCon();
        con.Open();
        SqlDataAdapter sda = new SqlDataAdapter(sqlString, con);
        DataSet ds = new DataSet();
        sda.Fill(ds);
        con.Close();
        return ds;
    }

    public static int excuteInt(string sqlString)
    {
        SqlConnection con = createCon();
        con.Open();
        SqlCommand cmd = new SqlCommand(sqlString, con);
        int count = 0;
        count = Convert.ToInt32(cmd.ExecuteScalar());
        con.Close();
        return count;
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

        public static SqlDataReader ExcuteSqlDataReader(string sql, SqlParameter[] parms)
        {
            SqlDataReader result = null;
            SqlConnection con = createCon();
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(sql, con);
                if (parms != null)
                    cmd.Parameters.AddRange(parms);
                result = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch
            { }
            return result;
        }
}

