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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.AspNetCore.Http.HttpResults;


namespace LRTV.Controllers;
public class PlayerController : Controller
{
    private readonly PlayersContext _context;
    private readonly IPhotoService _photoService;

    public List<PlayerModel>? ListPlayers { get; set; }
    public PlayerModel? playerCurent { get; set; }
    public PlayerController(PlayersContext context, IPhotoService photoService)
    {
        _context = context;
        _photoService = photoService;
    }

    [HttpGet]
    public IActionResult Index()
    {

        ListPlayers = _context.Players.Include(players => players.CurrentTeam).ToList();
        if (ListPlayers == null)
        {
            return RedirectToAction("Error", "Home");
        }
        return View(ListPlayers);
    }

    [HttpGet]
    public IActionResult Player(int playerId)
    {
        playerCurent = _context.Players.Where(players => players.Id == playerId).Include(players => players.CurrentTeam).FirstOrDefault();
        if (playerCurent == null)
        {
            return RedirectToAction("Error", "Home");
        }
        return View(playerCurent);
    }

    [HttpGet]
    public IActionResult AddPlayer()
    {
        var userRole = User?.Claims?.FirstOrDefault(claim => claim.Type == "Role")?.Value ?? "";
        if (User.Identity.IsAuthenticated)
        {
            if (userRole.ToLower() == "member")
            {
                return RedirectToAction("AccessForbidden", "Home");
            }
        }
        else
            return RedirectToAction("AccessForbidden", "Home");

        List<SelectListItem> teams = _context.Teams
            .Select(teams => new SelectListItem { Text = teams.Name, Value = teams.Id.ToString() }).ToList();

        ViewBag.Teams = teams;
        Console.WriteLine(ViewBag.Teams);
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddPlayer(CreatePlayerViewModel newPlayer)
    {
        if (ModelState.IsValid)
        {
            newPlayer.CurrentTeam = _context.Teams.Where(teams => teams.Id == newPlayer.TeamID).FirstOrDefault();
            var result = await _photoService.AddPhotoAsyncPlayers(newPlayer.Image);
            var playerVM = new PlayerModel
            {
                Nickname = newPlayer.Nickname,
                Name = newPlayer.Name,
                Age = newPlayer.Age,
                TeamID = newPlayer.TeamID,
                CurrentTeam = newPlayer.CurrentTeam,
                Achievements = newPlayer.Achievements,
                Rating = newPlayer.Rating,
                Headshots = newPlayer.Headshots,
                KD = newPlayer.KD,
                MapsPlayed = newPlayer.MapsPlayed,
                Image = result.Url.ToString()

            };
            _context.Players.Add(playerVM);
            _context.SaveChanges();
            return RedirectToAction("Index");

        }
        else
        {
            List<SelectListItem> teams = _context.Teams
                .Select(teams => new SelectListItem { Text = teams.Name, Value = teams.Id.ToString() }).ToList();
            ViewBag.Teams = teams;
            ModelState.AddModelError("", "Photo s a dus drq");
        }
        return View(newPlayer);

    }

    //[HttpGet]
    //public IActionResult ModifyPlayer(int playerId)
    //{

    //    List<SelectListItem> teams = _context.Teams
    //        .Select(teams => new SelectListItem { Text = teams.Name, Value = teams.Id.ToString() }).ToList();
    //    ViewBag.Teams = teams;

    //    PlayerModel? players = _context.Players.Where(players => players.Id == playerId).Include(players => players.CurrentTeam).FirstOrDefault();

    //    if (players == null)
    //    {
    //        return RedirectToAction("Error", "Home");
    //    }

    //    return View(players);
    //}


    [HttpPost]
    public async Task<IActionResult> ModifyPlayer(int playerId, ModifyPlayerViewModel playerVM)

    {
        List<SelectListItem> teams = _context.Teams
            .Select(teams => new SelectListItem { Text = teams.Name, Value = teams.Id.ToString() }).ToList();
        PlayerModel? players = _context.Players.Where(players => players.Id == playerId).Include(players => players.CurrentTeam).FirstOrDefault();
        ViewBag.Teams = teams;

        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("", "Failed to modify player");
            return View("ModifyPlayer", playerVM);
        }

       

        if (players != null) { 
        
            try
            {
                await _photoService.DeletPhotoAsync(players.Image);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to modify player again");
                return View(playerVM);
            }
           
        }
        //var photoResult = await _photoService.AddPhotoAsyncPlayers(playerVM.Image);
        Console.Write("poza");
        var player = new PlayerModel
        {
            Nickname = playerVM.Nickname,
            Name = playerVM.Name,
            Age = playerVM.Age,
            TeamID = playerVM.TeamID,
            CurrentTeam = playerVM.CurrentTeam,
            Achievements = playerVM.Achievements,
            Rating = playerVM.Rating,
            Headshots = playerVM.Headshots,
            KD = playerVM.KD,
            MapsPlayed = playerVM.MapsPlayed
            //Image = photoResult.Url.ToString()

        };

        _context.Update(player);
        _context.SaveChanges();
        return View("Player", players);

    }

