using Cineworld;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CineDataWorkerRole.MobileService
{
    public class CineMobileService : ICineMobileSerivce
    {
        public static MobileServiceClient MobileService = new MobileServiceClient("https://cineworld.azure-mobile.net/", "kpNUhnZFTNayzvLPaWxszNbpuBJnNQ87");

        public async Task<List<FilmReview>> GetFilmReviews(FilmInfo film)
        {
            List<FilmReview> filmReviews = await MobileService.GetTable<FilmReview>().Where(r => r.Movie == film.EDI || r.TmdbId == film.TmdbId && r.TmdbId != 0).ToListAsync();

            return filmReviews;
        }

        public async Task AddUpdateFilmReview(FilmReview filmReview)
        {
            if (filmReview.Id != 0)
            {
                await MobileService.GetTable<FilmReview>().UpdateAsync(filmReview);
            }
            else
            {
                await MobileService.GetTable<FilmReview>().InsertAsync(filmReview);
            }
        }

        public async Task<List<CinemaReview>> GetCinemaReviews(int Id)
        {
            List<CinemaReview> cinemaReviews = await MobileService.GetTable<CinemaReview>().Where(r => r.Cinema == Id).ToListAsync();

            return cinemaReviews;
        }

        public async Task AddUpdateCinemaReview(CinemaReview cinemaReview)
        {
            if (cinemaReview.Id != 0)
            {
                await MobileService.GetTable<CinemaReview>().UpdateAsync(cinemaReview);
            }
            else
            {
                await MobileService.GetTable<CinemaReview>().InsertAsync(cinemaReview);
            }
        }

        public async Task LogBooking(BookingHistory booking)
        {
            await MobileService.GetTable<BookingHistory>().InsertAsync(booking);
        }
    }
}
