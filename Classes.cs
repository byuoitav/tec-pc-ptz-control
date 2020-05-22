using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BYUPTZControl
{
    public class Preset
    {
        public string DisplayName { get; set; }
        public string SetPreset { get; set; }
    }

    public class Camera
    {
        public string DisplayName { get; set; }
        public string TiltUp { get; set; }
        public string TiltDown { get; set; }
        public string PanLeft { get; set; }
        public string PanRight { get; set; }
        public string PanTiltStop { get; set; }
        public string ZoomIn { get; set; }
        public string ZoomOut { get; set; }
        public string ZoomStop { get; set; }
        public string Stream { get; set; }
        public IList<Preset> Presets { get; set; }
    }

    public class CameraList
    {
        public List<Camera> Cameras { get; set; }

        public CameraList()
        {

            bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject());

            if (designTime)
            {                
                Cameras = new List<Camera>();
                Cameras.Add(new Camera() { DisplayName = "Front Camera" });
                Cameras.Add(new Camera() { DisplayName = "Back Camera" });
            }
        }
    }
}
