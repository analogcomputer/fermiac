namespace fermiac {

    public class LogMsg {
        public string msg {get; set;}
        public string type {get; set;}
        public System.Windows.Media.Brush forecolor
        {
            get
            {
                switch(type.ToLower())
                {
                    case "trace": return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(96, 96, 96));
                    case "err": return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(128, 0, 0));
                    default: return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 128, 0));
                }
            }
        }
    }
}