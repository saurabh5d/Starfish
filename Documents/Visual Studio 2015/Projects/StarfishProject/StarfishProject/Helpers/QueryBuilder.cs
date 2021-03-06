﻿using System;
using System.Collections.Generic;
using System.Text;

namespace StarfishProject.Helpers
{
    public class QueryBuilder
    {
        //Query for GetTable Without Join
        public static string SelectQueryBuilder(Table tbl, int page,DatabaseSchema schema)
        {
            StringBuilder str = new StringBuilder("select ");
            str.Append(tbl.primaryKey.pk_name+",");
            
            //Adding Columns which are in Json File
            foreach (Column col in tbl.columns)
                str.Append(col.column_name + ",");

            str.Remove(str.Length - 1, 1);
            str.Append(" From " + tbl.table_name);
            if(schema.database== "SqlServer")
            {
                str.Append(" Order by " + tbl.primaryKey.pk_name + " Offset " + page * 15 + " ROWS FETCH NEXT 15 ROWS ONLY ;");
            }
            else
            {
                str.Append(" Limit 15 Offset " + page * 15 + ";");
            }
            
            return str.ToString();
        }

        //Query for GetTable having Join
        public static string JoinQueryBuilder(Table tbl, int page, DatabaseSchema schema)
        {
            StringBuilder str = new StringBuilder("select ");
            str.Append(tbl.table_name+"."+ tbl.primaryKey.pk_name + ",");

            //Adding Columns which are in Json File
            foreach (Column col in tbl.columns)
            {
                //Adding Columns of Current Table
                if (col.referred_table == null)
                {
                    str.Append(col.column_name + ",");
                    continue;
                }
                //Adding Columns of Referred Table
                str.Append(col.referred_table + "." + col.column_name + ",");
            }
            str.Remove(str.Length - 1, 1);
            str.Append(" From ");
            bool start = true;
            //Joining the Different Tables 
            foreach (Column col in tbl.columns)
            {
                if (col.referred_table == null)
                    continue;
                if (start == true)
                {
                    ForeignKey fkey = DatabaseManager.getForeignKey(tbl, col.referred_table);
                    string join = fkey.fk_table + " inner join " + tbl.table_name + " on " + fkey.fk_table + "." + fkey.pk_name + "=" + tbl.table_name + "." + fkey.fk_name;
                    str.Append(join);
                    start = false;
                }
                else
                {
                    ForeignKey fkey = DatabaseManager.getForeignKey(tbl, col.referred_table);

                    if (str.ToString().Contains(fkey.pk_name + "=" + tbl.table_name + "." + fkey.fk_name))
                    {
                        continue;
                    }
                    string join = " inner join " + fkey.fk_table + " on " + fkey.fk_table + "." + fkey.pk_name + "=" + tbl.table_name + "." + fkey.fk_name;
                    str.Append(join);
                }
            }

            if (schema.database == "SqlServer")
            {
                str.Append(" Order by " + tbl.primaryKey.pk_name + " Offset " + page * 15 + " ROWS FETCH NEXT 15 ROWS ONLY ;");
            }
            else
            {
                str.Append(" Limit 15 Offset " + page * 15 + ";");
            }

            return str.ToString();
        }

        public static string BuildSearchQuery(Table table, List<SearchBox> searchElement, int page, DatabaseSchema schema)
        {
            StringBuilder str = new StringBuilder(SelectQueryBuilder(table, page,schema));
            int startindex = str.ToString().IndexOf("Limit");
            int endindex = str.Length;
            str.Remove(startindex, endindex - startindex);
            str.Append(" where ");
            //Matching Different values to Search  
            foreach (SearchBox element in searchElement)
            {
                foreach (var col in table.columns)
                    if (col.column_name == element.column_name)
                    {
                        if (col.data_type == "int")
                        {
                            str.Append(element.column_name + "=" + element.value + " AND ");
                            break;
                        }
                        if (col.data_type != "varchar" && col.data_type != "text" && col.data_type != "char")
                        {
                            str.Append("CAST(" + element.column_name + " as varchar)" + " = " + element.value + " AND ");
                            break;
                        }
                        else
                            str.Append(" UPPER ("+element.column_name + ") like '%" + element.value.ToUpper() + "%' AND ");
                        break;
                    }
            }
            str.Remove(str.Length - 4, 4);

            if (schema.database == "SqlServer")
            {
                str.Append(" Order by " + table.primaryKey.pk_name + " Offset " + page * 15 + " ROWS FETCH NEXT 15 ROWS ONLY ;");
            }
            else
            {
                str.Append(" Limit 15 Offset " + page * 15 + ";");
            }

            str.Append(";");
            return str.ToString();
        }