    [HttpGet]

    public async Task<IActionResult> ModifyPlayer(int playerId)
    {
        var userRole = User?.Claims?.FirstOrDefault(claim => claim.Type == "Role")?.Value ?? "";
        if (User.Identity.IsAuthenticated)
        {
            if (userRole.ToLower() == "member")
            {
                return RedirectToAction("AccessForbidden", "Home");
            }
        }
        else
            return RedirectToAction("AccessForbidden", "Home");
        PlayerModel? player = _context.Players.Where(players => players.Id == playerId).Include(players => players.CurrentTeam).FirstOrDefault();
        List<SelectListItem> teams = _context.Teams
            .Select(teams => new SelectListItem { Text = teams.Name, Value = teams.Id.ToString() }).ToList();
        ViewBag.Teams = teams;
        player.CurrentTeam = _context.Teams.Where(teams => teams.Id == player.TeamID).FirstOrDefault();

        if (player == null)
        {
            return RedirectToAction("Error", "Home");
        }

        var playerVM = new ModifyPlayerViewModel
        {
            Nickname = player.Nickname,
            Name = player.Name,
            Age = player.Age,
            TeamID = player.TeamID,
            CurrentTeam = player.CurrentTeam,
            Achievements = player.Achievements,
            Rating = player.Rating,
            Headshots = player.Headshots,
            KD = player.KD,
            MapsPlayed = player.MapsPlayed,
            Url = player.Image


        };
        return View(playerVM);
    }

    //[HttpPost]
    //public IActionResult ModifyPlayer(PlayerModel players)
    //{
    //    if (!ModelState.IsValid)
    //    {
    //        List<SelectListItem> teams = _context.Teams
    //            .Select(teams => new SelectListItem { Text = teams.Name, Value = teams.Id.ToString() }).ToList();
    //        ViewBag.Teams = teams;
    //        return View(players);
    //    }
    //    players.CurrentTeam = _context.Teams.Where(teams => teams.Id == players.TeamID).FirstOrDefault();
    //    _context.Update(players);
    //    _context.SaveChanges();
    //    return View("Player", players);
    //}

    [HttpGet]
    public IActionResult DeletePlayer(int playerId)
    {
        var userRole = User?.Claims?.FirstOrDefault(claim => claim.Type == "Role")?.Value ?? "";
        if (User.Identity.IsAuthenticated)
        {
            if (userRole.ToLower() == "member")
            {
                return RedirectToAction("AccessForbidden", "Home");
            }
        }
        else
            return RedirectToAction("AccessForbidden", "Home");
        PlayerModel? players = _context.Players.Where(players => players.Id == playerId).Include(players => players.CurrentTeam).FirstOrDefault();
        if (players == null)
        {
            return RedirectToAction("Error", "Home");
        }
        _context.Remove(players);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }


}
