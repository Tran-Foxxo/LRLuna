using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Gwen;
using Gwen.Controls;
using linerider.Tools;
using linerider.Utils;
using linerider.IO;
using linerider.Game;
using OpenTK;
using System.Linq;

namespace linerider.UI
{
    public class LineWindow : DialogBase
    {
        private PropertyTree _proptree;
        private GameLine _ownerline;
        private GameLine _linecopy;
        private NumberProperty _angleProp;
        private NumberProperty _length;
        private NumberProperty _width;
        private bool _linechangemade = false;
        private const string DefaultTitle = "Line Properties";
        private bool closing = false;
        public LineWindow(GameCanvas parent, Editor editor, GameLine line) : base(parent, editor)
        {
            _ownerline = line;
            _linecopy = _ownerline.Clone();
            Title = "Line Properties";
            Padding = new Padding(0, 0, 0, 0);
            AutoSizeToContents = true;
            _proptree = new PropertyTree(this)
            {
                Width = 220,
                Height = 200
            };
            _proptree.Dock = Dock.Top;
            MakeModal(true);
            Setup();
            _proptree.ExpandAll();
        }
        protected override void CloseButtonPressed(ControlBase control, EventArgs args)
        {
            if (closing || !_linechangemade)
            {
                closing = true;
                base.CloseButtonPressed(control, args);
            }
            else
            {
                WarnClose();
            }
        }
        public override bool Close()
        {
            if (closing || !_linechangemade)
            {
                closing = true;
                return base.Close();
            }
            else
            {
                WarnClose();
                return false;
            }
        }
        private void WarnClose()
        {
            var mbox = MessageBox.Show(
                _canvas,
                "The line has been modified. Do you want to save your changes?",
                "Save Changes?",
                MessageBox.ButtonType.YesNoCancel);
            mbox.RenameButtonsYN("Save", "Discard", "Cancel");
            mbox.MakeModal(false);
            mbox.Dismissed += (o, e) =>
            {
                switch (e)
                {
                    case DialogResult.Yes:
                        FinishChange();
                        closing = true;
                        base.Close();
                        break;
                    case DialogResult.No:
                        CancelChange();
                        closing = true;
                        base.Close();
                        break;
                }
            };
        }
        private void Setup()
        {
            SetupRedOptions(_proptree);
            SetupTriggers(_proptree);
            Panel bottom = new Panel(this)
            {
                Dock = Dock.Bottom,
                AutoSizeToContents = true,
                ShouldDrawBackground = false,
            };
            Button cancel = new Button(bottom)
            {
                Text = "Cancel",
                Dock = Dock.Right,
            };
            cancel.Clicked += (o, e) =>
            {
                CancelChange();
                closing = true;
                Close();
            };
            Button ok = new Button(bottom)
            {
                Text = "Okay",
                Dock = Dock.Right,
                Margin = new Margin(0, 0, 5, 0)
            };
            ok.Clicked += (o, e) =>
            {
                FinishChange();
                closing = true;
                Close();
            };
        }
        private void SetupRedOptions(PropertyTree tree)
        {
            var vec = _ownerline.GetVector();
            var len = vec.Length;
            var angle = Angle.FromVector(vec);
            angle.Degrees += 90;
            var lineProp = tree.Add("Line Properties", 120);

            Console.WriteLine(_ownerline.GetType().ToString());

            if (!(_ownerline is SceneryLine scenery))
            {
                var id = new NumberProperty(lineProp)
                {
                    Min = 0,
                    Max = int.MaxValue - 1,
                    NumberValue = _ownerline.ID,
                    OnlyWholeNumbers = true,
                };
                id.ValueChanged += (o, e) =>
                {
                    ChangeID((int)id.NumberValue);
                };
                lineProp.Add("ID", id);
            }

            _length = new NumberProperty(lineProp)
            {
                Min = 0.0000001,
                Max = double.MaxValue - 1,
                NumberValue = len,
            };
            _length.ValueChanged += (o, e) =>
            {
                ChangeLength(_length.NumberValue);
            };
            lineProp.Add("Length", _length);

            _angleProp = new NumberProperty(lineProp)
            {
                Min = 0,
                Max = 360,
                NumberValue = angle.Degrees,
            };
            _angleProp.ValueChanged += (o, e) =>
            {
                ChangeAngle(_angleProp.NumberValue);
            };
            lineProp.Add("Angle", _angleProp);

            if (!(_ownerline is SceneryLine))
            {
                var multilines = new NumberProperty(lineProp)
                {
                    Min = 1,
                    Max = int.MaxValue - 1,
                    OnlyWholeNumbers = true,
                };
                multilines.NumberValue = GetMultiLines(true).Count;
                multilines.ValueChanged += (o, e) =>
                {
                    Multiline((int)multilines.NumberValue);
                };
                lineProp.Add("Multilines", multilines);
            }

            if (_ownerline is SceneryLine sceneryLine)
            {
                _width = new NumberProperty(lineProp)
                {
                    Min = 0.1,
                    Max = 25.5,
                    NumberValue = _ownerline.Width,
                };
                _width.ValueChanged += (o, e) =>
                {
                    ChangeWidth(_width.NumberValue);
                };
                lineProp.Add("Width", _width);
            }

            if (_ownerline is RedLine red)
            {
                var acceleration = tree.Add("Acceleration", 120);
                var multiplier = new NumberProperty(acceleration)
                {
                    Min = 1,
                    Max = 255,
                    NumberValue = red.Multiplier,
                    OnlyWholeNumbers = true,
                };
                multiplier.ValueChanged += (o, e) =>
                {
                    ChangeMultiplier((int)multiplier.NumberValue);
                };
                acceleration.Add("Multiplier", multiplier);

                var accelinverse = GwenHelper.AddPropertyCheckbox(
                    acceleration,
                    "Inverse",
                    red.inv
                    );
                accelinverse.ValueChanged += (o, e) =>
                {
                    using (var trk = _editor.CreateTrackWriter())
                    {
                        var multi = GetMultiLines(false);
                        foreach (var l in multi)
                        {
                            var cpy = (StandardLine)l.Clone();
                            cpy.Position = l.Position2;
                            cpy.Position2 = l.Position;
                            cpy.inv = accelinverse.IsChecked;
                            UpdateLine(trk, l, cpy);
                        }

                        var owner = (StandardLine)_ownerline.Clone();
                        owner.Position = _ownerline.Position2;
                        owner.Position2 = _ownerline.Position;
                        owner.inv = accelinverse.IsChecked;
                        UpdateOwnerLine(trk, owner);
                    }
                };
            }
        }
        private void SetupTriggers(PropertyTree tree)
        {
            if (_ownerline is StandardLine physline)
            {
                var table = tree.Add("Triggers", 120);
                var currenttrigger = physline.Trigger;
                var triggerenabled = GwenHelper.AddPropertyCheckbox(
                    table,
                    "Enabled",
                    currenttrigger != null);

                var zoom = new NumberProperty(table)
                {
                    Min = Constants.MinimumZoom,
                    Max = Constants.MaxZoom,
                    NumberValue = 4
                };
                table.Add("Target Zoom", zoom);
                var frames = new NumberProperty(table)
                {
                    Min = 0,
                    Max = 40 * 60 * 2,//2 minutes is enough for a zoom trigger, you crazy nuts.
                    NumberValue = 40,
                    OnlyWholeNumbers = true,
                };
                if (currenttrigger != null)
                {
                    zoom.NumberValue = currenttrigger.ZoomTarget;
                    frames.NumberValue = currenttrigger.ZoomFrames;
                }
                table.Add("Frames", frames);
                zoom.ValueChanged += (o, e) =>
                {
                    using (var trk = _editor.CreateTrackWriter())
                    {
                        trk.DisableExtensionUpdating();
                        if (triggerenabled.IsChecked)
                        {
                            var cpy = (StandardLine)_ownerline.Clone();
                            cpy.Trigger.ZoomTarget = (float)zoom.NumberValue;
                            UpdateOwnerLine(trk, cpy);
                        }
                    }
                };
                frames.ValueChanged += (o, e) =>
                {
                    using (var trk = _editor.CreateTrackWriter())
                    {
                        trk.DisableExtensionUpdating();
                        if (triggerenabled.IsChecked)
                        {
                            var cpy = (StandardLine)_ownerline.Clone();
                            cpy.Trigger.ZoomFrames = (int)frames.NumberValue;
                            UpdateOwnerLine(trk, cpy);
                        }
                    }
                };
                triggerenabled.ValueChanged += (o, e) =>
                {
                    using (var trk = _editor.CreateTrackWriter())
                    {
                        trk.DisableExtensionUpdating();
                        var cpy = (StandardLine)_ownerline.Clone();
                        if (triggerenabled.IsChecked)
                        {
                            cpy.Trigger = new LineTrigger()
                            {
                                ZoomTrigger = true,
                                ZoomFrames = (int)frames.NumberValue,
                                ZoomTarget = (float)zoom.NumberValue
                            };

                            UpdateOwnerLine(trk, cpy);
                        }
                        else
                        {
                            cpy.Trigger = null;
                            UpdateOwnerLine(trk, cpy);
                        }
                    }
                };
            }
        }

