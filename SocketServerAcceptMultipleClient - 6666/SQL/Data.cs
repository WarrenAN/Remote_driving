using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

public class data
{
    public data()
    {
    }

    public static bool insertData(string data)
    {
        string sqlString = "insert into t_user values('" + data + "','" + data + "' )";
        return sqlhelper.excuteBool(sqlString);
    }
}
