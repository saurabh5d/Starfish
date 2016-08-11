using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;


namespace StarfishProject.Helpers
{
    public class SearchBox
    {
        public string column_name { get; set; }
        public string value { get; set; }
    }
    public class Column
    {
        public string column_name { get; set; }
        public string is_editable { get; set; }
        public string display_name { get; set; }
        public string referred_table { get; set; }
        public string data_type { get; set; }
    }
    public class ForeignKey
    {
        public string fk_name { get; set; }
        public string fk_table { get; set; }
        public string pk_name { get; set; }
    }

    public class Table
    {
        public string table_name { get; set; }
        public String table_display_name { get; set; }
        public string primaryKey { get; set; }
        public List<ForeignKey> foreignKeys { get; set; }
        public List<Column> columns { get; set; }

    }
    public class DatabaseSchema
    {
        public string database { get; set; }
        public List<Table> tables { get; set; }
        public DataSet dataset { get; set; }
    }
    public class DatabaseManager
    {
        public static OdbcConnection EstablishConnection(DatabaseSchema dbs)
        {
            //string ConnectionString = "Provider=SQLNCLI11;Data Source=(localdb)\\ProjectsV13;Integrated Security=SSPI;Initial Catalog=" + dbs.database;
            string ConnectionString = "Dsn=PostgreSQL30;uid=postgres";
            OdbcConnection myAccessConn = null;
            try
            {
                myAccessConn = new OdbcConnection(ConnectionString);


            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: Failed to create a database connection. \n{0}", ex.Message);

            }
            return myAccessConn;
        }
        //public static OleDbConnection EstablishConnection(DatabaseSchema dbs)
        //{
        //    //string ConnectionString = "Provider=SQLNCLI11;Data Source=(localdb)\\ProjectsV13;Integrated Security=SSPI;Initial Catalog=" + dbs.database;
        //    string ConnectionString = "Provider=PGNP.1;Data Source=localhost;User ID=postgres;Location=SFConnector_Trunk;password=admin123";
        //    OleDbConnection myAccessConn = null;
        //    try
        //    {
        //        myAccessConn = new OleDbConnection(ConnectionString);


        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error: Failed to create a database connection. \n{0}", ex.Message);

        //    }
        //    return myAccessConn;
        //}
        public static void ExecuteQuery(string tbl, OdbcConnection myAccessConn, DatabaseSchema schema, string query)
        {

            try
            {
               
                OdbcCommand myAccessCommand = new OdbcCommand(query, myAccessConn);
                myAccessCommand.CommandTimeout = 5000;
                OdbcDataAdapter myDataAdapter = new OdbcDataAdapter(myAccessCommand);

                // ADODB.Recordset adoRS = new ADODB.Recordset();
                myDataAdapter.Fill(schema.dataset, tbl);
                //schema.dataset.Tables.Add(table);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: Failed to retrieve the required data from the DataBase.\n{0}", ex.Message);

            }
            myAccessConn.Close();
        }
        //public static void ExecuteQuery(Table tbl, OleDbConnection myAccessConn, DatabaseSchema schema,string query)
        //{

        //    try
        //    {
        //        schema.dataset= new DataSet();
        //         OleDbCommand myAccessCommand = new OleDbCommand(query, myAccessConn);
        //        myAccessCommand.CommandTimeout = 5000;
        //        OleDbDataAdapter myDataAdapter = new OleDbDataAdapter(myAccessCommand);

        //       // ADODB.Recordset adoRS = new ADODB.Recordset();
        //        myDataAdapter.Fill(schema.dataset,tbl.table_name);
        //        //schema.dataset.Tables.Add(table);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error: Failed to retrieve the required data from the DataBase.\n{0}", ex.Message);

