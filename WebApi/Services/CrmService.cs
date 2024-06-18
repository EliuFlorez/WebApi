using WebApi.Models;

using System.Collections.Generic;
using System.Linq;
using WebApi.Context;

namespace WebApi.Services
{
    public class CrmService
    {
        private readonly AppDbContext _context;

        public CrmService(AppDbContext context)
        {
            _context = context;
        }

        public Crm GetCrm(int id)
        {
            return _context.Crms.FirstOrDefault(c => c.Id == id);
        }

        public void SaveCrm(Crm crm)
        {
            var existingCrm = _context.Crms.FirstOrDefault(c => c.Id == crm.Id);
            if (existingCrm != null)
            {
                existingCrm.Name = crm.Name;
                existingCrm.AccessToken = crm.AccessToken;
                existingCrm.RefreshToken = crm.RefreshToken;
            }
            else
            {
                _context.Crms.Add(crm);
            }
            _context.SaveChanges();
        }
    }
}
