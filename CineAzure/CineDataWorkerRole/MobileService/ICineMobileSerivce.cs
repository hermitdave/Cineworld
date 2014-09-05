using Cineworld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CineDataWorkerRole.MobileService
{
    [ServiceContract]
    public interface ICineMobileSerivce
    {
        [OperationContract]
        Task<List<FilmReview>> GetFilmReviews(FilmInfo film);

        [OperationContract]
        Task AddUpdateFilmReview(FilmReview filmReview);

        [OperationContract]
        Task<List<CinemaReview>> GetCinemaReviews(int Id);

        [OperationContract]
        Task AddUpdateCinemaReview(CinemaReview cinemaReview);

        [OperationContract]
        Task LogBooking(BookingHistory booking);
    }
}
