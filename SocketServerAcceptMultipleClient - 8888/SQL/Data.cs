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

    public static bool insertData(string[] data)
    {
        string sqlString = "insert into t_data_test values('" + data[0] + "','" + data[1] + "','" + data[2] + "','" + data[3] + "','" + data[4] + "','" + data[5] + "','" + data[6] + "','" + data[7] + "' )";
        return sqlhelper.excuteBool(sqlString);
    }
}
