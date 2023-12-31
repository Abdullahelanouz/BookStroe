﻿using BookStore.Models;
using BookStore.Models.Repositories;
using BookStore.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BookStore.Controllers
{
    public class BookController : Controller
    {
        private IBookStoreRepository<Book> bookRepository;
        private IBookStoreRepository<Author> authorRepository;
        private readonly IHostingEnvironment hosting;

        public BookController(IBookStoreRepository<Book> bookRepository, IBookStoreRepository<Author> authorRepository, IHostingEnvironment hosting)
        {

            this.bookRepository = bookRepository;
            this.authorRepository = authorRepository;
            this.hosting = hosting;
        }
        // GET: BookController
        public ActionResult Index()
        {
            var books = bookRepository.List();
            return View(books);
        }
     

        // GET: BookController/Details/5
        public ActionResult Details(int id)
        {
            var book = bookRepository.Find(id);
            return View(book);
        }

        // GET: BookController/Create
        public ActionResult Create()
        {
          
            return View(GetAllaithors());
        }

        // POST: BookController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BookAuthorViewModel model)
        {
            if (ModelState.IsValid) 
            {
                try

                {
                    string fileName = UploadFile(model.File) ?? string.Empty;
                   

                    if (model.AuthorId == -1)
                    {

                        ViewBag.Message = "Please Select an author from the list";
                       
                        return View(GetAllaithors());
                    }
                    var author = authorRepository.Find(model.AuthorId);
                    Book book = new Book
                    {
                        Id = model.BookId,
                        title = model.Title,
                        Description = model.Description,
                        Author = author,
                        ImageUrl=fileName
                    };
                    bookRepository.Add(book);
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return View();
                }

            }
          
            ModelState.AddModelError("", "You have to fill all the the require fields");
            return View(GetAllaithors());
        }

        // GET: BookController/Edit/5
        public ActionResult Edit(int id)
        {
            var book =bookRepository.Find(id);
           // var authorId =book.Author == null ? book.Author.Id =0 : book.Author.Id;
            var viewModel = new BookAuthorViewModel
            {
                BookId = book.Id,
                Title = book.title,
                Description = book.Description,
                AuthorId = book.Author.Id,
                Authors =authorRepository.List().ToList(),
                ImageUrl = book.ImageUrl,
            };
            return View(viewModel);
        }

        // POST: BookController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, BookAuthorViewModel viewModel)
        {
            try
            {
                string fileName = UploadFile(viewModel.File,viewModel.ImageUrl);
                var author = authorRepository.Find(viewModel.AuthorId);
                Book book = new Book
                {
                    Id =viewModel.BookId,
                    title = viewModel.Title,
                    Description = viewModel.Description,
                    Author = author,
                    ImageUrl = fileName
                };
                bookRepository.Update(viewModel.BookId, book);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        // GET: BookController/Delete/5
        public ActionResult Delete(int id)
        {
            var book = bookRepository.Find(id);
            return View(book);
        }

        // POST: BookController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult confirmDelete(int id)
        {
            try
            {
                bookRepository.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        List<Author> FillSelectList()
        {
            var authors = authorRepository.List().ToList();
            authors.Insert(0, new Author { Id = -1, FullName = "---- Please Select an author" });
            return authors;
        }

        BookAuthorViewModel GetAllaithors()
        {
            var vmodel = new BookAuthorViewModel
            {
                Authors = FillSelectList()
            };
            return vmodel;
        }
        string UploadFile(IFormFile file)
        {
            if (file != null)
            {

                string uploads = Path.Combine(hosting.WebRootPath, "uploads");
                 string FullPath = Path.Combine(uploads,file.FileName);
                file.CopyTo(new FileStream(FullPath, FileMode.Create));
                return file.FileName;
            }
            return null;
        }
        string UploadFile(IFormFile file, String imgUrl)
        {
            if (file != null)
            {

                string uploads = Path.Combine(hosting.WebRootPath, "uploads");
                string newPath = Path.Combine(uploads, file.FileName);            
                string OldPath = Path.Combine(uploads, imgUrl);

                if (OldPath != newPath)
                {
                    System.IO.File.Delete(OldPath);
                    //Save the new file
                    file.CopyTo(new FileStream(newPath, FileMode.Create));
                }
                return file.FileName;

            }
            return imgUrl;
        }

        public ActionResult Search(string term)
        {
            var result = bookRepository.Search(term);

            return View("Index", result);
        }

    }
}
