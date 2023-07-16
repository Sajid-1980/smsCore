using Microsoft.EntityFrameworkCore;
using Models;
using smsCore.Data.Models.ViewModels;

namespace smsCore.Data.Helpers
{
    public class MenuHelper
    {
        SchoolEntities db;
        ClaimHelper _claims;

        public MenuHelper(SchoolEntities _db, ClaimHelper claims)
        {
            db = _db;
            _claims = claims;
        }
        public void SetMenuHelper(bool isdev, bool showHidden)
        {
            IsDev = isdev;
            ShowHidden = showHidden;
        }
        private bool IsDev { get; set; }
        private bool ShowHidden { get; set; }
        List<MenuItemViewModel> data;
        List<string> allController;
        public List<MenuItemViewModel> GetMenuItems()
        {
            List<MenuItemViewModel> list = new List<MenuItemViewModel>();
            string[] role = _claims.GetRolesFromClaims();
            bool isAdmin = role.Contains("Admin") || role.Contains("Developer");
            var roleIds = db.Roles.Where(w => role.Contains(w.Name)).Select(s => s.Id).ToArray();
            data = db.MenuItems.
                Where(w => (ShowHidden || w.IsActive) &&
                (isAdmin ||
                w.Action.Privlidges.Where(ww => roleIds.Contains(ww.RoleId) && ww.Allow).Any()
                )).AsNoTracking().OrderBy(o => o.SortOrder).
                Select(s => new MenuItemViewModel
                {
                    Id = s.Id,
                    Action = s.Action,
                    DisplayText = s.Action.DisplayText,
                    Icon = s.Icon,
                    ParentId = s.ParentId,
                    SortOrder = s.SortOrder,
                    IsDev = IsDev,
                    Controller = s.Action.Controller,
                    _Type = s.Action._Type
                }).ToList();
            int count = data.Count;
            var parents = data.Where(w => w.ParentId == 0);
            for (int i = 0; i < parents.Count(); i++)
            {
                var item = parents.ElementAt(i);
                allController = new List<string>();
                item.ChildItem = GetChilds(item.Id);
                item.Controllers = allController;
                list.Add(item);
            }
            return list;
        }

        private List<MenuItemViewModel> GetChilds(int id)
        {
            var childs = data.Where(w => w.ParentId == id).ToList();
            for (int i = 0; i < childs.Count(); i++)
            {
                var item = childs[i];
                if (item.ChildItem == null)
                    item.ChildItem = new List<MenuItemViewModel>();
                item.ChildItem = GetChilds(item.Id);
                allController.Add(item.Controller);
            }
            return childs;
        }
    }
}