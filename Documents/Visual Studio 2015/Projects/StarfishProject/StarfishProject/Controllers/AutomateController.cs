﻿using StarfishProject.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList.Mvc;
using PagedList;
using System.Data;

namespace StarfishProject.Controllers
{
    public class AutomateController : Controller
    {
        //DatabaseSchema dbs = DatabaseManager.BuildDataset();

        // GET: Automate
        public ActionResult Index()
        {

            DatabaseSchema dbs = DatabaseManager.BuildDataset();
            ViewData["DatabaseSchema"] = dbs;
            return View();
        }
        public ActionResult GetTable(string TableName,int? page)
        {
            int page1 = page ?? default(int);
            var dbs = DatabaseManager.BuildDataset(TableName,page1);
            //var table= dbs.tables.
            //                Select(x => x.tblname.ToLower().Equals(TableName.ToLower()));

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
            StaticPagedList<DataRow> data = new StaticPagedList<DataRow>(trows, page?? 1, 15,rowcount);
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
            var dbs = DatabaseManager.BuildDatasetForSearch(table, SearchElement,page1);
            ViewData["DatabaseSchema"]=dbs;
            DataRowCollection rows = dbs.dataset.Tables[table.table_name].Rows;
            DataRow count = dbs.dataset.Tables["count"].Rows[0];
            int rowcount = Int32.Parse(count["count"].ToString());
            DataRow[] trows = rows.Cast<DataRow>().ToArray();
            StaticPagedList<DataRow> data = new StaticPagedList<DataRow>(trows, page ?? 1, 15, rowcount);
            //PagedList<DataRow> data = new PagedList<DataRow>(trows, page ?? 1, 15);
            Session["rowcount"] = count;
            //Session["rows"] = trows;
            //Session["DatabaseSchema"] = dbs;
            Session["SearchElement"] = SearchElement;
            return View(data);
        }


        [HttpGet]
        public ActionResult Search(string Table, int? page,string search)
        {
            //List<SearchBox> SearchElement = new List<SearchBox>();
            var table = Session["CurrentTable"] as Table;
            int page1 = page ?? default(int);
            // var dbs = Session["DatabaseSchema"] as DatabaseSchema;
            var SearchElement = Session["SearchElement"] as List<SearchBox>;
            var dbs=DatabaseManager.BuildDatasetForSearch(table, SearchElement, page1);
            ViewData["TableData"] = table;
            ViewData["DatabaseSchema"] = dbs;
            /*foreach (var col in table.columns)
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
            var dbs = DatabaseManager.BuildDatasetForSearch(table, SearchElement);
            ViewData["DatabaseSchema"] = dbs;*/
           
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

            var SearchElement = Session["SearchElement"] as List<SearchBox>;
            var dbs = DatabaseManager.BuildSortDataset(table.table_name, ColumnName, page1, SearchElement);

            //   if (!((table.table_name.ToLower().Equals(TableName.ToLower())) || (table.table_display_name.ToLower().Equals(TableName.ToLower()))))
            //{

            // }
            ViewData["TableData"] = table;
            ViewData["DatabaseSchema"] = dbs;
            ViewData["ColumnName"] = ColumnName;

            //DataRow count = dbs.dataset.Tables["count"].Rows[0];
            //int rowcount = Int32.Parse(count["count"].ToString());

            DataRow count = Session["rowcount"] as DataRow;
            int rowcount = Int32.Parse(count["count"].ToString());
            DataRowCollection rows = dbs.dataset.Tables[table.table_name].Rows;
            DataRow[] trows = rows.Cast<DataRow>().ToArray();
            StaticPagedList<DataRow> data = new StaticPagedList<DataRow>(trows, page ?? 1, 15, rowcount);
            return View(data);
        }


    }
}