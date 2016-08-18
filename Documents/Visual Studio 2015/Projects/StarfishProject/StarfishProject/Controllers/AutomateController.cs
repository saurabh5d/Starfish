using StarfishProject.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList.Mvc;
using PagedList;
using System.Data;
using System.Net;

namespace StarfishProject.Controllers
{
    public class AutomateController : Controller
    {
        // GET: Automate
        public ActionResult Index()
        {

            DatabaseSchema dbs = DatabaseManager.BuildDataset();
            ViewData["DatabaseSchema"] = dbs;
            var tbl = dbs.tables[0];
            return RedirectToAction("GetTable", "Automate", new { TableName = tbl.table_name});
        }

        public ActionResult GetTable(string TableName,int? page)
        {
            int page1 = page ?? default(int);

            if (page1 != 0)
                page1 = page1 - 1;

            Session["page"] = page;

            var dbs = DatabaseManager.BuildDataset(TableName,page1);

            var SearchElement = Session["SearchElement"] as List<SearchBox>;
            if (SearchElement != null)
                Session["SearchElement"] = null;

            Table tableData = new Table();

            foreach (var table in dbs.tables)
            {
                if ( (table.table_name.ToLower().Equals(TableName.ToLower())) || (table.table_display_name.ToLower().Equals(TableName.ToLower()))  )
                {
                    tableData = table;
                    break;
                }
            }
            ViewData["TableData"] = tableData;
            
            ViewData["DatabaseSchema"] = dbs;

        
            Table table1 = dbs.tables.
                           Where(x => x.table_name.ToLower().Equals(TableName.ToLower())).FirstOrDefault();

            ViewData["Table"] = table1.table_name;
            DataRow count = dbs.dataset.Tables["count"].Rows[0];
            int rowcount = Int32.Parse(count["count"].ToString());
            DataRowCollection rows= dbs.dataset.Tables[table1.table_name].Rows;
            DataRow[] trows = rows.Cast<DataRow>().ToArray();
            StaticPagedList<DataRow> data = new StaticPagedList<DataRow>(trows, page??1, 15,rowcount);
            Session["rowcount"] = count;
            Session["CurrentTable"] = table1;
            
            return View(data);
        }
       [HttpPost]
        public ActionResult Search(string Table, int? page)
        {
            int page1 = page ?? default(int);

            List<SearchBox> SearchElement = new List<SearchBox>();
           var table= Session["CurrentTable"] as Table;
            

           foreach (var col in table.columns)
           {
                SearchBox sbox = new SearchBox();
                try
                {
                    sbox.value = Request.Form[col.column_name].ToString();
                }
                catch { }
                if (sbox.value == null || sbox.value == "")
                    continue;
                sbox.column_name = col.column_name;
                SearchElement.Add(sbox);
           }
            ViewData["TableData"] = table;
            ViewData["Table"] = table.table_name;

            ViewData["SearchElement"] = SearchElement;

            var dbs = DatabaseManager.BuildDatasetForSearch(table, SearchElement,page1);
            ViewData["DatabaseSchema"]=dbs;
            DataRowCollection rows = dbs.dataset.Tables[table.table_name].Rows;
            DataRow count = dbs.dataset.Tables["count"].Rows[0];
            int rowcount = Int32.Parse(count["count"].ToString());
            DataRow[] trows = rows.Cast<DataRow>().ToArray();
            StaticPagedList<DataRow> data = new StaticPagedList<DataRow>(trows, page ?? 1, 15, rowcount);
            Session["rowcount"] = count;
            Session["SearchElement"] = SearchElement;
           
            return View(data);
        }


        [HttpGet]
        public ActionResult Search(string Table, int? page,string search)
        {
            var table = Session["CurrentTable"] as Table;
            int page1 = page ?? default(int);

            if (page1 != 0 )
                page1 = page1 - 1;

            Session["page"] = page;

            var SearchElement = Session["SearchElement"] as List<SearchBox>;

            var dbs=DatabaseManager.BuildDatasetForSearch(table, SearchElement, page1);

            ViewData["TableData"] = table;
            ViewData["DatabaseSchema"] = dbs;

            DataRow count = Session["rowcount"] as DataRow;
            int rowcount = Int32.Parse(count["count"].ToString());
            DataRowCollection rows = dbs.dataset.Tables[table.table_name].Rows;
            DataRow[] trows = rows.Cast<DataRow>().ToArray();
            StaticPagedList<DataRow> data = new StaticPagedList<DataRow>(trows, page ?? 1, 15, rowcount);
            return View(data);
        }

