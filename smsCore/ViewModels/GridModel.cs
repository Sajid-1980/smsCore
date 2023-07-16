using Syncfusion.EJ2.Grids;
using System;
using System.Collections.Generic;

namespace smsCore.ViewModels
{
    public class GridModel
    {
        public bool ShowHeader { get; set; } = true;
        public string DataUrl { get; set; }
        public string GridId { get; set; } = "Grid";
        public string HeaderTemplate { get; set; } //= "<div class='col text-center'>Total Students: 50</div><div class='col text-center'>Passed: 29</div><div class='col text-center'>Failed: 29</div><div class='col text-center'>Percentage: 29</div>";
        public bool AllowGrouping { get; set; } = false;
        public GridGroupSettings GroupSettings { get; set; } = null;
        public bool AllowExcelExport { get; set; } = true;
        public bool AllowPdfExport { get; set; } = true;
        public bool AllowPrint { get; set; } = true;
        public bool AllowSearch { get; set; } = false;
        public bool ShowColumnMenu { get; set; } = false;
        public bool AllowFiltering { get; set; } = false;
        public bool AllowResizing { get; set; } = false;
        public bool ShowColumnChooser { get; set; } = false;
        public bool AllowSorting { get; set; } = false;
        public bool AllowPaging { get; set; } = true;
        public string Height { get; set; } = "auto";
        public string Width { get; set; } = "auto";
        public string ToolBarClick { get; set; } = "toolBarClick_root";
        public int PageSize { get; set; } = 15;
        public string[] PageSizes { get; set; } = new string[] { "15", "30", "50", "All" };
        public string[] Toolbar { get; set; } 
        public List<Column> Columns { get; set; }
        public List<GridAggregateColumn> GridAggregateColumns { get; set; }
        public bool AddSerialNo { get; set; }
        public string ReportTitle { get; set; } = "Report";
    }

    public class Column
    {
        public string Template { get; set; }
        public string Type { get; set; }
        public string MinWidth { get; set; } 
        public string MaxWidth { get; set; } 
        public string HeaderTemplate { get; set; } 
        public string HeaderText { get; set; }
        public string DateFormat { get; set; }
        public object Format
        {
            get
            {
                return new { type = "dateTime", format = DateFormat };
            }
        }
        public string FieldName { get;set;}
        public string Width { get; set; }
        public TextAlign TextAlign { get; set; } = TextAlign.Left;
        public TextAlign HeaderTextAlign { get; set; } = TextAlign.Left;
        public bool AllowEditing { get; set; } = false;
        public bool AllowFiltering { get; set; } = false;
        public bool AllowGrouping { get; set; } = false;
        public bool AllowReordering { get; set; } = false;
        public bool AllowResizing { get; set; } = false;
        public bool AllowSearching { get; set; } = false;
        public bool AllowSorting { get; set; } = false;
        public bool AutoFit { get; set; } = false;
        public bool Visible { get; set; } = false;
        public bool ShowInColumnChooser { get; set; } = false;
        public bool ShowColumnMenu { get; set; } = false;
        public bool IsFrozen { get; set; } = false;
        public bool IsIdentity { get; set; } = false;
        public bool IsPrimaryKey { get; set; } = false;

        //().().()
    }
}