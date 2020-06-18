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
        public string Snapshot { get; set; }
        public string SnapshotLogin { get; set; }
        public List<Preset> Presets { get; set; }
    }

    public class Aver520LoginResultData
    {
        public string token { get; set; }
    }

    public class Aver520LoginResult
    {
        public int code { get; set; }
        public string msg { get; set; }
        public Aver520LoginResultData data { get; set; }
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
                Cameras.Add(new Camera() { DisplayName = "Front Camera", Presets = new List<Preset>(new Preset[] { new Preset() { DisplayName = "Left" }, new Preset() { DisplayName = "Right" }, new Preset() { DisplayName = "Front" } }) });
                Cameras.Add(new Camera() { DisplayName = "Back Camera" });
            }
        }
    }
}
