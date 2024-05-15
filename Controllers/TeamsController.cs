﻿using LRTV.ContextModels;
using LRTV.Models;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using LRTV.Interfaces;
using LRTV.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;


namespace LRTV.Controllers;
public class TeamsController : Controller
{
    private readonly PlayersContext _context;
    private readonly IPhotoService _photoService;

    public List<TeamModel>? Teams { get; set; }
    public TeamModel? teamCurent { get; set; }
    public TeamsController(PlayersContext context, IPhotoService photoService)
    {
        _context = context;
        _photoService = photoService;
    }


    [HttpGet]
    public IActionResult Index()
    {

        Teams = _context.Teams.ToList();
        if (Teams == null)
        {
            return RedirectToAction("Error", "Home");
        }
        return View(Teams);
    }

    [HttpGet]
    public IActionResult Team(int teamId)
    {
        teamCurent = _context.Teams.Where(team => team.Id == teamId).FirstOrDefault();
        if (teamCurent == null)
        {
            return RedirectToAction("Error", "Home");
        }
        return View(teamCurent);
    }

    [HttpGet]
    public IActionResult AddTeam()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddTeam(CreateTeamViewModel newTeam)
    {
        if (ModelState.IsValid)
        {
            var result = await _photoService.AddPhotoAsyncTeams(newTeam.Image);
            var teamVM = new TeamModel
            {
                Name = newTeam.Name,
                Ranking = newTeam.Ranking,
                Region = newTeam.Region,
                Image = result.Url.ToString()

            };
            _context.Teams.Add(teamVM);
            _context.SaveChanges();
            return RedirectToAction("Index");

        }
        else
        {
            ModelState.AddModelError("", "Photo s a dus drq");
        }
        return View(newTeam);

    }

    [HttpGet]
    public IActionResult ModifyTeam(int teamId)
    {
        TeamModel? team = _context.Teams.Where(team => team.Id == teamId).FirstOrDefault();

        if (team == null)
        {
            return RedirectToAction("Error", "Home");
        }

        return View(team);
    }

    [HttpPost]
    public IActionResult ModifyTeam(TeamModel team)
    {
        if (!ModelState.IsValid)
        {
            return View(team);
        }
        _context.Update(team);
        _context.SaveChanges();
        return View("Team", team);
    }

    [HttpGet]
    public IActionResult DeleteTeam(int teamId)
    {
        TeamModel? team = _context.Teams.Where(team => team.Id == teamId).FirstOrDefault();
        if (team == null)
        {
            return RedirectToAction("Error", "Home");
        }
        _context.Remove(team);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }


}