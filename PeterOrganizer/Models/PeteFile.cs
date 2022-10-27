using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace PeterOrganizer.Models
{
    public class PeteFile : PropertyChangedBase
    {
        private string _status = "PENDING";

        public string ProductName { get; set; }
        public string SizeState { get; set; }
        public string ColorProfile { get; set; }
        public string Extension { get; set; }

        public string Status
        {
            get => _status;
            set => Set(ref _status, value);
        }

        public string Origin { get; set; }
        public string Filename { get; set; }
    }
}
