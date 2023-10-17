using BEWebtoon.WebtoonDBContext;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BEWebtoon.Helpers;
using BEWebtoon.Repositories;
using BEWebtoon.Services;
using BEWebtoon.Repositories.Interfaces;
using BEWebtoon.Services.Interfaces;
var allowSpecificOrigins = "_allowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<WebtoonDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("WebtoonCS"))
);
var autoMapper = new MapperConfiguration(item => item.AddProfile(new AutoMapperProfiles()));
IMapper mapper = autoMapper.CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IRoleRepository, RoleRepository>();
builder.Services.AddTransient<IRoleService, RoleService>();
builder.Services.AddTransient<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddTransient<IUserProfileService, UserProfileService>();
builder.Services.AddTransient<IBookRepository, BookRepository>();
builder.Services.AddTransient<IBookService, BookService>();
builder.Services.AddTransient<IAuthorRepository, AuthorRepository>();
builder.Services.AddTransient<IAuthorService, AuthorService>();
builder.Services.AddTransient<ICategoryRepository, CategoryRepository>();
builder.Services.AddTransient<ICategoryService, CategoryService>();
builder.Services.AddControllers();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<SessionManager>();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
});
var origins = builder.Configuration["AllowedCors"];
if (origins != null)
{
    var allowHost = origins.Split(";");
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: allowSpecificOrigins,
                          policy =>
                          {
                              policy.WithOrigins(allowHost)
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .WithExposedHeaders("x-file-name", "Content-Disposition")
                              .AllowCredentials();
                          });
    });
}
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}*/

app.UseSwagger();

app.UseSwaggerUI();

app.UseSession();

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();