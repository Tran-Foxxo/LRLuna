using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using Gwen;
using Gwen.Controls;
using OpenTK.Graphics;
using linerider.Tools;
using linerider.Utils;
using linerider.Game;

namespace linerider.UI
{
    public class PreferencesWindow : DialogBase
    {
        private CollapsibleList _prefcontainer;
        private ControlBase _focus;
        private int _tabscount = 0;
        public PreferencesWindow(GameCanvas parent, Editor editor) : base(parent, editor)
        {
            Title = "Preferences";
            SetSize(450, 425);
            MinimumSize = Size;
            ControlBase bottom = new ControlBase(this)
            {
                Dock = Dock.Bottom,
                AutoSizeToContents = true,
            };
            Button defaults = new Button(bottom)
            {
                Dock = Dock.Right,
                Margin = new Margin(0, 2, 0, 0),
                Text = "Restore Defaults"
            };
            defaults.Clicked += (o, e) => RestoreDefaults();
            _prefcontainer = new CollapsibleList(this)
            {
                Dock = Dock.Left,
                AutoSizeToContents = false,
                Width = 100,
                Margin = new Margin(0, 0, 5, 0)
            };
            MakeModal(true);
            Setup();
        }
        private void RestoreDefaults()
        {
            var mbox = MessageBox.Show(
                _canvas,
                "Are you sure? This cannot be undone.", "Restore Defaults",
                MessageBox.ButtonType.OkCancel,
                true);
            mbox.RenameButtons("Restore");
            mbox.Dismissed += (o, e) =>
            {
                if (e == DialogResult.OK)
                {
                    Settings.RestoreDefaultSettings();
                    Settings.Save();
                    _editor.InitCamera();
                    Close();// this is lazy, but i don't want to update the ui
                }
            };
        }

        // Individual line options

        private void PopulateAccelLine(ControlBase parent)
        {
            var accel = GwenHelper.CreateHeaderPanel(parent, "Line options");
            GwenHelper.AddCheckbox(accel, "Line Customization", Settings.AccelerationColorChange, (o, e) =>
            {
                Settings.AccelerationColorChange = ((Checkbox)o).IsChecked;
                Settings.Save();
            });


            var coloraccel = GwenHelper.CreateHeaderPanel(parent, "Line Color");
            HorizontalSlider redSlider = new HorizontalSlider(null)
            {
                Min = 0,
                Max = 255,
                Value = Settings.AccelerationColorRed,
                Width = 80,
            };
            redSlider.ValueChanged += (o, e) =>
            {
                Settings.AccelerationColorRed = (float)redSlider.Value;
                Constants.AccelerationRed = (float)redSlider.Value;
                Settings.Save();
                Constants.RedLineColored = Color.FromArgb((int)Constants.AccelerationRed, (int)Constants.AccelerationGreen, (int)Constants.AccelerationBlue);
            };
            GwenHelper.CreateLabeledControl(coloraccel, "Red", redSlider);
            redSlider.Width = 200;
            HorizontalSlider greenSlider = new HorizontalSlider(null)
            {
                Min = 0,
                Max = 255,
                Value = Settings.AccelerationColorGreen,
                Width = 80,
            };
            greenSlider.ValueChanged += (o, e) =>
            {
                Settings.AccelerationColorGreen = (float)greenSlider.Value;
                Constants.AccelerationGreen = (float)greenSlider.Value;
                Settings.Save();
                Constants.RedLineColored = Color.FromArgb((int)Constants.AccelerationRed, (int)Constants.AccelerationGreen, (int)Constants.AccelerationBlue);
            };
            GwenHelper.CreateLabeledControl(coloraccel, "Green", greenSlider);
            greenSlider.Width = 200;
            HorizontalSlider blueSlider = new HorizontalSlider(null)
            {
                Min = 0,
                Max = 255,
                Value = Settings.AccelerationColorBlue,
                Width = 80,
            };
            blueSlider.ValueChanged += (o, e) =>
            {
                Settings.AccelerationColorBlue = (float)blueSlider.Value;
                Constants.AccelerationBlue = (float)blueSlider.Value;
                Settings.Save();
                Constants.RedLineColored = Color.FromArgb((int)Constants.AccelerationRed, (int)Constants.AccelerationGreen, (int)Constants.AccelerationBlue);
            };
            GwenHelper.CreateLabeledControl(coloraccel, "Blue", blueSlider);
            blueSlider.Width = 200;
        }

        private void PopulateNormalLine(ControlBase parent)
        {
            var normal = GwenHelper.CreateHeaderPanel(parent, "Line options");
            GwenHelper.AddCheckbox(normal, "Line Customization", Settings.NormalColorChange, (o, e) =>
            {
                Settings.NormalColorChange = ((Checkbox)o).IsChecked;
                Settings.Save();
            });


            var colornormal = GwenHelper.CreateHeaderPanel(parent, "Line Color");
            HorizontalSlider redSlider = new HorizontalSlider(null)
            {
                Min = 0,
                Max = 255,
                Value = Settings.NormalColorRed,
                Width = 80,
            };
            redSlider.ValueChanged += (o, e) =>
            {
                Settings.NormalColorRed = (float)redSlider.Value;
                Constants.NormalRed = (float)redSlider.Value;
                Settings.Save();
                Constants.BlueLineColored = Color.FromArgb((int)Constants.NormalRed, (int)Constants.NormalGreen, (int)Constants.NormalBlue);
            };
            GwenHelper.CreateLabeledControl(colornormal, "Red", redSlider);
            redSlider.Width = 200;
            HorizontalSlider greenSlider = new HorizontalSlider(null)
            {
                Min = 0,
                Max = 255,
                Value = Settings.NormalColorGreen,
                Width = 80,
            };
            greenSlider.ValueChanged += (o, e) =>
            {
                Settings.NormalColorGreen = (float)greenSlider.Value;
                Constants.NormalGreen = (float)greenSlider.Value;
                Settings.Save();
                Constants.BlueLineColored = Color.FromArgb((int)Constants.NormalRed, (int)Constants.NormalGreen, (int)Constants.NormalBlue);
            };
            GwenHelper.CreateLabeledControl(colornormal, "Green", greenSlider);
            greenSlider.Width = 200;
            HorizontalSlider blueSlider = new HorizontalSlider(null)
            {
                Min = 0,
                Max = 255,
                Value = Settings.NormalColorBlue,
                Width = 80,
            };
            blueSlider.ValueChanged += (o, e) =>
            {
                Settings.NormalColorBlue = (float)blueSlider.Value;
                Constants.NormalBlue = (float)blueSlider.Value;
                Settings.Save();
                Constants.BlueLineColored = Color.FromArgb((int)Constants.NormalRed, (int)Constants.NormalGreen, (int)Constants.NormalBlue);
            };
            GwenHelper.CreateLabeledControl(colornormal, "Blue", blueSlider);
            blueSlider.Width = 200;
        }

