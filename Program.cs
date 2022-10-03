using Microsoft.AspNetCore.Mvc;
using RestSharp;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using Zoom_API_Backend.Models;
using Zoom_Meeting.Models;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod();
                      });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);

app.MapPost("/login", async (String code, IConfiguration configuration) =>
{

    RestClient restClient = new RestClient("https://zoom.us/oauth/token");
    RestRequest request = new RestRequest();

    request.AddQueryParameter("grant_type", "authorization_code");
    request.AddQueryParameter("code", code);
    request.AddQueryParameter("redirect_uri", "http://localhost:5173/login");
    request.AddHeader("Authorization", "Basic xxx");
    request.AddHeader("Content-Type", "application/x-www-form-urlencode");
    var response = restClient.ExecutePostAsync(request);
    var token = response.Result.Content;
    System.IO.File.WriteAllText("D:/Webdev/Teknorix/Zoom-API-Backend/OAuthToken.json", token);
    return StatusCodes.Status200OK;
});

//GET Meetings

app.MapGet("/getmeetings", (IConfiguration configuration) =>
{
    RestClient restClient = new RestClient("https://api.zoom.us/v2/users/me/meetings");
    RestRequest request = new RestRequest();

    var token = System.IO.File.ReadAllText("D:/Webdev/Teknorix/Zoom-API-Backend/OAuthToken.json");
    var jsonToken = JsonSerializer.Deserialize<Token>(token);
    var access_token = jsonToken.access_token;

    request.AddHeader("Authorization", "Bearer " + access_token);
    var response = restClient.ExecuteGetAsync(request);
    return response.Result.Content;
});


//CREATE Meetings
app.MapPost("/createmeeting", (IConfiguration configuration,Meeting meeting) =>
{
    RestClient restClient = new RestClient("https://api.zoom.us/v2/users/me/meetings");
    RestRequest request = new RestRequest();
   
    var token = System.IO.File.ReadAllText("D:/Webdev/Teknorix/Zoom-API-Backend/OAuthToken.json");
    var jsonToken = JsonSerializer.Deserialize<Token>(token);
    var access_token = jsonToken.access_token;
    request.AddHeader("Content-Type", "application/json");
    request.AddHeader("Authorization", "Bearer " + access_token);

    var meetingModel = new JsonObject();    
    meetingModel["topic"] = meeting.Topic;
    meetingModel["agenda"] = meeting.Agenda;
    meetingModel["start_time"] = meeting.Date;
    meetingModel["duration"] = meeting.Duration;
    var model = JsonSerializer.Serialize(meetingModel);


    request.AddParameter("application/json", model, ParameterType.RequestBody);

    var response = restClient.ExecutePostAsync(request);
    return response.Result.Content;
});

//GET Meetings
app.MapDelete("/deletemeet", (IConfiguration configuration,long id) =>
{
    RestClient restClient = new ($"https://api.zoom.us/v2/users/me/meetings/{id}");
    RestRequest request = new ();

    var token = System.IO.File.ReadAllText("D:/Webdev/Teknorix/Zoom-API-Backend/OAuthToken.json");
    var jsonToken = JsonSerializer.Deserialize<Token>(token);
    var access_token = jsonToken.access_token;

    request.AddHeader("Authorization", "Bearer " + access_token);
    var response = restClient.Delete(request);
    return response.Content ;
});

app.Run();