        private void ChangeAngle(double numberValue)
        {
            var multilines = GetMultiLines(false);
            using (var trk = _editor.CreateTrackWriter())
            {
                var cpy = _ownerline.Clone();


                var angle = Angle.FromDegrees(numberValue - 90).Radians - Angle.FromVector(cpy.GetVector()).Radians;
                var ads = Angle.FromRadians(angle).Degrees;
                var newX = ((cpy.Position2.X - cpy.Position.X) * Math.Cos(angle)) - ((cpy.Position2.Y - cpy.Position.Y) * Math.Sin(angle)) + cpy.Position.X;
                var newY = ((cpy.Position2.Y - cpy.Position.Y) * Math.Cos(angle)) + ((cpy.Position2.X - cpy.Position.X) * Math.Sin(angle)) + cpy.Position.Y;
                var newPos = new Vector2d(newX, newY);
                cpy.Position2 = newPos;
                UpdateOwnerLine(trk, cpy);

                
                foreach (var line in multilines)
                {
                    var copy = line.Clone();
                    copy.Position2 = newPos;
                    UpdateLine(trk, line, copy);
                }
            }
        }

        private void ChangeWidth(double width)
        {
            using (var trk = _editor.CreateTrackWriter())
            {
                var cpy = _ownerline.Clone();
                cpy.Width = (float)width;
                UpdateOwnerLine(trk, cpy);
            }
        }

