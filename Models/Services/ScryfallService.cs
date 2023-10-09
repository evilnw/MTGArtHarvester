using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using MTGArtHarvester.Entities;

namespace MTGArtHarvester.Models.Services;

public class ScryfallService
{
    public static ConcurrentDictionary<string, Bitmap> _cachedImageItems = new(25, 50);
    
    private static DelayCalculator _DelayRequestCalc = new DelayCalculator(130);

    private const string _UrlApi = @"https://api.scryfall.com";

    private readonly HttpClient _httpClient;

    public ScryfallService()
        => _httpClient = HttpClientFactory.HttpClient;
    
    public async Task<IEnumerable<MtgCard>> SearchCardsAsync(string query, CancellationToken cancellationToken)
    {
        if (String.IsNullOrEmpty(query))
        {
            return Array.Empty<MtgCard>();
        }
        
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, @$"{_UrlApi}/cards/search?q={query}&include_extras=true");
        using var httpResponseMessage = await RequestApiAsync(httpRequestMessage, cancellationToken);

        if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
        {
            return Array.Empty<MtgCard>();
        }
        
        var jsonDocument = await JsonDocument.ParseAsync(await httpResponseMessage.Content.ReadAsStreamAsync(), default, cancellationToken);
        
        return jsonDocument.RootElement.GetProperty("data")
            .EnumerateArray()
            .Select(cardJsonElem => CardJsonElemToMtgCard(cardJsonElem))
            .ToArray();
    }

    /// <summary>
    /// https://scryfall.com/docs/api/cards/random
    /// </summary>
    /// <param name="query">https://scryfall.com/docs/syntax</param>
    /// <returns></returns>
    public async Task<IEnumerable<MtgCard>> RandomCards(string? query, CancellationToken cancellationToken, int count = 10)
    {
        var randomCards = new ConcurrentBag<MtgCard>();
        
        await Parallel.ForEachAsync(
            source: Enumerable.Range(0, count), 
            cancellationToken: cancellationToken,
            body: async(i, cancellationToken) => 
                {
                    var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, @$"{_UrlApi}/cards/random?q={query}");
                    using var httpResponseMessage = await RequestApiAsync(httpRequestMessage, cancellationToken);
                    
                    if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                    {
                        return;
                    }
                    
                    var jsonDocument = await JsonDocument.ParseAsync(await httpResponseMessage.Content.ReadAsStreamAsync(), default, cancellationToken);

                    randomCards.Add(CardJsonElemToMtgCard(jsonDocument.RootElement));
                });
        
        return randomCards;
    }

    /* public async Task<MtgCard?> RandomCard(string? query, CancellationToken cancellationToken)
    {
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, @$"{_UrlApi}/cards/random?q={query}");
        using var httpResponseMessage = await RequestApiAsync(httpRequestMessage, cancellationToken);
        
        if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }
        
        var jsonDocument = await JsonDocument.ParseAsync(await httpResponseMessage.Content.ReadAsStreamAsync(), default, cancellationToken);
        return CardJsonElemToMtgCard(jsonDocument.RootElement);
    } */

    /// <summary>
    /// Download preview image without card description.
    /// https://scryfall.com/docs/api/cards/id
    /// </summary>
    /// <param name="cardId">Scryfall Card ID</param>
    public async Task<Bitmap?> DownloadCropImageAsBitmapAsync(string cardId, CancellationToken cancellationToken = default)
    {
        if (_cachedImageItems.TryGetValue(cardId, out var image))
        {
            return image;
        }
        
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, @$"{_UrlApi}/cards/{cardId}?format=image&version=art_crop");
        using var httpResponseMessage = await RequestApiAsync(httpRequestMessage, cancellationToken);
        
        if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        return new Bitmap(await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken));
    }

    public bool AddToImageToCache(string cardId, Bitmap bitmap)
        => _cachedImageItems.TryAdd(cardId, bitmap);

    private async Task<HttpResponseMessage> RequestApiAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
    {
        await Task.Delay(_DelayRequestCalc.Delay, cancellationToken).ConfigureAwait(false);
        return await _httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
    }

    private MtgCard CardJsonElemToMtgCard(JsonElement cardJsonElem)
        => new MtgCard()
        {
            Name = cardJsonElem.GetProperty("name").GetString() ?? String.Empty,
            SfId = cardJsonElem.GetProperty("id").GetString() ?? String.Empty,
            SetAbbriviation = cardJsonElem.GetProperty("set").GetString() ?? String.Empty,
            CardNumber = cardJsonElem.GetProperty("collector_number").GetString() ?? String.Empty
        };
}