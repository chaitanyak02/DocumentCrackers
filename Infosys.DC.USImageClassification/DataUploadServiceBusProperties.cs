using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSA.Entity
{
    public class DataUploadServiceBusProperties
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public string Vendor { get; set; }
        public string DiskNumber { get; set; }
        public string DataProcessing { get; set; }
        public string IgnoreBackup { get; set; }
        //public string HashCheck { get; set; }
        public string QualityCheck { get; set; }
        public string Status { get; set; }
        public string Created { get; set; }
        public string CreatedBy { get; set; }
    }
}
