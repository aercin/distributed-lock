using Microsoft.AspNetCore.Mvc;
using Polly;
using Shared.Models;
using SingleNodeDistributedLockSample.Persistence;
using System.Diagnostics;

namespace SingleNodeDistributedLockSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LibraryController : ControllerBase
    {
        private readonly ICacheProvider _cache;
        private readonly Repository _repo;
        public LibraryController(ICacheProvider cache, Repository repo)
        {
            this._cache = cache;
            this._repo = repo;
        }

        [HttpPost("/rent-a-book")]
        public async Task<IActionResult> RentABook(BookRequest request)
        {
            var resRentABook = new BookResponse();
            var totalProcessTime = 0;
            var retryPolicy = Policy.HandleResult<bool>(p => p == false).WaitAndRetry(retryCount: 3, sleepDurationProvider: _ =>
            {
                Random rnd = new Random();
                return TimeSpan.FromMilliseconds(rnd.Next(200));
            }, onRetry: (result, sleepDuration, attemptNumber, context) =>
            {
                totalProcessTime += (int)sleepDuration.TotalMilliseconds; 
            });

            bool locked = true;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            retryPolicy.Execute(() =>
            { 
                locked = string.IsNullOrEmpty(this._cache.GetAsync<string>(request.SeriNo.ToString()).GetAwaiter().GetResult());
                if (locked)
                {
                    this._cache.SetAsync(request.SeriNo.ToString(), request.TCKimlikNo, cacheSettings =>
                    {
                        cacheSettings.AbsoluteExpiration = 1;
                    }).GetAwaiter();

                    var relatedBook = this._repo.Books.Single(x => x.SeriNo == request.SeriNo);
                    relatedBook.IsRented = true;
                    resRentABook.ResultCode = 1;
                    resRentABook.ResultDesc = "İşlem Başarılı";

                    stopWatch.Stop();
                    resRentABook.ProcessTimeByMiliseconds =  (int)stopWatch.Elapsed.TotalMilliseconds;
                }
                return locked;
            });

            if (!locked && resRentABook.ResultCode != 1)
            {
                resRentABook.ResultCode = -1;
                resRentABook.ResultDesc = "İşlem Başarısız - kilitli";
                resRentABook.ProcessTimeByMiliseconds = totalProcessTime;
            }   

            return Ok(resRentABook);
        }

        [HttpPost("/refund-a-book")]
        public async Task<IActionResult> RefundBook(BookRequest request)
        {
            var isLockMine = await this._cache.GetAsync<string>(request.SeriNo.ToString()) == request.TCKimlikNo;
            if (isLockMine)
            {
                var relatedBook = this._repo.Books.Single(x => x.SeriNo == request.SeriNo);
                relatedBook.IsRented = false;

                await this._cache.RemoveAsync(request.SeriNo.ToString());

                return Ok();
            }

            return NotFound();
        }


    } 
}
