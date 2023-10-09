using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using MTGArtHarvester.Entities;
using MTGArtHarvester.Models;

namespace MTGArtHarvester;

public class ArtDownloadService
{
    private readonly string _siteUrl = @"https://www.mtgpics.com";
    
    private readonly HttpClient _httpClient;

    private static DelayCalculator _DelayRequestCalc = new DelayCalculator(1000);

    public ArtDownloadService()
        => _httpClient = HttpClientFactory.HttpClient;

    public async Task<DownloadResult> DownloadArtAsync(MtgCard mtgCard, string destinationFolder, CancellationToken cancellationToken = default)
    {
        var cardNumber = FixCardNumber(mtgCard.CardNumber);
        
        var possibleArtUrls = new List<string>()
        {
            $"{_siteUrl}/pics/art/{mtgCard.SetAbbriviation}/{cardNumber}.jpg",
            $"{_siteUrl}/pics/art/{mtgCard.SetAbbriviation}/{cardNumber}_1.jpg",
            $"{_siteUrl}/pics/art/{mtgCard.SetAbbriviation}/{cardNumber}_2.jpg",
        };

        var tasks = possibleArtUrls
            .Select(async url => new KeyValuePair<string, Bitmap?>(url, await DownloadBitmapByUrlAsync(url, cancellationToken)));

        var urlBitmapPairArray = await Task.WhenAll(tasks);

        urlBitmapPairArray = urlBitmapPairArray
            .Where(pair => pair.Value != null)
            .OrderBy(pair => pair.Value?.Size.Width)
            .ThenBy(pair => pair.Value?.Size.Height)
            .ToArray();

        if (!urlBitmapPairArray.Any())
        {
            return new DownloadResult();
        }

        string bestArtUrl = urlBitmapPairArray.Last().Key;
        Bitmap bestArtBitmap = urlBitmapPairArray.Last().Value!;

        var downloadResult = new DownloadResult()
        {
            FileName = $"{mtgCard.SetAbbriviation}{mtgCard.CardNumber}.jpg",
            Url = bestArtUrl,
            Folder = destinationFolder,
            Width = bestArtBitmap.Size.Width,
            Height = bestArtBitmap.Size.Height
        };

        await Task.Run( () => 
        {
            bestArtBitmap.Save(Path.Combine(downloadResult.Folder, downloadResult.FileName));            
        }).ConfigureAwait(false);
        
        return downloadResult;
    }

    private string FixCardNumber(string cardNumber)
    {
        switch(cardNumber.Length)
        {
            case 1: return $"00{cardNumber}";
            case 2: return $"0{cardNumber}";
            default: return cardNumber;
        }
    }

    private async Task<Bitmap?> DownloadBitmapByUrlAsync(string url, CancellationToken cancellationToken)
    {
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        using var httpResponseMessage = await RequestAsync(httpRequestMessage, cancellationToken);
        
        if (httpResponseMessage.StatusCode != System.Net.HttpStatusCode.OK)
        {
            return null;
        }

        return new Bitmap(await httpResponseMessage.Content.ReadAsStreamAsync());
    }

    private async Task<HttpResponseMessage> RequestAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
    {
        await Task.Delay(_DelayRequestCalc.Delay, cancellationToken).ConfigureAwait(false);
        return await _httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
    }
}
