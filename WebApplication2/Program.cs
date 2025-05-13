using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.SignalR;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// ��������� CORS ��� React-��������
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactClients", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ���������� SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// ���������� CORS
app.UseCors("ReactClients");

// ��������
app.MapHub<VideoHub>("/videoHub");

// �������� ��� �������� ������ �� �������-�����������
app.MapPost("/upload", async (HttpContext context) =>
{
    try
    {
        // ������ ����� �� �������
        using var ms = new MemoryStream();
        await context.Request.Body.CopyToAsync(ms);
        var imageBytes = ms.ToArray();

        // ��������� ����� (������: grayscale)
        var processedBytes = ProcessFrame(imageBytes);

        // �������� ������������� ����� ����� SignalR
        var hubContext = context.RequestServices.GetRequiredService<IHubContext<VideoHub>>();
        await hubContext.Clients.All.SendAsync("ReceiveFrame", Convert.ToBase64String(processedBytes));

        return Results.Ok("Frame processed");
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// �������� ��� ��������� ����
app.MapGet("/", () => "Video Streaming Server");

app.Run();


byte[] ProcessFrame(byte[] imageBytes)
{
    using var image = Image.Load<Rgb24>(imageBytes);
    image.Mutate(x => x.Grayscale()); 

    using var outputStream = new MemoryStream();
    image.SaveAsJpeg(outputStream);
    return outputStream.ToArray();
}

public class VideoHub : Hub { }