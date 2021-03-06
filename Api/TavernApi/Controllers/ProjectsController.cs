﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TavernApi.Databases;
using TavernApi.Helpers;
using TavernApi.Models;

namespace TavernApi.Controllers
{
  [ApiController]
  [Route("api/projects")]
  public class ProjectsController : TavernController
  {
    public ProjectsController(TavernContext context) : base(context)
    {}

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<ProjectDTO>> GetProject(long id)
    {
      var project = await _context.Projects.FindAsync(id);

      if (project == null)
        return await Task.FromResult(new BadRequestResult());

      return await Task.FromResult(new OkObjectResult(new ProjectDTO(project)));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjects(int pageSize, int page = 1)
    {
      var projects = _context.Projects.GetPaged(page, pageSize);

      return await Task.FromResult(new OkObjectResult(projects.Select(p => new ProjectDTO(p))));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ProjectDTO>> CreateProject([FromBody]ProjectDCO model)
    {
      var category = await _context.Categories.FindAsync(model.CategoryId);
      if (category == null)
        return await Task.FromResult(new BadRequestResult());

      var creator = await GetLoggedUser();
      if(creator == null)
        return await Task.FromResult(new BadRequestResult());

      var functions = new List<ProjectFunction>();
      foreach(var funId in model.FunctionIds)
      {
        var fun = await _context.Functions.FindAsync(funId);
        if (fun == null)
          return await Task.FromResult(new BadRequestResult());

        var cross = new ProjectFunction
        {
          Function = fun,
          FunctionId = funId
        };

        functions.Add(cross);
      }

      var project = new Project
      {
        Title = model.Title,
        Category = category,
        Creator = creator,
        CreationTimeStamp = DateTime.Now,
        Description = model.Description,
        Functions = functions
      };

      await _context.Projects.AddAsync(project);
      await _context.SaveChangesAsync();
      return await Task.FromResult(new OkObjectResult(new ProjectDTO(project)));
    }

  }
}
