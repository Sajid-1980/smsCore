using Models;
using System.Collections.Generic;

namespace smsCore.Data.Models.ViewModels
{
    public class MenuItemViewModel : MenuItem
    {
        public MenuItemViewModel()
        {
            Controllers = new List<string>();
        }
        public bool IsDev { get; set; }
        public string DisplayText { get; set; }
        public string _Type { get; set; }
        public string Controller { get; set; }
        public List<string> Controllers { get; set; }
        public List<MenuItemViewModel> ChildItem { get; set; }
    }


}