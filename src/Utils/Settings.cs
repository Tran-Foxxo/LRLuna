﻿//  Author:
//       Noah Ablaseau <nablaseau@hotmail.com>
//
//  Copyright (c) 2017 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using linerider.Audio;
using linerider.UI;
using linerider.Utils;
using CallFunction;

namespace linerider
{
    static class Settings
    {

        public static class Recording
        {
            public static bool ShowTools = false;
            public static bool ShowFps = true;
            public static bool ShowPpf = true;
        }
        public static class Local
        {
            public static bool RecordingMode;
            public static float MaxZoom
            {
                get
                {
                    return Settings.SuperZoom ? Constants.MaxSuperZoom : Constants.MaxZoom;
                }
            }
            public static bool TrackOverlay = false;
            public static bool TrackOverlayFixed = false;
            public static int TrackOverlayFixedFrame = 0;
            public static int TrackOverlayOffset = -1;
        }
        public static class Editor
        {
            public static bool HitTest;
            public static bool SnapNewLines;
            public static bool SnapMoveLine;
            public static bool ForceXySnap;
            public static bool MomentumVectors;
            public static bool RenderGravityWells;
            public static bool DrawContactPoints;
            public static bool LifeLockNoOrange;
            public static bool LifeLockNoFakie;
            public static bool ShowLineLength;
            public static bool ShowLineAngle;
            public static bool ShowLineID;
        }
        public static float RedColored;
        public static float GreenColored;
        public static float BlueColored;

        public static int LineColorRed;
        public static int LineColorGreen;
        public static int LineColorBlue;

        public static int SceneryLineColorRed;
        public static int SceneryLineColorGreen;
        public static int SceneryLineColorBlue;

        public static int AccelerationLineColorRed;
        public static int AccelerationLineColorGreen;
        public static int AccelerationLineColorBlue;

        public static bool MainLine;

        //Configurable Toggle

        public static bool ConfigTContactPoints;
        public static bool ConfigTGravityWells;
        public static bool ConfigTHitTest;
        public static bool ConfigTOnionSkinning;
        public static bool ConfigTMomentumVectors;
        public static bool ToggleConfig;

        //Camera options

        public static int PointVar;
        public static bool OffsledVar;
        public static bool OffsledSledCam;

        public static bool FixedCam;
        public static int XFixed;
        public static int YFixed;

        public static bool HorizontalTracking;
        public static bool VerticalTracking;

        //end

        //individual line options

        public static bool NormalColorChange;
        public static int NormalColorRed;
        public static int NormalColorGreen;
        public static int NormalColorBlue;

        public static bool AccelerationColorChange;
        public static int AccelerationColorRed;
        public static int AccelerationColorGreen;
        public static int AccelerationColorBlue;

        public static bool SceneryColorChange;
        public static int SceneryColorRed;
        public static int SceneryColorGreen;
        public static int SceneryColorBlue;

        public static float XY;

        //end

        //scarf options

        public static int ScarfSegments;
        public static string SelectedScarf;
        public static int multiScarfAmount;
        public static int multiScarfSegments;

        //end

        //Rider options

        public static string SelectedBoshSkin;
        public static bool CustomScarfOnPng;

         //end

        //Timer

        public static float MinutesElapsed;

        //end

        //Fun stuff, cuz why not

        public static bool ConfettiLines;

        //end

