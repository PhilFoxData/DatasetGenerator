using System.Collections.Generic;

namespace DatasetGenerator
{
    public static class Transmitter
    {
        public static List<Dataset> Datasets { get; set; }

        public static Dataset CurrentlyEditedDataset { get; set; }
        public static Dataset NewDataset { get; set;}

        public static DatasetGeneratorSettings Settings { get; set; }

        public static string DeleteDatsetName { get; set; }
    }
}
