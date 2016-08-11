using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace StarfishProject.Helpers
{
    public class JsonReader
    {
        /*public static string trim_column(string oldColumn)
        {
            string newColumn = oldColumn.Replace("{", "").Replace("[", "").Replace("\"", "").Replace("\"", ""); ;
            newColumn = newColumn.Replace("\n", "").Replace("\t", "").Replace("}", "").Replace("]", "").Replace("\"", "");
            return newColumn.Replace("\r", "");
        }*/

        public static DatabaseSchema Read()
        {
            string jsonFilePath = ConfigurationManager.AppSettings["JsonFilePath"];
            DatabaseSchema dbs = new DatabaseSchema();

/*            using (StreamReader file = File.OpenText(jsonFilePath))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JObject jsonObject = (JObject)JToken.ReadFrom(reader);
                dbs.dbname = (string)jsonObject.SelectToken("database");
                dbs.tables = new List<Table>();

                for (int i = 0; i <jsonObject.SelectToken("tables").Count(); i++)
                {
                    Table tbl = new Table();
                    string index = "tables[" + i + "]['table_name']";
                    tbl.tblname = jsonObject.SelectToken(index).ToString();
                    tbl.columns = new List<Column>();
                    index = "tables[" + i + "]['columns']";
                    var col_builder = jsonObject.SelectToken(index).ToString();
                    string[] strarr = col_builder.Split(',');
                    for (int j = 0; j < jsonObject.SelectToken(index).Count(); j++)
                    {
                        Column col = new Column();
                        string column_index=index+"["+j+"]['column_name']";
                        col.db_col_name = jsonObject.SelectToken(column_index).ToString();
                        column_index= index + "[" + j + "]['is editable']";
                        col.is_editable = trim_column(jsonObject.SelectToken(column_index).ToString());
                        column_index = index + "[" + j + "]['display_name']";
                        col.display_name = trim_column(jsonObject.SelectToken(column_index).ToString());
                        */
                        var data = File.ReadAllText(jsonFilePath);
                        var dbSchema=Newtonsoft.Json.JsonConvert.DeserializeObject<DatabaseSchema>(data);
                        
                        //dbSchema.tables[0].columns[0].db_col_name

                        //var cols=dbSchema.tables.First().columns.Select(x => x.display_name).ToList();
                      
                        /*try
                        {
                            column_index = index + "[" + j + "]['referred_table']";
                            col.referred_table=jsonObject.SelectToken(column_index).ToString();
                        }
                        catch
                        {

                        }
                        tbl.columns.Add(col);
                        

                    }
                    dbs.tables.Add(tbl);
                }
            }*/
            return dbSchema;
        }
    }
}