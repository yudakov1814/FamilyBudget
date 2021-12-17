﻿using FamilyBudget.Data;
using FamilyBudget.Extensions;
using FamilyBudget.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FamilyBudget.Controllers
{
    public class ModalController : Controller
    {

        private readonly ApplicationDbContext _context;

        private IdentityUser user { get { return CurrentUser(); } }
        public ModalController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult ProjectDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = _context.Projects
                .FirstOrDefault(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            if (!user.CanDelete(project,_context))
            {
                return Forbid();
            }

            ViewBag.membersCount = project.ProjectMembers.Count();
            ViewBag.incomesCount = project.FinOperations.Where(fo => fo.FinType == FinType.Income).Count();
            ViewBag.chargesCount = project.FinOperations.Where(fo => fo.FinType == FinType.Charge).Count();

            return PartialView(project);
        }

        private IdentityUser CurrentUser()
        {
            var username = HttpContext.User.Identity.Name;
            return _context.Users
                .FirstOrDefault(m => m.UserName == username);
        }
    }
}
