using System;
using System.Collections.Generic;
using System.Linq;
using GameStore.Data;
using GameStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Controllers
{
    public class GameController : Controller
    {
        public IActionResult Index()
        {
            using(var db = new GameStoreDbContext())
            {
                var game = db.Games.ToList();
                return View(game);
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Game game)
        {
            using (var db = new GameStoreDbContext())
            {
                db.Games.Add(game);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            using(var db = new GameStoreDbContext())
            {
                var idOdGame = db.Games.Find(id);
                return this.View(idOdGame);
            }
        }

        [HttpPost]
        public IActionResult Edit(Game game)
        {
            using(var db = new GameStoreDbContext())
            {
                db.Games.Update(game);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            using (var db = new GameStoreDbContext())
            {
                var idOfGame = db.Games.Find(id);
                return View(idOfGame);
            }
        }

        [HttpPost]
        public IActionResult Delete(Game game)
        {
            using (var db = new GameStoreDbContext())
            {
                db.Remove(game);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}