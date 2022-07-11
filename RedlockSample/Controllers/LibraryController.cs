using Microsoft.AspNetCore.Mvc;
using RedlockSample.Common;
using Shared.Models;
using SingleNodeDistributedLockSample.Persistence;
using System.Diagnostics;

namespace RedlockSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LibraryController : ControllerBase
    {
        private readonly IDistributedLockManager _dlm;
        private readonly Repository _repo;
        public LibraryController(IDistributedLockManager dlm, Repository repo)
        {
            _dlm = dlm;
            _repo = repo;
        }

        [HttpPost("/rent-a-book")]
        public async Task<IActionResult> RentABook(BookRequest request)
        {
            var resRentABook = new BookResponse();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            bool locked = await this._dlm.Lock(request.SeriNo.ToString(), ttlByMinute: 1);

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;

            if (locked)
            {
                var relatedBook = this._repo.Books.Single(x => x.SeriNo == request.SeriNo);

                relatedBook.IsRented = true;
                relatedBook.RentedBy = request.TCKimlikNo;

                resRentABook.ResultCode = 0;
                resRentABook.ResultDesc = "İşlem Başarılı";
                resRentABook.ProcessTimeByMiliseconds = (int)ts.TotalMilliseconds;
            }
            else
            {
                resRentABook.ResultCode = -1;
                resRentABook.ResultDesc = "İşlem Başarısız";
                resRentABook.ProcessTimeByMiliseconds = (int)ts.TotalMilliseconds;
            }

            return Ok(resRentABook);
        }

        [HttpPost("/refund-a-book")]
        public async Task<IActionResult> RefundBook(BookRequest request)
        {
            var relatedBook = this._repo.Books.SingleOrDefault(x => x.SeriNo == request.SeriNo && x.RentedBy == request.TCKimlikNo);
            if(relatedBook != null)
            {
                relatedBook.IsRented = false;

                await this._dlm.UnLock(request.SeriNo.ToString());
            }
            else
            {
                return NotFound();
            }        

            return Ok();
        }
    }
}
