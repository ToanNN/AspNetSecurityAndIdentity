using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using Microsoft.AspNetCore.Authentication.Negotiate;

var builder = WebApplication.CreateBuilder(args);

// NegotiateDefaults.AuthenticationScheme specifies Kerberos because it's the default.

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate(options =>
       {
           if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
           {
               //Some configurations may require specific credentials to query the LDAP domain
               options.EnableLdap(settings =>
               {
                   settings.Domain = "contoso.com";
                   settings.MachineAccountName = "unsa";
                   settings.MachineAccountPassword = builder.Configuration["LDAP:Password"];
               });
           }
       }
   );

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});
builder.Services.AddRazorPages();

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

// Run impersonated user
app.Run(async (context) =>
{
    try
    {
        var user = (WindowsIdentity)context.User.Identity;
        await context.Response.WriteAsync($"User: {user.Name} \tState: {user.ImpersonationLevel}");

        await WindowsIdentity.RunImpersonatedAsync(user.AccessToken, async () =>
        {
            var impersonatedUser = WindowsIdentity.GetCurrent();
            var message =
                $"User: {impersonatedUser.Name}\t" +
                $"State: {impersonatedUser.ImpersonationLevel}";

            var bytes = Encoding.UTF8.GetBytes(message);
            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
        });

    }
    catch (Exception exception)
    {
        await context.Response.WriteAsync(exception.ToString());
    }
});
