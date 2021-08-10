using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatasetGenerator
{
    public class DatasetGeneratorSettings
    {
        public int ImageResolution { get; set; }

        public DatasetGeneratorSettings()
        {
            ImageResolution = 50;
        }
    }
}
