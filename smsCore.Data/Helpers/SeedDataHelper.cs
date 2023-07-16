
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace smsCore.Data.Helpers
{
    public class SeedDataHelper
    {

        private SchoolEntities _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SeedDataHelper(SchoolEntities context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        //public async Task SaveMenu()
        //{
        //    foreach (var p in menu)
        //    {
        //        var childs = menu.Where(w => w.ParentId == p.TempId).ToList();
        //        await AddItems(childs);
        //    }
        //    await _context.SaveChangesAsync();
        //    async Task AddParents(List<DynamicMenuAction> actions)
        //    {
        //        foreach (var m in actions)
        //        {
        //            await CreateOrUpdate(m);
        //        }
        //        await _context.SaveChangesAsync();
        //    }

        //    async Task AddItems(List<DynamicMenuAction> actions)
        //    {
        //        foreach (var m in actions)
        //        {
        //            if (m._Type == "parent")
        //            {
        //                await AddParents(new List<DynamicMenuAction> { m });
        //                var childs = menu.Where(w => w.ParentId == m.TempId).ToList();
        //                await AddItems(childs);
        //            }
        //            else
        //            {
        //                await CreateOrUpdate(m);
        //            }
        //        }
        //    }

        //    async Task CreateOrUpdate(DynamicMenuAction m)
        //    {
        //        var exist = _context.MenuActions.Where(w => w.Controller == m.Controller && w.Name == m.Name).FirstOrDefault();
        //        exist = exist ?? new MenuAction { };
        //        exist.Controller = m.Controller;
        //        exist.DisplayText = m.DisplayText;
        //        exist.IsActive = m.IsActive;
        //        exist.Name = m.Name;
        //        exist._Type = m._Type;
        //        if (exist.Id == 0)
        //            _context.MenuActions.Add(exist);
        //        if (m.CreateMenuItem)
        //        {
        //            var item = _context.MenuItems.FirstOrDefault(w => w.ActionId == exist.Id) ?? new MenuItem { };
        //            item.Icon = m.Icon;
        //            item.SortOrder = m.SortOrder;
        //            item.IsActive = m.IsActive;
        //            item.ParentId = 0;
        //            var parent = m.ParentId > 0 ? menu.Where(w => w.TempId == m.ParentId).Select(s => s.MenuItemId).FirstOrDefault() : 0;
        //            item.ParentId = parent;
        //            if (item.Id == 0)
        //            {
        //                exist.MenuItems.Add(item);
        //            }
        //            else if (exist.Id > 0 && item.Id == 0)
        //            {
        //                item.ActionId = exist.Id;
        //                _context.MenuItems.Add(item);
        //            }
        //            //if (m._Type=="parent")
        //            //await _context.SaveChangesAsync();

        //            m.MenuItemId = item.Id;
        //        }
        //        else if (exist.Id > 0)
        //        {
        //            var item = _context.MenuItems.FirstOrDefault(w => w.ActionId == exist.Id);
        //            if (item != null)
        //            {
        //                exist.MenuItems.Remove(item);
        //            }
        //        }
        //    }

        //}
        public async Task SeedData()
        {
            #region user and db
            ApplicationUser appUser = null;

            if (await _roleManager.FindByNameAsync("Developer") == null)
                await _roleManager.CreateAsync(new IdentityRole { Name = "Developer" });

            if (await _roleManager.FindByNameAsync("Admin") == null)
                await _roleManager.CreateAsync(new IdentityRole { Name = "Admin" });


            var user = _context.Users.Any(a => a.UserName == "developer");
            if (!user)
            {
                appUser = new ApplicationUser
                {
                    UserName = "developer",
                    Email = "admin@app.com"
                };
                await _userManager.CreateAsync(appUser, "Abc123!@#");

                await _userManager.AddToRoleAsync(appUser, "Developer");
            }

            #endregion

            var menu = GetMenu();

            foreach (var p in menu)
            {
                var exist = _context.MenuActions.FirstOrDefault(w => w.Controller == p.Controller && w.Name == p.Name);
                if (exist == null)
                {
                    exist = new MenuAction() { IsActive = true };
                    _context.MenuActions.Add(exist);
                }
                exist.Controller = p.Controller;
                exist.DisplayText = p.DisplayText;
                exist.Name = p.Name;
                exist._Type = p._Type;
            }
            await _context.SaveChangesAsync();

            var allactions = menu.Select(s => new { s.Controller, s.Name }).ToList();
            var all = _context.MenuActions.Select(s => new { s.Controller, s.Name }).ToList();
            var deleted = all.Except(allactions);
            foreach (var d in deleted)
            {
                var del = _context.MenuActions.Where(w => w.Controller == d.Controller && w.Name == d.Name);
                _context.MenuActions.RemoveRange(del);
            }
            await _context.SaveChangesAsync();

            _context.MenuItems.RemoveRange(_context.MenuItems);
            await _context.SaveChangesAsync();

            var parents = menu.Where(w => w._Type == "parent" && w.ParentId == 0);

            foreach (var p in parents)
            {
                await AddMenuItem(p, null);
            }
            await _context.SaveChangesAsync();

            async Task AddMenuItem(DynamicMenuAction p, MenuItem parent)
            {
                var exist = _context.MenuActions.FirstOrDefault(w => w.Controller == p.Controller && w.Name == p.Name);
                if (exist == null || p.CreateMenuItem == false)
                {
                    return;
                }

                var parent_e = new MenuItem { ActionId = exist.Id, IsActive = true, SortOrder = p.SortOrder, Icon = p.Icon, ParentId = parent?.Id ?? 0 };
                _context.MenuItems.Add(parent_e);
                await _context.SaveChangesAsync();


                var childs = menu.Where(w => w.ParentId == p.TempId);

                Console.WriteLine($"No of Childs {childs.Count()}");

                foreach (var c in childs)
                {
                    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(c));
                    var action = _context.MenuActions.FirstOrDefault(w => w.Controller == c.Controller && w.Name == c.Name);

                    //Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(action));

                    if (action != null)
                    {
                        Console.WriteLine($"found child {action.Controller} - {action.Name}");

                        if (c.CreateMenuItem)
                        {

                            if (c._Type == "parent")
                            {
                                await AddMenuItem(c, parent_e);
                            }
                            else
                            {
                                Console.WriteLine($"adding item item {action.Controller} - {action.Name}");

                                var item = new MenuItem { ActionId = action.Id, IsActive = true, SortOrder = c.SortOrder, Icon = c.Icon, ParentId = parent_e?.Id ?? 0 };
                                _context.MenuItems.Add(item);

                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"the action is null");
                    }
                }
            }


            //_context.MenuItems.RemoveRange(_context.MenuItems);
            //await _context.SaveChangesAsync();



            if (_context.AccountGroups.Count() == 0)
            {
                //just to effect reseed, this record will be deleted.
                var groupdummy = new tbl_AccountGroup { AccountGroupName = "Primary", GroupUnder = -1, Narration = string.Empty, IsDefault = true, Nature = "NA", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groupdummy.tbl_AccountLedger.Add(new tbl_AccountLedger { LedgerName = "Cash", Address = string.Empty, CreditLimit = 0, CreditPeriod = 0, CrOrDr = "Dr", Email = string.Empty, IsDefault = true, MailingName = string.Empty, Mobile = string.Empty, Narration = string.Empty, OpeningBalance = 0, EntryDate = DateTime.Now, UserId = appUser?.Id, ModifiedOn = DateTime.Now, ModifiedBy = appUser?.Id });
                _context.AccountGroups.Add(groupdummy);

                var groups = new List<tbl_AccountGroup>();
                var group0 = new tbl_AccountGroup { Id = 0, AccountGroupName = "Primary", GroupUnder = -1, Narration = string.Empty, IsDefault = true, Nature = "NA", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group0);

                var group1 = new tbl_AccountGroup { Id = 1, AccountGroupName = "Capital Account", GroupUnder = 0, Narration = string.Empty, IsDefault = true, Nature = "Liabilities", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group1);
                var group2 = new tbl_AccountGroup { Id = 2, AccountGroupName = "Loans (Liability)", GroupUnder = 0, Narration = string.Empty, IsDefault = true, Nature = "Liabilities", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group2);
                var group3 = new tbl_AccountGroup { Id = 3, AccountGroupName = "Current Liabilities", GroupUnder = 0, Narration = string.Empty, IsDefault = true, Nature = "Liabilities", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group3);
                var group4 = new tbl_AccountGroup { Id = 4, AccountGroupName = "Fixed Assets", GroupUnder = 0, Narration = string.Empty, IsDefault = true, Nature = "Assets", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group4);
                var group5 = new tbl_AccountGroup { Id = 5, AccountGroupName = "Investments", GroupUnder = 0, Narration = string.Empty, IsDefault = true, Nature = "Assets", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group5);
                var group6 = new tbl_AccountGroup { Id = 6, AccountGroupName = "Current Assets", GroupUnder = 0, Narration = string.Empty, IsDefault = true, Nature = "Assets", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group6);
                var group7 = new tbl_AccountGroup { Id = 7, AccountGroupName = "Branch /Divisions", GroupUnder = 0, Narration = string.Empty, IsDefault = true, Nature = "Liabilities", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group7);
                var group8 = new tbl_AccountGroup { Id = 8, AccountGroupName = "Misc.Expenses (ASSET)", GroupUnder = 0, Narration = string.Empty, IsDefault = true, Nature = "Assets", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group8);
                var group9 = new tbl_AccountGroup { Id = 9, AccountGroupName = "Suspense A/C", GroupUnder = 0, Narration = string.Empty, IsDefault = true, Nature = "Liabilities", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group9);
                var group10 = new tbl_AccountGroup { Id = 10, AccountGroupName = "Sales Account", GroupUnder = 0, Narration = string.Empty, IsDefault = true, Nature = "Income", AffectGrossProfit = "True", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group10);
                var group11 = new tbl_AccountGroup { Id = 11, AccountGroupName = "Purchase Account", GroupUnder = 0, Narration = string.Empty, IsDefault = true, Nature = "Expenses", AffectGrossProfit = "True", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group11);
                var group12 = new tbl_AccountGroup { Id = 12, AccountGroupName = "Direct Income", GroupUnder = 0, Narration = string.Empty, IsDefault = true, Nature = "Income", AffectGrossProfit = "True", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group12);
                var group13 = new tbl_AccountGroup { Id = 13, AccountGroupName = "Direct Expenses", GroupUnder = 0, Narration = string.Empty, IsDefault = true, Nature = "Expenses", AffectGrossProfit = "True", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group13);
                var group14 = new tbl_AccountGroup { Id = 14, AccountGroupName = "Indirect Income", GroupUnder = 0, Narration = string.Empty, IsDefault = true, Nature = "Income", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group14);
                var group15 = new tbl_AccountGroup { Id = 15, AccountGroupName = "Indirect Expenses", GroupUnder = 0, Narration = string.Empty, IsDefault = true, Nature = "Expenses", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group15);
                var group16 = new tbl_AccountGroup { Id = 16, AccountGroupName = "Reservers &Surplus", GroupUnder = 1, Narration = string.Empty, IsDefault = true, Nature = "Liabilities", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group16);
                var group17 = new tbl_AccountGroup { Id = 17, AccountGroupName = "Bank OD A/C", GroupUnder = 2, Narration = string.Empty, IsDefault = true, Nature = "Liabilities", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group17);
                var group18 = new tbl_AccountGroup { Id = 18, AccountGroupName = "Secured Loans", GroupUnder = 2, Narration = string.Empty, IsDefault = true, Nature = "Liabilities", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group18);
                var group19 = new tbl_AccountGroup { Id = 19, AccountGroupName = "UnSecured Loans", GroupUnder = 2, Narration = string.Empty, IsDefault = true, Nature = "Liabilities", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group19);
                var group20 = new tbl_AccountGroup { Id = 20, AccountGroupName = "Duties& Taxes", GroupUnder = 3, Narration = string.Empty, IsDefault = true, Nature = "Liabilities", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group20);
                var group21 = new tbl_AccountGroup { Id = 21, AccountGroupName = "Provisions", GroupUnder = 3, Narration = string.Empty, IsDefault = true, Nature = "Liabilities", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group21);
                var group22 = new tbl_AccountGroup { Id = 22, AccountGroupName = "Sundry Creditors", GroupUnder = 3, Narration = string.Empty, IsDefault = true, Nature = "Liabilities", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group22);
                var group23 = new tbl_AccountGroup { Id = 23, AccountGroupName = "Stock in Hand", GroupUnder = 6, Narration = string.Empty, IsDefault = true, Nature = "Assets", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group23);
                var group24 = new tbl_AccountGroup { Id = 24, AccountGroupName = "Deposits(Assets)", GroupUnder = 6, Narration = string.Empty, IsDefault = true, Nature = "Assets", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group24);
                var group25 = new tbl_AccountGroup { Id = 25, AccountGroupName = "Loans & Advances(Asset)", GroupUnder = 6, Narration = string.Empty, IsDefault = true, Nature = "Assets", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group25);
                var group26 = new tbl_AccountGroup { Id = 26, AccountGroupName = "Sundry Debtors", GroupUnder = 6, Narration = string.Empty, IsDefault = true, Nature = "Assets", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group26);
                var group27 = new tbl_AccountGroup { Id = 27, AccountGroupName = "Cash-in Hand", GroupUnder = 6, Narration = string.Empty, IsDefault = true, Nature = "Assets", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group27);
                var group28 = new tbl_AccountGroup { Id = 28, AccountGroupName = "Bank Account", GroupUnder = 6, Narration = string.Empty, IsDefault = true, Nature = "Assets", AffectGrossProfit = "False", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group28);
                var group29 = new tbl_AccountGroup { Id = 29, AccountGroupName = "Service Account", GroupUnder = 12, Narration = string.Empty, IsDefault = true, Nature = "Income", AffectGrossProfit = "True", CrDr = "Dr", EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, ModifiedOn = DateTime.Now, UserId = appUser?.Id };
                groups.Add(group29);

                group27.tbl_AccountLedger.Add(new tbl_AccountLedger { Id = 1, LedgerName = "Cash", Address = string.Empty, CreditLimit = 0, CreditPeriod = 0, CrOrDr = "Dr", Email = string.Empty, IsDefault = true, MailingName = string.Empty, Mobile = string.Empty, Narration = string.Empty, OpeningBalance = 0, EntryDate = DateTime.Now, UserId = appUser?.Id, ModifiedOn = DateTime.Now, ModifiedBy = appUser?.Id });
                group0.tbl_AccountLedger.Add(new tbl_AccountLedger { Id = 2, LedgerName = "Profit And Loss", Address = string.Empty, CreditLimit = 0, CreditPeriod = 0, CrOrDr = "Dr", Email = string.Empty, IsDefault = true, MailingName = string.Empty, Mobile = string.Empty, Narration = string.Empty, OpeningBalance = 0, EntryDate = DateTime.Now, UserId = appUser?.Id, ModifiedOn = DateTime.Now, ModifiedBy = appUser?.Id });
                group25.tbl_AccountLedger.Add(new tbl_AccountLedger { Id = 3, LedgerName = "Advance Payment", Address = string.Empty, CreditLimit = 0, CreditPeriod = 0, CrOrDr = "Cr", Email = string.Empty, IsDefault = true, MailingName = string.Empty, Mobile = string.Empty, Narration = string.Empty, OpeningBalance = 0, EntryDate = DateTime.Now, UserId = appUser?.Id, ModifiedOn = DateTime.Now, ModifiedBy = appUser?.Id });
                group15.tbl_AccountLedger.Add(new tbl_AccountLedger { Id = 4, LedgerName = "Salary", Address = string.Empty, CreditLimit = 0, CreditPeriod = 0, CrOrDr = "Dr", Email = string.Empty, IsDefault = true, MailingName = string.Empty, Mobile = string.Empty, Narration = string.Empty, OpeningBalance = 0, EntryDate = DateTime.Now, UserId = appUser?.Id, ModifiedOn = DateTime.Now, ModifiedBy = appUser?.Id });
                group29.tbl_AccountLedger.Add(new tbl_AccountLedger { Id = 5, LedgerName = "Service Account", Address = string.Empty, CreditLimit = 0, CreditPeriod = 0, CrOrDr = "Cr", Email = string.Empty, IsDefault = true, MailingName = string.Empty, Mobile = string.Empty, Narration = string.Empty, OpeningBalance = 0, EntryDate = DateTime.Now, UserId = appUser?.Id, ModifiedOn = DateTime.Now, ModifiedBy = appUser?.Id });
                group3.tbl_AccountLedger.Add(new tbl_AccountLedger { Id = 6, LedgerName = "PDC Payable", Address = string.Empty, CreditLimit = 0, CreditPeriod = 0, CrOrDr = "Cr", Email = string.Empty, IsDefault = true, MailingName = string.Empty, Mobile = string.Empty, Narration = string.Empty, OpeningBalance = 0, EntryDate = DateTime.Now, UserId = appUser?.Id, ModifiedOn = DateTime.Now, ModifiedBy = appUser?.Id });
                group6.tbl_AccountLedger.Add(new tbl_AccountLedger { Id = 7, LedgerName = "PDC Receivable", Address = string.Empty, CreditLimit = 0, CreditPeriod = 0, CrOrDr = "Dr", Email = string.Empty, IsDefault = true, MailingName = string.Empty, Mobile = string.Empty, Narration = string.Empty, OpeningBalance = 0, EntryDate = DateTime.Now, UserId = appUser?.Id, ModifiedOn = DateTime.Now, ModifiedBy = appUser?.Id });
                group15.tbl_AccountLedger.Add(new tbl_AccountLedger { Id = 8, LedgerName = "Discount Allowed", Address = string.Empty, CreditLimit = 0, CreditPeriod = 0, CrOrDr = "Dr", Email = string.Empty, IsDefault = true, MailingName = string.Empty, Mobile = string.Empty, Narration = string.Empty, OpeningBalance = 0, EntryDate = DateTime.Now, UserId = appUser?.Id, ModifiedOn = DateTime.Now, ModifiedBy = appUser?.Id });
                group14.tbl_AccountLedger.Add(new tbl_AccountLedger { Id = 9, LedgerName = "Discount Received", Address = string.Empty, CreditLimit = 0, CreditPeriod = 0, CrOrDr = "Cr", Email = string.Empty, IsDefault = true, MailingName = string.Empty, Mobile = string.Empty, Narration = string.Empty, OpeningBalance = 0, EntryDate = DateTime.Now, UserId = appUser?.Id, ModifiedOn = DateTime.Now, ModifiedBy = appUser?.Id });
                group10.tbl_AccountLedger.Add(new tbl_AccountLedger { Id = 10, LedgerName = "Sales Account", Address = string.Empty, CreditLimit = 0, CreditPeriod = 0, CrOrDr = "Cr", Email = string.Empty, IsDefault = true, MailingName = string.Empty, Mobile = string.Empty, Narration = string.Empty, OpeningBalance = 0, EntryDate = DateTime.Now, UserId = appUser?.Id, ModifiedOn = DateTime.Now, ModifiedBy = appUser?.Id });
                group11.tbl_AccountLedger.Add(new tbl_AccountLedger { Id = 11, LedgerName = "Purchase Account", Address = string.Empty, CreditLimit = 0, CreditPeriod = 0, CrOrDr = "Dr", Email = string.Empty, IsDefault = true, MailingName = string.Empty, Mobile = string.Empty, Narration = string.Empty, OpeningBalance = 0, EntryDate = DateTime.Now, UserId = appUser?.Id, ModifiedOn = DateTime.Now, ModifiedBy = appUser?.Id });
                group14.tbl_AccountLedger.Add(new tbl_AccountLedger { Id = 12, LedgerName = "Forex Gain/Loss", Address = string.Empty, CreditLimit = 0, CreditPeriod = 0, CrOrDr = "Cr", Email = string.Empty, IsDefault = true, MailingName = string.Empty, Mobile = string.Empty, Narration = string.Empty, OpeningBalance = 0, EntryDate = DateTime.Now, UserId = appUser?.Id, ModifiedOn = DateTime.Now, ModifiedBy = appUser?.Id });
                group12.tbl_AccountLedger.Add(new tbl_AccountLedger { Id = 13, LedgerName = "School Fee Revenue", Address = string.Empty, CreditLimit = 0, CreditPeriod = 0, CrOrDr = "Cr", Email = string.Empty, IsDefault = true, MailingName = string.Empty, Mobile = string.Empty, Narration = string.Empty, OpeningBalance = 0, EntryDate = DateTime.Now, UserId = appUser?.Id, ModifiedOn = DateTime.Now, ModifiedBy = appUser?.Id });
                group6.tbl_AccountLedger.Add(new tbl_AccountLedger { Id = 14, LedgerName = "School Fee Receiveable", Address = string.Empty, CreditLimit = 0, CreditPeriod = 0, CrOrDr = "Cr", Email = string.Empty, IsDefault = true, MailingName = string.Empty, Mobile = string.Empty, Narration = string.Empty, OpeningBalance = 0, EntryDate = DateTime.Now, UserId = appUser?.Id, ModifiedOn = DateTime.Now, ModifiedBy = appUser?.Id });

                //using (var t = _context.Database.BeginTransaction())
                {
                    await _context.SaveChangesAsync();

                    await _context.Database.ExecuteSqlRawAsync("delete from tbl_AccountGroup");

                    await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT (tbl_AccountGroup, RESEED, -1)");
                    await _context.Database.ExecuteSqlRawAsync("delete from tbl_AccountLedger");
                    await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT (tbl_AccountLedger, RESEED, 0)");

                    _context.AccountGroups.AddRange(groups);
                    //foreach (var g in groups)
                    //{
                    //    //var exist = await _context.AccountGroups.FirstOrDefaultAsync(w => w.AccountGroupName == g.AccountGroupName);
                    //    //if (exist == null)
                    //    //{
                    //    //  _context.AccountGroups.Add(g); 
                    //    //}
                    //}
                    await _context.SaveChangesAsync();
                    //t.Commit();
                }
            }

            if (_context.VoucherTypes.Count() < 29)
            {
                //just to effect reseed, this record will be deleted.
                _context.VoucherTypes.Add(new tbl_VoucherType { Id = 0, VoucherTypeName = "Opening Balance", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Opening Balance", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 0, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });

                var vouchers = new List<tbl_VoucherType>();
                vouchers.Add(new tbl_VoucherType { Id = 1, VoucherTypeName = "Opening Balance", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Opening Balance", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 0, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 2, VoucherTypeName = "Opening Stock", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Opening Stock", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 0, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 3, VoucherTypeName = "Contra Voucher", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Contra Voucher", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 5, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 4, VoucherTypeName = "Payment Voucher", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Payment Voucher", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 1, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 5, VoucherTypeName = "Receipt Voucher", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Receipt Voucher", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 2, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 6, VoucherTypeName = "Journal Voucher", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Journal Voucher", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 3, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 7, VoucherTypeName = "PDC Payable", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "PDC Payable", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 20, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 8, VoucherTypeName = "PDC Receivable", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "PDC Receivable", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 21, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 9, VoucherTypeName = "PDC Clearance", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "PDC Clearance", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 22, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 10, VoucherTypeName = "Purchase Order", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Purchase Order", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 11, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 11, VoucherTypeName = "Material Receipt", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Material Receipt", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 16, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 12, VoucherTypeName = "Rejection Out", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Rejection Out", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 17, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 13, VoucherTypeName = "Purchase Invoice", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Purchase Invoice", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 9, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 14, VoucherTypeName = "Purchase Return", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Purchase Return", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 13, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 15, VoucherTypeName = "Sales Quotation", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Sales Quotation", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 18, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 16, VoucherTypeName = "Sales Order", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Sales Order", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 10, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 17, VoucherTypeName = "Delivery Note", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Delivery Note", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 14, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 18, VoucherTypeName = "Rejection In", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Rejection In", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 15, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 19, VoucherTypeName = "Sales Invoice", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Sales Invoice", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 8, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 20, VoucherTypeName = "Sales Return", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Sales Return", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 12, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 21, VoucherTypeName = "Service Voucher", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Service Voucher", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 4, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 22, VoucherTypeName = "Credit Note", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Credit Note", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 6, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 23, VoucherTypeName = "Debit Note", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Debit Note", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 7, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 24, VoucherTypeName = "Stock Journal", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Stock Journal", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 0, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 25, VoucherTypeName = "Physical Stock", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Physical Stock", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 19, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 26, VoucherTypeName = "Daily Salary Voucher", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Daily Salary Voucher", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 0, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 27, VoucherTypeName = "Monthly Salary Voucher", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Monthly Salary Voucher", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 0, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 28, VoucherTypeName = "Advance Payment", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Advance Payment", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 0, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });
                vouchers.Add(new tbl_VoucherType { Id = 29, VoucherTypeName = "Damage Stock", MethodOfVoucherNumbering = "Automatic", TypeOfVoucher = "Damage Stock", IsActive = true, IsDefault = true, IsTaxApplicable = false, MasterId = 0, Declaration = string.Empty, EntryDate = DateTime.Now, ModifiedBy = appUser?.Id, UserId = appUser?.Id, Heading1 = string.Empty, Heading2 = string.Empty, Heading3 = string.Empty, Heading4 = string.Empty, Narration = string.Empty });


                //using (var t = _context.Database.BeginTransaction())
                {
                    await _context.SaveChangesAsync();

                    await _context.Database.ExecuteSqlRawAsync("delete from  tbl_VoucherType");
                    //_context = new SchoolEntities();
                    await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT (tbl_VoucherType, RESEED, 0)");
                    _context.VoucherTypes.AddRange(vouchers);
                    //foreach (var g in vouchers)
                    //{
                    //    //var exist = await _context.VoucherTypes.FirstOrDefaultAsync(w => w.VoucherTypeName == g.VoucherTypeName);
                    //    //if (exist == null)
                    //    //{
                    //    _context.VoucherTypes.Add(g);
                    //    //}
                    //}
                    await _context.SaveChangesAsync();
                    //t.Commit();
                }
            }

            //Present=1, Absent=2, Leave=3,LateComing=4
            if (!_context.Campuses.Any())
            {
                _context.Campuses.Add(new Campus { address = string.Empty, CampusName = "Default", emailId = string.Empty, mailingName = string.Empty, web = string.Empty, phone = string.Empty, mobile = string.Empty });
                await _context.SaveChangesAsync();
            }

            if (!_context.StudentAttendanceTypes.Any())
            {
                _context.StudentAttendanceTypes.Add(new StudentAttendanceType { AttendanceName = "Present", Code = "P", Fine = 0 });
                _context.StudentAttendanceTypes.Add(new StudentAttendanceType { AttendanceName = "Absent", Code = "A", Fine = 0 });
                _context.StudentAttendanceTypes.Add(new StudentAttendanceType { AttendanceName = "Leave", Code = "L", Fine = 0 });
                _context.StudentAttendanceTypes.Add(new StudentAttendanceType { AttendanceName = "LateComing", Code = "LC", Fine = 0 });
                await _context.SaveChangesAsync();
            }

            if (!_context.EmployeeAttendanceTypes.Any())
            {
                _context.EmployeeAttendanceTypes.Add(new EmployeeAttendanceType { AttendanceName = "Present", Code = "P", FineInDays = 0, YearlyAllowed = 0 });
                _context.EmployeeAttendanceTypes.Add(new EmployeeAttendanceType { AttendanceName = "Absent", Code = "A", FineInDays = 0, YearlyAllowed = 0 });
                _context.EmployeeAttendanceTypes.Add(new EmployeeAttendanceType { AttendanceName = "Leave", Code = "L", FineInDays = 0, YearlyAllowed = 0 });
                _context.EmployeeAttendanceTypes.Add(new EmployeeAttendanceType { AttendanceName = "LateComing", Code = "LC", FineInDays = 0, YearlyAllowed = 0 });
                await _context.SaveChangesAsync();
            }
            //Transport = 4,SMSCharges = 6,AttendanceFine = 7
            if (!_context.FeeTypes.Any())
            {
                _context.FeeTypes.Add(new FeeType { AllowDiscount = true, AllowEdit = true, IsOptional = true, Notes = "Admission Fee", SortOrder = 0, TypeName = "Admission Fee" });
                _context.FeeTypes.Add(new FeeType { AllowDiscount = true, AllowEdit = true, IsOptional = true, Notes = "Tuition Fee", SortOrder = 1, TypeName = "Tuition Fee" });
                _context.FeeTypes.Add(new FeeType { AllowDiscount = false, AllowEdit = false, IsOptional = false, Notes = "Late Fee", SortOrder = 2, TypeName = "Late Fee" });
                _context.FeeTypes.Add(new FeeType { AllowDiscount = true, AllowEdit = false, IsOptional = false, Notes = "Transport Fee is calculated automatically from tranport fare.", SortOrder = 3, TypeName = "Transport Fee" });
                _context.FeeTypes.Add(new FeeType { AllowDiscount = true, AllowEdit = true, IsOptional = true, Notes = "IT Charges", SortOrder = 4, TypeName = "Library Charges" });
                _context.FeeTypes.Add(new FeeType { AllowDiscount = true, AllowEdit = false, IsOptional = false, Notes = "SMSCharge is calculted automatically on basis of monthly sent text messages.", SortOrder = 5, TypeName = "SMS Charges" });
                _context.FeeTypes.Add(new FeeType { AllowDiscount = true, AllowEdit = false, IsOptional = false, Notes = "Attendance Fine is calculated on basis of attendance fine.", SortOrder = 6, TypeName = "Attendance Fine" });
                await _context.SaveChangesAsync();
            }

            string proc_FeeSystemGetAllReceiveable = @"
CREATE OR ALTER PROCEDURE [dbo].[FeeSystemGetAllReceiveable]
(
	@regno int
	)
AS
	SELECT TOP (100) PERCENT dbo.Students.RegistrationNo, dbo.Admissions.ID as AdmissionId,dbo.FeeSlips.Id , dbo.FeeSlips.ForMonth, SUM(dbo.FeeSlipDetails.Amount) - dbo.GetReceviedAmountInSlip(dbo.FeeSlips.Id) AS Balance, dbo.GetLateFeeAmount(dbo.FeeSlips.Id, 
                  dbo.Admissions.CampuseID) AS LateFee
FROM     dbo.Admissions INNER JOIN
                  dbo.FeeSlips ON dbo.Admissions.ID = dbo.FeeSlips.AdmissionId INNER JOIN
                  dbo.Students ON dbo.Admissions.StudentID = dbo.Students.ID LEFT OUTER JOIN
                  dbo.FeeSlipDetails ON dbo.FeeSlips.Id = dbo.FeeSlipDetails.FeeSlipId
GROUP BY dbo.FeeSlips.ForMonth, dbo.FeeSlips.Id, dbo.Students.RegistrationNo, dbo.Admissions.ID, dbo.Admissions.CampuseID
HAVING (dbo.Students.RegistrationNo = @regno) AND (SUM(dbo.FeeSlipDetails.Amount) - dbo.GetReceviedAmountInSlip(dbo.FeeSlips.Id) > 0)
ORDER BY dbo.Admissions.ID				 
RETURN";
            await _context.Database.ExecuteSqlRawAsync(proc_FeeSystemGetAllReceiveable);

            string proc_FeeSystemGetAllReceiveableAmount = @"
CREATE OR ALTER  PROCEDURE [dbo].[FeeSystemGetAllReceiveableAmount]
(
	@regno int
	)
AS

DECLARE @tblVar TABLE (
   RegistrationNo int,
   AdmissionId int,
   Id int,
   ForMonth datetime,
Balance decimal(18,2),
LateFee decimal(18,2)
)
INSERT INTO @tblVar Exec  FeeSystemGetAllReceiveable @regno

Select Sum(Balance) as Balance, Sum(LateFee) as LateFee from @tblVar				 
RETURN";
            await _context.Database.ExecuteSqlRawAsync(proc_FeeSystemGetAllReceiveableAmount);

            string proc_FeeSystemProc = @"
CREATE OR ALTER PROCEDURE [dbo].[FeeSystemProc]
(
	@month int,
	@year int,
	@id nvarchar(max)
	)
AS
	DECLARE @SQL nvarchar(max);
SET @SQL = 
	'
SELECT        TOP (100) PERCENT dbo.Admissions.ID AS AdmissionID, dbo.Students.RegistrationNo, dbo.Students.StudentName, dbo.Students.FName, dbo.Students.Sex, dbo.Students.Email, dbo.Students.PostalAddress, 
                         dbo.Campuses.CampusName, dbo.Classes.ClassName, dbo.Sections.SectionName, dbo.FeeTypes.TypeName, dbo.FeeSlips.DueDate, CONVERT(datetime, dbo.FeeSlips.ForMonth) AS ForMonthYear, dbo.FeeSlipDetails.Amount, 
                         dbo.FeeSlipDetails.Discount AS DiscountGiven, dbo.FeeSlipDetails.DiscountInAmount AS DiscountInAmountGiven, 
						 (SELECT SUM(fs.Amount)
FROM     dbo.FeeSlipReceipts as fs where fs.FeeSlipId=dbo.FeeSlips.Id 
GROUP BY fs.FeeSlipId) AS Received,  MAX(dbo.FeeSlipReceipts.PaymentMethodId) 
                         AS PaymentMethod, MAX(dbo.FeeSlipReceipts.EntryDate) AS EntryDate,dbo.GetLateFeeAmount(dbo.FeeSlips.Id, dbo.Campuses.ID) AS LateFee
FROM            dbo.Campuses INNER JOIN
                         dbo.Sections INNER JOIN
                         dbo.FeeSlips INNER JOIN
                         dbo.Admissions INNER JOIN
                         dbo.Students ON dbo.Admissions.StudentID = dbo.Students.ID INNER JOIN
                         dbo.ClassSections ON dbo.Admissions.ClassSectionID = dbo.ClassSections.ID ON dbo.FeeSlips.AdmissionId = dbo.Admissions.ID INNER JOIN
                         dbo.FeeGroups INNER JOIN
                         dbo.ClassFeeGroups ON dbo.FeeGroups.ID = dbo.ClassFeeGroups.FeeGroupID INNER JOIN
                         dbo.Classes ON dbo.ClassFeeGroups.ClassID = dbo.Classes.ID ON dbo.ClassSections.ClassID = dbo.Classes.ID ON dbo.Sections.ID = dbo.ClassSections.SectionID ON dbo.Campuses.ID = dbo.Admissions.CampuseID AND 
                         dbo.Campuses.ID = dbo.ClassFeeGroups.CampusID INNER JOIN
                         dbo.FeeSlipDetails ON dbo.FeeSlips.Id = dbo.FeeSlipDetails.FeeSlipId INNER JOIN
                         dbo.FeeTypes ON dbo.FeeSlipDetails.FeeTypeId = dbo.FeeTypes.Id LEFT OUTER JOIN
                         dbo.FeeSlipReceipts ON dbo.FeeSlips.Id = dbo.FeeSlipReceipts.FeeSlipId LEFT OUTER JOIN
                         dbo.FeeDiscounts ON dbo.Students.ID = dbo.FeeDiscounts.StudentId AND dbo.FeeTypes.Id = dbo.FeeDiscounts.FeeTypeId
WHERE        (dbo.Admissions.IsExpell = 0) AND DatePart(month,dbo.FeeSlips.ForMonth)=@month AND DatePart(year,dbo.FeeSlips.ForMonth)=@year AND dbo.Admissions.ID IN  (' + @id + ')
GROUP BY dbo.Admissions.ID,FeeSlips.Id,dbo.Campuses.ID, dbo.Students.RegistrationNo, dbo.Students.StudentName, dbo.Students.FName, dbo.Students.Sex, dbo.Students.Email, dbo.Students.PostalAddress, dbo.Campuses.CampusName, dbo.Classes.ClassName, 
                         dbo.Sections.SectionName, dbo.FeeTypes.TypeName, dbo.FeeSlips.DueDate, dbo.FeeSlipDetails.Amount, dbo.FeeSlipDetails.Discount, dbo.FeeTypes.SortOrder, dbo.FeeSlipDetails.DiscountInAmount, dbo.FeeSlips.ForMonth
						 Having (dbo.GetTotalFeeAmountInSlip(FeeSlips.Id)-SUM(ISNULL(dbo.FeeSlipReceipts.Amount, 0)))>0 order by dbo.Admissions.ID,dbo.FeeTypes.SortOrder
						 '

	execute sp_executesql @statement = @SQL, @parameters = N'@month int,@year int', @month = @month,@year=@year
RETURN";
            await _context.Database.ExecuteSqlRawAsync(proc_FeeSystemProc);

            string proc_FeeSystemProcArrears = @"
CREATE OR ALTER PROCEDURE [dbo].[FeeSystemProcArrears]
(
	@month int,
	@year int,
	@id nvarchar(max)
	)
AS

declare @monthyear varchar(10) = Cast(@month as varchar)+'/'+Cast(@year as varchar);
DECLARE @SQL nvarchar(max);
SET @SQL = 
	'
	SELECT TOP (100) PERCENT dbo.Students.RegistrationNo, dbo.Admissions.ID, dbo.FeeSlips.ForMonth, SUM(dbo.FeeSlipDetails.Amount) - dbo.GetReceviedAmountInSlip(dbo.FeeSlips.Id) + dbo.GetLateFeeAmount(dbo.FeeSlips.Id, 
                  dbo.Admissions.CampuseID) AS Balance
FROM     dbo.FeeSlipDetails INNER JOIN
                  dbo.FeeSlips ON dbo.FeeSlipDetails.FeeSlipId = dbo.FeeSlips.Id INNER JOIN
                  dbo.Admissions ON dbo.FeeSlips.AdmissionId = dbo.Admissions.ID INNER JOIN
                  dbo.Students ON dbo.Admissions.StudentID = dbo.Students.ID 
GROUP BY dbo.FeeSlips.ForMonth, dbo.FeeSlips.Id, dbo.Students.RegistrationNo, dbo.Admissions.ID, dbo.Admissions.CampuseID
HAVING (SUM(dbo.FeeSlipDetails.Amount) - dbo.GetReceviedAmountInSlip(dbo.FeeSlips.Id) > 0) AND (format(dbo.FeeSlips.ForMonth, ''M/yyyy'') <> @monthyear) AND (dbo.Admissions.ID IN (' + @id + '))
ORDER BY dbo.Admissions.ID						 
'

	execute sp_executesql @statement = @SQL, @parameters = N'@monthyear varchar(10)', @monthyear=@monthyear;
RETURN";
            await _context.Database.ExecuteSqlRawAsync(proc_FeeSystemProcArrears);

            string func_GetTransportOfStudents = @"
CREATE OR ALTER FUNCTION [dbo].[GetTransportOfStudents](@StudentId int)  
RETURNS decimal(18,2)   
AS   
BEGIN
Declare @discount decimal(18,2);
Declare @discountInAmount bit;
  SELECT       @discount= Discount,@discountInAmount= DiscountInAmount
FROM            dbo.FeeDiscounts
WHERE        (StudentId = @StudentId)  AND (FeeTypeId = 4)

IF (@discount IS NULL)   
        SET @discount = 0; 
IF (@discountInAmount IS NULL)   
        SET @discountInAmount = 1; 

    DECLARE @ret decimal(18,2);  
    SELECT       @ret= Fare
FROM            dbo.StudentsTransports
WHERE        (IsClosed = 0) and StudentID=@StudentId
 
	 IF (@ret IS NULL)   
        SET @ret = 0; 
		
		IF(@discountInAmount=1)
		Set @ret-=@discount;
		ELSE SET @ret=@ret-(@ret*(@discount/100));
		 
    RETURN @ret;  
END;";
            await _context.Database.ExecuteSqlRawAsync(func_GetTransportOfStudents);

            string func_GetTotalFeeAmountInSlip = @"
CREATE OR ALTER FUNCTION [dbo].[GetTotalFeeAmountInSlip](@slipid int)  
RETURNS decimal(18,2)   
AS   
BEGIN


    DECLARE @ret decimal(18,2);  
SELECT       @ret= SUM(dbo.FeeSlipDetails.Amount)
FROM            dbo.FeeSlips INNER JOIN
                         dbo.FeeSlipDetails ON dbo.FeeSlips.Id = dbo.FeeSlipDetails.FeeSlipId
GROUP BY dbo.FeeSlips.Id
HAVING        (dbo.FeeSlips.Id = @slipid)
	 IF (@ret IS NULL)   
        SET @ret = 0; 
		
		 
    RETURN @ret;  
END;";
            await _context.Database.ExecuteSqlRawAsync(func_GetTotalFeeAmountInSlip);

            string func_GetTotalFeeAmount = @"
CREATE OR ALTER FUNCTION [dbo].[GetTotalFeeAmount](@Admission int,@month int, @year int)  
RETURNS decimal(18,2)   
AS   
BEGIN


    DECLARE @ret decimal(18,2);  
SELECT       @ret= SUM(dbo.FeeSlipDetails.Amount)
FROM            dbo.FeeSlips INNER JOIN
                         dbo.FeeSlipDetails ON dbo.FeeSlips.Id = dbo.FeeSlipDetails.FeeSlipId
GROUP BY dbo.FeeSlips.AdmissionId, dbo.FeeSlips.ForMonth
HAVING        (dbo.FeeSlips.AdmissionId = @Admission) AND (MONTH(dbo.FeeSlips.ForMonth) = @month) AND (YEAR(dbo.FeeSlips.ForMonth) = @year)

	 IF (@ret IS NULL)   
        SET @ret = 0; 
		
		 
    RETURN @ret;  
END;";
            await _context.Database.ExecuteSqlRawAsync(func_GetTotalFeeAmount);

            string func_GetSentMessagesToStudents = @"
CREATE OR ALTER FUNCTION [dbo].[GetSentMessagesToStudents](@CampuId int, @StudentId int,@startDate date, @endDate date)  
RETURNS decimal(18,2)   
AS   
-- Returns the stock level for the product.  
BEGIN  

Declare @discount decimal(18,2);
Declare @discountInAmount bit;
  SELECT       @discount= Discount,@discountInAmount= DiscountInAmount
FROM            dbo.FeeDiscounts
WHERE        (StudentId = @StudentId) AND (FeeTypeId = 6)


IF (@discount IS NULL)   
        SET @discount = 0; 
IF (@discountInAmount IS NULL)   
        SET @discountInAmount = 1;


    DECLARE @ret decimal(18,2);  
    SELECT @ret = Count(m.ID)   
    FROM MessageRecords m   
	Group by m.StudentID, m.EntryDate
    having m.StudentID = @StudentId and m.EntryDate >= @startDate and m.EntryDate<= @endDate;

     Declare @attendance bit;
	 Declare @result bit;
	 Declare @feeRec bit;
	 Declare @feeIss bit;
	 SELECT       @attendance= Attendance, @result=Result, @feeIss=IssuedFee, @feeRec=ReceiveFee
FROM            dbo.ComSubs
WHERE        (StudentID = @StudentId);


	 DECLARE @persmscharges decimal(18,2);
	   
    SELECT @persmscharges = ISNULL(TRY_CONVERT(DECIMAL(16, 2), LTRIM(RTRIM(MAX(dbo.SMSApplicationProperty.PropertyValue)))),0)
                               FROM            dbo.SMSApplicationProperty
                               WHERE        (dbo.SMSApplicationProperty.PropertyName = N'SMSRate' and dbo.SMSApplicationProperty.CampusID=@CampuId)


    DECLARE @attendancecharges decimal(18,2);

	if(@attendance>1)
		SELECT @attendancecharges = ISNULL(TRY_CONVERT(DECIMAL(16, 2), LTRIM(RTRIM(MAX(dbo.SMSApplicationProperty.PropertyValue)))),0) FROM dbo.SMSApplicationProperty  WHERE (dbo.SMSApplicationProperty.PropertyName = N'AttendanceCharges'  and dbo.SMSApplicationProperty.CampusID=@CampuId)
	else
		Set @attendancecharges=0;
	

	DECLARE @resultcharges decimal(18,2);
	 
	 if(@result>1)
		SELECT @resultcharges = ISNULL(TRY_CONVERT(DECIMAL(16, 2), LTRIM(RTRIM(MAX(dbo.SMSApplicationProperty.PropertyValue)))),0) FROM dbo.SMSApplicationProperty  WHERE (dbo.SMSApplicationProperty.PropertyName = N'ResultCharges'  and dbo.SMSApplicationProperty.CampusID=@CampuId)
	else
		Set @resultcharges=0;
	


	DECLARE @feeissuedcharges decimal(18,2); 
	if(@feeIss>1)
		SELECT @feeissuedcharges = ISNULL(TRY_CONVERT(DECIMAL(16, 2), LTRIM(RTRIM(MAX(dbo.SMSApplicationProperty.PropertyValue)))),0) FROM dbo.SMSApplicationProperty  WHERE (dbo.SMSApplicationProperty.PropertyName = N'FeeIssuedCharges'  and dbo.SMSApplicationProperty.CampusID=@CampuId)
	else
		Set @feeissuedcharges=0;
	
	
	DECLARE @feereceivedcharges decimal(18,2); 
	if(@feeRec>1)
		SELECT @feereceivedcharges = ISNULL(TRY_CONVERT(DECIMAL(16, 2), LTRIM(RTRIM(MAX(dbo.SMSApplicationProperty.PropertyValue)))),0) FROM dbo.SMSApplicationProperty  WHERE (dbo.SMSApplicationProperty.PropertyName = N'FeeReceivedCharges'  and dbo.SMSApplicationProperty.CampusID=@CampuId)
	else
		Set @feereceivedcharges=0;
	

IF (@persmscharges IS NULL)   
        SET @persmscharges = 0;  
	 IF (@ret IS NULL)   
        SET @ret = 0;  
		Declare @finalAmount decimal(18,2);
		Set @finalAmount=(@ret*@persmscharges)+@attendancecharges+@resultcharges+@feeissuedcharges+@feereceivedcharges;

		IF(@discountInAmount=1)
		Set @finalAmount-=@discount;
		ELSE SET @finalAmount=@finalAmount-( @finalAmount*(@discount/100));

    RETURN @finalAmount;  
END;";
            await _context.Database.ExecuteSqlRawAsync(func_GetSentMessagesToStudents);

            string func_GetLateFeeAmount = @"
CREATE OR ALTER FUNCTION [dbo].[GetLateFeeAmount](@feeslipid int, @CampuId int)  
RETURNS decimal(18,2)   
AS   
BEGIN

Declare @latefee int;
SELECT @latefee = Id
FROM     dbo.FeeTypes
WHERE  (TypeName = N'Late Fee')

declare @exist int;
select @exist= id from feeslipdetails where feetypeid=@latefee and feeslipid=@feeslipid
    if(@exist>0)
	Return 0;
Declare @duedate datetime;
Declare @lastfine datetime;
Declare @cdate datetime =getdate();
Declare @applyLateFee bit;

select @applyLateFee=s.ApplyLateFee,@duedate = f.DueDate, @lastfine=f.LastFineDate from Feeslips as f,admissions as a, students as s  where a.id=f.admissionid and a.studentid=s.id and f.id = @feeslipid;


if(@duedate>@cdate or @applyLateFee=0)
 RETURN 0;

DECLARE @lateFine decimal(18,2);  
SELECT @lateFine = ISNULL(TRY_CONVERT(DECIMAL(16, 2), LTRIM(RTRIM(MAX(dbo.SMSApplicationProperty.PropertyValue)))),0)
FROM dbo.SMSApplicationProperty  WHERE (dbo.SMSApplicationProperty.PropertyName = N'LateReceivedFeeFine'  and dbo.SMSApplicationProperty.CampusID=@CampuId)
IF (@lateFine IS NULL)   
        RETURN 0;

DECLARE @dailyFine varchar(15);  
SELECT @dailyFine = ISNULL(LTRIM(RTRIM(MAX(dbo.SMSApplicationProperty.PropertyValue))),'False')
FROM dbo.SMSApplicationProperty  WHERE (dbo.SMSApplicationProperty.PropertyName = N'ApplyDailyLateFee'  and dbo.SMSApplicationProperty.CampusID=@CampuId)
IF (@dailyFine IS NULL)   
        SET @dailyFine = 'False';


 
 Declare @diff int = Datediff(day,@duedate,@lastfine);
 if(@diff<=0)
  set  @diff=0;

 
  if(@dailyFine='True')
  RETURN @lateFine * @diff;
  else RETURN @lateFine;

		 
    RETURN @lateFine;  
END;";
            await _context.Database.ExecuteSqlRawAsync(func_GetLateFeeAmount);

            string proc_GetAttandanceFineOfStudents = @"
CREATE OR ALTER FUNCTION [dbo].[GetAttandanceFineOfStudents](@CampuId int,@StudentId int,@startDate date, @endDate date)  
RETURNS decimal(18,2)   
AS   
-- Returns the stock level for the product.  
BEGIN  
    DECLARE @absentees decimal(18,2);  
    SELECT @absentees = COUNT(dbo.StudentAttendences.AdmissionID)
FROM dbo.StudentAttendanceTypes INNER JOIN dbo.StudentAttendences ON dbo.StudentAttendanceTypes.ID = dbo.StudentAttendences.AttendanceTypeID
GROUP BY dbo.StudentAttendanceTypes.AttendanceName, dbo.StudentAttendences.AttendanceDate
HAVING (dbo.StudentAttendanceTypes.AttendanceName = N'Absent') AND (dbo.StudentAttendences.AttendanceDate >= @startDate) AND (dbo.StudentAttendences.AttendanceDate <= @endDate) ;

IF (@absentees IS NULL)   
  SET @absentees = 0;



DECLARE @latecoming decimal(18,2);  
SELECT @latecoming = COUNT(dbo.StudentAttendences.AdmissionID)
FROM dbo.StudentAttendanceTypes INNER JOIN dbo.StudentAttendences ON dbo.StudentAttendanceTypes.ID = dbo.StudentAttendences.AttendanceTypeID
GROUP BY dbo.StudentAttendanceTypes.AttendanceName, dbo.StudentAttendences.AttendanceDate
HAVING (dbo.StudentAttendanceTypes.AttendanceName = N'LateComing') AND (dbo.StudentAttendences.AttendanceDate >= @startDate) AND (dbo.StudentAttendences.AttendanceDate <= @endDate);

IF (@latecoming IS NULL)   
  SET @latecoming = 0;
    
DECLARE @absentFine decimal(18,2);  
SELECT @absentFine = ISNULL(TRY_CONVERT(DECIMAL(16, 2), LTRIM(RTRIM(MAX(dbo.SMSApplicationProperty.PropertyValue)))),0)
FROM dbo.SMSApplicationProperty  WHERE (dbo.SMSApplicationProperty.PropertyName = N'StudentAbsentFine'  and dbo.SMSApplicationProperty.CampusID=@CampuId)
IF (@absentFine IS NULL)   
        SET @absentFine = 0;     

DECLARE @latecomingFine decimal(18,2);  
SELECT @latecomingFine = ISNULL(TRY_CONVERT(DECIMAL(16, 2), LTRIM(RTRIM(MAX(dbo.SMSApplicationProperty.PropertyValue)))),0)
FROM dbo.SMSApplicationProperty WHERE (dbo.SMSApplicationProperty.PropertyName = N'StudentLateComingFine'  and dbo.SMSApplicationProperty.CampusID=@CampuId)
IF (@latecomingFine IS NULL)   
        SET @latecomingFine = 0; 

		declare @finalAmount decimal(18,2);
		Set @finalAmount= (@absentees*@absentFine )+(@latecomingFine*@latecoming)
	 
    RETURN @finalAmount;  
END;";
            await _context.Database.ExecuteSqlRawAsync(proc_GetAttandanceFineOfStudents);

            string func_month_duty = @"
CREATE OR ALTER FUNCTION [dbo].[func_month_duty]
	(
	@id int,
	@atdate date
	)
RETURNS varchar(30)
AS
	BEGIN

	declare @s int ;
	


SELECT  @s=    
sum(dbo.func_duty(Staffid,EmployeeAttendance.attendancedate))  FROM
EmployeeAttendance
Where datepart(month,attendancedate)=datepart(month,@atdate) and StaffID=@id
group by StaffID

declare @time varchar(30)=dbo.seconds_to_time(@s/2)
	RETURN @time
	END";
            await _context.Database.ExecuteSqlRawAsync(func_month_duty);

            string func_duty = @"
CREATE OR ALTER   FUNCTION [dbo].[func_duty]
	(
	@id int,
	@atdate date
	/*
	@parameter1 int = 5,
	@parameter2 datatype
	*/
	)
RETURNS int
AS
	BEGIN
	/*declare @duty int
	*/
	
	declare @intime datetime
	declare @outtime datetime
	set @intime=null;
	set @outtime=null;
	 select @intime=attendancedate from EmployeeAttendance Where staffid=@id and cast(attendancedate as date)=cast(@atdate as date) and attendancetypeid=2 or attendancetypeid=4
	 select @outtime=attendancedate from EmployeeAttendance Where staffid=@id and cast(attendancedate as date)=cast(@atdate as date) and attendancetypeid<>2 and attendancetypeid=5
	 
	  
	declare @s int =datediff(Second, @intime,@outtime)
	

	RETURN @s
	END
";
            await _context.Database.ExecuteSqlRawAsync(func_duty);

            string proc_GetFeeEntryStrucure = @"CREATE OR ALTER PROCEDURE [dbo].[GetFeeEntryStrucure]
	-- Add the parameters for the stored procedure here
	@startDate datetime,
	@endDate datetime,
	@admissionIds varchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	DECLARE @SQL nvarchar(max);

	SET @SQL = 
	'SELECT TOP (100) PERCENT dbo.Admissions.ID AS AdmissionID, 
	dbo.FeeStructures.FeeTypeId, 
	dbo.FeeStructures.Amount, 
	ISNULL(dbo.FeeDiscounts.Discount, 0) AS Discount, 
	ISNULL(dbo.FeeDiscounts.DiscountInAmount, 0) AS DiscountInAmount, 
	CASE WHEN IsNull(dbo.FeeDiscounts.Discount, 0) <> 0 
	THEN 
		CASE WHEN IsNull(dbo.FeeDiscounts.DiscountInAmount, 0) = 1 
		THEN dbo.FeeStructures.Amount - IsNULL(dbo.FeeDiscounts.Discount, 0) 
     ELSE (dbo.FeeStructures.Amount)- (((IsNULL(dbo.FeeDiscounts.Discount, 0)/100) *  (dbo.FeeStructures.Amount))) 
	 END 
	 ELSE dbo.FeeStructures.Amount END AS ActualAmount, 
	 dbo.GetSentMessagesToStudents(dbo.Admissions.CampuseID, dbo.Students.ID, @startDate, @endDate) AS SMSRate, 
	 dbo.GetTransportOfStudents(dbo.Students.ID) AS TransportFare, 
	 dbo.GetAttandanceFineOfStudents(dbo.Admissions.CampuseID, dbo.Students.ID, @startDate, @endDate) AS AttendanceFine
FROM     dbo.FeeTypes INNER JOIN
                  dbo.FeeStructures INNER JOIN
                  dbo.FeeGroups ON dbo.FeeStructures.FeeGroupId = dbo.FeeGroups.ID INNER JOIN
                  dbo.ClassFeeGroups ON dbo.FeeGroups.ID = dbo.ClassFeeGroups.FeeGroupID INNER JOIN
                  dbo.Admissions INNER JOIN
                  dbo.Students ON dbo.Admissions.StudentID = dbo.Students.ID INNER JOIN
                  dbo.ClassSections ON dbo.Admissions.ClassSectionID = dbo.ClassSections.ID INNER JOIN
                  dbo.Classes ON dbo.ClassSections.ClassID = dbo.Classes.ID ON dbo.ClassFeeGroups.ClassID = dbo.Classes.ID ON dbo.FeeTypes.Id = dbo.FeeStructures.FeeTypeId INNER JOIN
                  dbo.Campuses ON dbo.FeeStructures.CampusID = dbo.Campuses.ID AND dbo.ClassFeeGroups.CampusID = dbo.Campuses.ID AND dbo.Admissions.CampuseID = dbo.Campuses.ID AND 
                  dbo.ClassSections.CampusID = dbo.Campuses.ID LEFT OUTER JOIN
                  dbo.FeeDiscounts ON dbo.Students.ID = dbo.FeeDiscounts.StudentId AND dbo.FeeTypes.Id = dbo.FeeDiscounts.FeeTypeId
WHERE  (dbo.FeeStructures.Revised = 0) AND (dbo.Admissions.IsExpell = 0) AND (dbo.Admissions.ID IN (' + @admissionIds + '))
ORDER BY dbo.Admissions.ID'

	execute sp_executesql @statement = @SQL, @parameters = N'@startDate datetime,@endDate datetime', @endDate = @endDate,@startDate=@startDate

END";
            await _context.Database.ExecuteSqlRawAsync(proc_GetFeeEntryStrucure);

            string SP_FeeEntryStructureAmount = @"Create or ALTER      PROCEDURE [dbo].[GetAmountFeeEntryStrucure]
	-- Add the parameters for the stored procedure here
	@admissionIds varchar(max)
AS
BEGIN
  DECLARE @SQL nvarchar(max);

	SET @SQL = 
	'
	SELECT        TOP (100) PERCENT dbo.Students.RegistrationNo,dbo.Classes.ClassName, dbo.Students.StudentName, dbo.Students.FName, dbo.Admissions.ID AS AdmissionID, dbo.FeeStructures.FeeTypeId, dbo.FeeStructures.Amount, 
                         ISNULL(dbo.FeeDiscounts.Discount, 0) AS ODiscount, ISNULL(dbo.FeeDiscounts.DiscountInAmount, 0) AS DiscountInAmount, CASE WHEN IsNull(dbo.FeeDiscounts.Discount, 0) 
                         <> 0 THEN CASE WHEN IsNull(dbo.FeeDiscounts.DiscountInAmount, 0) = 1 THEN IsNULL(dbo.FeeDiscounts.Discount, 0) ELSE (((IsNULL(dbo.FeeDiscounts.Discount, 0) / 100) * (dbo.FeeStructures.Amount))) 
                         END ELSE 0 END AS Discount, CASE WHEN IsNull(dbo.FeeDiscounts.Discount, 0) <> 0 THEN CASE WHEN IsNull(dbo.FeeDiscounts.DiscountInAmount, 0) = 1 THEN dbo.FeeStructures.Amount - IsNULL(dbo.FeeDiscounts.Discount,
                          0) ELSE (dbo.FeeStructures.Amount) - (((IsNULL(dbo.FeeDiscounts.Discount, 0) / 100) * (dbo.FeeStructures.Amount))) END ELSE dbo.FeeStructures.Amount END AS ActualAmount, dbo.GetTransportOfStudents(dbo.Students.ID) 
                         AS TransportFare
FROM            dbo.FeeTypes INNER JOIN
                         dbo.FeeStructures INNER JOIN
                         dbo.FeeGroups ON dbo.FeeStructures.FeeGroupId = dbo.FeeGroups.ID INNER JOIN
                         dbo.ClassFeeGroups ON dbo.FeeGroups.ID = dbo.ClassFeeGroups.FeeGroupID INNER JOIN
                         dbo.Admissions INNER JOIN
                         dbo.Students ON dbo.Admissions.StudentID = dbo.Students.ID INNER JOIN
                         dbo.ClassSections ON dbo.Admissions.ClassSectionID = dbo.ClassSections.ID INNER JOIN
                         dbo.Classes ON dbo.ClassSections.ClassID = dbo.Classes.ID ON dbo.ClassFeeGroups.ClassID = dbo.Classes.ID ON dbo.FeeTypes.Id = dbo.FeeStructures.FeeTypeId INNER JOIN
                         dbo.Campuses ON dbo.FeeStructures.CampusID = dbo.Campuses.ID AND dbo.ClassFeeGroups.CampusID = dbo.Campuses.ID AND dbo.Admissions.CampuseID = dbo.Campuses.ID AND 
                         dbo.ClassSections.CampusID = dbo.Campuses.ID LEFT OUTER JOIN
                         dbo.FeeDiscounts ON dbo.Students.ID = dbo.FeeDiscounts.StudentId AND dbo.FeeTypes.Id = dbo.FeeDiscounts.FeeTypeId
WHERE        (dbo.FeeStructures.Revised = 0) AND (dbo.Admissions.IsExpell = 0) AND (dbo.Admissions.ID IN (' + @admissionIds + '))
ORDER BY AdmissionID'

	execute sp_executesql @statement = @SQL
	
 
END";
            await _context.Database.ExecuteSqlRawAsync(SP_FeeEntryStructureAmount);
            //_context.Database.ExecuteSql(SP_FeeEntryStructureAmount);
            //await _context.Database.ExecuteSqlRawAsync(SP_FeeEntryStructureAmount);
            string func_GetReceviedAmountInSlip = @"CREATE OR ALTER  FUNCTION [dbo].[GetReceviedAmountInSlip](@slipid int)  
RETURNS decimal(18,2)   
AS   
BEGIN


    DECLARE @ret decimal(18,2);  
SELECT       @ret= SUM(dbo.FeeSlipReceipts.Amount)
FROM            
                         dbo.FeeSlipReceipts 
Where        (dbo.FeeSlipReceipts.FeeSlipId = @slipid)
	 IF (@ret IS NULL)   
        SET @ret = 0; 
		
		 
    RETURN @ret;  
END;";
            await _context.Database.ExecuteSqlRawAsync(func_GetReceviedAmountInSlip);
        }


        public List<DynamicMenuAction> GetMenu()
        {
            List<DynamicMenuAction> actions = new List<DynamicMenuAction>();

            actions.Add(new DynamicMenuAction { Controller = "Home", DisplayText = "Dashboard", IsActive = true, Name = "Dashboard", _Type = "menu", SortOrder = 0, Icon = "fas fa-tachometer-alt", CreateMenuItem = true });

            #region Admin Menu
            actions.Add(new DynamicMenuAction { TempId = 1, Controller = "Home", DisplayText = "Admin", IsActive = true, Name = "", _Type = "parent", SortOrder = 1, Icon = "fas fa-user-cog", CreateMenuItem = true });

            actions.Add(new DynamicMenuAction { Controller = "Developer", DisplayText = "Menu Setting", IsActive = true, Name = "DragMenu", CreateMenuItem = true, ParentId = 1, _Type = "menu" });
            actions.Add(new DynamicMenuAction { Controller = "School", DisplayText = "Approve App", IsActive = true, Name = "ApproveApp", CreateMenuItem = true, ParentId = 1, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "School", DisplayText = "Campus Edit", IsActive = true, Name = "CampusEdit", ParentId = 1, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "School", DisplayText = "Create Campus", IsActive = true, Name = "CreateCampus", ParentId = 1, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "School", DisplayText = "Delete Payment method", IsActive = true, Name = "Deletepaymentmethod", ParentId = 1, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "School", DisplayText = "Fee Payment Method", IsActive = true, Name = "feepaymentMethod", CreateMenuItem = true, ParentId = 1, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "School", DisplayText = "Fee Payment Method Edit", IsActive = true, Name = "FeepaymentmethodEdit", ParentId = 1, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "ApplicationConfiguration", DisplayText = "New Academic Session", IsActive = true, Name = "NewAcademicSession", CreateMenuItem = true, ParentId = 1, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "ApplicationConfiguration", DisplayText = "Configurations", IsActive = true, Name = "Create", CreateMenuItem = true, ParentId = 1, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "UploadData", DisplayText = "Upload Data", IsActive = true, Name = "Index", CreateMenuItem = true, ParentId = 1, _Type = "action" });

            actions.Add(new DynamicMenuAction { Controller = "Visitor", DisplayText = "Visitor", IsActive = true, Name = "Index", _Type = "action", ParentId = 1, CreateMenuItem = true });
            actions.Add(new DynamicMenuAction { Controller = "Visitor", DisplayText = "New Visitor", IsActive = true, Name = "Create", _Type = "action", ParentId = 1 });
            actions.Add(new DynamicMenuAction { Controller = "Visitor", DisplayText = "Delete Visitor", IsActive = true, Name = "DeleteVisitor", _Type = "action", ParentId = 1 });
            actions.Add(new DynamicMenuAction { Controller = "Visitor", DisplayText = "Edit Visitor", IsActive = true, Name = "Edit", _Type = "action", ParentId = 1 });

            actions.Add(new DynamicMenuAction { TempId = 101, Icon = "fas fa-user-cog", Controller = "Users", DisplayText = "User Management", IsActive = true, Name = "", CreateMenuItem = true, ParentId = 1, _Type = "parent" });
            actions.Add(new DynamicMenuAction { Controller = "Users", DisplayText = "User", IsActive = true, Name = "Index", CreateMenuItem = true, ParentId = 101, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Users", DisplayText = "Authentications", IsActive = true, Name = "Authentications", CreateMenuItem = true, ParentId = 101, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Users", DisplayText = "Role List", IsActive = true, Name = "RoleList", CreateMenuItem = true, ParentId = 101, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Users", DisplayText = "Create User", IsActive = true, Name = "Create", CreateMenuItem = true, ParentId = 101, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Users", DisplayText = "Create Role", IsActive = true, Name = "CreateRole", CreateMenuItem = false, ParentId = 101, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Users", DisplayText = "Delete User", IsActive = true, Name = "Delete", ParentId = 101, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Users", DisplayText = "Delete Role", IsActive = true, Name = "DeleteRole", ParentId = 101, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Users", DisplayText = "Edit Role", IsActive = true, Name = "EditRole", ParentId = 101, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Users", DisplayText = "User Details", IsActive = true, Name = "UserDetails", ParentId = 101, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Users", DisplayText = "Users In Role", IsActive = true, Name = "UsersInRole", ParentId = 101, _Type = "action" });


            #endregion

            #region Students
            actions.Add(new DynamicMenuAction { TempId = 2, Controller = "Students", DisplayText = "Student", IsActive = true, Name = "", _Type = "parent", SortOrder = 2, Icon = "fas fa-user-edit", CreateMenuItem = true });

            actions.Add(new DynamicMenuAction { Controller = "Students", DisplayText = "Create Student", IsActive = true, Name = "Create", ParentId = 2, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Students", DisplayText = "Delete Student", IsActive = true, Name = "Delete", ParentId = 2, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Students", DisplayText = "Student Details", IsActive = true, Name = "Details", ParentId = 2, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Students", DisplayText = "Edit Student", IsActive = true, Name = "Edit", ParentId = 2, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Students", DisplayText = "Expell Detail", IsActive = true, Name = "ExpellDetail", ParentId = 2, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Students", DisplayText = "Promote Student", IsActive = true, Name = "Promote", ParentId = 2, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Students", DisplayText = "Search Student", IsActive = true, Name = "Search", ParentId = 2, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Students", DisplayText = "Search By contact", IsActive = true, Name = "searchbycontact", ParentId = 2, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Students", DisplayText = "View Students", IsActive = true, Name = "Studentview", ParentId = 2, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Students", DisplayText = "UnExpell Students", IsActive = true, Name = "UnExpell", ParentId = 2, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Students", DisplayText = "Sessionwise Students List", IsActive = true, Name = "SessionWiseSearch", ParentId = 2, CreateMenuItem = true, _Type = "action" });
            #endregion
            #region Fee
            actions.Add(new DynamicMenuAction { TempId = 3, Controller = "Fee", DisplayText = "Fee", IsActive = true, Name = "", _Type = "parent", SortOrder = 3, Icon = "fas fa-dollar-sign", CreateMenuItem = true });

            actions.Add(new DynamicMenuAction { Controller = "Fee", DisplayText = "Class Fee Delete", IsActive = true, Name = "ClassfeeDelete", ParentId = 3, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Fee", DisplayText = "Class Fee ", IsActive = true, Name = "ClassfeeIndex", SortOrder = 0, ParentId = 3, _Type = "action", CreateMenuItem = true });
            actions.Add(new DynamicMenuAction { Controller = "Fee", DisplayText = "Edit Fee Group", IsActive = true, Name = "EditFeeGroup", ParentId = 3, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Fee", DisplayText = "Edit Fee ", IsActive = true, Name = "EditFeeIndex", ParentId = 3, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Fee", DisplayText = "Fee Group Delete", IsActive = true, Name = "FeeGroupDelete", ParentId = 3, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Fee", DisplayText = "Fee Group", IsActive = true, Name = "FeeGroupIndex", ParentId = 3, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Fee", DisplayText = "Fee Slip Detail Delete", IsActive = true, Name = "FeeSlipDetailDelete", ParentId = 3, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Fee", DisplayText = "Fee Slip Detail Edit", IsActive = true, Name = "FeeSlipDetailEdit", ParentId = 3, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Fee", DisplayText = "Fee Slip Receipt Delete", IsActive = true, Name = "FeeSlipReceiptDelete", ParentId = 3, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Fee", DisplayText = "Fee Slip Receipt Edit", IsActive = true, Name = "FeeSlipReceiptEdit", ParentId = 3, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Fee", DisplayText = "Fee Structure Edit", IsActive = true, Name = "FeestructEdit", ParentId = 3, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Fee", DisplayText = "Fee Structure", IsActive = true, Name = "FeestructIndex", ParentId = 3, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Fee", DisplayText = "Fee Type Delete", IsActive = true, Name = "FeeTypeDelete", ParentId = 3, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Fee", DisplayText = "Edit Fee Type", IsActive = true, Name = "FeeTypeEdit", ParentId = 3, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Fee", DisplayText = "Fee Type", IsActive = true, Name = "FeeTypeIndex", ParentId = 3, _Type = "action" });

            actions.Add(new DynamicMenuAction { Controller = "Fee", DisplayText = "Fee Structure", IsActive = true, SortOrder = 1, Name = "FeeStructure", ParentId = 3, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Fee", DisplayText = "Fee Slip", IsActive = true, SortOrder = 2, Name = "FeeSlipIndex", ParentId = 3, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Fee", DisplayText = "Apply Extra Fee", IsActive = true, SortOrder = 3, Name = "DiscountorFine", ParentId = 3, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Fee", DisplayText = "Edit Transport Fee", IsActive = true, SortOrder = 4, Name = "TransportFeeEdit", ParentId = 3, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Fee", DisplayText = "Advance Fee", IsActive = true, SortOrder = 5, Name = "AdvanceFee", ParentId = 3, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Fee", DisplayText = "Receive Fee", IsActive = true, SortOrder = 6, Name = "RecieveFee", ParentId = 3, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Fee", DisplayText = "Receive Fee (By Student)", SortOrder = 7, IsActive = true, Name = "RecieveFeeByIndividualStd", ParentId = 3, CreateMenuItem = true, _Type = "action" });
            #endregion

            #region Exams
            actions.Add(new DynamicMenuAction { TempId = 4, Controller = "Exams", DisplayText = "Exam", IsActive = true, Name = "", _Type = "parent", SortOrder = 4, Icon = "fas fa-tachometer-alt", CreateMenuItem = true });

            actions.Add(new DynamicMenuAction { Controller = "Exams", DisplayText = "New Exam Helds", IsActive = true, Name = "NewExamHelds", ParentId = 4, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Exams", DisplayText = "DateSheet Entry", IsActive = true, Name = "DateSheetEntry", ParentId = 4, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Exams", DisplayText = "Remark Configuration", IsActive = true, Name = "RemarkConfiguration", ParentId = 4, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Exams", DisplayText = "Result missing Student", IsActive = true, Name = "ResultmissingStudent", ParentId = 4, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Results", DisplayText = "Add Exam Remarks", IsActive = true, Name = "AddExamRemarks", ParentId = 4, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Results", DisplayText = "Add/Edit Result", IsActive = true, Name = "BulkResultEntry", ParentId = 4, CreateMenuItem = true, _Type = "action" });
            #endregion

            #region Attendances
            actions.Add(new DynamicMenuAction { TempId = 5, Controller = "Attendances", DisplayText = "Attendance", IsActive = true, Name = "", _Type = "parent", SortOrder = 5, Icon = "fas fa-stopwatch", CreateMenuItem = true });

            actions.Add(new DynamicMenuAction { Controller = "Attendances", DisplayText = "Student Attendance", IsActive = true, Name = "StudentAttendance", ParentId = 5, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Attendances", DisplayText = "Attendance By Class", IsActive = true, Name = "AttendanceByClass", ParentId = 5, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Attendances", DisplayText = "Attendance By Reg Nos", IsActive = true, Name = "AttendanceByRegNos", ParentId = 5, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Attendances", DisplayText = "Attendance By Selected Class", IsActive = true, Name = "AttendanceBySelectedClass", ParentId = 5, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Attendances", DisplayText = "Edit Staff Attendance", IsActive = true, Name = "EditStaffAttendance", ParentId = 5, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Attendances", DisplayText = "Edit Student Attendance", IsActive = true, Name = "EditStudentAttendance", ParentId = 5, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Attendances", DisplayText = "Staff Attendance", IsActive = true, Name = "StaffAttendance", ParentId = 5, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Attendances", DisplayText = "Staff Short Leave", IsActive = true, Name = "StaffShortLeave", ParentId = 5, CreateMenuItem = true, _Type = "action" });
            #endregion

            #region Class Management
            actions.Add(new DynamicMenuAction { TempId = 6, Controller = "ClassManagement", DisplayText = "Class Management", IsActive = true, SortOrder = 6, Name = "", _Type = "parent", Icon = "fas fa-layer-group", CreateMenuItem = true });

            actions.Add(new DynamicMenuAction { Controller = "ClassManagement", DisplayText = "Class Section", IsActive = true, Name = "ClassSection", ParentId = 6, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "ClassManagement", DisplayText = "Class Subject", IsActive = true, Name = "ClassSubject", ParentId = 6, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "ClassManagement", DisplayText = "Change Class Sections", IsActive = true, Name = "changeclasssections", ParentId = 6, CreateMenuItem = true, _Type = "action" });

            #endregion

            #region Hostel
            actions.Add(new DynamicMenuAction { TempId = 7, Controller = "Hostel", DisplayText = "Hostel", IsActive = true, Name = "", _Type = "parent", SortOrder = 7, Icon = "fas fa-building", CreateMenuItem = true });
            actions.Add(new DynamicMenuAction { Controller = "Hostel", DisplayText = "Hostel", IsActive = true, Name = "Index", ParentId = 7, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Hostel", DisplayText = "Hostel Room", IsActive = true, Name = "HostelRoom", ParentId = 7, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Hostel", DisplayText = "Hostel Admission", IsActive = true, Name = "HostelAdmission", ParentId = 7, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Hostel", DisplayText = "Hostel Attendance", IsActive = true, Name = "HostelAttendance", ParentId = 7, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Hostel", DisplayText = "Visitors", IsActive = true, Name = "HostelVisitor", ParentId = 7, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Hostel", DisplayText = "Edit Hostel", IsActive = true, Name = "Edit", ParentId = 7, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Hostel", DisplayText = "Delete Hostel", IsActive = true, Name = "Delete", ParentId = 7, _Type = "action" });

            #endregion


            #region Library
            actions.Add(new DynamicMenuAction { TempId = 8, Controller = "Library", DisplayText = "Library", IsActive = true, Name = "", _Type = "parent", SortOrder = 8, Icon = "fas fa-book-open", CreateMenuItem = true });

            actions.Add(new DynamicMenuAction { Controller = "Library", DisplayText = "Library", IsActive = true, Name = "index", ParentId = 8, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Library", DisplayText = "Book List", IsActive = true, Name = "BookList", ParentId = 8, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Library", DisplayText = "Books Record", IsActive = true, Name = "BooksRecord", ParentId = 8, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Library", DisplayText = "Books Issued To Students", IsActive = true, Name = "BooksIssued", ParentId = 8, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Library", DisplayText = "Books Issued To Staff", IsActive = true, Name = "BooksIssuedToStaff", ParentId = 8, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Library", DisplayText = "Delete Book Issue", IsActive = true, Name = "DeleteBookIssue", ParentId = 8, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Library", DisplayText = "Delete Book List", IsActive = true, Name = "DeleteBookList", ParentId = 8, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Library", DisplayText = "Delete Book Record", IsActive = true, Name = "DeleteBookRecord", ParentId = 8, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Library", DisplayText = "Delete Library", IsActive = true, Name = "DeleteLibrary", ParentId = 8, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Library", DisplayText = "Delete Staff Book List", IsActive = true, Name = "DeleteStaffBookList", ParentId = 8, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Library", DisplayText = "Edit Book Issue", IsActive = true, Name = "EditBookIssue", ParentId = 8, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Library", DisplayText = "Edit Book List", IsActive = true, Name = "EditBookList", ParentId = 8, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Library", DisplayText = "Edit Book Record", IsActive = true, Name = "EditBookRecord", ParentId = 8, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Library", DisplayText = "Edit Library", IsActive = true, Name = "EditLibrary", ParentId = 8, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Library", DisplayText = "Edit Staff Books Issued", IsActive = true, Name = "EditStaffBooksIssued", ParentId = 8, _Type = "action" });

            #endregion

            #region School Schedule
            actions.Add(new DynamicMenuAction { TempId = 9, Controller = "Schedules", DisplayText = "School Schedule", IsActive = true, SortOrder = 9, Name = "", _Type = "parent", Icon = "fas fa-business-time", CreateMenuItem = true });
            actions.Add(new DynamicMenuAction { Controller = "Schedules", DisplayText = "Schedules", IsActive = true, Name = "Index", ParentId = 9, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Schedules", DisplayText = "Holiday", IsActive = true, Name = "Holyday", ParentId = 9, CreateMenuItem = false, _Type = "action" });

            #endregion

            #region Time Table
            actions.Add(new DynamicMenuAction { TempId = 10, Controller = "TimeTable", DisplayText = "Time Table", IsActive = true, Name = "", _Type = "parent", SortOrder = 9, Icon = "far fa-calendar-check", CreateMenuItem = true });
            actions.Add(new DynamicMenuAction { Controller = "TimeTable", DisplayText = "Class Teacher", IsActive = true, Name = "AssignTeacher", ParentId = 10, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "TimeTable", DisplayText = "Teaching Subject", IsActive = true, Name = "TeachingSubject", ParentId = 10, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "TimeTable", DisplayText = "Time Table", IsActive = true, Name = "TimeTableInManual", ParentId = 10, CreateMenuItem = true, _Type = "action" });

            #endregion

            #region Messages
            actions.Add(new DynamicMenuAction { TempId = 11, Controller = "Message", DisplayText = "Message", IsActive = true, Name = "", _Type = "parent", SortOrder = 10, Icon = "fas fa-comment-dots", CreateMenuItem = true });

            actions.Add(new DynamicMenuAction { Controller = "Message", DisplayText = "Message Subscription", IsActive = true, Name = "MessageSubscribtion", ParentId = 11, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Message", DisplayText = "Send Message by Student Name", IsActive = true, Name = "SendMessagebyStudentName", ParentId = 11, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Message", DisplayText = "Send Result", IsActive = true, Name = "SentResult", ParentId = 11, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Message", DisplayText = "Send Complaints", IsActive = true, Name = "SentComplaints", ParentId = 11, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Message", DisplayText = "Send Issued Fee", IsActive = true, Name = "SendmessageIssuedFee", ParentId = 11, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Message", DisplayText = "Send Message To Staff", IsActive = true, Name = "SendMessagetoStaff", ParentId = 11, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Message", DisplayText = "Send Receive Fee", IsActive = true, Name = "SendReceiveFee", ParentId = 11, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Message", DisplayText = "Send Public Message", IsActive = true, Name = "SentPublicmessage", ParentId = 11, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Message", DisplayText = "Send Datesheet", IsActive = true, Name = "SendDateSheet", ParentId = 11, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Message", DisplayText = "Send Attendance", IsActive = true, Name = "SendAttendence", ParentId = 11, CreateMenuItem = true, _Type = "action" });

            #endregion
            #region HR and Payroll
            actions.Add(new DynamicMenuAction { TempId = 12, Controller = "Payroll", DisplayText = "HR", IsActive = true, Name = "", _Type = "parent", SortOrder = 11, Icon = "fas fa-dollar-sign", CreateMenuItem = true });
            actions.Add(new DynamicMenuAction { Controller = "Payroll", DisplayText = "Designation", IsActive = true, Name = "Designation", ParentId = 12, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Payroll", DisplayText = "Employee Creation", IsActive = true, Name = "EmployeeCreation", ParentId = 12, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Payroll", DisplayText = "Employee Register", IsActive = true, Name = "EmployeeRegister", ParentId = 12, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Payroll", DisplayText = "Pay Head", IsActive = true, Name = "Payhead", ParentId = 12, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Payroll", DisplayText = "Salary Package Creation", IsActive = true, Name = "SalaryPackageCreation", ParentId = 12, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Payroll", DisplayText = "Salary Package Register", IsActive = true, Name = "SalaryPackageRegister", ParentId = 12, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Payroll", DisplayText = "Monthly Salary Register", IsActive = true, Name = "MonthlySalaryRegister", ParentId = 12, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Payroll", DisplayText = "Rating Master Entry", IsActive = true, Name = "RatingMasterEntry", ParentId = 12, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Payroll", DisplayText = "Rating Entry", IsActive = true, Name = "RatingEntry", ParentId = 12, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Payroll", DisplayText = "Salary Payment", IsActive = true, Name = "SalaryPayment", ParentId = 12, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Payroll", DisplayText = "Bonus/Deduction", IsActive = true, Name = "BonusDeduction", ParentId = 12, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Payroll", DisplayText = "Salary Payment Register", IsActive = true, Name = "SalaryPaymentRegister", ParentId = 12, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Payroll", DisplayText = "Monthly Salary Setting", IsActive = true, Name = "MonthlySalarySetting", ParentId = 12, CreateMenuItem = true, _Type = "action" });

            #endregion

            #region Reports
            actions.Add(new DynamicMenuAction { TempId = 13, Controller = "ExamsReport", DisplayText = "Report", IsActive = true, Name = "", _Type = "parent", SortOrder = 13, Icon = "fas fa-align-justify", CreateMenuItem = true });

            actions.Add(new DynamicMenuAction { TempId = 1301, Controller = "Reports", DisplayText = "Admin Reports", IsActive = true, Name = "AdminReports", ParentId = 13, CreateMenuItem = true, _Type = "report" });
            actions.Add(new DynamicMenuAction { TempId = 1302, Controller = "Reports", DisplayText = " Fee Reports", IsActive = true, Name = "FeeReports", ParentId = 13, CreateMenuItem = true, _Type = "report" });
            actions.Add(new DynamicMenuAction { TempId = 1303, Controller = "Reports", DisplayText = "Student Reports", IsActive = true, Name = "StudentReports", ParentId = 13, CreateMenuItem = true, _Type = "report" });
            actions.Add(new DynamicMenuAction { TempId = 1304, Controller = "Reports", DisplayText = "Exam Reports", IsActive = true, Name = "ExamReports", ParentId = 13, CreateMenuItem = true, _Type = "report" });
            actions.Add(new DynamicMenuAction { TempId = 1305, Controller = "Reports", DisplayText = "Attendance Reports", IsActive = true, Name = "AttendanceReports", ParentId = 13, CreateMenuItem = true, _Type = "report" });
            actions.Add(new DynamicMenuAction { TempId = 1306, Controller = "Reports", DisplayText = "Payroll Reports", IsActive = true, Name = "PayrollReports", ParentId = 13, CreateMenuItem = true, _Type = "report" });
            actions.Add(new DynamicMenuAction { TempId = 1307, Controller = "Reports", DisplayText = "Accounts Report", IsActive = true, Name = "AccountsReport", ParentId = 13, CreateMenuItem = true, _Type = "report" });
            actions.Add(new DynamicMenuAction { TempId = 1308, Controller = "Reports", DisplayText = "Hostel Reports", IsActive = true, Name = "HostelReports", ParentId = 13, CreateMenuItem = true, _Type = "report" });
            actions.Add(new DynamicMenuAction { TempId = 1309, Controller = "Reports", DisplayText = "Employee Reports", IsActive = true, Name = "EmployeeReports", ParentId = 13, CreateMenuItem = true, _Type = "report" });


            actions.Add(new DynamicMenuAction { Controller = "AdminReports", DisplayText = "Daily Cash Report", IsActive = true, Name = "DailyCashReport", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AdminReports", DisplayText = "Income Map", IsActive = true, Name = "IncomeMap", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AdminReports", DisplayText = "Unwanted Clients", IsActive = true, Name = "UnwantedClients", _Type = "report" });

            actions.Add(new DynamicMenuAction { Controller = "AttendanceReport", DisplayText = "Attendance Record", IsActive = true, Name = "AttendanceRecord", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AttendanceReport", DisplayText = "Attendance Sheet By Class", IsActive = true, Name = "AttendanceSheetByClass", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AttendanceReport", DisplayText = "Attendances On Leave", IsActive = true, Name = "AttendancesOnLeave", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AttendanceReport", DisplayText = "Consecutive Attendance", IsActive = true, Name = "ConsecutiveAttendance", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AttendanceReport", DisplayText = "Daily Attendance Time", IsActive = true, Name = "DailyAttendanceTime", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AttendanceReport", DisplayText = "Daily Attendance Time Veiwer", IsActive = true, Name = "DailyAttendanceTimeVeiwer", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AttendanceReport", DisplayText = "Monthly Attendance", IsActive = true, Name = "MonthlyAttendance", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AttendanceReport", DisplayText = "Monthly Attendance Register", IsActive = true, Name = "MonthlyAttendanceRegister", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AttendanceReport", DisplayText = "Short Leave", IsActive = true, Name = "ShortLeave", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AttendanceReport", DisplayText = "Staff Attendance Record", IsActive = true, Name = "StaffAttendanceRecord", _Type = "report" });


            actions.Add(new DynamicMenuAction { Controller = "PayrollReports", DisplayText = "Employee Card", IsActive = true, Name = "EmployeeCard", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "PayrollReports", DisplayText = "Service Card", IsActive = true, Name = "ServiceCard", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "PayrollReports", DisplayText = "Experience Certificate", IsActive = true, Name = "ExperienceCertificate", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "PayrollReports", DisplayText = "All Employee List", IsActive = true, Name = "AllEmployeeList", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "PayrollReports", DisplayText = "No Objection Certificate", IsActive = true, Name = "NoObjectionCertificate", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "PayrollReports", DisplayText = "Employee Phone List", IsActive = true, Name = "EmployeePhoneList", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "PayrollReports", DisplayText = "Qualification Wise List", IsActive = true, Name = "QualificationWiseList", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "PayrollReports", DisplayText = "Monthly Salary Record", IsActive = true, Name = "MonthlySalaryRecord", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "PayrollReports", DisplayText = "Pay Head Report", IsActive = true, Name = "PayHeadReport", _Type = "report" });

            actions.Add(new DynamicMenuAction { Controller = "ExamsReport", DisplayText = "Date Sheet", IsActive = true, Name = "DateSheet", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "ExamsReport", DisplayText = "MarkSheet", IsActive = true, Name = "MarkSheet", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "ExamsReport", DisplayText = "Result Gazzete", IsActive = true, Name = "ResultGazzete", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "ExamsReport", DisplayText = "Award List", IsActive = true, Name = "AwardList", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "ExamsReport", DisplayText = " Award List blank", IsActive = true, Name = "AwardListblank", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "ExamsReport", DisplayText = "Remarks Sheet", IsActive = true, Name = "RemarksSheet", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "ExamsReport", DisplayText = "Subject List With No Result", IsActive = true, Name = "subjectlistwithnoresult", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "ExamsReport", DisplayText = "Result Summary", IsActive = true, Name = "ResultSummary", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "ExamsReport", DisplayText = "Result Subject Summary", IsActive = true, Name = "ResultSubjectSummary", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "ExamsReport", DisplayText = "Progress Report", IsActive = true, Name = "ProgressReport", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "ExamsReport", DisplayText = "Examination Print", IsActive = true, Name = "ExaminationPrint", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "ExamsReport", DisplayText = "Remarks Entry Sheet", IsActive = true, Name = "RemarksEntrySheet", _Type = "report" });

            actions.Add(new DynamicMenuAction { Controller = "FeeReports", DisplayText = "Fee Slip", IsActive = true, Name = "FeeSlip", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "FeeReports", DisplayText = "Dues Statement", IsActive = true, Name = "DuesStatement", _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "FeeReports", DisplayText = "Received Fee By Users", IsActive = true, Name = "ReceivedFeeByUsers", _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "FeeReports", DisplayText = "ClassWise List Of Fee Group", IsActive = true, Name = "ClassWiseListOfFeeGroup", _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "FeeReports", DisplayText = " Fee Group List", IsActive = true, Name = "FeeGroupList", _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "FeeReports", DisplayText = "Dues Statement Amount", IsActive = true, Name = "DuesStatementAmount", _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "FeeReports", DisplayText = "Fee Record Register", IsActive = true, Name = "FeeRecordRegister", _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "FeeReports", DisplayText = "Exempt From Late Fee", IsActive = true, Name = "ExemptFromLateFee", _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "FeeReports", DisplayText = "Scholarship List", IsActive = true, Name = "SholarshipList", _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "FeeReports", DisplayText = "Fee Due Statement By Class", IsActive = true, Name = "FeeDueStatementByClass", _Type = "report" });

            actions.Add(new DynamicMenuAction { Controller = "HostelReports", DisplayText = "Admission Record", IsActive = true, Name = "AdmissionRecord", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "HostelReports", DisplayText = "Room Record", IsActive = true, Name = "RoomRecord", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "HostelReports", DisplayText = "Vistor Record", IsActive = true, Name = "VistorRecord", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AccountsReport", DisplayText = "Contra Report", IsActive = true, Name = "ContraReport", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AccountsReport", DisplayText = "Payment Report", IsActive = true, Name = "PaymentReport", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AccountsReport", DisplayText = "Receipt Report", IsActive = true, Name = "ReceiptReport", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AccountsReport", DisplayText = "Journal Report", IsActive = true, Name = "JournalReport", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AccountsReport", DisplayText = "PDC Payable Report", IsActive = true, Name = "PDCPayableReport", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AccountsReport", DisplayText = "PDC Receiveable Report", IsActive = true, Name = "PDCReceiveableReport", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AccountsReport", DisplayText = "PDC Clearance Report", IsActive = true, Name = "PDCClearanceReport", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AccountsReport", DisplayText = "Expense Report", IsActive = true, Name = "ExpenseReport", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AccountsReport", DisplayText = "Trial Balance", IsActive = true, Name = "TrialBalance", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AccountsReport", DisplayText = "Working Trial Balance", IsActive = true, Name = "WorkingTrialBalance", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AccountsReport", DisplayText = "Balance Sheet", IsActive = true, Name = "BalanceSheet", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AccountsReport", DisplayText = "Profit And Loss", IsActive = true, Name = "ProfitAndLoss", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AccountsReport", DisplayText = "Cash Flow", IsActive = true, Name = "CashFlow", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AccountsReport", DisplayText = "Fund Flow", IsActive = true, Name = "FundFlow", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "AccountsReport", DisplayText = "Chart Of Account", IsActive = true, Name = "ChartOfAccount", _Type = "report" });

            actions.Add(new DynamicMenuAction { Controller = "PayrollReports", DisplayText = "Employee", IsActive = true, Name = "Employee", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "PayrollReports", DisplayText = "Salary Package", IsActive = true, Name = "SalaryPackage", _Type = "report" });

            actions.Add(new DynamicMenuAction { Controller = "StudentReports", DisplayText = "Class Wise Student Reports", IsActive = true, Name = "ClassWise", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "StudentReports", DisplayText = " Driver Wise", IsActive = true, Name = "DriverWise", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "StudentReports", DisplayText = "Trip Wise", IsActive = true, Name = "TripWise", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "StudentReports", DisplayText = "Phone List", IsActive = true, Name = "PhoneList", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "StudentReports", DisplayText = "Missing Documents", IsActive = true, Name = "MissingDocuments", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "StudentReports", DisplayText = "Parents List", IsActive = true, Name = "ParentsList", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "StudentReports", DisplayText = "Message Subscription", IsActive = true, Name = "SubWiseList", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "StudentReports", DisplayText = " Student ID Card", IsActive = true, Name = "IDCard", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "StudentReports", DisplayText = "Birth Certificate", IsActive = true, Name = "BirthCertificate", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "StudentReports", DisplayText = "Leaving Certificate", IsActive = true, Name = "LeavingCertificate", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "StudentReports", DisplayText = "Student Register", IsActive = true, Name = "StudentRegister", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "StudentReports", DisplayText = "Present Students List", IsActive = true, Name = "PresentStudentsInformation", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "StudentReports", DisplayText = "Class Wise Student List", IsActive = true, Name = "ClassWiseList", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "StudentReports", DisplayText = "Student Identification", IsActive = true, Name = "StudentIdentification", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "StudentReports", DisplayText = "Withdrawn Student List", IsActive = true, Name = "WithdrewStudent", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "StudentReports", DisplayText = "Report By Gender", IsActive = true, Name = "ReportByGender", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "StudentReports", DisplayText = "List Of Free Admissions", IsActive = true, Name = "ListofFreeAdmissions", _Type = "report" });
            actions.Add(new DynamicMenuAction { Controller = "StudentReports", DisplayText = "Orphan List", IsActive = true, Name = "OrphanList", _Type = "report" });


            #endregion


            #region Accounting
            actions.Add(new DynamicMenuAction { TempId = 14, Controller = "Accounts", DisplayText = "Accounts", IsActive = false, Name = "", _Type = "parent", SortOrder = 12, Icon = "fab fa-stripe-s", CreateMenuItem = true });
            actions.Add(new DynamicMenuAction { Controller = "AccountGroup", DisplayText = "Chart of Accounts", SortOrder = 0, IsActive = true, Name = "ChartofAccounts", ParentId = 14, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "AccountGroup", DisplayText = "Account Group", SortOrder = 1, IsActive = true, Name = "AccountGroup", ParentId = 14, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "AccountGroup", DisplayText = "Account Ledgers", SortOrder = 2, IsActive = true, Name = "AccountLedger", ParentId = 14, CreateMenuItem = true, _Type = "action" });

            actions.Add(new DynamicMenuAction { Controller = "Accounts", DisplayText = "Expense Register", SortOrder = 6, IsActive = true, Name = "ExpenseRegister", ParentId = 14, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Accounts", DisplayText = "Expense Entry", IsActive = true, Name = "ExpenseVoucher", ParentId = 14, CreateMenuItem = false, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Accounts", DisplayText = "Bank Reconciliation", SortOrder = 9, IsActive = true, Name = "BankReconciliation", ParentId = 14, CreateMenuItem = true, _Type = "action" });
            //actions.Add(new DynamicMenuAction { Controller = "Accounts", DisplayText = "Debit Note", IsActive = true, Name = "DebitNote", ParentId = 14, CreateMenuItem=true, _Type = "action" });
            //actions.Add(new DynamicMenuAction { Controller = "Accounts", DisplayText = "Credit Note", IsActive = true, Name = "CreditNote", ParentId = 14, CreateMenuItem=true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Accounts", DisplayText = "Payment Register", SortOrder = 4, IsActive = true, Name = "PaymentRegister", ParentId = 14, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Accounts", DisplayText = "Payment Voucher", IsActive = true, Name = "PaymentVoucher", ParentId = 14, CreateMenuItem = false, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Accounts", DisplayText = "Receipt Register", SortOrder = 5, IsActive = true, Name = "ReceiptRegister", ParentId = 14, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Accounts", DisplayText = "Receipt Voucher", IsActive = true, Name = "ReceiptVoucher", ParentId = 14, CreateMenuItem = false, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Accounts", DisplayText = "Journal Register", SortOrder = 3, IsActive = true, Name = "JournalRegister", ParentId = 14, CreateMenuItem = true, _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Accounts", DisplayText = "Journal Voucher", IsActive = true, Name = "JournalVoucher", ParentId = 14, CreateMenuItem = false, _Type = "action" });

            actions.Add(new DynamicMenuAction { Controller = "Budget", DisplayText = "Budget", SortOrder = 7, ParentId = 14, CreateMenuItem = true, IsActive = true, Name = "Index", _Type = "action" });
            actions.Add(new DynamicMenuAction { Controller = "Budget", DisplayText = "Budget Variance", SortOrder = 8, ParentId = 14, CreateMenuItem = true, IsActive = true, Name = "BudgetVariance", _Type = "action" });

            //actions.Add(new DynamicMenuAction { Controller = "AccountTransaction", DisplayText = "Contra Voucher", IsActive = true, Name = "ContraVoucher", ParentId = 14, CreateMenuItem=true, _Type = "action" });
            //actions.Add(new DynamicMenuAction { Controller = "AccountTransaction", DisplayText = "Vendor Pay Bill", IsActive = true, Name = "VendorPayBill", ParentId = 14, CreateMenuItem=true, _Type = "action" });
            //actions.Add(new DynamicMenuAction { Controller = "AccountTransaction", DisplayText = "Contra", IsActive = true, Name = "IndexContra", ParentId = 14, CreateMenuItem=true, _Type = "action" });
            //actions.Add(new DynamicMenuAction { Controller = "AccountTransaction", DisplayText = " Payment", IsActive = true, Name = "IndexPayment", ParentId = 14, CreateMenuItem=true, _Type = "action" });
            //actions.Add(new DynamicMenuAction { Controller = "AccountTransaction", DisplayText = " Receipt", IsActive = true, Name = "IndexReceipt", ParentId = 14, CreateMenuItem=true, _Type = "action" });
            //actions.Add(new DynamicMenuAction { Controller = "AccountTransaction", DisplayText = " Journal", IsActive = true, Name = "IndexJournal", ParentId = 14, CreateMenuItem=true, _Type = "action" });
            //actions.Add(new DynamicMenuAction { Controller = "AccountTransaction", DisplayText = " Vender", IsActive = true, Name = "Indexvender", ParentId = 14, CreateMenuItem=true, _Type = "action" });

            //actions.Add(new DynamicMenuAction { Controller = "PDCAccount", DisplayText = "PDC Receiveable", IsActive = true, Name = "PDCReceiveable", ParentId = 14, CreateMenuItem=true, _Type = "action" });
            //actions.Add(new DynamicMenuAction { Controller = "PDCAccount", DisplayText = "PDC Payable", IsActive = true, Name = "PDCPayable", ParentId = 14, CreateMenuItem=true, _Type = "action" });
            //actions.Add(new DynamicMenuAction { Controller = "PDCAccount", DisplayText = "PDC Clearance", IsActive = true, Name = "PDCClearance", ParentId = 14, CreateMenuItem=true, _Type = "action" });
            //actions.Add(new DynamicMenuAction { Controller = "PDCAccount", DisplayText = " PDC Payable", IsActive = true, Name = "IndexPDCPayable", ParentId = 14, CreateMenuItem=true, _Type = "action" });
            //actions.Add(new DynamicMenuAction { Controller = "PDCAccount", DisplayText = " PDC Receiveable", IsActive = true, Name = "IndexPDCReceiveable", ParentId = 14, CreateMenuItem=true, _Type = "action" });
            //actions.Add(new DynamicMenuAction { Controller = "PDCAccount", DisplayText = "PDC Clearance", IsActive = true, Name = "IndexPDCClearance", ParentId = 14, CreateMenuItem=true, _Type = "action" });

            #endregion
            #region Public Site
            actions.Add(new DynamicMenuAction { TempId = 15, Icon = "fas fa-globe", Controller = "Post", DisplayText = "Public Site", SortOrder = 14, IsActive = true, Name = "", _Type = "parent", CreateMenuItem = true });
            actions.Add(new DynamicMenuAction { Controller = "Post", DisplayText = "Posts", IsActive = true, Name = "Index", _Type = "action", ParentId = 15, CreateMenuItem = true });
            actions.Add(new DynamicMenuAction { Controller = "Post", DisplayText = "New Post", IsActive = true, Name = "CreatePost", _Type = "action", ParentId = 15 });
            actions.Add(new DynamicMenuAction { Controller = "Post", DisplayText = "Delete Post", IsActive = true, Name = "Delete", _Type = "action", ParentId = 15 });
            actions.Add(new DynamicMenuAction { Controller = "Post", DisplayText = "Edit Post", IsActive = true, Name = "PostEdit", _Type = "action", ParentId = 15 });
            actions.Add(new DynamicMenuAction { Controller = "Post", DisplayText = "Pages", IsActive = true, Name = "PageList", _Type = "action", ParentId = 15, CreateMenuItem = true });
            actions.Add(new DynamicMenuAction { Controller = "Post", DisplayText = "New Page", IsActive = true, Name = "CreatePage", _Type = "action", ParentId = 15 });
            actions.Add(new DynamicMenuAction { Controller = "Post", DisplayText = "Delete Page", IsActive = true, Name = "DeletePage", _Type = "action", ParentId = 15 });
            actions.Add(new DynamicMenuAction { Controller = "Post", DisplayText = "Edit Page", IsActive = true, Name = "PageEdit", _Type = "action", ParentId = 15 });

            actions.Add(new DynamicMenuAction { Controller = "Post", DisplayText = "Sliders", IsActive = true, Name = "SliderList", _Type = "action", ParentId = 15, CreateMenuItem = true });
            actions.Add(new DynamicMenuAction { Controller = "Post", DisplayText = "New Slider", IsActive = true, Name = "CreateSlider", _Type = "action", ParentId = 15 });
            actions.Add(new DynamicMenuAction { Controller = "Post", DisplayText = "Delete Slider", IsActive = true, Name = "DeleteSlider", _Type = "action", ParentId = 15 });
            actions.Add(new DynamicMenuAction { Controller = "Post", DisplayText = "Edit Slider", IsActive = true, Name = "SliderEdit", _Type = "action", ParentId = 15 });
            #endregion
            return actions;
        }
    }


    public class DynamicMenuAction
    {
        public int MenuItemId { get; set; }
        public int TempId { get; set; }
        public int ParentId { get; set; }
        public string DisplayText { get; set; }
        public string Controller { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public string _Type { get; set; }
        public bool CreateMenuItem { get; set; }
        public string Icon { get; set; } = "far fa-circle";

    }



}