        public static string BuildJoinSearchQuery(Table currTable, List<SearchBox> searchElement, int page, DatabaseSchema schema)
        {
            StringBuilder str = new StringBuilder(JoinQueryBuilder(currTable, page,schema));
            int startindex = str.ToString().IndexOf("Limit");
            int endindex = str.Length;
            str.Remove(startindex, endindex - startindex);

            if (searchElement != null)
            {
                str.Append(" where ");

                foreach (SearchBox element in searchElement)
                {
                    foreach (var col in currTable.columns)
                    {
                        if (col.column_name == element.column_name)
                        {
                            string ref_tbl = col.referred_table;
                            if (ref_tbl == null)
                                ref_tbl = currTable.table_name;
                            if (col.data_type == "int")
                            {
                                str.Append(ref_tbl + "." + element.column_name + "=" + element.value + " AND ");
                                break;
                            }
                            if (col.data_type != "varchar")
                            {
                                str.Append("CAST(" + ref_tbl + "." + element.column_name + " as varchar)" + " like '%" + element.value + "%' AND ");
                                break;
                            }
                            else
                                str.Append(" UPPER(" +ref_tbl + "." + element.column_name + ") like '%" + element.value.ToUpper() + "%' AND ");
                            break;

                        }
                    }
                }
                str.Remove(str.Length - 4, 4);
            }

            if (schema.database == "SqlServer")
            {
                str.Append(" Order by " + currTable.primaryKey.pk_name + " Offset " + page * 15 + " ROWS FETCH NEXT 15 ROWS ONLY ;");
            }
            else
            {
                str.Append(" Limit 15 Offset " + page * 15 + ";");
            }

            return str.ToString();
        }

        public static string SortQueryBuilder(Table tbl, string ColumnName, int page, List<SearchBox> searchElement, DatabaseSchema schema)
        {
            StringBuilder str = new StringBuilder("select ");
            str.Append(tbl.primaryKey.pk_name + ",");

            foreach (Column col in tbl.columns)
                str.Append(col.column_name + ",");
            str.Remove(str.Length - 1, 1);
            str.Append(" From " + tbl.table_name);
            if (searchElement != null)
            {
                str.Append(" where ");
                foreach (SearchBox element in searchElement)
                {
                    str.Append(element.column_name + " like '%" + element.value + "%' AND ");
                }
                str.Remove(str.Length - 4, 4);
            }
            

            if (schema.database == "SqlServer")
            {
                str.Append(" Order by " + ColumnName + " Offset " + page * 15 + " ROWS FETCH NEXT 15 ROWS ONLY ;");
            }
            else
            {
                str.Append(" Order by " + ColumnName);
                str.Append(" Limit 15 Offset " + page * 15 + ";");
            }

            return str.ToString();

        }

        public static string JoinSortQueryBuilder(Table tbl, string ColumnName, int page, List<SearchBox> searchElement, DatabaseSchema schema)
        {
            StringBuilder str = new StringBuilder(BuildJoinSearchQuery(tbl, searchElement, page,schema));
         
            int limitindex = str.ToString().IndexOf("Limit");
            str.Remove(limitindex, str.Length - limitindex);

            if (schema.database == "SqlServer")
            {
                str.Append(" Order by " + ColumnName + " Offset " + page * 15 + " ROWS FETCH NEXT 15 ROWS ONLY ;");
            }
            else
            {
                str.Append(" Order by " + ColumnName);
                str.Append(" Limit 15 Offset " + page * 15 + ";");
            }

            return str.ToString();

        }

        public static string EditQueryBuilder(Table tbl, string id)
        {
            StringBuilder str = new StringBuilder("select * ");
            /*foreach (Column col in tbl.columns)
                str.Append(col.column_name + ",");

            str.Remove(str.Length - 1, 1);*/
            str.Append(" from " + tbl.table_name);
            var pkey = DatabaseManager.getPrimaryKey(tbl);
            str.Append(" where " + pkey + "='" + id + "';");
            return str.ToString();
        }

        public static string DeleteQueryBuilder(Table tbl,string id)
        {
            StringBuilder str = new StringBuilder("DELETE ");
            
            str.Append("From " + tbl.table_name);
            var pkey = DatabaseManager.getPrimaryKey(tbl);
               
            str.Append(" where "+pkey+ "='"+ id +"';");           
            return str.ToString();
        }

        public static string BuildCountQuery(string exequery)
        {
            int index = exequery.IndexOf("From");

            StringBuilder str = new StringBuilder(exequery);
            str.Remove(0, index);

            str.Insert(0, "select count(*) as count ");
            int limindex=0;
            if (str.ToString().Contains("Limit"))
                limindex = str.ToString().IndexOf("Limit");
            else
                 limindex = str.ToString().IndexOf("Order");
            return str.Remove(limindex, str.Length - limindex).Append(";").ToString();
        }

        public static string InsertRowQuery(List<SearchBox> AddElement, Table tbl)
        {
            StringBuilder str =new StringBuilder( "insert into " + tbl.table_name);
            StringBuilder cols = new StringBuilder("(");
            StringBuilder values = new StringBuilder("(");
            
            foreach (var element in AddElement)
            {
                cols.Append(element.column_name + ",");
                values.Append("'" + element.value + "'" + ",");
            }
            cols.Remove(cols.Length - 1, 1);
            values.Remove(values.Length - 1, 1);
            cols.Append(")");
            values.Append(")");
            str.Append(" " + cols + " values " + values + " ;");
            return str.ToString();
        }

        public static string UpdateRowQuery(List<SearchBox> AddElement, Table tbl,string id)
        {
            StringBuilder str = new StringBuilder("update " + tbl.table_name + " set ");

            foreach (var element in AddElement)
            {
                str.Append(element.column_name + "=" + "'" + element.value + "' ,");
            }
            str.Remove(str.Length - 1, 1);
            
            str.Append(" where " + tbl.primaryKey.pk_name + "='" + id + "';");

            return str.ToString();
        }



    }
}
    
