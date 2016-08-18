using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;


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
    public class PrimaryKey
    {
        public string pk_name { get; set; }
        public string pk_data_type { get; set; }
    }
    public class Table
    {
        public string table_name { get; set; }
        public string table_display_name { get; set; }
        public PrimaryKey primaryKey { get; set; }
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

        public static void ExecuteQuery(string tbl, OdbcConnection myAccessConn, DatabaseSchema schema, string query)
        {

            try
            {
               
                OdbcCommand myAccessCommand = new OdbcCommand(query, myAccessConn);
                myAccessCommand.CommandTimeout = 5000;
                OdbcDataAdapter myDataAdapter = new OdbcDataAdapter(myAccessCommand);
                
                myDataAdapter.Fill(schema.dataset, tbl);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: Failed to retrieve the required data from the DataBase.\n{0}", ex.Message);

            }
            myAccessConn.Close();
        }

        //index
        public static DatabaseSchema BuildDataset()
        {
            DatabaseSchema schema = JsonReader.Read();
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
                        AddKeys(myAccessConn, schema, table.table_name);
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
        }//Build Get Table Data Set

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
        }//Build Search Data Set

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
                     
                        if (table.foreignKeys.Count == 0)
                        {
                            query = QueryBuilder.SortQueryBuilder(table, ColumnName, page, SearchElement);
                        }
                        else
                        {
                            query = QueryBuilder.JoinSortQueryBuilder(table, ColumnName, page, SearchElement);
                        }

                        ExecuteQuery(table.table_name, myAccessConn, schema, query);

                        break;
                    }
                }
            }
            return schema;
        }//Build Sort Data Set

        public static void BuildDeleteDataset(String tblname, string id)
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
                        getColumnType(myAccessConn, schema, table.table_name);

                        string query = "";

                        query = QueryBuilder.DeleteQueryBuilder(table, id);
                        ExecuteQuery(table.table_name, myAccessConn, schema, query);

                        break;
                    }
                }
            }
        }//Build Delete Data Set

        //To Add foreign keys
        public static void AddKeys(OdbcConnection myAccessConn,DatabaseSchema schema, String tblname)
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
            
        }//AddKeys

        //for primary key
        public static string AddPrimaryKeys(OdbcConnection myAccessConn, DatabaseSchema schema, String tblname)
        {
            try
            {
                myAccessConn.Open();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            
            DatabaseSchema pschema = new DatabaseSchema();
            string query = "SELECT k.column_name AS pk_Column_Name FROM information_schema.table_constraints t JOIN information_schema.key_column_usage k USING(constraint_name,table_name) WHERE t.constraint_type='PRIMARY KEY' AND t.table_name= \'" + tblname + "\'";
            try
            {
                pschema.dataset = new DataSet();
                ExecuteQuery(tblname, myAccessConn, pschema, query);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            string pk="";

            //tbl.primaryKey = pschema.dataset.Tables[tblname].Rows.ToString();

            foreach (DataRow dr in pschema.dataset.Tables[tblname].Rows)
            {
                pk =  dr["pk_Column_Name"].ToString();
            }
            return pk;
        }

        public static string getPrimaryKey(Table tbl)
        {
            return tbl.primaryKey.pk_name;
        }// getPrimary Keys

        public static Table getTableFromName(string tableName,DatabaseSchema schema)
        {
            foreach(var table in schema.tables)
            {
                if (table.table_name == tableName)
                {
                    return table;
                }
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
                    // add data type of columns
                    foreach (var col in tbl.columns)
                    {
                        if (col.referred_table==null)
                            dt = myAccessConn.GetSchema("Columns",new string[] { null,null, tblname, col.column_name });
                        else
                            dt = myAccessConn.GetSchema("Columns", new string[] { null, null, col.referred_table, col.column_name });
                        col.data_type = dt.Rows[0]["Type_Name"].ToString();
                    }
                    // add data type of priimary key
                    dt = myAccessConn.GetSchema("Columns", new string[] { null, null, tblname,tbl.primaryKey.pk_name });
                    tbl.primaryKey.pk_data_type= dt.Rows[0]["Type_Name"].ToString();
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
        }// Get Column Type

        //new add
        public static void InsertRow(List<SearchBox> AddElement, Table tbl)
        {
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

            string query = QueryBuilder.InsertRowQuery(AddElement, tbl);
            ExecuteInsertCmd(myAccessConn, query);



        }
        public static void addDataForDropdown(DatabaseSchema dbs, Table table)
        {
            string pk="";
            if (table.foreignKeys != null)
            {
                OdbcConnection myAccessConn = EstablishConnection(dbs);

                foreach (Column col in table.columns)
                {
                    if (col.referred_table != null)
                    {
                        Table tbl = getTableFromName(col.referred_table, dbs);
                        if (tbl == null)
                            pk = AddPrimaryKeys(myAccessConn, dbs , table.table_name);
                        else 
                             pk = getPrimaryKey(tbl);
                        string localaquery = "select distinct " +pk+" , "+ col.column_name + " from " + col.referred_table;
                        ExecuteQuery(col.column_name, myAccessConn, dbs, localaquery);

                    }
                }


            }
        }

        public static void ExecuteInsertCmd(OdbcConnection myAccessConn, string query)
        {
            OdbcCommand cmd = new OdbcCommand(query);
            OdbcTransaction transaction = null;
            cmd.Connection = myAccessConn;
            try
            {
                myAccessConn.Open();

                // Start a local transaction
                transaction = myAccessConn.BeginTransaction();

                // Assign transaction object for a pending local transaction.

                cmd.Transaction = transaction;

                // Execute the commands.

                cmd.ExecuteNonQuery();

                // Commit the transaction.
                transaction.Commit();

            }
            catch { }
        }

        public static DatabaseSchema getAllColumnInfo(Table table)
        {
            DatabaseSchema colschema = JsonReader.Read();
            colschema.dataset = new DataSet();
            DataTable dt = new DataTable();
            OdbcConnection myAccessConn = EstablishConnection(colschema);
            foreach (Table tbl in colschema.tables)
            {
                if (tbl.table_name == table.table_name)
                {
                    try
                    {
                        myAccessConn.Open();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    foreach (var col in tbl.columns)
                    {
                        dt.Merge(myAccessConn.GetSchema("Columns", new string[] { null, null, table.table_name, col.column_name }));
                    }
                    break;
                }
            }
            colschema.dataset.Tables.Add(dt);
            return colschema;
        }

    }
}
