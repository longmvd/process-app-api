using MISA.PROCESS.BL;
using MISA.PROCESS.Common.DTO;
using MISA.PROCESS.DL;

var builder = WebApplication.CreateBuilder(args);


//Dependency injection
builder.Services.AddScoped(typeof(IBaseBL<>), typeof(BaseBL<>));
builder.Services.AddScoped(typeof(IBaseDL<>), typeof(BaseDL<>));
builder.Services.AddScoped<IUserBL, UserBL>();
builder.Services.AddScoped<IUserDL, UserDL>();
builder.Services.AddScoped<IDepartmentBL, DepartmentBL>();
builder.Services.AddScoped<IDepartmentDL, DepartmentDL>();
builder.Services.AddScoped<IJobPositionBL, JobPositionBL>();
builder.Services.AddScoped<IJobPositionDL, JobPositionDL>();
builder.Services.AddScoped<IRoleBL, RoleBL>();
builder.Services.AddScoped<IRoleDL, RoleDL>();
//Lấy dữ liệu connection string từ appsettings.Development.json 
DatabaseContext.ConnectionString = builder.Configuration.GetConnectionString("MySql");
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

//Bỏ validate tự động
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

    app.UseCors(x => x
            .AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed(origin => true) // allow any origin
            .AllowCredentials());
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
