using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MyEStore;
using MyEStore.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<MyeStoreContext>(options => {
	options.UseSqlServer(builder.Configuration.GetConnectionString("MyDb"));
});

// 2. Khai bao su dung Session trong Program.cs
// Nhớ là cái này mới khai báo thôi chứ chưa có sử dụng, kéo xuống dưới mới có
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30); //ko config 30p thì tự động là 20p
});

// c. Add cookie authentication cho web
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true; // cái cơ chế giúp tránh sự chênh lệch giờ giấc ở client hay server, chỉnh true sẽ work
        options.LoginPath = "/Customer/Login";
        options.AccessDeniedPath = "/Forbidden/";
    });

// 3. Đăng ký lớp PaymentClient dạng Singleton
builder.Services.AddSingleton(x => new PaypalClient(
    builder.Configuration["PayPalOptions:ClientId"],
    builder.Configuration["PayPalOptions:ClientSecret"],
    builder.Configuration["PayPalOptions:Mode"]
    )
);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 2. Khai bao su dung Session trong Program.cs
// Cái trên là khai báo, cái dưới mới là sử dụng
app.UseSession();

// Chèn trước app.Authorization();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
