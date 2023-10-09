using System;
using System.Net.Http;

namespace MTGArtHarvester.Models;

public class HttpClientFactory
{
    public static readonly HttpClient HttpClient = new HttpClient();
}