        //    }
        //    myAccessConn.Close();
        //}
        public static DatabaseSchema BuildDatasetForSearch(Table tbl, List<SearchBox> searchElement, int page)
        {
            DatabaseSchema schema = JsonReader.Read();
            schema.dataset = new DataSet();
            OdbcConnection myAccessConn = EstablishConnection(schema);
            foreach (var table in schema.tables)
            {
                if (table.table_name.ToLower().Equals(tbl.table_name.ToLower()) || table.table_display_name.ToLower().Equals(tbl.table_name.ToLower()))
                {

                    AddKeys(myAccessConn,schema, table.table_name);
                    getColumnType(myAccessConn, schema, table.table_name);
                    string query = "";
                    string countquery = "";
                    if (table.foreignKeys.Count == 0)
                    {
                        query = QueryBuilder.BuildSearchQuery(table, searchElement, page);
                    }
                    else
                    {
                        query = QueryBuilder.BuildJoinSearchQuery(table, searchElement, page);
                    }
                    countquery = QueryBuilder.BuildCountQuery(query);
                    ExecuteQuery(table.table_name, myAccessConn, schema, query);
                    ExecuteQuery("count", myAccessConn, schema, countquery);
                    break;
                }

            }
            return schema;
        }
        //public static DatabaseSchema BuildDatasetForSearch(Table tbl, List<SearchBox> searchElement,int page)
        //{
        //    DatabaseSchema schema = JsonReader.Read();
        //    schema.dataset = new DataSet();
        //    OleDbConnection myAccessConn = EstablishConnection(schema);
        //    foreach (var table in schema.tables)
        //    {
        //        if (table.table_name.ToLower().Equals(tbl.table_name.ToLower()) || table.table_display_name.ToLower().Equals(tbl.table_name.ToLower()))
        //        {
        //            AddKeys(myAccessConn, schema, table.table_name);
        //            getColumnType(myAccessConn, schema, table.table_name);
        //            string query = "";
        //            if (table.foreignKeys.Count == 0)
        //            {
        //                query = QueryBuilder.BuildSearchQuery(table, searchElement,page);
        //            }
        //            else
        //            {
        //                query = QueryBuilder.BuildJoinSearchQuery(table, searchElement,page);
        //            }

        //            ExecuteQuery(table, myAccessConn,schema,query);
        //            break;
        //        }

        //    }
        //    return schema;
        //}
        //building dataset for showing sidebar(no need to build full dataset)
        //index
        public static DatabaseSchema BuildDataset()
        {
            DatabaseSchema schema = JsonReader.Read();
            /*schema.dataset = new DataSet();
            OleDbConnection myAccessConn = EstablishConnection(schema);
            AddKeys(myAccessConn, schema);
            foreach (var table in schema.tables)
            {
                ExecuteQuery(table, myAccessConn, schema);
            }*/
            return schema;
        }
        public static DatabaseSchema BuildDataset(String tblname, int page)
        {
            DatabaseSchema schema = JsonReader.Read();
            schema.dataset = new DataSet();

            if (!String.IsNullOrEmpty(tblname))
            {

                OdbcConnection myAccessConn = EstablishConnection(schema);
               
                foreach (var table in schema.tables)
                {
                    if (table.table_name.ToLower().Equals(tblname.ToLower()) || table.table_display_name.ToLower().Equals(tblname.ToLower()))
                    {
                        AddKeys(myAccessConn,schema, table.table_name);
                        getColumnType(myAccessConn, schema, table.table_name);
                        string query = "";
                        string countquery = "";
                        if (table.foreignKeys.Count == 0)
                        {
                            query = QueryBuilder.SelectQueryBuilder(table, page);
                        }
                        else
                        {
                            query = QueryBuilder.JoinQueryBuilder(table, page);
                        }
                        countquery = QueryBuilder.BuildCountQuery(query);
                        ExecuteQuery(table.table_name, myAccessConn, schema, query);
                        ExecuteQuery("count", myAccessConn, schema, countquery);
                        break;
                    }
                }
            }
            return schema;
        }
        //for each table
        //public static DatabaseSchema BuildDataset(String tblname,int page)
        //{
        //    DatabaseSchema schema = JsonReader.Read();
        //    schema.dataset = new DataSet();