        private void ChangeID(int newID)
        {
            
        }

        private void ChangeLength(double length)
        {
            var multilines = GetMultiLines(false);
            using (var trk = _editor.CreateTrackWriter())
            {
                var cpy = _ownerline.Clone();
                var angle = Angle.FromVector(cpy.GetVector()).Radians;

                var x2 = cpy.Position.X + (length * Math.Cos(angle));
                var y2 = cpy.Position.Y + (length * Math.Sin(angle));

                var newPos = new Vector2d(x2,y2);
                cpy.Position2 = newPos;
                UpdateOwnerLine(trk, cpy);

                //System.Diagnostics.Debug.WriteLine(Angle.FromVector(cpy.GetVector()).Degrees);

                foreach (var line in multilines)
                {
                    var copy = line.Clone();
                    copy.Position2 = newPos;
                    UpdateLine(trk, line, copy);
                }
            }
        }

        private void UpdateOwnerLine(TrackWriter trk, GameLine replacement)
        {
            UpdateLine(trk, _ownerline, replacement);
            _ownerline = replacement;
        }
        private void UpdateLine(TrackWriter trk, GameLine current, GameLine replacement)
        {
            MakingChange();

            if (replacement is StandardLine stl)
            {
                stl.CalculateConstants();
            }
            trk.ReplaceLine(current, replacement);
            _editor.NotifyTrackChanged();
            _editor.Invalidate();
        }
        private void ChangeMultiplier(int mul)
        {
            var lines = GetMultiLines(false);
            using (var trk = _editor.CreateTrackWriter())
            {
                var cpy = (RedLine)_ownerline.Clone();
                cpy.Multiplier = mul;
                UpdateOwnerLine(trk, cpy);
                foreach (var line in lines)
                {
                    var copy = (RedLine)line.Clone();
                    copy.Multiplier = mul;
                    UpdateLine(trk, line, copy);
                }
            }
        }
        private SimulationCell GetMultiLines(bool includeowner)
        {
            SimulationCell multilines = new SimulationCell();
            using (var trk = _editor.CreateTrackReader())
            {
                var owner = (StandardLine)_ownerline;
                var lines = trk.GetLinesInRect(new Utils.DoubleRect(owner.Position, new Vector2d(1, 1)), false);
                foreach (var line in lines)
                {
                    if (
                        line is RedLine stl &&
                        owner is RedLine stls &&
                        line.Position == owner.Position &&
                        line.Position2 == owner.Position2 &&
                        (includeowner || line.ID != owner.ID))
                    {
                        multilines.AddLine(stl);
                    }
                    if (
                        line is StandardLine stlstd &&
                        owner is StandardLine stlstds &&
                        line.Position == owner.Position &&
                        line.Position2 == owner.Position2 &&
                        (includeowner || line.ID != owner.ID))
                    {
                        multilines.AddLine(stlstd);
                    }
                }
            }
            return multilines;
        }
        private void Multiline(int count)
        {
            SimulationCell multilines = GetMultiLines(false);
            using (var trk = _editor.CreateTrackWriter())
            {
                var owner = (StandardLine)_ownerline;
                MakingChange();
                // owner line doesn't count, but our min bounds is 1
                var diff = (count - 1) - multilines.Count;
                if (diff < 0)
                {
                    for (int i = 0; i > diff; i--)
                    {
                        trk.RemoveLine(multilines.First());
                        multilines.RemoveLine(multilines.First().ID);
                    }
                }
                else if (diff > 0)
                {
                    if (_ownerline is RedLine redline)
                    {
                        for (int i = 0; i < diff; i++)
                        {
                            var red = new RedLine(owner.Position, owner.Position2, owner.inv) { Multiplier = ((RedLine)owner).Multiplier };
                            red.CalculateConstants();
                            trk.AddLine(red);
                        }
                    }
                    else if (_ownerline is StandardLine blueline)
                    {
                        for (int i = 0; i < diff; i++)
                        {
                            var blue = new StandardLine(owner.Position, owner.Position2, owner.inv);
                            blue.CalculateConstants();
                            trk.AddLine(blue);
                        }
                    }
                }
            }
            _editor.NotifyTrackChanged();
        }
        private void MakingChange()
        {
            if (!_linechangemade)
            {
                _editor.UndoManager.BeginAction();
                _linechangemade = true;
                Title = DefaultTitle + " *";
            }
        }
        private void CancelChange()
        {
            if (_linechangemade)
            {
                _editor.UndoManager.CancelAction();
                _linechangemade = false;
            }
        }
        private void FinishChange()
        {
            if (_linechangemade)
            {
                _editor.UndoManager.EndAction();
                _linechangemade = false;
            }
        }
    }
}
