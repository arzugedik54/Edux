﻿using Edux.Data;
using Edux.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Edux.ViewComponents
{
    public class FormComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        public FormComponent(ApplicationDbContext context)
        {
            this._context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(Models.Component component)
        {
            var viewName = component.View ?? "Default";
            var formName = component.ParameterValues.FirstOrDefault(f => f.Parameter.Name == "FormName")?.Value;
            if (String.IsNullOrEmpty(Request.Query["returnUrl"].ToString())) {
                ViewBag.ReturnUrl = Request.Path;
            } else
            {
                ViewBag.ReturnUrl = Request.Query["returnUrl"].ToString();
            }
            string mode = Request.Query["mode"].ToString().ToLowerInvariant();
            if (String.IsNullOrEmpty(mode))
            {
                mode = "create";
            }
            ViewBag.Mode = mode;
            var rowId = Request.Query["id"].ToString();
            ViewBag.RowId = rowId;
            ViewBag.Form = await _context.Forms.Include(f => f.Fields).ThenInclude(ff => ff.Property).ThenInclude(d=>d.DataSourceProperty).Include("Fields.Property.PropertyValues").SingleOrDefaultAsync(f => f.Name == formName);
            var entityName = ((Form)ViewBag.Form).EntityName;
            if ((mode == "edit" || mode == "delete") && !String.IsNullOrEmpty(ViewBag.RowId))
            {
                ViewBag.RowValues = await _context.PropertyValues.Include(i => i.Entity).Include(i => i.Property).ThenInclude(d=>d.DataSourceProperty).ThenInclude(t=>t.PropertyValues).Where(p => p.Entity.Name == entityName && p.RowId == Convert.ToInt64(rowId)).OrderBy(r => r.RowId).ToListAsync();
            }
            return await Task.FromResult(View(viewName, component));
        }
    }
}