        //    if (!String.IsNullOrEmpty(tblname))
        //    {

        //        OleDbConnection myAccessConn = EstablishConnection(schema);
        //        foreach (var table in schema.tables)
        //        {
        //            if (table.table_name.ToLower().Equals(tblname.ToLower()) || table.table_display_name.ToLower().Equals(tblname.ToLower()))
        //            {
        //                AddKeys(myAccessConn, schema, table.table_name);
        //                string query = "";
        //                if (table.foreignKeys.Count == 0)
        //                {
        //                    query = QueryBuilder.SelectQueryBuilder(table,page);
        //                }
        //                else
        //                {
        //                    query = QueryBuilder.JoinQueryBuilder(table,page);
        //                }
        //                ExecuteQuery(table, myAccessConn, schema, query);
        //                break;
        //            }
        //        }
        //    }
        //    return schema;
        //}
        public static void AddKeys(OdbcConnection myAccessConn,DatabaseSchema schema, String tblname)
        {
           // string ConnectionString = "Provider=PGNP.1;Data Source=localhost;User ID=postgres;Location=SFConnector_Trunk;password=admin123";
           // using (OleDbConnection myAccessConn1 = new OleDbConnection(ConnectionString))
            
                try
                {
                    myAccessConn.Open();
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }
                foreach (var tbl in schema.tables)
                {
                    if (tbl.table_name == tblname)
                    {
                        //OdbcConnection connection = new OdbcConnection();
                        //DataTable dt = myAccessConn.GetSchema("Indexes", new[] {schema.database, null, tblname });
                        //DataSet db = new DataSet();
                        DatabaseSchema fschema = new DatabaseSchema();
                        string query = "SELECT  kcu.column_name As FK_COLUMN_NAME, ccu.table_name As PK_TABLE_NAME,ccu.column_name As pK_COLUMN_NAME FROM information_schema.table_constraints AS tc JOIN information_schema.key_column_usage AS kcu ON tc.constraint_name = kcu.constraint_name JOIN information_schema.constraint_column_usage AS ccu ON ccu.constraint_name = tc.constraint_name WHERE constraint_type = 'FOREIGN KEY' AND tc.table_name =\'" + tblname+"\'";
                    try
                    {
                        fschema.dataset=new DataSet();
                        ExecuteQuery(tblname, myAccessConn, fschema, query);
                    }
                    catch(Exception e)
                    {
                        Console.Write(e.Message);
                    }
                        //OdbcDataReader dr = ;
                        //dr.GetSchemaTable();
                        //           var dtt = myAccessConn1.GetOleDbSchemaTable(OleDbSchemaGuid.Primary_Keys, new string[] { null, null, tbl.table_name });
                        //foreach (DataRow dr in dtt.Rows)
                        //{
                        //    tbl.primaryKey = dr["COLUMN_NAME"].ToString();
                        //}
                        //var dtf = myAccessConn1.GetOleDbSchemaTable(OleDbSchemaGuid.Foreign_Keys, new string[] { null, null, null, null, null, tbl.table_name });
                        tbl.foreignKeys = new List<ForeignKey>();

                        foreach (DataRow dr in fschema.dataset.Tables[tblname].Rows)
                        {

                            ForeignKey fitem = new ForeignKey();
                            fitem.fk_name = dr["FK_COLUMN_NAME"].ToString();
                            fitem.fk_table = dr["PK_TABLE_NAME"].ToString();
                            fitem.pk_name = dr["pK_COLUMN_NAME"].ToString();
                            tbl.foreignKeys.Add(fitem);
                        }
                        break;
                    }
                }
            
        }

    
    //public static void AddKeys(OleDbConnection myAccessConn, DatabaseSchema schema,String tblname) 
    //{
    //    try
    //    {
    //        myAccessConn.Open();
    //    }
    //    catch (Exception e)
    //    {
    //        Console.Write(e.Message);
    //    }
    //    foreach (var tbl in schema.tables)
    //    {
    //        if (tbl.table_name == tblname)
    //        {
    //            //OdbcConnection connection = new OdbcConnection();


