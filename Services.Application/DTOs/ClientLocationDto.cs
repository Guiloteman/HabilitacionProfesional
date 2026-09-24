using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Application.DTOs;

public class ClientLocationDto
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public Guid ProviderId { get; set; }
}

public class ProviderLocationDto
{
    public decimal ProviderLatitude { get; set; }
    public decimal ProviderLongitude { get; set; }
}