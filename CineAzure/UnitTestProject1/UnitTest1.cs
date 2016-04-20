using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Diagnostics;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        public UnitTest1()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
        }

        [TestMethod]
        public async Task TestGetCinemas()
        {
            Cineworld.CineworldService cineSvc = new Cineworld.CineworldService();
            var cinemas = await cineSvc.GetCinemas(Cineworld.RegionDef.GB);

            Assert.IsNotNull(cinemas);

            Assert.IsNotNull(cinemas.cinemas);

            Assert.IsTrue(cinemas.cinemas.Count > 0);
        }

        [TestMethod]
        public async Task TestGetCinema()
        {
            Cineworld.CineworldService cineSvc = new Cineworld.CineworldService();

            var cinemas = await cineSvc.GetCinemas(Cineworld.RegionDef.GB);

            Assert.IsNotNull(cinemas);

            Assert.IsNotNull(cinemas.cinemas);

            Assert.IsTrue(cinemas.cinemas.Count > 0);

            Cineworld.Cinema firstCinema = cinemas.cinemas[0];

            cinemas = await cineSvc.GetCinemas(Cineworld.RegionDef.GB, true, firstCinema.id);

            Assert.IsNotNull(cinemas);

            Assert.IsNotNull(cinemas.cinemas);

            Assert.IsTrue(cinemas.cinemas.Count > 0);

            Assert.IsTrue(cinemas.cinemas[0].id == firstCinema.id);
        }

        [TestMethod]
        public async Task TestGetFilms()
        {
            Cineworld.CineworldService cineSvc = new Cineworld.CineworldService();
            var films = await cineSvc.GetFilms(Cineworld.RegionDef.GB);

            Assert.IsNotNull(films);

            Assert.IsNotNull(films.films);

            Assert.IsTrue(films.films.Count > 0);
        }

        [TestMethod]
        public async Task TestGetFilm()
        {
            Cineworld.CineworldService cineSvc = new Cineworld.CineworldService();

            var films = await cineSvc.GetFilms(Cineworld.RegionDef.GB);

            Assert.IsNotNull(films);

            Assert.IsNotNull(films.films);

            Assert.IsTrue(films.films.Count > 0);

            Cineworld.Film firstFilm = films.films[0];

            films = await cineSvc.GetFilms(Cineworld.RegionDef.GB, true, int.MinValue, firstFilm.edi);

            Assert.IsNotNull(films);

            Assert.IsNotNull(films.films);

            Assert.IsTrue(films.films.Count > 0);

            Assert.IsTrue(films.films[0].edi == firstFilm.edi);
        }

        [TestMethod]
        public async Task TestGetFilmsAtCinema()
        {
            Cineworld.CineworldService cineSvc = new Cineworld.CineworldService();
            var cinemas = await cineSvc.GetCinemas(Cineworld.RegionDef.GB);

            Assert.IsNotNull(cinemas);

            Assert.IsNotNull(cinemas.cinemas);

            Assert.IsTrue(cinemas.cinemas.Count > 0);

            var firstCinema = cinemas.cinemas[0];

            var films = await cineSvc.GetFilms(Cineworld.RegionDef.GB, true, firstCinema.id);

            Assert.IsNotNull(films);

            Assert.IsNotNull(films.films);

            Assert.IsTrue(films.films.Count > 0);
        }

        [TestMethod]
        public async Task TestGetCinemasForFilm()
        {
            Cineworld.CineworldService cineSvc = new Cineworld.CineworldService();
            var films = await cineSvc.GetFilms(Cineworld.RegionDef.GB);

            Assert.IsNotNull(films);

            Assert.IsNotNull(films.films);

            Assert.IsTrue(films.films.Count > 0);

            Cineworld.Cinema cinema = null;

            for (int i = 0; i < films.films.Count; i++)
            {
                var firstFilm = films.films[i];

                var cinemas = await cineSvc.GetCinemas(Cineworld.RegionDef.GB, true, int.MinValue, firstFilm.edi);

                if (cinemas != null && cinemas.cinemas != null && cinemas.cinemas.Count > 0)
                {
                    cinema = cinemas.cinemas[0];
                    break;
                }
            }
            Assert.IsNotNull(cinema);
        }

        [TestMethod]
        public async Task TestGetDates()
        {
            int edi = 446064;
            int cinema = 88;

            Cineworld.CineworldService cineSvc = new Cineworld.CineworldService();

            var dates = await cineSvc.GetDates(Cineworld.RegionDef.GB, cinema, edi);

            Assert.IsNotNull(dates);

            Assert.IsNotNull(dates.dates);

            Assert.IsTrue(dates.dates.Count > 0);
        }

        [TestMethod]
        public async Task TestGetPerformances()
        {
            int edi = 446064;
            int cinema = 88;

            Cineworld.CineworldService cineSvc = new Cineworld.CineworldService();

            var dates = await cineSvc.GetDates(Cineworld.RegionDef.GB, cinema, edi);

            Assert.IsNotNull(dates);

            Assert.IsNotNull(dates.dates);

            Assert.IsTrue(dates.dates.Count > 0);

            var date = dates.dates[0];

            var performances = await cineSvc.GetPerformances(Cineworld.RegionDef.GB, cinema, edi, date);

            Assert.IsNotNull(performances);

            Assert.IsNotNull(performances.performances);

            Assert.IsTrue(performances.performances.Count > 0);
        }
    }
}
