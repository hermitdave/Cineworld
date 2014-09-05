using CineWorld.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace CineWorld.Services
{
    public class FilmAppointmentSource : AppointmentSource
    {
        public FilmAppointmentSource(List<FilmAppointment> appointments)
        {
            this.AllAppointments.Clear();
            this.AllAppointments.AddRange(appointments);
        }

        public override void FetchData(DateTime startDate, DateTime endDate)
        {
            
        }
    }
}
