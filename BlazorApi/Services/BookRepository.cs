﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorApi.Contracts;
using BlazorApi.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorApi.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class BookRepository : IBookRepository
    {
        private readonly ApplicationDbContext _db;

        public BookRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IList<Book>> FindAll()
        {
          var books = await _db.Books.ToListAsync();
          return books;
        }

        public async Task<Book> FindById(int Id)
        {
            var book = await _db.Books.FindAsync(Id);
            return book;
        }

        public async Task<bool> IsExists(int Id)
        {
            return await _db.Books.AnyAsync(q => q.Id == Id);
        }

        public async Task<bool> Create(Book entity)
        {
            await _db.Books.AddAsync(entity);
            return await Save();
        }

        public async Task<bool> Update(Book entity)
        {
             _db.Books.Update(entity);
            return await Save();
        }

        public async Task<bool> Delete(Book entity)
        {
            _db.Books.Remove(entity);
            return await Save();
        }

        public async Task<bool> Save()
        {
            var changes = await _db.SaveChangesAsync();
            return changes > 0;

        }
    }
}
