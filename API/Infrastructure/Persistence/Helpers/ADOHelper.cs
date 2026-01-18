using System.Data;
using Microsoft.Data.SqlClient;
using System.Reflection;

namespace API.Infrastructure.Persistence.Helpers
{
    public static class ADOHelper
    {
        public static string connectionString { get; set; }="Data Source=34.27.79.134;Initial Catalog=Foodics;Persist Security Info=True;Company ID=rbst-g-db-mac-dispatch-admin;Password=njvwKWmlb3kT2482Vfsc2AoegUfIP5z;MultipleActiveResultSets=True;pooling=true;Max Pool Size=1000;TrustServerCertificate=Yes;Encrypt=False;";
        public static List<T> ExcuteStored<T>(string storedName, Dictionary<string, object> parameters = null) where T : new()
        {
            //string connectionString = ConfigurationHelper.GetConnectionString();
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand;
            SqlDataAdapter dataAdapter;
            DataTable dataTable;
            sqlCommand = new SqlCommand(storedName, sqlConnection);
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.CommandTimeout = 3;
            if (parameters != null)
                foreach (var param in parameters)
                {
                    sqlCommand.Parameters.AddWithValue("@" + param.Key, param.Value);
                }
            dataAdapter = new SqlDataAdapter(sqlCommand);
            dataTable = new DataTable();
            dataAdapter.Fill(dataTable);

            return dataTable.ConvertTo<T>();
        }

        public static List<T> ExcuteStored<T>(string storedName, params object[] parameters) where T : new()
        {
            //string connectionString = ConfigurationHelper.GetConnectionString();
            string command = string.Join(",", parameters.Select(p => FormatParameter(p)).ToList());


            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand;
            SqlDataAdapter dataAdapter;
            DataTable dataTable;
            sqlCommand = new SqlCommand($"{storedName} {command}", sqlConnection);
            sqlCommand.CommandTimeout = 3;

            dataAdapter = new SqlDataAdapter(sqlCommand);
            dataTable = new DataTable();
            dataAdapter.Fill(dataTable);

            return dataTable.ConvertTo<T>();
        }


      
         public static DataSet ExcuteStored(string storedName, Dictionary<string, object> parameters)
        {
            SqlConnection sqlConnection = new SqlConnection(connectionString); ;
            SqlCommand sqlCommand;
            SqlDataAdapter dataAdapter;
            DataSet dataSet;
            sqlCommand = new SqlCommand(storedName, sqlConnection);
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.CommandTimeout = 3;
            foreach (var param in parameters)
            {
                sqlCommand.Parameters.AddWithValue("@" + param.Key, param.Value);
            }
            dataAdapter = new SqlDataAdapter(sqlCommand);
            dataSet = new DataSet();
            dataAdapter.Fill(dataSet);
            return dataSet;
        }

        private static List<T> ConvertTo<T>(this DataSet dataSet) where T : new()
        {
            DataTable datatable = dataSet.Tables[0];
            List<T> Temp = new List<T>();
            try
            {
                List<string> columnsNames = new List<string>();
                foreach (DataColumn DataColumn in datatable.Columns)
                    columnsNames.Add(DataColumn.ColumnName);
                Temp = datatable.AsEnumerable().ToList().ConvertAll<T>(row => getObject<T>(row, columnsNames));
                return Temp;
            }
            catch
            {
                return Temp;
            }

        }
        private static List<T> ConvertTo<T>(this DataTable datatable) where T : new()
        {
            List<T> Temp = new List<T>();
            try
            {
                List<string> columnsNames = new List<string>();
                foreach (DataColumn DataColumn in datatable.Columns)
                    columnsNames.Add(DataColumn.ColumnName);
                Temp = datatable.AsEnumerable().ToList().ConvertAll<T>(row => getObject<T>(row, columnsNames));
                return Temp;
            }
            catch
            {
                return Temp;
            }

        }

        private static T getObject<T>(DataRow row, List<string> columnsName) where T : new()
        {
            T obj = new T();

            string columnname = "";
            string value = "";
            PropertyInfo[] Properties;
            Properties = typeof(T).GetProperties();
            foreach (PropertyInfo objProperty in Properties)
            {
                try
                {
                    columnname = columnsName.Find(name => name.ToLower() == objProperty.Name.ToLower());
                    if (!string.IsNullOrEmpty(columnname))
                    {
                        value = row[columnname].ToString();
                        if (!string.IsNullOrEmpty(value))
                        {
                            if (Nullable.GetUnderlyingType(objProperty.PropertyType) != null)
                            {
                                value = row[columnname].ToString().Replace("$", "").Replace(",", "");
                                objProperty.SetValue(obj, Convert.ChangeType(value, Type.GetType(Nullable.GetUnderlyingType(objProperty.PropertyType).ToString())), null);
                            }
                            else
                            {
                                value = row[columnname].ToString().Replace("%", "");
                                objProperty.SetValue(obj, Convert.ChangeType(value, Type.GetType(objProperty.PropertyType.ToString())), null);
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    //return obj;
                }
            }
            return obj;
        }

        private static string FormatParameter(object parameter)
        {
            if (parameter == null)
                return "null";
            if (parameter is string && (string)parameter == "default")
                return $"{parameter}";
            if (parameter is string || parameter is TimeSpan)
                return $"'{parameter}'";
            if (parameter is DateTime)
            {
                var str = ((DateTime)parameter).ToString("yyyy-MM-dd hh:mm:ss");
                return $"'{str}'";
            }
            return parameter.ToString();
        }
        public static void Excute(string SQLCommand)
        {
            SqlConnection con = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(SQLCommand, con);
            con.Open();
            var result = cmd.ExecuteNonQuery();
            con.Close();
        }
       
    }
    
}