    //            var dtt = myAccessConn.GetOleDbSchemaTable(OleDbSchemaGuid.Primary_Keys, new string[] { null, null, tbl.table_name });
    //            foreach (DataRow dr in dtt.Rows)
    //            {
    //                tbl.primaryKey = dr["COLUMN_NAME"].ToString();
    //            }
    //            var dtf = myAccessConn.GetOleDbSchemaTable(OleDbSchemaGuid.Foreign_Keys, new string[] { null, null, null, null, null, tbl.table_name });
    //            tbl.foreignKeys = new List<ForeignKey>();
    //            foreach (DataRow dr in dtf.Rows)
    //            {

    //                ForeignKey fitem = new ForeignKey();
    //                fitem.fk_name = dr["FK_COLUMN_NAME"].ToString();
    //                fitem.fk_table = dr["PK_TABLE_NAME"].ToString();
    //                fitem.pk_name = dr["pK_COLUMN_NAME"].ToString();
    //                tbl.foreignKeys.Add(fitem);
    //            }
    //        }
    //    }
    //    myAccessConn.Close();
    //}
    public static string getPrimaryKey(Table tbl)
        {
            return tbl.primaryKey;
        }
        public static Table getTableFromName(string tableName,DatabaseSchema schema)
        {
            foreach(var table in schema.tables)
            {
                if (table.table_name == tableName)
                    return table;
            }
            return null;
        }
        public static ForeignKey getForeignKey(Table current,string reffered)
        {
            foreach(var fkey in current.foreignKeys)
            {
                if (fkey.fk_table == reffered)
                    return fkey;
            }
            return null;
        }
        public static void getColumnType(OdbcConnection myAccessConn, DatabaseSchema schema, String tblname)
        {

            try
            {
                myAccessConn.Open();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            foreach (var tbl in schema.tables)
            {
                if (tbl.table_name == tblname)
                {
                    DataTable dt = new DataTable("col");
                    foreach (var col in tbl.columns)
                    {
                        if (col.referred_table==null)
                            dt = myAccessConn.GetSchema("Columns",new string[] { null,null, tblname, col.column_name });
                        else
                            dt = myAccessConn.GetSchema("Columns", new string[] { null, null, col.referred_table, col.column_name });
                        col.data_type = dt.Rows[0]["Type_Name"].ToString();
                    }
                }
            }
            try
            {
                myAccessConn.Close();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

        }
        public static DatabaseSchema BuildSortDataset(String tblname, string ColumnName, int page, List<SearchBox> SearchElement)
        {
            DatabaseSchema schema = JsonReader.Read();
            schema.dataset = new DataSet();

            if (!String.IsNullOrEmpty(tblname))
            {

                OdbcConnection myAccessConn = EstablishConnection(schema);

                foreach (var table in schema.tables)
                {
                    if (table.table_name.ToLower().Equals(tblname.ToLower()) || table.table_display_name.ToLower().Equals(tblname.ToLower()))
                    {
                        AddKeys(myAccessConn, schema, table.table_name);
                        string query = "";
                        //  string countquery = "";

                        if (table.foreignKeys.Count == 0)
                        {
                            query = QueryBuilder.SortQueryBuilder(table, ColumnName, page, SearchElement);
                        }
                        else
                        {
                            query = QueryBuilder.JoinSortQueryBuilder(table, ColumnName, page, SearchElement);
                        }
                        //  countquery = QueryBuilder.BuildCountQuery(query);
                        ExecuteQuery(table.table_name, myAccessConn, schema, query);
                        //  ExecuteQuery("count", myAccessConn, schema, countquery);
                        break;
                    }
                }
            }
            return schema;
        }
        
    }
}
