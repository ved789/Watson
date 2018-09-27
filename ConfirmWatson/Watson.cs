using System.Collections.Generic;

namespace ConfirmWatson
{
    public class Class
    {
        public string @class { get; set; }
        public double score { get; set; }
        public string type_hierarchy { get; set; }
    }

    public class Classifier
    {
        public string classifier_id { get; set; }
        public string name { get; set; }
        public List<Class> classes { get; set; }
    }

    public class Image
    {
        public List<Classifier> classifiers { get; set; }
        public string image { get; set; }
    }

    public class Watson
    {
        public List<Image> images { get; set; }
        public int images_processed { get; set; }
        public int custom_classes { get; set; }
    }
}
