﻿using AutoMapper;
using BEWebtoon.DataTransferObject.BooksDto;
using BEWebtoon.DataTransferObject.CategoriesDto;
using BEWebtoon.Helpers;
using BEWebtoon.Models;
using BEWebtoon.Pagination;
using BEWebtoon.Repositories.Interfaces;
using BEWebtoon.Requests.BookRequest;
using BEWebtoon.WebtoonDBContext;
using IOC.ApplicationLayer.Utilities;
using IOCBEWebtoon.Utilities;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;

namespace BEWebtoon.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly WebtoonDbContext _dBContext;
        private readonly IMapper _mapper;
        private readonly SessionManager _sessionManager;
        private readonly IWebHostEnvironment _env;
        public BookRepository(WebtoonDbContext dbContext, IMapper mapper, SessionManager sessionManager, IWebHostEnvironment env)
        {
            _dBContext = dbContext;
            _mapper = mapper;
            _sessionManager = sessionManager;
            _env = env;
        }
        public async Task CreateBook(CreateBookDto createBookDto)
        {
            if (_sessionManager.CheckRole(ROLE_CONSTANTS.AdminAuthor))
            {
                var data = _mapper.Map<Book>(createBookDto);

                var userId = _sessionManager.GetSessionValueInt("UserId");
                var userProfile = await _dBContext.UserProfiles.Where(x => x.Id == userId).Include(x => x.Authors).FirstOrDefaultAsync();

                if (userProfile != null)
                {
                    var author = await _dBContext.Authors.FindAsync(userProfile.AuthorId);

                    if (author != null)
                    {
                        var bookFollow = new BookFollow
                        {
                            AuthorId = userProfile.AuthorId,
                            Authors = author,
                            Books = data
                        };

                        if (data.BookFollows == null)
                        {
                            data.BookFollows = new List<BookFollow>();
                        }

                        data.BookFollows.Add(bookFollow);
                    }
                }

                data.CategoryBooks = createBookDto.CategoryId.Select(categoryId => new CategoryBook { CategoryId = categoryId }).ToList();
                data.BookFollows = createBookDto.AuthorId.Select(authorId => new BookFollow { AuthorId = authorId }).ToList();
                if (createBookDto.File != null && createBookDto.File.Length > 0)
                {
                    string fileName = ImageHelper.ImageName(createBookDto.Title);
                    string filePath = Path.Combine(_env.ContentRootPath, "wwwroot/resource/book/images", fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await createBookDto.File.CopyToAsync(fileStream);
                    }
                    data.ImagePath = ImageHelper.BookImageUri(fileName);
                }

                try
                {
                    await _dBContext.Books.AddAsync(data);
                    await _dBContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw new CustomException($"Danh muc da ton tai" + ex);
                }
            }
        }
        public async Task DeleteBook(int id)
        {
            if (_sessionManager.CheckRole(ROLE_CONSTANTS.Admin))
            {
                var book = await _dBContext.Books.FindAsync(id);
                if (book != null)
                {
                    _dBContext.Books.Remove(book);
                    await _dBContext.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Khong tim thay sach voi id" + id);
                }
            }
        }

        public async Task<List<BookDto>> GetAll()
        {
            if (_sessionManager.CheckRole(ROLE_CONSTANTS.AdminAuthor))
            {
                List<BookDto> booksDto = new List<BookDto>();
                var books = await _dBContext.Books
                    .Include(x=>x.BookFollows)
                    .ThenInclude(x=>x.Authors)
                    .Include(x => x.CategoryBooks)
                    .ThenInclude(x => x.Categories)
                    .Include(x=>x.Comments)
                    .ThenInclude(x => x.UserProfiles)
                    .ToListAsync();
                if (books != null)
                {
                    booksDto = _mapper.Map<List<Book>, List<BookDto>>(books);
                }
                foreach (var item in booksDto)
                {
                    if (item.ImagePath != null)
                    {
                        if (File.Exists(Path.Combine(item.ImagePath)))
                        {
                            byte[] imageArray = System.IO.File.ReadAllBytes(Path.Combine(item.ImagePath));
                            item.Image = imageArray;
                        }
                        else
                            item.Image = null;
                    }
                    else
                        item.Image = null;
                }
                return booksDto;
            }
            return null;
        }

        public async Task<PagedResult<BookDto>> GetBookPagination(BookRequest request)
        {
            var query = await _dBContext.Books
                    .Include(x => x.BookFollows)
                    .ThenInclude(x => x.Authors)
                    .Include(x => x.CategoryBooks)
                    .ThenInclude(x => x.Categories)
                    .Include(x => x.Comments)
                             .ThenInclude(x => x.UserProfiles)
                             .ToListAsync();
            if (!string.IsNullOrEmpty(request.keyword.TrimAndLower()))
                query = query.Where(x => x.Title.ToLower().Contains(request.keyword.ToLower())
                                        || SearchHelper.ConvertToUnSign(x.Title).ToLower().Contains(request.keyword.ToLower())).ToList();
            if (!string.IsNullOrEmpty(request.AuthorName.TrimAndLower()))
            {
                query = query.Where(b => b.BookFollows.Any(bf => bf.Authors.AuthorName.TrimAndLower().Contains(request.AuthorName.TrimAndLower())
                                                           || SearchHelper.ConvertToUnSign(bf.Authors.AuthorName).TrimAndLower().Contains(request.AuthorName.TrimAndLower()))).ToList();
            }
            if (!string.IsNullOrEmpty(request.CategoryName.TrimAndLower()))
            {
                query = query.Where(b => b.CategoryBooks.Any(bf => bf.Categories.CategoryName.TrimAndLower().Contains(request.CategoryName.TrimAndLower())
                                                           || SearchHelper.ConvertToUnSign(bf.Categories.CategoryName).TrimAndLower().Contains(request.CategoryName.TrimAndLower()))).ToList();
            }
            var items = _mapper.Map<IEnumerable<BookDto>>(query);
            foreach (var item in items)
            {
                if (item.ImagePath != null)
                {
                    if (File.Exists(Path.Combine(item.ImagePath)))
                    {
                        byte[] imageArray = System.IO.File.ReadAllBytes(Path.Combine(item.ImagePath));
                        item.Image = imageArray;
                    }
                    else
                        item.Image = null;
                }
                else
                    item.Image = null;
            }
            return PagedResult<BookDto>.ToPagedList(items, request.PageIndex, request.PageSize);
        }

        public async Task<BookDto> GetById(int id)
        {
                var book = await _dBContext.Books
                         .Include(b => b.BookFollows)
                             .ThenInclude(bf => bf.Authors)
                         .Include(b => b.CategoryBooks)
                             .ThenInclude(cb => cb.Categories)
                         .Include(x => x.Comments)
                             .ThenInclude(x => x.UserProfiles)
                         .FirstOrDefaultAsync(b => b.Id == id);
                if (book != null)
                {

                BookDto bookDto = _mapper.Map<Book, BookDto>(book);
                if (book.ImagePath != null)
                {
                    if (File.Exists(Path.Combine(book.ImagePath)))
                    {
                        byte[] imageArray = System.IO.File.ReadAllBytes(Path.Combine(book.ImagePath));
                        bookDto.Image = imageArray;
                    }
                    else
                        bookDto.Image = null;
                }
                else
                    bookDto.Image = null;
                return bookDto;

                }
                else
                {
                    throw new Exception("Khong tim thay sach");
                }
        }

        public async Task UpdateBook(UpdateBookDto updateBookDto)
        {
            if (_sessionManager.CheckRole(ROLE_CONSTANTS.AdminAuthor))
            {
                var book = await _dBContext.Books.Where(x => x.Id == updateBookDto.Id).Include(a => a.CategoryBooks).Include(w => w.BookFollows).FirstOrDefaultAsync();
                var data = _mapper.Map<Book>(book);
                if (book != null)
                {
                    data.CategoryBooks = updateBookDto.CategoryId.Select(categoryId => new CategoryBook { CategoryId = categoryId }).ToList();
                    data.BookFollows = updateBookDto.AuthorId.Select(authorId => new BookFollow { AuthorId = authorId }).ToList();
                    data.Title = updateBookDto.Title;
                    data.Description = updateBookDto.Description;
                    data.Content = updateBookDto.Content;
                    data.Status = updateBookDto.Status;

                    if (updateBookDto.File != null && updateBookDto.File.Length > 0)
                    {
                        string oldImageName = ImageHelper.ImageName(book.Title);
                        string oldImagePath = Path.Combine(_env.ContentRootPath, "wwwroot/resource/book/images", oldImageName);
                        if (File.Exists(oldImagePath))
                        {
                            File.Delete(oldImagePath);
                        }
                        string newImageName = ImageHelper.ImageName(updateBookDto.Title);
                        string newImagePath = Path.Combine(_env.ContentRootPath, "wwwroot/resource/book/images", newImageName);
                        using (var fileStream = new FileStream(newImagePath, FileMode.Create, FileAccess.Write))
                        {
                            await updateBookDto.File.CopyToAsync(fileStream);
                        }
                        data.ImagePath = ImageHelper.BookImageUri(newImageName);
                    }
                    await _dBContext.SaveChangesAsync();
                }
            }
        }
        private async Task ProcessBookData(Book book, CreateOrUpdateBookDto bookDto)
        {
            book.CategoryBooks = bookDto.CategoryId.Select(categoryId => new CategoryBook { CategoryId = categoryId }).ToList();
            book.BookFollows = bookDto.AuthorId.Select(authorId => new BookFollow { AuthorId = authorId }).ToList();

            if (bookDto.File != null && bookDto.File.Length > 0)
            {
                if (bookDto.ImagePath != null)
                {
                    if (File.Exists(Path.Combine(bookDto.ImagePath)))
                        File.Delete(Path.Combine(bookDto.ImagePath));
                }
                book.ImagePath = await FileHelper.SaveFile(bookDto.File, "BookImage");
            }
        }

    }
}
