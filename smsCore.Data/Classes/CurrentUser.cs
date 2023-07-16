using System.Security.Claims;
using System.Web;
using Microsoft.AspNetCore.Http;
using Models;
using smsCore.Data.Helpers;

namespace smsCore.Data
{
    public  class CurrentUser
    {
        IHttpContextAccessor _context;
        private SchoolEntities database;
        ClaimHelper ClaimHelper;
        public CurrentUser(IHttpContextAccessor context, SchoolEntities _database, ClaimHelper claimHelper)
        {
            _context = context;
            database = _database;
            ClaimHelper = claimHelper;
        }

        public bool isLoggedIn => _context.HttpContext.User.Identity!=null && _context.HttpContext.User.Identity.IsAuthenticated;

        public  EnumManager.BasicUserType BasicUserType => ClaimHelper.GetBasicUserTypeFromClaims();

        public  string FullName => ClaimHelper.GetClaimByKey("FullName");

        public  string Email => ClaimHelper.GetClaimByKey("Email");

        public  string ProfilePic => ClaimHelper.GetClaimByKey("ProfilePic");

        public  string primaryId => ClaimHelper.GetClaimByKey("primaryId");

        public string? UserID => _context.HttpContext.User.Identity != null ? _context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) : string.Empty;

        public  decimal dec_primaryId => ClaimHelper.GetIdFromClaims();

        public  bool Isautomatic()
        {
            return false;
        }

        public  int[] GetCampusIds()
        {
            return ClaimHelper.GetCampusIdFromClaims();
        }

        public int SelectedCampusId
        {
            get
            {
                if (_context.HttpContext.Request.Cookies.TryGetValue("campusid", out string idstr))
                {
                    if (int.TryParse(idstr, out int id))
                        return id;
                }

                return 0;
            }
        }
    }
}