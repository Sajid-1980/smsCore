using System.Collections.Generic;

namespace smsCore.Data.Models.ViewModels
{
    public class BoldReportViewModel
    {
        public string ReportName { get; set; }
        public string DataSetName { get; set; } = "DataSet1";
        public string ViewTitle { get; set; } = "Report";

       public Dictionary<string, object> Params { get; set; }=
        new Dictionary<string, object>();

    }
    public class ReportDesignerViewModel
    {
        public report report { get; set; }
        public string outputFormat { get; set; }
        public bool isTestData { get; set; }
    }
    public class report
    {
        public List<docElement> docElements { get; set; }
        public List<parameter> parameters { get; set; }
        public List<style> styles { get; set; }
        public int version { get; set; }
        public List<documentProperty> documentProperties { get; set; }
    }

    public class docElement
    {
        public string elementType { get; set; }
        public int id { get; set; }
        public string containerId { get; set; } //"0_content",
        public decimal x { get; set; }//450,
        public decimal y { get; set; }//:10,
        public decimal width { get; set; }//:130,
        public decimal height { get; set; }//:150,
        public string source { get; set; }//${StudentName}",
        public string image { get; set; }//"",
        public string imageFilename { get; set; }//"",
        public string horizontalAlignment { get; set; }//"left",
        public string verticalAlignment { get; set; }//"top",
        public string backgroundColor { get; set; }//:"",
        public string printIf { get; set; }//:"",
        public bool removeEmptyElement { get; set; }//:false,
        public string link { get; set; }//:"",
        public bool spreadsheet_hide { get; set; }//false,
        public string spreadsheet_column { get; set; }//:"",
        public bool spreadsheet_addEmptyRow { get; set; }//:false
    }
    public class parameter
    {
        public int id { get; set; } //:17,
        public string name { get; set; } //:"StudentName",
        public string type { get; set; } //"string",
        public string arrayItemType { get; set; } //:"string",
        public bool eval { get; set; } //:true,
        public bool nullable { get; set; } //:false,
        public string pattern { get; set; } //:"",
        public string expression { get; set; } //:"${StudentName}",
        public bool showOnlyNameType { get; set; } //:false,
        public string testData { get; set; } //:""
    }
    public class style
    {

    }
    public class documentProperty
    {
        public string pageFormat { get; set; } //"A4",
        public string pageWidth { get; set; } //"",
        public string pageHeight { get; set; } //"",
        public string unit { get; set; } //"mm",
        public string orientation { get; set; } //"portrait",
        public string contentHeight { get; set; } //"",
        public string marginLeft { get; set; } //"",
        public string marginTop { get; set; } //"",
        public string marginRight { get; set; } //"",
        public string marginBottom { get; set; } //"",
        public bool header { get; set; } //true,
        public string headerSize { get; set; } //"80",
        public string headerDisplay { get; set; } //"always",
        public bool footer { get; set; } //true,
        public string footerSize { get; set; } //"80",
        public string footerDisplay { get; set; } //"always",
        public string patternLocale { get; set; } //"en",
        public string patternCurrencySymbol { get; set; } //"$"
    }

}