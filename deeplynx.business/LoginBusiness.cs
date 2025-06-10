using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;


namespace deeplynx.business;

public class LoginBusiness : ILoginBusiness
{
    private readonly DeeplynxContext _context;

    public LoginBusiness(DeeplynxContext context)
    {
        _context = context;
    }

    public async void Login ()
    {
        var httpClient = new HttpClient();
        using HttpResponseMessage response = await httpClient.GetAsync("https://identity-preview.inl.gov/");
        return;
    }
    
}