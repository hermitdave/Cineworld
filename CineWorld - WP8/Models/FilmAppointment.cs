using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace CineWorld.Models
{
    public class FilmAppointment : IAppointment
    {
        /// <summary>
        /// Gets the subject of the appointment.
        /// </summary>
        public string Subject
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the start date and time of the appointment.
        /// </summary>
        public DateTime StartDate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the end date and time of the appointment.
        /// </summary>
        public DateTime EndDate
        {
            get;
            set;
        }
    }
}