        private void PopulateSceneryLine(ControlBase parent)
        {
            var scenery = GwenHelper.CreateHeaderPanel(parent, "Line options");
            GwenHelper.AddCheckbox(scenery, "Line Customization", Settings.SceneryColorChange, (o, e) =>
            {
                Settings.SceneryColorChange = ((Checkbox)o).IsChecked;
                Settings.Save();
            });


            var colorscenery = GwenHelper.CreateHeaderPanel(parent, "Line Color");
            HorizontalSlider redSlider = new HorizontalSlider(null)
            {
                Min = 0,
                Max = 255,
                Value = Settings.SceneryColorRed,
                Width = 80,
            };
            redSlider.ValueChanged += (o, e) =>
            {
                Settings.SceneryColorRed = (float)redSlider.Value;
                Constants.SceneryRed = (float)redSlider.Value;
                Settings.Save();
                Constants.SceneryLineColored = Color.FromArgb((int)Constants.SceneryRed, (int)Constants.SceneryGreen, (int)Constants.SceneryBlue);
            };
            GwenHelper.CreateLabeledControl(colorscenery, "Red", redSlider);
            redSlider.Width = 200;
            HorizontalSlider greenSlider = new HorizontalSlider(null)
            {
                Min = 0,
                Max = 255,
                Value = Settings.SceneryColorGreen,
                Width = 80,
            };
            greenSlider.ValueChanged += (o, e) =>
            {
                Settings.SceneryColorGreen = (float)greenSlider.Value;
                Constants.SceneryGreen = (float)greenSlider.Value;
                Settings.Save();
                Constants.SceneryLineColored = Color.FromArgb((int)Constants.SceneryRed, (int)Constants.SceneryGreen, (int)Constants.SceneryBlue);
            };
            GwenHelper.CreateLabeledControl(colorscenery, "Green", greenSlider);
            greenSlider.Width = 200;
            HorizontalSlider blueSlider = new HorizontalSlider(null)
            {
                Min = 0,
                Max = 255,
                Value = Settings.SceneryColorBlue,
                Width = 80,
            };
            blueSlider.ValueChanged += (o, e) =>
            {
                Settings.SceneryColorBlue = (float)blueSlider.Value;
                Constants.SceneryBlue = (float)blueSlider.Value;
                Settings.Save();
                Constants.SceneryLineColored = Color.FromArgb((int)Constants.SceneryRed, (int)Constants.SceneryGreen, (int)Constants.SceneryBlue);
            };
            GwenHelper.CreateLabeledControl(colorscenery, "Blue", blueSlider);
            blueSlider.Width = 200;
        }

        // End of Individual line options

        private void PopulateLines2(ControlBase parent)
        {
            var lineoptions2 = GwenHelper.CreateHeaderPanel(parent, "XY Lock options");
            Spinner xylock = new Spinner(lineoptions2)
            {
                Dock = Dock.Bottom,
                Max = 180,
                Min = 1,
                Value = Settings.XY,
            };
            xylock.ValueChanged += (o, e) =>
            {
                Settings.XY = (int)xylock.Value;
                Settings.Save();
            };
        }

