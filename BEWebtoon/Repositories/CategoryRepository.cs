﻿using AutoMapper;
using BEWebtoon.DataTransferObject.CategoriesDto;
using BEWebtoon.Helpers;
using BEWebtoon.Models;
using BEWebtoon.Pagination;
using BEWebtoon.Repositories.Interfaces;
using BEWebtoon.Requests;
using BEWebtoon.WebtoonDBContext;
using IOC.ApplicationLayer.Utilities;
using IOCBEWebtoon.Utilities;
using Microsoft.EntityFrameworkCore;

namespace BEWebtoon.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly WebtoonDbContext _dBContext;
        private readonly IMapper _mapper;
        private readonly SessionManager _sessionManager;
        private readonly IWebHostEnvironment _env;
        public CategoryRepository(WebtoonDbContext dbContext, IMapper mapper, SessionManager sessionManager, IWebHostEnvironment env)
        {
            _dBContext = dbContext;
            _mapper = mapper;
            _sessionManager = sessionManager;
            _env = env;
        }
        public async Task CreateCategory(CreateCategoryDto createCategoryDto)
        {
            if (_sessionManager.CheckRole(ROLE_CONSTANTS.Admin))
            {

                var data = _mapper.Map<Category>(createCategoryDto);
                if (createCategoryDto.File != null && createCategoryDto.File.Length > 0)
                {
                    string fileName = ImageHelper.ImageName(createCategoryDto.CategoryName);
                    string filePath = Path.Combine(_env.ContentRootPath, "wwwroot/resource/category/images", fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        await createCategoryDto.File.CopyToAsync(fileStream);
                    }
                    data.ImagePath = ImageHelper.CategoryImageUri(fileName);
                }
                try
                {
                    await _dBContext.Categories.AddAsync(data);
                    await _dBContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw new CustomException($"Da ton tai the loai nay roi" + ex);
                }
            }
        }

        public async Task DeleteCategory(int id)
        {
            if (_sessionManager.CheckRole(ROLE_CONSTANTS.Admin))
            {
                var category = await _dBContext.Categories.FindAsync(id);
                if (category != null)
                {
                    _dBContext.Categories.Remove(category);
                    await _dBContext.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Khong tim thay the loai nay");
                }
            }
        }

        public async Task<List<CategoryDto>> GetAll()
        {
            if (_sessionManager.CheckRole(ROLE_CONSTANTS.Admin))
            {
                List<CategoryDto> categoriesDto = new List<CategoryDto>();
                var categories = await _dBContext.Categories.ToListAsync();
                if (categories != null)
                {
                    categoriesDto = _mapper.Map<List<Category>, List<CategoryDto>>(categories);

                }
                return categoriesDto;
            }
            return null;
        }

        public async Task<CategoryDto> GetById(int id)
        {
            if (_sessionManager.CheckRole(ROLE_CONSTANTS.Admin))
            {
                var category = await _dBContext.Categories.FindAsync(id);
                if (category != null)
                {

                    CategoryDto categoryDto = _mapper.Map<Category, CategoryDto>(category);
                    return categoryDto;

                }
                else
                {
                    throw new Exception("Khong tim thay the loai nay");
                }
            }
            return null;
        }

        public async Task<PagedResult<CategoryDto>> GetCategoryPagination(SeacrhPagingRequest request)
        {
            
            var query = await _dBContext.Categories.ToListAsync();
            if (!string.IsNullOrEmpty(request.keyword))
                query = query.Where(x => x.CategoryName.ToLower().Contains(request.keyword.ToLower())
                                        || SearchHelper.ConvertToUnSign(x.CategoryName).ToLower().Contains(request.keyword.ToLower())).ToList();
            var items = _mapper.Map<IEnumerable<CategoryDto>>(query);
            return PagedResult<CategoryDto>.ToPagedList(items, request.PageIndex, request.PageSize);
            
        }

        public async Task UpdateCategory(UpdateCategoryDto updateCategoryDto)
        {
            if (_sessionManager.CheckRole(ROLE_CONSTANTS.Admin))
            {
                var category = await _dBContext.Categories.Where(x => x.Id == updateCategoryDto.Id).FirstOrDefaultAsync();
                var data = _mapper.Map<Category>(category);
                if (updateCategoryDto.File != null && updateCategoryDto.File.Length > 0)
                {
                    string oldImageName = ImageHelper.ImageName(category.CategoryName);
                    string oldImagePath = Path.Combine(_env.ContentRootPath, "wwwroot/resource/category/images", oldImageName);

                    if (File.Exists(oldImagePath))
                    {
                        File.Delete(oldImagePath);
                    }
                    string newImageName = ImageHelper.ImageName(updateCategoryDto.CategoryName);
                    string newImagePath = Path.Combine(_env.ContentRootPath, "wwwroot/resource/category/images", newImageName);

                    using (var fileStream = new FileStream(newImagePath, FileMode.Create, FileAccess.Write))
                    {
                        await updateCategoryDto.File.CopyToAsync(fileStream);
                    }
                    data.ImagePath = ImageHelper.CategoryImageUri(newImageName);
                }

                await _dBContext.SaveChangesAsync();
            }
        }
    }
}
