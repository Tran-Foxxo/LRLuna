using System;
using System.Drawing;
using OpenTK.Graphics;
namespace linerider.Utils
{
    static class Constants
    {
        public static float Red = Settings.RedColored;
        public static float Green = Settings.GreenColored;
        public static float Blue = Settings.BlueColored;

        public static int LineRed = Settings.LineColorRed;
        public static int LineGreen = Settings.LineColorGreen;
        public static int LineBlue = Settings.LineColorBlue;

        public static int SceneryLineRed = Settings.SceneryLineColorRed;
        public static int SceneryLineGreen = Settings.SceneryLineColorGreen;
        public static int SceneryLineBlue = Settings.SceneryLineColorBlue;

        public static int AccelerationLineRed = Settings.AccelerationLineColorRed;
        public static int AccelerationLineGreen = Settings.AccelerationLineColorGreen;
        public static int AccelerationLineBlue = Settings.AccelerationLineColorBlue;

        public static int NormalRed = Settings.NormalColorRed;
        public static int NormalGreen = Settings.NormalColorGreen;
        public static int NormalBlue = Settings.NormalColorBlue;

        public static int AccelerationRed = Settings.AccelerationColorRed;
        public static int AccelerationGreen = Settings.AccelerationColorGreen;
        public static int AccelerationBlue = Settings.AccelerationColorBlue;

        public static int SceneryRed = Settings.SceneryColorRed;
        public static int SceneryGreen = Settings.SceneryColorGreen;
        public static int SceneryBlue = Settings.SceneryColorBlue;

        public static readonly Color4 ColorOffwhite = new Color4(244, 245, 249, 255);
        public static readonly Color4 ColorWhite = new Color4(255, 255, 255, 255);
        public static readonly Color4 ColorNightMode = new Color4(22, 22, 22, 255);

        public static Color4 ColorColored = new Color4((byte)Red, (byte)Green, (byte)Blue, (byte)255);

        public static readonly int[] MotionArray =
        {
            1, 2, 5, 10, 20, 30, 40, 80, 160, 320, 640
        };
        public static readonly Color RedLineColor = Color.FromArgb(0xCC, 0, 0);
        public static Color RedLineColored = Color.FromArgb(AccelerationRed, AccelerationGreen, AccelerationBlue);

        public static readonly Color BlueLineColor = Color.FromArgb(0, 0x66, 0xFF);
        public static Color BlueLineColored = Color.FromArgb(NormalRed, NormalGreen, NormalBlue);

        public static readonly Color SceneryLineColor = Color.FromArgb(0, 0xCC, 0);
        public static Color SceneryLineColored = Color.FromArgb(SceneryRed, SceneryGreen, SceneryBlue);

        public static readonly Color TriggerLineColor = Color.FromArgb(0xFF, 0x95, 0x4F);

        public static readonly Color DefaultLineColor = Color.FromArgb(0, 0, 0);

        public static Color ColorDefaultLine = Color.FromArgb(Settings.LineColorRed, Settings.LineColorGreen, Settings.LineColorBlue);
        public static Color ColorSceneryLine = Color.FromArgb(Settings.SceneryLineColorRed, Settings.SceneryLineColorGreen, Settings.SceneryLineColorBlue);
        public static Color ColorAccelerationLine = Color.FromArgb(Settings.AccelerationLineColorRed, Settings.AccelerationLineColorGreen, Settings.AccelerationLineColorBlue);

        public static readonly Color DefaultNightLineColor = Color.FromArgb(255, 255, 255);

        public static Color ConstraintColor = Color.FromArgb(unchecked((int)0xFFCC72B7));
        public static Color ConstraintRepelColor = Color.CornflowerBlue;
        public static Color ConstraintFirstBreakColor = Color.FromArgb(unchecked((int)0xFFFF8C00));
        public static Color ConstraintBreakColor = Color.FromArgb(unchecked((int)0xffe67e00));
        public static Color ContactPointColor = Color.Cyan;
        public static Color ContactPointFakieColor = Color.Blue;

        public static Color MomentumVectorColor = Color.Red;
        public static readonly string TracksDirectory = Program.UserDirectory + TracksFolderName + System.IO.Path.DirectorySeparatorChar;
        public const string TracksFolderName = "Tracks";
        public const string DefaultTrackName = "<untitled>";
        public const float DefaultZoom = 4;
        public const int PhysicsRate = 40;
        public static bool ScaleCamera = true;
        public const double MinimumZoom = 0.1;
        public const float MaxZoom = 24;
        public const float MaxSuperZoom = 200;
        public const int MaxFrames = 40 * 60 * 60 * 3;// 3 hours of frames
        public static Color DefaultKnobColor => Settings.NightMode
        ? Color.FromArgb(ColorNightMode.ToArgb())
        : Color.FromArgb(ColorWhite.ToArgb());
    }
}