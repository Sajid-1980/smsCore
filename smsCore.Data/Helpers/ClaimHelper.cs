using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models;
using Newtonsoft.Json;

namespace smsCore.Data.Helpers
{
    public class AuthorizeForm
    {
        public decimal formId { get; set; }
        public string action { get; set; }
    }

    public class ClaimHelper
    {
        IHttpContextAccessor _context;
        public ClaimHelper(IHttpContextAccessor context)
        {
            _context = context;
        }
        public string GetClaimByKey(string key)
        {
            return _context.HttpContext.User.FindFirstValue(key) ?? "";
        }

        public string[] GetRolesFromClaims()
        {
            return _context.HttpContext.User.FindAll(ClaimTypes.Role).Select(s => s.Value).ToArray();
            //return ClaimsPrincipal.Current?.FindAll().Select(s=>s.Value).ToArray();
        }

        public List<AuthorizeForm> GetAuthenticatedFormFromClaims1()
        {
            var AuthorizedForms = _context.HttpContext.User.FindFirstValue("AuthorizedForms") ?? "";// ClaimsPrincipal.Current?.FindFirst("AuthorizedForms")?.Value;
            var forms = new List<AuthorizeForm>();
            try
            {
                forms = JsonConvert.DeserializeObject<List<AuthorizeForm>>(AuthorizedForms);
            }
            catch (Exception ex)
            {

            }

            return forms;
        }

        public List<decimal> GetAuthenticatedFromIdsFromClaims1()
        {
            var AuthorizedForms = GetAuthenticatedFormFromClaims1();
            var ids = AuthorizedForms.Select(s => s.formId).Distinct().ToList();
            return ids;
        }

        public EnumManager.BasicUserType GetBasicUserTypeFromClaims()
        {
            var claim = _context.HttpContext.User.FindFirst("BasicUserType");// AuthorizedForms ClaimsPrincipal.Current?.FindFirst("");
            return claim == null
                ? EnumManager.BasicUserType.none
                : (EnumManager.BasicUserType)Enum.Parse(typeof(EnumManager.BasicUserType), claim.Value);
        }

        public int[] GetCampusIdFromClaims()
        {
            var claim = _context.HttpContext.User.FindFirst("CampusIds");// ClaimsPrincipal.Current?.FindFirst("");
            if (claim != null && !string.IsNullOrEmpty(claim?.Value))
            {
                var ids = claim.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).DefaultIfEmpty("0").Select(s => int.Parse(s)).ToArray();
                return ids.Length == 1 & ids[0] == 0 ? new int[] { } : ids;
            }
            return new int[] { };
        }

        public decimal GetIdFromClaims()
        {
            var claim = _context.HttpContext.User.FindFirst("primaryId");// ClaimsPrincipal.Current?.FindFirst("");
            if (claim != null) return decimal.Parse(claim.Value);
            return -1;
        }
    }
}