        public static int PlaybackZoomType;
        public static float PlaybackZoomValue;
        public static float Volume;
        public static bool SuperZoom;
        public static bool WhiteBG;
        public static bool ColoredBG;
        public static bool NightMode;
        public static bool SmoothCamera;
        public static bool PredictiveCamera;
        public static bool RoundLegacyCamera;
        public static bool SmoothPlayback;
        public static bool CheckForUpdates;
        public static bool Record1080p;
        public static bool RecordSmooth;
        public static bool RecordMusic;
        public static float ScrollSensitivity;
        public static int SettingsPane;
        public static bool MuteAudio;
        public static bool PreviewMode;
        public static int SlowmoSpeed;
        public static float DefaultPlayback;
        public static bool ColorPlayback;
        public static bool OnionSkinning;
        public static float OnionSkinningFront;
        public static float OnionSkinningBack;
        public static string LastSelectedTrack = "";
        public static Dictionary<Hotkey, KeyConflicts> KeybindConflicts = new Dictionary<Hotkey, KeyConflicts>();
        public static Dictionary<Hotkey, List<Keybinding>> Keybinds = new Dictionary<Hotkey, List<Keybinding>>();
        private static Dictionary<Hotkey, List<Keybinding>> DefaultKeybinds = new Dictionary<Hotkey, List<Keybinding>>();
        static Settings()
        {
            RestoreDefaultSettings();
            foreach (Hotkey hk in Enum.GetValues(typeof(Hotkey)))
            {
                if (hk == Hotkey.None)
                    continue;
                KeybindConflicts.Add(hk, KeyConflicts.General);
                Keybinds.Add(hk, new List<Keybinding>());
            }
            //conflicts, for keybinds that depend on a state, so keybinds 
            //outside of its state can be set as long
            //as its dependant state (general) doesnt have a keybind set
            KeybindConflicts[Hotkey.PlaybackZoom] = KeyConflicts.Playback;
            KeybindConflicts[Hotkey.PlaybackUnzoom] = KeyConflicts.Playback;
            KeybindConflicts[Hotkey.PlaybackSpeedUp] = KeyConflicts.Playback;
            KeybindConflicts[Hotkey.PlaybackSpeedDown] = KeyConflicts.Playback;

            KeybindConflicts[Hotkey.LineToolFlipLine] = KeyConflicts.LineTool;

            KeybindConflicts[Hotkey.ToolXYSnap] = KeyConflicts.Tool;
            KeybindConflicts[Hotkey.ToolToggleSnap] = KeyConflicts.Tool;
            KeybindConflicts[Hotkey.EditorCancelTool] = KeyConflicts.Tool;

            KeybindConflicts[Hotkey.ToolLengthLock] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolAngleLock] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolAxisLock] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolPerpendicularAxisLock] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolLifeLock] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolLengthLock] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolCopy] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolCut] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolPaste] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolDelete] = KeyConflicts.SelectTool;

            KeybindConflicts[Hotkey.PlayButtonIgnoreFlag] = KeyConflicts.HardCoded;
            KeybindConflicts[Hotkey.EditorCancelTool] = KeyConflicts.HardCoded;
            KeybindConflicts[Hotkey.ToolAddSelection] = KeyConflicts.HardCoded;
            KeybindConflicts[Hotkey.ToolToggleSelection] = KeyConflicts.HardCoded;
            KeybindConflicts[Hotkey.ToolScaleAspectRatio] = KeyConflicts.HardCoded;
            SetupDefaultKeybinds();
        }
        public static void RestoreDefaultSettings()
        {
            Editor.HitTest = false;
            Editor.SnapNewLines = true;
            Editor.SnapMoveLine = true;
            Editor.ForceXySnap = false;
            Editor.MomentumVectors = false;
            Editor.RenderGravityWells = false;
            Editor.DrawContactPoints = false;
            Editor.LifeLockNoOrange = false;
            Editor.LifeLockNoFakie = false;
            Editor.ShowLineLength = true;
            Editor.ShowLineAngle = true;
            Editor.ShowLineID = false;
            PlaybackZoomType = 0;
            PlaybackZoomValue = 4;
            Volume = 100;
            SuperZoom = false;
            WhiteBG = false;
            ColoredBG = false;
            NightMode = false;
            SmoothCamera = true;
            PredictiveCamera = false;
            RoundLegacyCamera = true;
            SmoothPlayback = true;
            CheckForUpdates = true;
            Record1080p = false;
            RecordSmooth = true;
            RecordMusic = true;
            ScrollSensitivity = 1;
            SettingsPane = 0;
            MuteAudio = false;
            PreviewMode = false;
            SlowmoSpeed = 2;
            DefaultPlayback = 1f;
            ColorPlayback = false;
            OnionSkinning = false;
            OnionSkinningFront = 20;
            OnionSkinningBack = 20;
            RedColored = 127;
            GreenColored = 127;
            BlueColored = 127;

            LineColorRed = 127;
            LineColorGreen = 127;
            LineColorBlue = 127;

            SceneryLineColorRed = 127;
            SceneryLineColorGreen = 127;
            SceneryLineColorBlue = 127;

            AccelerationLineColorRed = 127;
            AccelerationLineColorGreen = 127;
            AccelerationLineColorBlue = 127;

            MainLine = false;

            ConfigTContactPoints = false;
            ConfigTGravityWells = false;
            ConfigTHitTest = false;
            ConfigTOnionSkinning = false;
            ConfigTMomentumVectors = false;
            ToggleConfig = false;
            
            PointVar = 0;
            OffsledSledCam = false;
            OffsledVar = false;

            FixedCam = false;
            XFixed = 0;
            YFixed = 0;

            HorizontalTracking = false;
            VerticalTracking = false;

            XY = 15;

            NormalColorChange = false;
            NormalColorRed = 0;
            NormalColorGreen = 102;
            NormalColorBlue = 255;

            AccelerationColorChange = false;
            AccelerationColorRed = 204;
            AccelerationColorGreen = 0;
            AccelerationColorBlue = 0;

            SceneryColorChange = false;
            SceneryColorRed = 0;
            SceneryColorGreen = 204;
            SceneryColorBlue = 0;

            ScarfSegments = 6;
            SelectedScarf = "*default*";
            multiScarfAmount = 1;
            multiScarfSegments = 6;

            SelectedBoshSkin = "*default*";
            CustomScarfOnPng = false;

            ConfettiLines = false;
        }
        public static void ResetKeybindings()
        {
            foreach (var kb in Keybinds)
            {
                kb.Value.Clear();
            }
            LoadDefaultKeybindings();
        }
        private static void SetupDefaultKeybinds()
        {
            SetupDefaultKeybind(Hotkey.ToggleAll, new Keybinding(Key.O, KeyModifiers.Shift));

            SetupDefaultKeybind(Hotkey.ScenerySetSmall, new Keybinding(Key.Number4));
            SetupDefaultKeybind(Hotkey.SceneryIncreaseBig, new Keybinding(Key.Number6));
            SetupDefaultKeybind(Hotkey.SceneryDecreaseBig, new Keybinding(Key.Number7));

            SetupDefaultKeybind(Hotkey.AccelerationSetSmall, new Keybinding(Key.Number4, KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.AccelerationSetMedium, new Keybinding(Key.Number5, KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.AccelerationSetLarge, new Keybinding(Key.Number6, KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.AccelerationSetMax, new Keybinding(Key.Number7, KeyModifiers.Shift));

            SetupDefaultKeybind(Hotkey.EditorPencilTool, new Keybinding(Key.Q));
            SetupDefaultKeybind(Hotkey.EditorLineTool, new Keybinding(Key.W));
            SetupDefaultKeybind(Hotkey.EditorEraserTool, new Keybinding(Key.E));
            SetupDefaultKeybind(Hotkey.EditorSelectTool, new Keybinding(Key.R));
            SetupDefaultKeybind(Hotkey.EditorPanTool, new Keybinding(Key.T));
            SetupDefaultKeybind(Hotkey.EditorToolColor1, new Keybinding(Key.Number1));
            SetupDefaultKeybind(Hotkey.EditorToolColor2, new Keybinding(Key.Number2));
            SetupDefaultKeybind(Hotkey.EditorToolColor3, new Keybinding(Key.Number3));

            SetupDefaultKeybind(Hotkey.EditorCycleToolSetting, new Keybinding(Key.Tab));
            SetupDefaultKeybind(Hotkey.EditorMoveStart, new Keybinding(Key.D));

            SetupDefaultKeybind(Hotkey.EditorRemoveLatestLine, new Keybinding(Key.BackSpace));
            SetupDefaultKeybind(Hotkey.EditorFocusStart, new Keybinding(Key.Home));
            SetupDefaultKeybind(Hotkey.EditorFocusLastLine, new Keybinding(Key.End));
            SetupDefaultKeybind(Hotkey.EditorFocusRider, new Keybinding(Key.F1));
            SetupDefaultKeybind(Hotkey.EditorFocusFlag, new Keybinding(Key.F2));
            SetupDefaultKeybind(Hotkey.ToolLifeLock, new Keybinding(KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.ToolAngleLock, new Keybinding(KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.ToolAxisLock, new Keybinding(KeyModifiers.Control | KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.ToolPerpendicularAxisLock, new Keybinding(Key.X, KeyModifiers.Control | KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.ToolLengthLock, new Keybinding(Key.L));
            SetupDefaultKeybind(Hotkey.ToolXYSnap, new Keybinding(Key.X));
            SetupDefaultKeybind(Hotkey.ToolToggleSnap, new Keybinding(Key.S));
            SetupDefaultKeybind(Hotkey.ToolSelectBothJoints, new Keybinding(KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.LineToolFlipLine, new Keybinding(KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.EditorUndo, new Keybinding(Key.Z, KeyModifiers.Control));

            SetupDefaultKeybind(Hotkey.EditorRedo,
                new Keybinding(Key.Y, KeyModifiers.Control),
                new Keybinding(Key.Z, KeyModifiers.Control | KeyModifiers.Shift));

            SetupDefaultKeybind(Hotkey.PlaybackStartIgnoreFlag, new Keybinding(Key.Y, KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.PlaybackStartGhostFlag, new Keybinding(Key.I, KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.PlaybackStartSlowmo, new Keybinding(Key.Y, KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.PlaybackFlag, new Keybinding(Key.I));
            SetupDefaultKeybind(Hotkey.PlaybackStart, new Keybinding(Key.Y));
            SetupDefaultKeybind(Hotkey.PlaybackStop, new Keybinding(Key.U));
            SetupDefaultKeybind(Hotkey.PlaybackSlowmo, new Keybinding(Key.M));
            SetupDefaultKeybind(Hotkey.PlaybackZoom, new Keybinding(Key.Z));
            SetupDefaultKeybind(Hotkey.PlaybackUnzoom, new Keybinding(Key.X));

            SetupDefaultKeybind(Hotkey.PlaybackSpeedUp,
                new Keybinding(Key.Plus),
                new Keybinding(Key.KeypadPlus));

            SetupDefaultKeybind(Hotkey.PlaybackSpeedDown,
                new Keybinding(Key.Minus),
                new Keybinding(Key.KeypadMinus));

            SetupDefaultKeybind(Hotkey.PlaybackFrameNext, new Keybinding(Key.Right));
            SetupDefaultKeybind(Hotkey.PlaybackFramePrev, new Keybinding(Key.Left));
            SetupDefaultKeybind(Hotkey.PlaybackForward, new Keybinding(Key.Right, KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.PlaybackBackward, new Keybinding(Key.Left, KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.PlaybackIterationNext, new Keybinding(Key.Right, KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.PlaybackIterationPrev, new Keybinding(Key.Left, KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.PlaybackTogglePause, new Keybinding(Key.Space));

            SetupDefaultKeybind(Hotkey.PreferencesWindow,
                new Keybinding(Key.P, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.GameMenuWindow, new Keybinding(Key.Escape));
            SetupDefaultKeybind(Hotkey.TrackPropertiesWindow, new Keybinding(Key.T, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.PreferenceOnionSkinning, new Keybinding(Key.O, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.LoadWindow, new Keybinding(Key.O));
            SetupDefaultKeybind(Hotkey.Quicksave, new Keybinding(Key.S, KeyModifiers.Control));

            SetupDefaultKeybind(Hotkey.PlayButtonIgnoreFlag, new Keybinding(KeyModifiers.Alt));

            SetupDefaultKeybind(Hotkey.EditorQuickPan, new Keybinding(Key.Space, KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.EditorDragCanvas, new Keybinding(MouseButton.Middle));

            SetupDefaultKeybind(Hotkey.EditorCancelTool, new Keybinding(Key.Escape));
            SetupDefaultKeybind(Hotkey.PlayButtonIgnoreFlag, new Keybinding(KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.PlaybackResetCamera, new Keybinding(Key.N));
            SetupDefaultKeybind(Hotkey.ToolCopy, new Keybinding(Key.C, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.ToolCut, new Keybinding(Key.X, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.ToolPaste, new Keybinding(Key.V, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.ToolDelete, new Keybinding(Key.Delete));
            SetupDefaultKeybind(Hotkey.ToolAddSelection, new Keybinding(KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.ToolToggleSelection, new Keybinding(KeyModifiers.Control));

            SetupDefaultKeybind(Hotkey.ToolScaleAspectRatio, new Keybinding(KeyModifiers.Shift));

            SetupDefaultKeybind(Hotkey.ToolToggleOverlay, new Keybinding(Key.V));
        }
        private static void SetupDefaultKeybind(Hotkey hotkey, Keybinding keybinding, Keybinding secondary = null)
        {
            if (keybinding.IsEmpty)
                return;
            DefaultKeybinds[hotkey] = new List<Keybinding>();
            DefaultKeybinds[hotkey].Add(keybinding);
            if (secondary != null)
            {
                DefaultKeybinds[hotkey].Add(secondary);
            }
        }
        private static void LoadDefaultKeybindings()
        {
            foreach (Hotkey hk in Enum.GetValues(typeof(Hotkey)))
            {
                if (hk == Hotkey.None)
                    continue;
                LoadDefaultKeybind(hk);
            }
        }
        public static List<Keybinding> GetHotkeyDefault(Hotkey hotkey)
        {
            if (!DefaultKeybinds.ContainsKey(hotkey))
                return null;
            return DefaultKeybinds[hotkey];
        }
        private static void LoadDefaultKeybind(Hotkey hotkey)
        {
            if (DefaultKeybinds.ContainsKey(hotkey))
            {
                var defaults = DefaultKeybinds[hotkey];
                if (defaults == null || defaults.Count == 0)
                    return;
                var list = Keybinds[hotkey];
                if (list.Count == 0)
                    CreateKeybind(hotkey, defaults[0]);
                if (defaults.Count > 1)
                {
                    var secondary = defaults[1];
                    if (secondary != null && list.Count == 1 && list[0].IsBindingEqual(defaults[0]))
                        CreateKeybind(hotkey, secondary);
                }
            }
        }
        private static void CreateKeybind(Hotkey hotkey, Keybinding keybinding)
        {
            var conflict = CheckConflicts(keybinding, hotkey);
            if (keybinding.IsEmpty || conflict != Hotkey.None)
                return;
            Keybinds[hotkey].Add(keybinding);
        }
        public static Hotkey CheckConflicts(Keybinding keybinding, Hotkey hotkey)
        {
            if (!keybinding.IsEmpty)
            {
                var inputconflicts = Settings.KeybindConflicts[hotkey];
                if (inputconflicts == KeyConflicts.HardCoded)
                    return Hotkey.None;
                foreach (var keybinds in Settings.Keybinds)
                {
                    var hk = keybinds.Key;
                    var conflicts = Settings.KeybindConflicts[hk];
                    //if the conflicts is equal to or below inputconflicts
                    //then we can compare for conflict
                    //if conflicts is above inputconflicts, ignore
                    //also, if theyre both hardcoded they cannot conflict.
                    if (inputconflicts.HasFlag(conflicts))
                    {
                        foreach (var keybind in keybinds.Value)
                        {
                            if (keybind.IsBindingEqual(keybinding) && 
                                !(inputconflicts == KeyConflicts.HardCoded &&
                                  inputconflicts == conflicts))
                                return hk;
                        }
                    }
                }
            }
            return Hotkey.None;
        }
        public static void Load()
        {
            string[] lines = null;
            try
            {
                if (!File.Exists(Program.UserDirectory + "settings.conf"))
                {
                    Save();
                }
                lines = File.ReadAllLines(Program.UserDirectory + "settings.conf");
            }
            catch
            {
            }
            MinTimer.Timer();
            LoadFloat(GetSetting(lines, nameof(MinutesElapsed)), ref MinutesElapsed);

            LoadBool(GetSetting(lines, nameof(ConfigTContactPoints)), ref ConfigTContactPoints);
            LoadBool(GetSetting(lines, nameof(ConfigTGravityWells)), ref ConfigTGravityWells);
            LoadBool(GetSetting(lines, nameof(ConfigTHitTest)), ref ConfigTHitTest);
            LoadBool(GetSetting(lines, nameof(ConfigTMomentumVectors)), ref ConfigTMomentumVectors);
            LoadBool(GetSetting(lines, nameof(ConfigTOnionSkinning)), ref ConfigTOnionSkinning);
            LoadBool(GetSetting(lines, nameof(ToggleConfig)), ref ToggleConfig);

            LoadInt(GetSetting(lines, nameof(PointVar)), ref PointVar);
            LoadBool(GetSetting(lines, nameof(OffsledSledCam)), ref OffsledSledCam);
            LoadBool(GetSetting(lines, nameof(OffsledVar)), ref OffsledVar);
            LoadBool(GetSetting(lines, nameof(FixedCam)), ref FixedCam);
            LoadInt(GetSetting(lines, nameof(XFixed)), ref XFixed);
            LoadInt(GetSetting(lines, nameof(YFixed)), ref YFixed);

            LoadBool(GetSetting(lines, nameof(HorizontalTracking)), ref HorizontalTracking);
            LoadBool(GetSetting(lines, nameof(VerticalTracking)), ref VerticalTracking);

            LoadFloat(GetSetting(lines, nameof(RedColored)), ref RedColored);
            LoadFloat(GetSetting(lines, nameof(GreenColored)), ref GreenColored);
            LoadFloat(GetSetting(lines, nameof(BlueColored)), ref BlueColored);

            LoadFloat(GetSetting(lines, nameof(XY)), ref XY);

            LoadInt(GetSetting(lines, nameof(LineColorRed)), ref LineColorRed);
            LoadInt(GetSetting(lines, nameof(LineColorGreen)), ref LineColorGreen);
            LoadInt(GetSetting(lines, nameof(LineColorBlue)), ref LineColorBlue);

            LoadInt(GetSetting(lines, nameof(SceneryLineColorRed)), ref SceneryLineColorRed);
            LoadInt(GetSetting(lines, nameof(SceneryLineColorGreen)), ref SceneryLineColorGreen);
            LoadInt(GetSetting(lines, nameof(SceneryLineColorBlue)), ref SceneryLineColorBlue);

            LoadInt(GetSetting(lines, nameof(AccelerationLineColorRed)), ref AccelerationLineColorRed);
            LoadInt(GetSetting(lines, nameof(AccelerationLineColorGreen)), ref AccelerationLineColorGreen);
            LoadInt(GetSetting(lines, nameof(AccelerationLineColorBlue)), ref AccelerationLineColorBlue);

            LoadBool(GetSetting(lines, nameof(MainLine)), ref MainLine);

            LoadBool(GetSetting(lines, nameof(NormalColorChange)), ref NormalColorChange);
            LoadInt(GetSetting(lines, nameof(NormalColorRed)), ref NormalColorRed);
            LoadInt(GetSetting(lines, nameof(NormalColorGreen)), ref NormalColorGreen);
            LoadInt(GetSetting(lines, nameof(NormalColorBlue)), ref NormalColorBlue);

            LoadBool(GetSetting(lines, nameof(AccelerationColorChange)), ref AccelerationColorChange);
            LoadInt(GetSetting(lines, nameof(AccelerationColorRed)), ref AccelerationColorRed);
            LoadInt(GetSetting(lines, nameof(AccelerationColorGreen)), ref AccelerationColorGreen);
            LoadInt(GetSetting(lines, nameof(AccelerationColorBlue)), ref AccelerationColorBlue);

            LoadBool(GetSetting(lines, nameof(SceneryColorChange)), ref SceneryColorChange);
            LoadInt(GetSetting(lines, nameof(SceneryColorRed)), ref SceneryColorRed);
            LoadInt(GetSetting(lines, nameof(SceneryColorGreen)), ref SceneryColorGreen);
            LoadInt(GetSetting(lines, nameof(SceneryColorBlue)), ref SceneryColorBlue);

            LoadInt(GetSetting(lines, nameof(PlaybackZoomType)), ref PlaybackZoomType);
            LoadFloat(GetSetting(lines, nameof(PlaybackZoomValue)), ref PlaybackZoomValue);
            LoadFloat(GetSetting(lines, nameof(Volume)), ref Volume);
            LoadFloat(GetSetting(lines, nameof(ScrollSensitivity)), ref ScrollSensitivity);
            LoadBool(GetSetting(lines, nameof(SuperZoom)), ref SuperZoom);
            LoadBool(GetSetting(lines, nameof(WhiteBG)), ref WhiteBG);
            LoadBool(GetSetting(lines, nameof(ColoredBG)), ref ColoredBG);
            LoadBool(GetSetting(lines, nameof(NightMode)), ref NightMode);
            LoadBool(GetSetting(lines, nameof(SmoothCamera)), ref SmoothCamera);
            LoadBool(GetSetting(lines, nameof(PredictiveCamera)), ref PredictiveCamera);
            LoadBool(GetSetting(lines, nameof(CheckForUpdates)), ref CheckForUpdates);
            LoadBool(GetSetting(lines, nameof(SmoothPlayback)), ref SmoothPlayback);
            LoadBool(GetSetting(lines, nameof(RoundLegacyCamera)), ref RoundLegacyCamera);
            LoadBool(GetSetting(lines, nameof(Record1080p)), ref Record1080p);
            LoadBool(GetSetting(lines, nameof(RecordSmooth)), ref RecordSmooth);
            LoadBool(GetSetting(lines, nameof(RecordMusic)), ref RecordMusic);
            LoadBool(GetSetting(lines, nameof(Editor.LifeLockNoFakie)), ref Editor.LifeLockNoFakie);
            LoadBool(GetSetting(lines, nameof(Editor.LifeLockNoOrange)), ref Editor.LifeLockNoOrange);
            LoadInt(GetSetting(lines, nameof(SettingsPane)), ref SettingsPane);
            LoadBool(GetSetting(lines, nameof(MuteAudio)), ref MuteAudio);
            LoadBool(GetSetting(lines, nameof(Editor.HitTest)), ref Editor.HitTest);
            LoadBool(GetSetting(lines, nameof(Editor.SnapNewLines)), ref Editor.SnapNewLines);
            LoadBool(GetSetting(lines, nameof(Editor.SnapMoveLine)), ref Editor.SnapMoveLine);
            LoadBool(GetSetting(lines, nameof(Editor.ForceXySnap)), ref Editor.ForceXySnap);
            LoadBool(GetSetting(lines, nameof(Editor.MomentumVectors)), ref Editor.MomentumVectors);
            LoadBool(GetSetting(lines, nameof(Editor.RenderGravityWells)), ref Editor.RenderGravityWells);
            LoadBool(GetSetting(lines, nameof(Editor.DrawContactPoints)), ref Editor.DrawContactPoints);
            LoadBool(GetSetting(lines, nameof(PreviewMode)), ref PreviewMode);
            LoadInt(GetSetting(lines, nameof(SlowmoSpeed)), ref SlowmoSpeed);
            LoadFloat(GetSetting(lines, nameof(DefaultPlayback)), ref DefaultPlayback);
            LoadBool(GetSetting(lines, nameof(ColorPlayback)), ref ColorPlayback);
            LoadBool(GetSetting(lines, nameof(OnionSkinning)), ref OnionSkinning);
            LoadBool(GetSetting(lines, nameof(Editor.ShowLineLength)), ref Editor.ShowLineLength);
            LoadBool(GetSetting(lines, nameof(Editor.ShowLineAngle)), ref Editor.ShowLineAngle);
            LoadBool(GetSetting(lines, nameof(Editor.ShowLineID)), ref Editor.ShowLineID);
            LoadFloat(GetSetting(lines, nameof(OnionSkinningFront)), ref OnionSkinningFront);
            LoadFloat(GetSetting(lines, nameof(OnionSkinningBack)), ref OnionSkinningBack);
            var lasttrack = GetSetting(lines, nameof(LastSelectedTrack));

            SelectedScarf = GetSetting(lines, nameof(SelectedScarf));
            LoadInt(GetSetting(lines, nameof(ScarfSegments)), ref ScarfSegments);
            LoadInt(GetSetting(lines, nameof(multiScarfSegments)), ref multiScarfSegments);
            LoadInt(GetSetting(lines, nameof(multiScarfAmount)), ref multiScarfAmount);

            SelectedBoshSkin = GetSetting(lines, nameof(SelectedBoshSkin));
            LoadBool(GetSetting(lines, nameof(CustomScarfOnPng)), ref CustomScarfOnPng);

            LoadBool(GetSetting(lines, nameof(ConfettiLines)), ref ConfettiLines);

            if (File.Exists(lasttrack) && lasttrack.StartsWith(Constants.TracksDirectory))
            {
                LastSelectedTrack = lasttrack;
            }
            foreach (Hotkey hk in Enum.GetValues(typeof(Hotkey)))
            {
                if (hk == Hotkey.None)
                    continue;
                LoadKeybinding(lines, hk);
            }

            Volume = MathHelper.Clamp(Settings.Volume, 0, 100);
            LoadDefaultKeybindings();
        }
        public static void Save()
        {
            string config = MakeSetting(nameof(LastSelectedTrack), LastSelectedTrack);

            config += "\r\n" + MakeSetting(nameof(MinutesElapsed), MinutesElapsed.ToString(Program.Culture));

            config += "\r\n" + MakeSetting(nameof(ConfigTContactPoints), ConfigTContactPoints.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(ConfigTGravityWells), ConfigTGravityWells.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(ConfigTHitTest), ConfigTHitTest.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(ConfigTMomentumVectors), ConfigTMomentumVectors.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(ConfigTOnionSkinning), ConfigTOnionSkinning.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(ToggleConfig), ToggleConfig.ToString(Program.Culture));

            config += "\r\n" + MakeSetting(nameof(PointVar), PointVar.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(OffsledSledCam), OffsledSledCam.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(OffsledVar), OffsledVar.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(FixedCam), FixedCam.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(XFixed), XFixed.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(YFixed), YFixed.ToString(Program.Culture));

            config += "\r\n" + MakeSetting(nameof(HorizontalTracking), HorizontalTracking.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(VerticalTracking), VerticalTracking.ToString(Program.Culture));

            config += "\r\n" + MakeSetting(nameof(XY), XY.ToString(Program.Culture));

            config += "\r\n" + MakeSetting(nameof(RedColored), RedColored.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(GreenColored), GreenColored.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(BlueColored), BlueColored.ToString(Program.Culture));

            config += "\r\n" + MakeSetting(nameof(LineColorRed), LineColorRed.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(LineColorGreen), LineColorGreen.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(LineColorBlue), LineColorBlue.ToString(Program.Culture));

            config += "\r\n" + MakeSetting(nameof(AccelerationLineColorRed), AccelerationLineColorRed.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(AccelerationLineColorGreen), AccelerationLineColorGreen.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(AccelerationLineColorBlue), AccelerationLineColorBlue.ToString(Program.Culture));

            config += "\r\n" + MakeSetting(nameof(SceneryLineColorRed), SceneryLineColorRed.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(SceneryLineColorGreen), SceneryLineColorGreen.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(SceneryLineColorBlue), SceneryLineColorBlue.ToString(Program.Culture));
                
            config += "\r\n" + MakeSetting(nameof(MainLine), MainLine.ToString(Program.Culture));

            config += "\r\n" + MakeSetting(nameof(NormalColorChange), NormalColorChange.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(NormalColorRed), NormalColorRed.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(NormalColorGreen), NormalColorGreen.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(NormalColorBlue), NormalColorBlue.ToString(Program.Culture));

            config += "\r\n" + MakeSetting(nameof(AccelerationColorChange), AccelerationColorChange.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(AccelerationColorRed), AccelerationColorRed.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(AccelerationColorGreen), AccelerationColorGreen.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(AccelerationColorBlue), AccelerationColorBlue.ToString(Program.Culture));

            config += "\r\n" + MakeSetting(nameof(SceneryColorChange), SceneryColorChange.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(SceneryColorRed), SceneryColorRed.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(SceneryColorGreen), SceneryColorGreen.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(SceneryColorBlue), SceneryColorBlue.ToString(Program.Culture));

            config += "\r\n" + MakeSetting(nameof(Volume), Volume.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(SuperZoom), SuperZoom.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(WhiteBG), WhiteBG.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(ColoredBG), ColoredBG.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(NightMode), NightMode.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(SmoothCamera), SmoothCamera.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(PredictiveCamera), PredictiveCamera.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(CheckForUpdates), CheckForUpdates.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(SmoothPlayback), SmoothPlayback.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(PlaybackZoomType), PlaybackZoomType.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(PlaybackZoomValue), PlaybackZoomValue.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(RoundLegacyCamera), RoundLegacyCamera.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Record1080p), Record1080p.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(RecordSmooth), RecordSmooth.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(RecordMusic), RecordMusic.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(ScrollSensitivity), ScrollSensitivity.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.LifeLockNoFakie), Editor.LifeLockNoFakie.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.LifeLockNoOrange), Editor.LifeLockNoOrange.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(SettingsPane), SettingsPane.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(MuteAudio), MuteAudio.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.HitTest), Editor.HitTest.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.SnapNewLines), Editor.SnapNewLines.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.SnapMoveLine), Editor.SnapMoveLine.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.ForceXySnap), Editor.ForceXySnap.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.MomentumVectors), Editor.MomentumVectors.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.RenderGravityWells), Editor.RenderGravityWells.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.DrawContactPoints), Editor.DrawContactPoints.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(PreviewMode), PreviewMode.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(SlowmoSpeed), SlowmoSpeed.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(DefaultPlayback), DefaultPlayback.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(ColorPlayback), ColorPlayback.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(OnionSkinning), OnionSkinning.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.ShowLineAngle), Editor.ShowLineAngle.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.ShowLineLength), Editor.ShowLineLength.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(Editor.ShowLineID), Editor.ShowLineID.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(OnionSkinningFront), OnionSkinningFront.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(OnionSkinningBack), OnionSkinningBack.ToString(Program.Culture));

            config += "\r\n" + MakeSetting(nameof(SelectedScarf), SelectedScarf);
            config += "\r\n" + MakeSetting(nameof(ScarfSegments), ScarfSegments.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(multiScarfAmount), multiScarfAmount.ToString(Program.Culture));
            config += "\r\n" + MakeSetting(nameof(multiScarfSegments), multiScarfSegments.ToString(Program.Culture));

            config += "\r\n" + MakeSetting(nameof(SelectedBoshSkin), SelectedBoshSkin);
            config += "\r\n" + MakeSetting(nameof(CustomScarfOnPng), CustomScarfOnPng.ToString(Program.Culture));

            config += "\r\n" + MakeSetting(nameof(ConfettiLines), ConfettiLines.ToString(Program.Culture));

            foreach (var binds in Keybinds)
            {
                foreach (var bind in binds.Value)
                {
                    if (KeybindConflicts[binds.Key] == KeyConflicts.HardCoded)
                        continue;
                    if (!bind.IsEmpty)
                    {
                        string keybind = "";
                        if (bind.UsesModifiers)
                            keybind += bind.Modifiers.ToString();
                        if (bind.UsesKeys)
                        {
                            if (keybind.Length > 0)
                                keybind += "+";
                            keybind += bind.Key.ToString();
                        }
                        if (bind.UsesMouse)
                        {
                            if (keybind.Length > 0)
                                keybind += "+";
                            keybind += bind.MouseButton.ToString();
                        }
                        config += "\r\n" +
                            MakeSetting(binds.Key.ToString(), $"[{keybind}]");
                    }
                }
            }
            try
            {
                File.WriteAllText(Program.UserDirectory + "settings.conf", config);
            }
            catch { }
        }
        private static void LoadKeybinding(string[] config, Hotkey hotkey)
        {
            if (KeybindConflicts[hotkey] == KeyConflicts.HardCoded)
                return;
            int line = 0;
            var hotkeyname = hotkey.ToString();
            var setting = GetSetting(config, hotkeyname, ref line);
            if (setting != null)
                Keybinds[hotkey] = new List<Keybinding>();
            while (setting != null)
            {
                line++;
                var items = setting.Trim(' ', '\t', '[', ']').Split('+');
                Keybinding ret = new Keybinding();
                foreach (var item in items)
                {
                    if (!ret.UsesModifiers &&
                        Enum.TryParse<KeyModifiers>(item, true, out var modifiers))
                    {
                        ret.Modifiers = modifiers;
                    }
                    else if (!ret.UsesKeys &&
                        Enum.TryParse<Key>(item, true, out Key key))
                    {
                        ret.Key = key;
                    }
                    else if (!ret.UsesMouse &&
                        Enum.TryParse<MouseButton>(item, true, out var mouse))
                    {
                        ret.MouseButton = mouse;
                    }
                }

                try
                {
                    if (!ret.IsEmpty)
                        CreateKeybind(hotkey, ret);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"An error occured loading the hotkey {hotkey}\n{e}");
                }
                setting = GetSetting(config, hotkeyname, ref line);
            }

        }
        private static string GetSetting(string[] config, string name)
        {
            int start = 0;
            return GetSetting(config, name, ref start);
        }
        private static string GetSetting(string[] config, string name, ref int start)
        {
            for (int i = start; i < config.Length; i++)
            {
                var idx = config[i].IndexOf("=");
                if (idx != -1 && idx + 1 < config[i].Length && config[i].Substring(0, idx) == name)//split[0] == name && split.Length > 1)
                {

                    var split = config[i].Substring(idx + 1);
                    start = i;
                    return split;
                }
            }
            return null;
        }
        private static string MakeSetting(string name, string value)
        {
            return name + "=" + value;
        }
        private static void LoadInt(string setting, ref int var)
        {
            int val;
            if (int.TryParse(setting, System.Globalization.NumberStyles.Integer, Program.Culture, out val))
                var = val;
        }
        private static void LoadFloat(string setting, ref float var)
        {
            float val;
            if (float.TryParse(setting, System.Globalization.NumberStyles.Float, Program.Culture, out val))
                var = val;
        }
        private static void LoadBool(string setting, ref bool var)
        {
            bool val;
            if (bool.TryParse(setting, out val))
                var = val;
        }
    }
}
