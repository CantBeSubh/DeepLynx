using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business;

public class TimeseriesBusiness : ITimeseriesBusiness
{
    private readonly DeeplynxContext _context;
    
    public TimeseriesBusiness(DeeplynxContext context)
    {
    }
    
    // todo: get methods implemented here

}