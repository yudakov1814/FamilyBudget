using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FamilyBudget.Data;
using FamilyBudget.Models;
using FamilyBudget.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace FamilyBudget.Controllers
{
    [Authorize]
    public class FinOperationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        private IdentityUser user { get { return CurrentUser(); } }
        public FinOperationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FinOperations
        public async Task<IActionResult> Index()
        {
            var all_fin_operations = await _context.FinOperations
                .Include(f => f.Category)
                .Include(f => f.Project)
                .Include(f => f.ProjectMember).ToListAsync();

            var viewable_fin_operations = all_fin_operations.Where(x => user.CanView(x, _context)).ToList();
            return View(viewable_fin_operations);
        }

        // GET: FinOperations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var finOperation = await _context.FinOperations
                .Include(f => f.Category)
                .Include(f => f.Project)
                .Include(f => f.ProjectMember)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (finOperation == null)
            {
                return NotFound();
            }

            if (!user.CanView(finOperation, _context))
            {
                return Forbid();
            }

            return View(finOperation);
        }

        // GET: FinOperations/Create
        public IActionResult Create()
        {
            var categories = _context.Categories.ToList()
                .Where(c => user.CanEdit(c, _context)).ToList();

            var projects = _context.Projects.ToList()
                .Where(p => user.CanEdit(p,_context)).ToList();

            var project_members = _context.ProjectMembers.ToList()
                .Where(pm => user.CanEdit(pm, _context)).ToList();

            if (!(categories.Any() && projects.Any() && project_members.Any()))
            {
                return Forbid();
            }

            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name");
            ViewData["ProjectId"] = new SelectList(projects, "Id", "Name");
            ViewData["ProjectMemberId"] = new SelectList(project_members, "Id", "NameInProject");
            return View();
        }

        // POST: FinOperations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FinType,ForAll,Value,CategoryId,ProjectMemberId,ProjectId,CreateTime,UpdateTime")] FinOperation finOperation)
        {
            if (ModelState.IsValid)
            {
                finOperation.CreateTime = DateTime.Now;
                finOperation.UpdateTime = DateTime.Now;
                _context.Add(finOperation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", finOperation.CategoryId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", finOperation.ProjectId);
            ViewData["ProjectMemberId"] = new SelectList(_context.ProjectMembers, "Id", "NameInProject", finOperation.ProjectMemberId);
            return View(finOperation);
        }

        // GET: FinOperations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var finOperation = await _context.FinOperations.FindAsync(id);
            if (finOperation == null)
            {
                return NotFound();
            }

            if (!user.CanEdit(finOperation, _context))
            {
                return Forbid();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", finOperation.CategoryId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", finOperation.ProjectId);
            ViewData["ProjectMemberId"] = new SelectList(_context.ProjectMembers, "Id", "NameInProject", finOperation.ProjectMemberId);
            return View(finOperation);
        }

        // POST: FinOperations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FinType,ForAll,Value,CategoryId,ProjectMemberId,ProjectId,UpdateTime")] FinOperation finOperation)
        {
            if (id != finOperation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    finOperation.UpdateTime = DateTime.Now;
                    _context.Update(finOperation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FinOperationExists(finOperation.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", finOperation.CategoryId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", finOperation.ProjectId);
            ViewData["ProjectMemberId"] = new SelectList(_context.ProjectMembers, "Id", "NameInProject", finOperation.ProjectMemberId);
            return View(finOperation);
        }

        // GET: FinOperations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var finOperation = await _context.FinOperations
                .Include(f => f.Category)
                .Include(f => f.Project)
                .Include(f => f.ProjectMember)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (!user.CanDelete(finOperation, _context))
            {
                return Forbid();
            }

            if (finOperation == null)
            {
                return NotFound();
            }

            return View(finOperation);
        }

        // POST: FinOperations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var finOperation = await _context.FinOperations.FindAsync(id);
            _context.FinOperations.Remove(finOperation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FinOperationExists(int id)
        {
            return _context.FinOperations.Any(e => e.Id == id);
        }
        private IdentityUser CurrentUser()
        {
            var username = HttpContext.User.Identity.Name;
            return _context.Users
                .FirstOrDefault(m => m.UserName == username);
        }
    }
}
