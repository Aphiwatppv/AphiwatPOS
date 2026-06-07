using AccessEngine.Services;
using AphiwatPOS.Services;
using AuthenticationEngine.Services;
using CustomerEngine.Services;
using InventoryEngine.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using ProductEngine.Services;
using SalesEngine.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWindowsService();

// Add services to the container.
builder.Services
    .AddRazorPages(options =>
    {
        options.Conventions.AuthorizeFolder("/");
        options.Conventions.AllowAnonymousToPage("/Account/Login");
        options.Conventions.AllowAnonymousToPage("/Error");
        options.Conventions.AllowAnonymousToPage("/Sales/CustomerDisplay/Index");
        options.Conventions.AuthorizeFolder("/Users", "AdminOnly");
        options.Conventions.AuthorizeFolder("/Products", "ProductManage");
        options.Conventions.AuthorizeFolder("/Inventory", "InventoryManage");
        options.Conventions.AuthorizeFolder("/Customer", "CustomerManage");
        options.Conventions.AuthorizeFolder("/Customers", "CustomerManage");
        options.Conventions.AuthorizePage("/Sales/DailyClosing/Index", "ReportView");
        options.Conventions.AuthorizeFolder("/Employees", "AdminOnly");
    });

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "AphiwatPOS.Auth";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(10);
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ProductManage", policy => policy.RequireAssertion(context =>
        context.User.IsInRole("Admin") || context.User.HasClaim("Permission", "PRODUCT_MANAGE")));
    options.AddPolicy("InventoryManage", policy => policy.RequireAssertion(context =>
        context.User.IsInRole("Admin") || context.User.HasClaim(claim => claim.Type == "Permission" && claim.Value.StartsWith("INVENTORY_", StringComparison.OrdinalIgnoreCase))));
    options.AddPolicy("CustomerManage", policy => policy.RequireAssertion(context =>
        context.User.IsInRole("Admin") || context.User.HasClaim("Permission", "CUSTOMER_MANAGE")));
    options.AddPolicy("SalesManage", policy => policy.RequireAssertion(context =>
        context.User.IsInRole("Admin") || context.User.HasClaim(claim => claim.Type == "Permission" && claim.Value.StartsWith("SALES_", StringComparison.OrdinalIgnoreCase))));
    options.AddPolicy("ReportView", policy => policy.RequireAssertion(context =>
        context.User.IsInRole("Admin") || context.User.HasClaim("Permission", "REPORT_VIEW") || context.User.HasClaim("Permission", "CASH_CLOSING")));
});

builder.Services.Configure<PromptPayOptions>(builder.Configuration.GetSection("PromptPay"));
builder.Services.Configure<ReceiptPrinterOptions>(builder.Configuration.GetSection("ReceiptPrinter"));
builder.Services.AddSignalR();
builder.Services.AddSingleton<IPromptPayQrService, PromptPayQrService>();
builder.Services.AddScoped<IAccessService, AccessService>();
builder.Services.AddScoped<AuthenticationEngine.Services.IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
builder.Services.AddScoped<IProductBrandService, ProductBrandService>();
builder.Services.AddScoped<IProductUnitService, ProductUnitService>();
builder.Services.AddScoped<IProductUnitConversionService, ProductUnitConversionService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductPriceHistoryService, ProductPriceHistoryService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICustomerMemberTypeService, CustomerMemberTypeService>();
builder.Services.AddScoped<ICustomerRegistrationService, CustomerRegistrationService>();
builder.Services.AddScoped<IMemberLevelService, MemberLevelService>();
builder.Services.AddScoped<IMemberLevelUpgradeRuleService, MemberLevelUpgradeRuleService>();
builder.Services.AddScoped<ILoyaltyPointService, LoyaltyPointService>();
builder.Services.AddScoped<IMemberLoyaltyService, MemberLoyaltyService>();
builder.Services.AddScoped<IRubberPurchaseService, RubberPurchaseService>();
builder.Services.AddScoped<IRubberPriceService, RubberPriceService>();
builder.Services.AddScoped<IRubberAuctionLocationService, RubberAuctionLocationService>();
builder.Services.AddScoped<ICustomerCreditService, CustomerCreditService>();
builder.Services.AddScoped<IMemberSalesCreditService, MemberSalesCreditService>();
builder.Services.AddScoped<ICustomerHistoryService, CustomerHistoryService>();
builder.Services.AddScoped<ICustomerReportService, CustomerReportService>();
builder.Services.AddScoped<ICustomerAuditService, CustomerAuditService>();
builder.Services.AddScoped<IInventoryDashboardService, InventoryDashboardService>();
builder.Services.AddScoped<IInventoryLocationService, InventoryLocationService>();
builder.Services.AddScoped<IInventoryStockService, InventoryStockService>();
builder.Services.AddScoped<IInventoryMovementService, InventoryMovementService>();
builder.Services.AddScoped<IStockAdjustmentService, StockAdjustmentService>();
builder.Services.AddScoped<IStockCountService, StockCountService>();
builder.Services.AddScoped<IStockTransferService, StockTransferService>();
builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();
builder.Services.AddScoped<ISalesCheckoutService, SalesCheckoutService>();
builder.Services.AddScoped<ISalesHistoryService, SalesHistoryService>();
builder.Services.AddScoped<ISalesClosingService, SalesClosingService>();
builder.Services.AddScoped<IHeldSaleService, HeldSaleService>();
builder.Services.AddScoped<ISalesReturnService, SalesReturnService>();
builder.Services.AddScoped<IReceiptService, ReceiptService>();
builder.Services.AddScoped<IReceiptPrinterService, ReceiptPrinterService>();
builder.Services.AddScoped<ICashDrawerService, CashDrawerService>();
builder.Services.AddScoped<IWholesalePosService, WholesalePosService>();
builder.Services.AddScoped<IManagerDashboardService, ManagerDashboardService>();
builder.Services.AddScoped<ICashierDashboardService, CashierDashboardService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<PosDisplayHub>("/pos-display-hub");

app.Run();