        public ActionResult SortColumn(string TableName, string ColumnName, int? page)
        {
            var table = Session["CurrentTable"] as Table;
            if (Session["prevtbl"] != table && Session["prevtbl"] != null)
            {
                Session["prevtbl"] = table;
                Session["SearchElement"] = null;
            }
            if (Session["ColumnName"] as string != ColumnName)
                Session["ColumnName"] = ColumnName;
            else
                ColumnName = Session["ColumnName"] as string;
            int page1 = page ?? default(int);

            if (page1 != 0)
                page1 = page1 - 1;

            var SearchElement = Session["SearchElement"] as List<SearchBox>;
            var dbs = DatabaseManager.BuildSortDataset(table.table_name, ColumnName, page1, SearchElement);

            
            ViewData["TableData"] = table;
            ViewData["DatabaseSchema"] = dbs;
            ViewData["ColumnName"] = ColumnName;

            DataRow count = Session["rowcount"] as DataRow;
            int rowcount = Int32.Parse(count["count"].ToString());
            DataRowCollection rows = dbs.dataset.Tables[table.table_name].Rows;
            DataRow[] trows = rows.Cast<DataRow>().ToArray();
            StaticPagedList<DataRow> data = new StaticPagedList<DataRow>(trows, page ?? 1, 15, rowcount);
            return View(data);
        }

        //Edit
        public ActionResult Edit(string TableName, string id)
        {
            var table = Session["CurrentTable"] as Table;

            var dbs = DatabaseManager.BuildEditDataset(table.table_name, id);

            ViewData["DatabaseSchema"] = dbs;
            ViewData["TableData"] = table;

            return View(dbs);
        }

        //Delete
        public ActionResult Delete(string TableName, string id)
        {
            var table = Session["CurrentTable"] as Table;

            int? page = Session["page"] as int?;

            DatabaseManager.BuildDeleteDataset(table.table_name,id);

            var SearchElement = Session["SearchElement"] as List<SearchBox>;

            if(SearchElement == null)
                return RedirectToAction("GetTable", "Automate", new { TableName = table.table_name ,page });
            else
                return RedirectToAction("Search", "Automate", new { TableName = table.table_name, page });
        }

        //add
        public ActionResult Insert()
        {
            var table = Session["CurrentTable"] as Table;
            List<SearchBox> AddElement = new List<SearchBox>();
            foreach (var col in table.columns)
            {
                SearchBox sbox = new SearchBox();
                try
                {
                    
                        sbox.value = Request.Form[col.column_name].ToString();
                    
                }
                catch { }
                if (sbox.value == null || sbox.value == "")
                    continue;
                if (col.referred_table == null)
                {
                    sbox.column_name = col.column_name;
                }

                else
                {
                    foreach (ForeignKey fkey in table.foreignKeys)
                    {
                        if (fkey.fk_table == col.referred_table)
                        {
                            sbox.column_name = fkey.fk_name;
                            break;
                        }
                    }
                }
            AddElement.Add(sbox);
            }
            DatabaseManager.InsertRow(AddElement, table);
            return RedirectToAction("GetTable", "Automate", new { TableName = table.table_name });
        }

        public ActionResult Add(string TableName)
        {
            var table = Session["CurrentTable"] as Table;
            var dbs = DatabaseManager.getAllColumnInfo(table);
            DatabaseManager.addDataForDropdown(dbs, table);
            ViewData["DatabaseSchema"] = dbs;
            ViewData["CurrentTable"] = table;

            return View(dbs);
        }
        public ActionResult Update(string id)
        {
            var table = Session["CurrentTable"] as Table;
            List<SearchBox> AddElement = new List<SearchBox>();
            foreach (var col in table.columns)
            {
                SearchBox sbox = new SearchBox();
                try
                {

                    sbox.value = Request.Form[col.column_name].ToString();

                }
                catch { }
                if (sbox.value == null || sbox.value == "")
                    continue;
                if (col.referred_table == null)
                {
                    sbox.column_name = col.column_name;
                }

                else
                {
                    foreach (ForeignKey fkey in table.foreignKeys)
                    {
                        if (fkey.fk_table == col.referred_table)
                        {
                            sbox.column_name = fkey.fk_name;
                            break;
                        }
                    }
                }
                AddElement.Add(sbox);
            }
            DatabaseManager.UpdateRow(AddElement, table,id);
            return RedirectToAction("GetTable", "Automate", new { TableName = table.table_name });
        }
    }
}

