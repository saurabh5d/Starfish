using System;
using System.Collections.Generic;
using System.Text;

namespace StarfishProject.Helpers
{
    public class QueryBuilder
    {
        public static string SelectQueryBuilder(Table tbl, int page)
        {
            StringBuilder str = new StringBuilder("select ");
            int offset = (page) * 15;
            foreach (Column col in tbl.columns)
                str.Append(col.column_name + ",");
            str.Remove(str.Length - 1, 1);
            str.Append(" From " + tbl.table_name);
            str.Append(" Limit 15 Offset " + page * 15 + ";");
            return str.ToString();
        }
        public static string JoinQueryBuilder(Table tbl, int page)
        {
            StringBuilder str = new StringBuilder("select ");
            int offset = (page) * 15;
            foreach (Column col in tbl.columns)
            {
                if (col.referred_table == null)
                {
                    str.Append(col.column_name + ",");
                    continue;
                }
                str.Append(col.referred_table + "." + col.column_name + ",");
            }
            str.Remove(str.Length - 1, 1);
            str.Append(" From ");
            bool start = true;
            foreach (Column col in tbl.columns)
            {
                //Table reffered = DatabaseManager.getTableFromName(col.referred_table,schema);
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
            str.Append(" Limit 15 Offset " + page * 15 + ";");
            return str.ToString();
        }
        public static string BuildSearchQuery(Table table, List<SearchBox> searchElement, int page)
        {
            StringBuilder str = new StringBuilder(SelectQueryBuilder(table, page));
            int startindex = str.ToString().IndexOf("Limit");
            int endindex = str.Length;
            str.Remove(startindex, endindex - startindex);
            str.Append(" where ");
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
                        if (col.data_type != "varchar")
                        {
                            str.Append("CAST(" + element.column_name + "as varchar)" + "=" + element.value + " AND ");
                            break;
                        }
                        else
                            str.Append(element.column_name + " like '%" + element.value + "%' AND ");
                        break;
                    }
            }
            str.Remove(str.Length - 4, 4);
            str.Append(" Limit 15 Offset " + page * 15);
            str.Append(";");
            return str.ToString();
        }
        public static string BuildJoinSearchQuery(Table currTable, List<SearchBox> searchElement, int page)
        {
            StringBuilder str = new StringBuilder(JoinQueryBuilder(currTable, page));
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
                                str.Append(ref_tbl + "." + element.column_name + " like '%" + element.value + "%' AND ");
                            break;


                        }
                    }
                }
                str.Remove(str.Length - 4, 4);
            }
            
                
            str.Append(" Limit 15 Offset " + page * 15 + ";");
            return str.ToString();
        }
        public static string BuildCountQuery(string exequery)
        {
            int index = exequery.IndexOf("From");

            StringBuilder str = new StringBuilder(exequery);
            str.Remove(0, index);
            //str.Remove()

            str.Insert(0, "select count(*) ");
            int limindex = str.ToString().IndexOf("Limit");
            if (str.ToString().Contains("Offset 0"))
                return str.ToString();
            else
                return str.Remove(limindex, str.Length - limindex).Append(";").ToString();
        }

        public static string SortQueryBuilder(Table tbl, string ColumnName, int page, List<SearchBox> searchElement)
        {
            StringBuilder str = new StringBuilder("select ");

            foreach (Column col in tbl.columns)
                str.Append(col.column_name + ",");
            str.Remove(str.Length - 1, 1);
            str.Append(" From " + tbl.table_name);
            if (searchElement != null)
            {
                str.Append(" where ");
                foreach (SearchBox element in searchElement)
                {
                    str.Append(element.column_name + " like '%" + element.value + "%' AND");
                }
                str.Remove(str.Length - 4, 4);
            }
            str.Append(" Order by " + ColumnName);
            str.Append(" Limit 15 Offset " + page * 15 + ";");
            return str.ToString();

        }
        public static string JoinSortQueryBuilder(Table tbl, string ColumnName, int page, List<SearchBox> searchElement)
        {
            StringBuilder str = new StringBuilder(BuildJoinSearchQuery(tbl, searchElement, page));

            //StringBuilder str = new StringBuilder("select ");
            int limitindex = str.ToString().IndexOf("Limit");
            str.Remove(limitindex, str.Length - limitindex);
            //foreach (Column col in tbl.columns)
            //    str.Append(col.column_name + ",");
            //str.Remove(str.Length - 1, 1);
            //str.Append(" From " + tbl.table_name);
            str.Append(" Order by " + ColumnName);
            str.Append(" Limit 15 Offset " + page * 15 + ";");
            return str.ToString();

        }
    }
}
    
