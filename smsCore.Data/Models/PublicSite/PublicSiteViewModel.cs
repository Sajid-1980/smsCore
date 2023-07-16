using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace smsCore.Data.Models.PublicSite
{
    public class PublicPageViewModel 
    {
        public int Id { get; set; }
        public string Tags { get; set; }
        public string Description { get; set; }
        public string PageName { get; set; }
        public string PageTitle { get; set; }
        //[AllowHtml]
        public string PageContent { get; set; }
        public string Slug { get; set; }
    }
}