        private void PopulateLines(ControlBase parent)
        {
            var lineoptions = GwenHelper.CreateHeaderPanel(parent, "Line options");
            GwenHelper.AddCheckbox(lineoptions, "Line Customization", Settings.MainLine, (o, e) =>
            {
                Settings.MainLine = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var coloroptions = GwenHelper.CreateHeaderPanel(parent, "Color options");
            HorizontalSlider redSlider = new HorizontalSlider(null)
            {
                Min = 0,
                Max = 255,
                Value = Settings.LineColorRed,
                Width = 80,
            };
            redSlider.ValueChanged += (o, e) =>
            {
                Settings.LineColorRed = (float)redSlider.Value;
                Constants.LineRed = (float)redSlider.Value;
                Settings.Save();
                Constants.ColorDefaultLine = Color.FromArgb((int)Constants.LineRed, (int)Constants.LineGreen, (int)Constants.LineBlue);
            };
            GwenHelper.CreateLabeledControl(coloroptions, "Main Red", redSlider);
            redSlider.Width = 200;
            HorizontalSlider greenSlider = new HorizontalSlider(null)
            {
                Min = 0,
                Max = 255,
                Value = Settings.LineColorGreen,
                Width = 80,
            };
            greenSlider.ValueChanged += (o, e) =>
            {
                Settings.LineColorGreen = (float)greenSlider.Value;
                Constants.LineGreen = (float)greenSlider.Value;
                Settings.Save();
                Constants.ColorDefaultLine = Color.FromArgb((int)Constants.LineRed, (int)Constants.LineGreen, (int)Constants.LineBlue);
            };
            GwenHelper.CreateLabeledControl(coloroptions, "Main Green", greenSlider);
            greenSlider.Width = 200;
            HorizontalSlider blueSlider = new HorizontalSlider(null)
            {
                Min = 0,
                Max = 255,
                Value = Settings.LineColorBlue,
                Width = 80,
            };
            blueSlider.ValueChanged += (o, e) =>
            {
                Settings.LineColorBlue = (float)blueSlider.Value;
                Constants.LineBlue = (float)blueSlider.Value;
                Settings.Save();
                Constants.ColorDefaultLine = Color.FromArgb((int)Constants.LineRed, (int)Constants.LineGreen, (int)Constants.LineBlue);
            };
            GwenHelper.CreateLabeledControl(coloroptions, "Main Blue", blueSlider);
            blueSlider.Width = 200;

            //Buttons

            Button lyellow = new Button(parent)
            {
                Dock = Dock.Left,
                Margin = new Margin(0, 2, 0, 150),
                Text = "Yellow"
            };
            lyellow.Clicked += (o, e) => LineYellow();
            void LineYellow()
            {
                Settings.LineColorRed = 255;
                Constants.LineRed = Settings.LineColorRed;
                Settings.LineColorGreen = 255;
                Constants.LineGreen = Settings.LineColorGreen;
                Settings.LineColorBlue = 0;
                Constants.LineBlue = Settings.LineColorBlue;
                Constants.ColorDefaultLine = Constants.ColorDefaultLine = Color.FromArgb((int)Constants.LineRed, (int)Constants.LineGreen, (int)Constants.LineBlue);
            }

            Button laqua = new Button(parent)
            {
                Dock = Dock.Left,
                Margin = new Margin(0, 2, 0, 150),
                Text = "Aqua"
            };
            laqua.Clicked += (o, e) => LineAqua();
            void LineAqua()
            {
                Settings.LineColorRed = 0;
                Constants.LineRed = Settings.LineColorRed;
                Settings.LineColorGreen = 255;
                Constants.LineGreen = Settings.LineColorGreen;
                Settings.LineColorBlue = 255;
                Constants.LineBlue = Settings.LineColorBlue;
                Constants.ColorDefaultLine = Constants.ColorDefaultLine = Color.FromArgb((int)Constants.LineRed, (int)Constants.LineGreen, (int)Constants.LineBlue);
            }

            Button lmagenta = new Button(parent)
            {
                Dock = Dock.Left,
                Margin = new Margin(0, 2, 0, 150),
                Text = "Magenta"
            };
            lmagenta.Clicked += (o, e) => LineMagenta();
            void LineMagenta()
            {
                Settings.LineColorRed = 255;
                Constants.LineRed = Settings.LineColorRed;
                Settings.LineColorGreen = 0;
                Constants.LineGreen = Settings.LineColorGreen;
                Settings.LineColorBlue = 255;
                Constants.LineBlue = Settings.LineColorBlue;
                Constants.ColorDefaultLine = Constants.ColorDefaultLine = Color.FromArgb((int)Constants.LineRed, (int)Constants.LineGreen, (int)Constants.LineBlue);
            }

            Button lred = new Button(parent)
            {
                Dock = Dock.Left,
                Margin = new Margin(0, 2, 0, 150),
                Text = "Red"
            };
            lred.Clicked += (o, e) => LineRed();
            void LineRed()
            {
                Settings.LineColorRed = 255;
                Constants.LineRed = Settings.LineColorRed;
                Settings.LineColorGreen = 0;
                Constants.LineGreen = Settings.LineColorGreen;
                Settings.LineColorBlue = 0;
                Constants.LineBlue = Settings.LineColorBlue;
                Constants.ColorDefaultLine = Constants.ColorDefaultLine = Color.FromArgb((int)Constants.LineRed, (int)Constants.LineGreen, (int)Constants.LineBlue);
            }

            Button lgreen = new Button(parent)
            {
                Dock = Dock.Left,
                Margin = new Margin(0, 2, 0, 150),
                Text = "Green"
            };
            lgreen.Clicked += (o, e) => LineGreen();
            void LineGreen()
            {
                Settings.LineColorRed = 0;
                Constants.LineRed = Settings.LineColorRed;
                Settings.LineColorGreen = 255;
                Constants.LineGreen = Settings.LineColorGreen;
                Settings.LineColorBlue = 0;
                Constants.LineBlue = Settings.LineColorBlue;
                Constants.ColorDefaultLine = Constants.ColorDefaultLine = Color.FromArgb((int)Constants.LineRed, (int)Constants.LineGreen, (int)Constants.LineBlue);
            }

            Button lblue = new Button(parent)
            {
                Dock = Dock.Left,
                Margin = new Margin(0, 2, 0, 150),
                Text = "Blue"
            };
            lblue.Clicked += (o, e) => LineBlue();
            void LineBlue()
            {
                Settings.LineColorRed = 0;
                Constants.LineRed = Settings.LineColorRed;
                Settings.LineColorGreen = 0;
                Constants.LineGreen = Settings.LineColorGreen;
                Settings.LineColorBlue = 255;
                Constants.LineBlue = Settings.LineColorBlue;
                Constants.ColorDefaultLine = Constants.ColorDefaultLine = Color.FromArgb((int)Constants.LineRed, (int)Constants.LineGreen, (int)Constants.LineBlue);
            }

            Button lgray = new Button(parent)
            {
                Dock = Dock.Left,
                Margin = new Margin(0, 2, 0, 150),
                Text = "Gray"
            };
            lgray.Clicked += (o, e) => LineGray();
            void LineGray()
            {
                Settings.LineColorRed = 127;
                Constants.LineRed = Settings.LineColorRed;
                Settings.LineColorGreen = 127;
                Constants.LineGreen = Settings.LineColorGreen;
                Settings.LineColorBlue = 127;
                Constants.LineBlue = Settings.LineColorBlue;
                Constants.ColorDefaultLine = Constants.ColorDefaultLine = Color.FromArgb((int)Constants.LineRed, (int)Constants.LineGreen, (int)Constants.LineBlue);
            }

            //End of Buttons
        }
        private void PopulateAudio(ControlBase parent)
        {
            var opts = GwenHelper.CreateHeaderPanel(parent, "Sync options");
            var syncenabled = GwenHelper.AddCheckbox(opts, "Mute", Settings.MuteAudio, (o, e) =>
               {
                   Settings.MuteAudio = ((Checkbox)o).IsChecked;
                   Settings.Save();
               });
            HorizontalSlider vol = new HorizontalSlider(null)
            {
                Min = 0,
                Max = 100,
                Value = Settings.Volume,
                Width = 80,
            };
            vol.ValueChanged += (o, e) =>
              {
                  Settings.Volume = (float)vol.Value;
                  Settings.Save();
              };
            GwenHelper.CreateLabeledControl(opts, "Volume", vol);
            vol.Width = 200;
        }
        private void PopulateKeybinds(ControlBase parent)
        {
            var hk = new HotkeyWidget(parent);
        }
        private void PopulateModes(ControlBase parent)
        {
            var background = GwenHelper.CreateHeaderPanel(parent, "Background Color");
            GwenHelper.AddCheckbox(background, "Night Mode", Settings.NightMode, (o, e) =>
               {
                   Settings.NightMode = ((Checkbox)o).IsChecked;
                   Settings.Save();
               });
            var whitebg = GwenHelper.AddCheckbox(background, "Pure White Background", Settings.WhiteBG, (o, e) =>
               {
                   Settings.WhiteBG = ((Checkbox)o).IsChecked;
                   Settings.Save();
               });
            var coloredbg = GwenHelper.AddCheckbox(background, "Colored Background", Settings.ColoredBG, (o, e) =>
            {
                Settings.ColoredBG = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var panelgeneral = GwenHelper.CreateHeaderPanel(parent, "General");
            var superzoom = GwenHelper.AddCheckbox(panelgeneral, "Superzoom", Settings.SuperZoom, (o, e) =>
               {
                   Settings.SuperZoom = ((Checkbox)o).IsChecked;
                   Settings.Save();
               });
            ComboBox scroll = GwenHelper.CreateLabeledCombobox(panelgeneral, "Scroll Sensitivity:");
            scroll.Margin = new Margin(0, 0, 0, 0);
            scroll.Dock = Dock.Bottom;
            scroll.AddItem("0.25x").Name = "0.25";
            scroll.AddItem("0.5x").Name = "0.5";
            scroll.AddItem("0.75x").Name = "0.75";
            scroll.AddItem("1x").Name = "1";
            scroll.AddItem("2x").Name = "2";
            scroll.AddItem("3x").Name = "3";
            scroll.SelectByName("1");//default if user setting fails.
            scroll.SelectByName(Settings.ScrollSensitivity.ToString(Program.Culture));
            scroll.ItemSelected += (o, e) =>
            {
                if (e.SelectedItem != null)
                {
                    Settings.ScrollSensitivity = float.Parse(e.SelectedItem.Name, Program.Culture);
                    Settings.Save();
                }
            };
            superzoom.Tooltip = "Allows the user to zoom in\nnearly 10x more than usual.";

            var colbg = GwenHelper.CreateHeaderPanel(parent, "Colored background options");
            HorizontalSlider redSlider = new HorizontalSlider(null)
            {
                Min = 0,
                Max = 255,
                Value = Settings.RedColored,
                Width = 80,
            };
            redSlider.ValueChanged += (o, e) =>
            {
                Settings.RedColored = (float)redSlider.Value;
                Constants.Red = (float)redSlider.Value;
                Settings.Save();
                Constants.ColorColored = new Color4(((float)Constants.Red / 255), ((float)Constants.Green / 255), ((float)Constants.Blue / 255), 255);
            };
            GwenHelper.CreateLabeledControl(colbg, "Red", redSlider);
            redSlider.Width = 200;
            HorizontalSlider greenSlider = new HorizontalSlider(null)
            {
                Min = 0,
                Max = 255,
                Value = Settings.GreenColored,
                Width = 80,
            };
            greenSlider.ValueChanged += (o, e) =>
            {
                Settings.GreenColored = (float)greenSlider.Value;
                Constants.Green = (float)greenSlider.Value;
                Settings.Save();
                Constants.ColorColored = new Color4(((float)Constants.Red / 255), ((float)Constants.Green / 255), ((float)Constants.Blue / 255), 255);
            };
            GwenHelper.CreateLabeledControl(colbg, "Green", greenSlider);
            greenSlider.Width = 200;
            HorizontalSlider blueSlider = new HorizontalSlider(null)
            {
                Min = 0,
                Max = 255,
                Value = Settings.BlueColored,
                Width = 80,
            };
            blueSlider.ValueChanged += (o, e) =>
            {
                Settings.BlueColored = (float)blueSlider.Value;
                Constants.Blue = (float)blueSlider.Value;
                Settings.Save();
                Constants.ColorColored = new Color4(((float)Constants.Red / 255), ((float)Constants.Green / 255), ((float)Constants.Blue / 255), 255);
            };
            GwenHelper.CreateLabeledControl(colbg, "Blue", blueSlider);
            blueSlider.Width = 200;

            //Buttons

            Button bgyellow = new Button(parent)
            {
                Dock = Dock.Left,
                Margin = new Margin(0, 2, 0, 0),
                Text = "Yellow"
            };
            bgyellow.Clicked += (o, e) => backgroundYellow();
            void backgroundYellow()
            {
                Settings.RedColored = 255;
                Constants.Red = Settings.RedColored;
                Settings.GreenColored = 255;
                Constants.Green = Settings.GreenColored;
                Settings.BlueColored = 0;
                Constants.Blue = Settings.BlueColored;
                Constants.ColorColored = new Color4(((float)Constants.Red / 255), ((float)Constants.Green / 255), ((float)Constants.Blue / 255), 255);
            }

            Button bgaqua = new Button(parent)
            {
                Dock = Dock.Left,
                Margin = new Margin(0, 2, 0, 0),
                Text = "Aqua"
            };
            bgaqua.Clicked += (o, e) => backgroundAqua();
            void backgroundAqua()
            {
                Settings.RedColored = 0;
                Constants.Red = Settings.RedColored;
                Settings.GreenColored = 255;
                Constants.Green = Settings.GreenColored;
                Settings.BlueColored = 255;
                Constants.Blue = Settings.BlueColored;
                Constants.ColorColored = new Color4(((float)Constants.Red / 255), ((float)Constants.Green / 255), ((float)Constants.Blue / 255), 255);
            }

            Button bgmagenta = new Button(parent)
            {
                Dock = Dock.Left,
                Margin = new Margin(0, 2, 0, 0),
                Text = "Magenta"
            };
            bgmagenta.Clicked += (o, e) => backgroundMagenta();
            void backgroundMagenta()
            {
                Settings.RedColored = 255;
                Constants.Red = Settings.RedColored;
                Settings.GreenColored = 0;
                Constants.Green = Settings.GreenColored;
                Settings.BlueColored = 255;
                Constants.Blue = Settings.BlueColored;
                Constants.ColorColored = new Color4(((float)Constants.Red / 255), ((float)Constants.Green / 255), ((float)Constants.Blue / 255), 255);
            }

            Button bgred = new Button(parent)
            {
                Dock = Dock.Left,
                Margin = new Margin(0, 2, 0, 0),
                Text = "Red"
            };
            bgred.Clicked += (o, e) => backgroundRed();
            void backgroundRed()
            {
                Settings.RedColored = 255;
                Constants.Red = Settings.RedColored;
                Settings.GreenColored = 0;
                Constants.Green = Settings.GreenColored;
                Settings.BlueColored = 0;
                Constants.Blue = Settings.BlueColored;
                Constants.ColorColored = new Color4(((float)Constants.Red / 255), ((float)Constants.Green / 255), ((float)Constants.Blue / 255), 255);
            }

            Button bggreen = new Button(parent)
            {
                Dock = Dock.Left,
                Margin = new Margin(0, 2, 0, 0),
                Text = "Green"
            };
            bggreen.Clicked += (o, e) => backgroundGreen();
            void backgroundGreen()
            {
                Settings.RedColored = 0;
                Constants.Red = Settings.RedColored;
                Settings.GreenColored = 255;
                Constants.Green = Settings.GreenColored;
                Settings.BlueColored = 0;
                Constants.Blue = Settings.BlueColored;
                Constants.ColorColored = new Color4(((float)Constants.Red / 255), ((float)Constants.Green / 255), ((float)Constants.Blue / 255), 255);
            }

            Button bgblue = new Button(parent)
            {
                Dock = Dock.Left,
                Margin = new Margin(0, 2, 0, 0),
                Text = "Blue"
            };
            bgblue.Clicked += (o, e) => backgroundBlue();
            void backgroundBlue()
            {
                Settings.RedColored = 0;
                Constants.Red = Settings.RedColored;
                Settings.GreenColored = 0;
                Constants.Green = Settings.GreenColored;
                Settings.BlueColored = 255;
                Constants.Blue = Settings.BlueColored;
                Constants.ColorColored = new Color4(((float)Constants.Red / 255), ((float)Constants.Green / 255), ((float)Constants.Blue / 255), 255);
            }

            Button bggray = new Button(parent)
            {
                Dock = Dock.Left,
                Margin = new Margin(0, 2, 0, 0),
                Text = "Gray"
            };
            bggray.Clicked += (o, e) => backgroundGray();
            void backgroundGray()
            {
                Settings.RedColored = 127;
                Constants.Red = Settings.RedColored;
                Settings.GreenColored = 127;
                Constants.Green = Settings.GreenColored;
                Settings.BlueColored = 127;
                Constants.Blue = Settings.BlueColored;
                Constants.ColorColored = new Color4(((float)Constants.Red / 255), ((float)Constants.Green / 255), ((float)Constants.Blue / 255), 255);
            }

            //End of Buttons
        }
        private void PopulateCamera(ControlBase parent)
        {
            var camtype = GwenHelper.CreateHeaderPanel(parent, "Camera Type");
            var camtracking = GwenHelper.CreateHeaderPanel(parent, "Camera Tracking");
            var camprops = GwenHelper.CreateHeaderPanel(parent, "Camera Properties");
            RadioButtonGroup rbcamera = new RadioButtonGroup(camtype)
            {
                Dock = Dock.Top,
                ShouldDrawBackground = false,
            };
            var soft = rbcamera.AddOption("Soft Camera");
            var predictive = rbcamera.AddOption("Predictive Camera");
            var legacy = rbcamera.AddOption("Legacy Camera");

            RadioButtonGroup xycamera = new RadioButtonGroup(camtracking)
            {
                Dock = Dock.Top,
                ShouldDrawBackground = false,
            };
            var horizontal = xycamera.AddOption("Horizontal Tracking");
            var vertical = xycamera.AddOption("Vertical Tracking");
            var horizontalvertical = xycamera.AddOption("Normal Tracking");

            var round = GwenHelper.AddCheckbox(camprops, "Round Legacy Camera", Settings.RoundLegacyCamera, (o, e) =>
            {
                Settings.RoundLegacyCamera = ((Checkbox)o).IsChecked;
                Settings.Save();
                _editor.InitCamera();
            });
            var offsledsled = GwenHelper.AddCheckbox(camprops, "Offsled Sled Camera", Settings.OffsledSledCam, (o, e) =>
            {
            Settings.OffsledSledCam = ((Checkbox)o).IsChecked;
            Settings.Save();
            _editor.InitCamera();
            });
            var offsledvar = GwenHelper.AddCheckbox(camprops, "Variable Contact Point Camera", Settings.OffsledVar, (o, e) =>
            {
                Settings.OffsledVar = ((Checkbox)o).IsChecked;
                Settings.Save();
                _editor.InitCamera();
            });
            var fixedcam = GwenHelper.AddCheckbox(camprops, "Fixed Camera", Settings.FixedCam, (o, e) =>
            {
                Settings.FixedCam = ((Checkbox)o).IsChecked;
                Settings.Save();
                _editor.InitCamera();
            });
            var variables = GwenHelper.CreateHeaderPanel(parent, "Contact Point Settings");
            Spinner pointvar = new Spinner(variables)
            {
                Dock = Dock.Bottom,
                Max = 9,
                Min = 0,
                Value = Settings.PointVar,
            };
            pointvar.ValueChanged += (o, e) =>
            {
                Settings.PointVar = (int)pointvar.Value;
                Settings.Save();
            };
            var fixedpos = GwenHelper.CreateHeaderPanel(parent, "Fixed Camera X and Y Position");
            Spinner yfixed = new Spinner(fixedpos)
            {
                Dock = Dock.Bottom,
                Max = 2147483648,
                Min = -2147483648,
                Value = Settings.XFixed 
            };
            yfixed.ValueChanged += (o, e) =>
            {
                Settings.YFixed = (int)yfixed.Value;
                Settings.Save();
                _editor.InitCamera();
            };
            Spinner xfixed = new Spinner(fixedpos)
            {
                Dock = Dock.Bottom,
                Max = 2147483648,
                Min = -2147483648,
                Value = Settings.YFixed
            };
            xfixed.ValueChanged += (o, e) =>
            {
                Settings.XFixed = (int)xfixed.Value;
                Settings.Save();
                _editor.InitCamera();
            };
            if (Settings.SmoothCamera)
            {
                if (Settings.PredictiveCamera)
                    predictive.Select();
                else
                    soft.Select();
            }
            else
            {
                legacy.Select();
            }

            if (Settings.HorizontalTracking)
            {
                horizontal.Select();
            }
            else if (Settings.VerticalTracking)
            {
                vertical.Select();
            }
            else horizontalvertical.Select();

            horizontal.Checked += (o, e) =>
            {
                Settings.HorizontalTracking = true;
                Settings.VerticalTracking = false;
                Settings.Save();
                _editor.InitCamera();
            };
            vertical.Checked += (o, e) =>
            {
                Settings.HorizontalTracking = false;
                Settings.VerticalTracking = true;
                Settings.Save();
                _editor.InitCamera();
            };
            horizontalvertical.Checked += (o, e) =>
            {
                Settings.HorizontalTracking = false;
                Settings.VerticalTracking = false;
                Settings.Save();
                _editor.InitCamera();

            };

            soft.Checked += (o, e) =>
            {
                Settings.SmoothCamera = true;
                Settings.PredictiveCamera = false;
                Settings.Save();
                round.IsDisabled = Settings.SmoothCamera;
                _editor.InitCamera();
            };
            predictive.Checked += (o, e) =>
            {
                Settings.SmoothCamera = true;
                Settings.PredictiveCamera = true;
                Settings.Save();
                round.IsDisabled = Settings.SmoothCamera;
                _editor.InitCamera();
            };
            legacy.Checked += (o, e) =>
            {
                Settings.SmoothCamera = false;
                Settings.PredictiveCamera = false;
                Settings.Save();
                round.IsDisabled = Settings.SmoothCamera;
                _editor.InitCamera();

            };
            predictive.Tooltip = "This is the camera that was added in 1.03\nIt moves relative to the future of the track";
        }
        private void PopulateEditor(ControlBase parent)
        {
            Panel advancedtools = GwenHelper.CreateHeaderPanel(parent, "Advanced Visualization");

            var contact = GwenHelper.AddCheckbox(advancedtools, "Contact Points", Settings.Editor.DrawContactPoints, (o, e) =>
            {
                Settings.Editor.DrawContactPoints = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var momentum = GwenHelper.AddCheckbox(advancedtools, "Momentum Vectors", Settings.Editor.MomentumVectors, (o, e) =>
            {
                Settings.Editor.MomentumVectors = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var hitbox = GwenHelper.AddCheckbox(advancedtools, "Line Hitbox", Settings.Editor.RenderGravityWells, (o, e) =>
            {
                Settings.Editor.RenderGravityWells = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var hittest = GwenHelper.AddCheckbox(advancedtools, "Hit Test", Settings.Editor.HitTest, (o, e) =>
             {
                 Settings.Editor.HitTest = ((Checkbox)o).IsChecked;
                 Settings.Save();
             });
            var onion = GwenHelper.AddCheckbox(advancedtools, "Onion Skinning", Settings.OnionSkinning, (o, e) =>
            {
                Settings.OnionSkinning = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            Panel pblifelock = GwenHelper.CreateHeaderPanel(parent, "Lifelock Conditions");
            GwenHelper.AddCheckbox(pblifelock, "Next frame constraints", Settings.Editor.LifeLockNoOrange, (o, e) =>
            {
                Settings.Editor.LifeLockNoOrange = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            GwenHelper.AddCheckbox(pblifelock, "No Fakie Death", Settings.Editor.LifeLockNoFakie, (o, e) =>
            {
                Settings.Editor.LifeLockNoFakie = ((Checkbox)o).IsChecked;
                Settings.Save();
            });

            var overlay = GwenHelper.CreateHeaderPanel(parent, "Frame Overlay");
            PopulateOverlay(overlay);
            
            onion.Tooltip = "Visualize the rider before/after\nthe current frame.";
            momentum.Tooltip = "Visualize the direction of\nmomentum for each contact point";
            contact.Tooltip = "Visualize the parts of the rider\nthat interact with lines.";
            hitbox.Tooltip = "Visualizes the hitbox of lines\nUsed for advanced editing";
            hittest.Tooltip = "Lines that have been hit by\nthe rider will glow.";
        }
        private void PopulatePlayback(ControlBase parent)
        {
            var playbackzoom = GwenHelper.CreateHeaderPanel(parent, "Playback Zoom");
            RadioButtonGroup pbzoom = new RadioButtonGroup(playbackzoom)
            {
                Dock = Dock.Left,
                ShouldDrawBackground = false,
            };
            pbzoom.AddOption("Default Zoom");
            pbzoom.AddOption("Current Zoom");
            pbzoom.AddOption("Specific Zoom");
            Spinner playbackspinner = new Spinner(pbzoom)
            {
                Dock = Dock.Bottom,
                Max = 24,
                Min = 1,
            };
            pbzoom.SelectionChanged += (o, e) =>
            {
                Settings.PlaybackZoomType = ((RadioButtonGroup)o).SelectedIndex;
                Settings.Save();
                playbackspinner.IsHidden = (((RadioButtonGroup)o).SelectedLabel != "Specific Zoom");
            };
            playbackspinner.ValueChanged += (o, e) =>
            {
                Settings.PlaybackZoomValue = (float)((Spinner)o).Value;
                Settings.Save();
            };
            pbzoom.SetSelection(Settings.PlaybackZoomType);
            playbackspinner.Value = Settings.PlaybackZoomValue;

            var playbackmode = GwenHelper.CreateHeaderPanel(parent, "Playback Color");
            GwenHelper.AddCheckbox(playbackmode, "Color Playback", Settings.ColorPlayback, (o, e) =>
               {
                   Settings.ColorPlayback = ((Checkbox)o).IsChecked;
                   Settings.Save();
               });
            var preview = GwenHelper.AddCheckbox(playbackmode, "Preview Mode", Settings.PreviewMode, (o, e) =>
               {
                   Settings.PreviewMode = ((Checkbox)o).IsChecked;
                   Settings.Save();
               });
            var recording = GwenHelper.AddCheckbox(playbackmode, "Recording Mode", Settings.Local.RecordingMode, (o, e) =>
               {
                   Settings.Local.RecordingMode = ((Checkbox)o).IsChecked;
               });
            var framerate = GwenHelper.CreateHeaderPanel(parent, "Frame Control");
            var smooth = GwenHelper.AddCheckbox(framerate, "Smooth Playback", Settings.SmoothPlayback, (o, e) =>
               {
                   Settings.SmoothPlayback = ((Checkbox)o).IsChecked;
                   Settings.Save();
               });
            ComboBox pbrate = GwenHelper.CreateLabeledCombobox(framerate, "Playback Rate:");
            for (var i = 0; i < Constants.MotionArray.Length; i++)
            {
                var f = (Constants.MotionArray[i] / (float)Constants.PhysicsRate);
                pbrate.AddItem(f + "x", f.ToString(CultureInfo.InvariantCulture), f);
            }
            pbrate.SelectByName(Settings.DefaultPlayback.ToString(CultureInfo.InvariantCulture));
            pbrate.ItemSelected += (o, e) =>
            {
                Settings.DefaultPlayback = (float)e.SelectedItem.UserData;
                Settings.Save();
            };
            var cbslowmo = GwenHelper.CreateLabeledCombobox(framerate, "Slowmo FPS:");
            var fpsarray = new[] { 1, 2, 5, 10, 20 };
            for (var i = 0; i < fpsarray.Length; i++)
            {
                cbslowmo.AddItem(fpsarray[i].ToString(), fpsarray[i].ToString(CultureInfo.InvariantCulture),
                    fpsarray[i]);
            }
            cbslowmo.SelectByName(Settings.SlowmoSpeed.ToString(CultureInfo.InvariantCulture));
            cbslowmo.ItemSelected += (o, e) =>
            {
                Settings.SlowmoSpeed = (int)e.SelectedItem.UserData;
                Settings.Save();
            };
            smooth.Tooltip = "Interpolates frames from the base\nphysics rate of 40 frames/second\nup to 60 frames/second";
        }
        private void PopulateOverlay(ControlBase parent)
        {
            var offset = new Spinner(null)
            {
                Min = -999,
                Max = 999,
                Value = Settings.Local.TrackOverlayOffset,
            };
            offset.ValueChanged += (o,e)=>
            {
                Settings.Local.TrackOverlayOffset = (int)offset.Value;
            };
            var fixedspinner = new Spinner(null)
            {
                Min = 0,
                Max = _editor.FrameCount,
                Value = Settings.Local.TrackOverlayFixedFrame,
            };
            fixedspinner.ValueChanged += (o, e) =>
            {
                Settings.Local.TrackOverlayFixedFrame = (int)fixedspinner.Value;
            };
            void updatedisabled()
            {
                offset.IsDisabled = Settings.Local.TrackOverlayFixed;
                fixedspinner.IsDisabled = !Settings.Local.TrackOverlayFixed;
            }
            var enabled = GwenHelper.AddCheckbox(parent, "Enabled", Settings.Local.TrackOverlay, (o, e) =>
            {
                Settings.Local.TrackOverlay = ((Checkbox)o).IsChecked;
                updatedisabled();
            });
            GwenHelper.AddCheckbox(parent, "Fixed Frame", Settings.Local.TrackOverlayFixed, (o, e) =>
            {
                Settings.Local.TrackOverlayFixed = ((Checkbox)o).IsChecked;
                updatedisabled();
            });
            GwenHelper.CreateLabeledControl(parent, "Frame Offset", offset);
            GwenHelper.CreateLabeledControl(parent, "Frame ID", fixedspinner);
            updatedisabled();
            enabled.Tooltip = "Display an onion skin of the track\nat a specified offset for animation";
        }
        private void PopulateTools(ControlBase parent)
        {
            var select = GwenHelper.CreateHeaderPanel(parent, "Select Tool -- Line Info");
            var length = GwenHelper.AddCheckbox(select, "Show Length", Settings.Editor.ShowLineLength, (o, e) =>
               {
                   Settings.Editor.ShowLineLength = ((Checkbox)o).IsChecked;
                   Settings.Save();
               });
            var angle = GwenHelper.AddCheckbox(select, "Show Angle", Settings.Editor.ShowLineAngle, (o, e) =>
               {
                   Settings.Editor.ShowLineAngle = ((Checkbox)o).IsChecked;
                   Settings.Save();
               });
            var showid = GwenHelper.AddCheckbox(select, "Show ID", Settings.Editor.ShowLineID, (o, e) =>
               {
                   Settings.Editor.ShowLineID = ((Checkbox)o).IsChecked;
                   Settings.Save();
               });
            Panel panelSnap = GwenHelper.CreateHeaderPanel(parent, "Snapping");
            var linesnap = GwenHelper.AddCheckbox(panelSnap, "Snap New Lines", Settings.Editor.SnapNewLines, (o, e) =>
            {
                Settings.Editor.SnapNewLines = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var movelinesnap = GwenHelper.AddCheckbox(panelSnap, "Snap Line Movement", Settings.Editor.SnapMoveLine, (o, e) =>
            {
                Settings.Editor.SnapMoveLine = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var forcesnap = GwenHelper.AddCheckbox(panelSnap, "Force X/Y snap", Settings.Editor.ForceXySnap, (o, e) =>
            {
                Settings.Editor.ForceXySnap = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var onsk = GwenHelper.CreateHeaderPanel(parent, "Onion Skinning Options");
            HorizontalSlider osb = new HorizontalSlider(null)
            {
                Min = 0,
                Max = 50,
                Value = Settings.OnionSkinningBack,
                Width = 80,
            };
            osb.ValueChanged += (o, e) =>
            {
                Settings.OnionSkinningBack = (int)osb.Value;
                Settings.Save();
            };
            GwenHelper.CreateLabeledControl(onsk, "Onionskins Back", osb);
            osb.Width = 200;

            HorizontalSlider osf = new HorizontalSlider(null)
            {
                Min = 0,
                Max = 50,
                Value = Settings.OnionSkinningFront,
                Width = 80,
            };
            osf.ValueChanged += (o, e) =>
            {
                Settings.OnionSkinningFront = (int)osf.Value;
                Settings.Save();
            };
            GwenHelper.CreateLabeledControl(onsk, "Onionskins Front", osf);
            osf.Width = 200;

            forcesnap.Tooltip = "Forces all lines drawn to\nsnap to a 45 degree angle";
            movelinesnap.Tooltip = "Snap to lines when using the\nselect tool to move a single line";
        }
        private void PopulateOther(ControlBase parent)
        {
            var updates = GwenHelper.CreateHeaderPanel(parent, "Updates");

            var showid = GwenHelper.AddCheckbox(updates, "Check For Updates", Settings.CheckForUpdates, (o, e) =>
               {
                   Settings.CheckForUpdates = ((Checkbox)o).IsChecked;
                   Settings.Save();
               });
            var accel = GwenHelper.CreateHeaderPanel(parent, ("Time Elapsed: " + Settings.MinutesElapsed + " minutes"));

        }
        private void Setup()
        {
            var cat = _prefcontainer.Add("Settings");
            var page = AddPage(cat, "Editor");
            PopulateEditor(page);
            page = AddPage(cat, "Playback");
            PopulatePlayback(page);
            page = AddPage(cat, "Tools");
            PopulateTools(page);
            page = AddPage(cat, "Environment");
            PopulateModes(page);
            page = AddPage(cat, "Line Settings");
            PopulateLines(page);
            page = AddPage(cat, "Line Settings 2");
            PopulateLines2(page);
            page = AddPage(cat, "Normal Lines");
            PopulateNormalLine(page);
            page = AddPage(cat, "Accel Lines");
            PopulateAccelLine(page);
            page = AddPage(cat, "Scenery Lines");
            PopulateSceneryLine(page);
            page = AddPage(cat, "Camera");
            PopulateCamera(page);
            cat = _prefcontainer.Add("Application");
            page = AddPage(cat, "Audio");
            PopulateAudio(page);
            page = AddPage(cat, "Keybindings");
            PopulateKeybinds(page);
            page = AddPage(cat, "Other");
            PopulateOther(page);
            if (Settings.SettingsPane >= _tabscount && _focus == null)
            {
                Settings.SettingsPane = 0;
                _focus = page;
                page.Show();
            }

        }
        private void CategorySelected(object sender, ItemSelectedEventArgs e)
        {
            if (_focus != e.SelectedItem.UserData)
            {
                if (_focus != null)
                {
                    _focus.Hide();
                }
                _focus = (ControlBase)e.SelectedItem.UserData;
                _focus.Show();
                Settings.SettingsPane = (int)_focus.UserData;
                Settings.Save();
            }
        }
        private ControlBase AddPage(CollapsibleCategory category, string name)
        {
            var btn = category.Add(name);
            Panel panel = new Panel(this);
            panel.Dock = Dock.Fill;
            panel.Padding = Padding.Five;
            panel.Hide();
            panel.UserData = _tabscount;
            btn.UserData = panel;
            category.Selected += CategorySelected;
            if (_tabscount == Settings.SettingsPane)
                btn.Press();
            _tabscount += 1;
            return panel;
        }
    }
}
