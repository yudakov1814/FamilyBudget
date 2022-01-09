﻿using FamilyBudget.Data;
using FamilyBudget.Extensions;
using FamilyBudget.ModalViewModels;
using FamilyBudget.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
            var projectDelete = new ProjectDeleteModel();

            var project = _context.Projects
                .Include(c => c.ProjectMembers)
                .Include(c => c.FinOperations)
                .FirstOrDefault(p => p.Id == id);

            if (project == null)
            {
                projectDelete.IsNotFound = true;
                return PartialView(projectDelete);
            }

            if (!user.CanDelete(project,_context))
            {
                projectDelete.IsForbid = true;
                return PartialView(projectDelete);
            }

            projectDelete.Id                = project.Id;
            projectDelete.Name              = project.Name;
            projectDelete.Owner             = project.Owner;
            projectDelete.CreateTime        = project.CreateTime;
            projectDelete.UpdateTime        = project.UpdateTime;
            projectDelete.membersCount      = project.ProjectMembers.Count();
            projectDelete.incomesCount      = project.FinOperations.Where(fo => fo.FinType == FinType.Income).Count();
            projectDelete.chargesCount      = project.FinOperations.Where(fo => fo.FinType == FinType.Charge).Count();

            return PartialView(projectDelete);
        }

        public IActionResult AddCharge(int? id)
        {
            var addCharge = new AddChargeModel();

            var project = _context.Projects
                .Include(c => c.ProjectMembers)
                .Include(c => c.FinOperations)
                .FirstOrDefault(p => p.Id == id);

            if (project == null)
            {
                addCharge.IsNotFound = true;
                return PartialView(addCharge);
            }

            if (!user.CanEdit(project, _context))
            {
                addCharge.IsForbid = true;
                return PartialView(addCharge);
            }

            addCharge.ForAll = true;
            addCharge.FinType = FinType.Charge;
            addCharge.ProjectId = project.Id;

            ViewData["ProjectMemberId"] = new SelectList(_context.ProjectMembers, "Id", "NameInProject");
            return PartialView(addCharge);
        }

        public IActionResult AddIncome(int? id)
        {
            var addIncome = new AddIncomeModel();

            var project = _context.Projects
                .Include(c => c.ProjectMembers)
                .Include(c => c.FinOperations)
                .FirstOrDefault(p => p.Id == id);

            if (project == null)
            {
                addIncome.IsNotFound = true;
                return PartialView(addIncome);
            }

            if (!user.CanEdit(project, _context))
            {
                addIncome.IsForbid = true;
                return PartialView(addIncome);
            }

            addIncome.ForAll = true;
            addIncome.FinType = FinType.Income;
            addIncome.ProjectId = project.Id;
            addIncome.CategoryId = _context.Categories.FirstOrDefault().Id;

            return PartialView(addIncome);
        }

        private IdentityUser CurrentUser()
        {
            var username = HttpContext.User.Identity.Name;
            return _context.Users
                .FirstOrDefault(m => m.UserName == username);
        }
    }
}
