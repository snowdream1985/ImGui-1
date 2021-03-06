﻿using ImGui.Common.Primitive;
using ImGui.Rendering;
using Xunit;

namespace ImGui.UnitTest.Rendering
{
    public class PathPrimitiveFacts
    {
        public class PathMoveTo
        {
            [Fact]
            public void Works()
            {
                var primitive = new PathPrimitive();
                primitive.PathMoveTo(Point.Zero);

                Assert.Single(primitive.Path);
                Assert.IsType<MoveToCommand>(primitive.Path[0]);
                var cmd = (MoveToCommand) primitive.Path[0];
                Assert.Equal(Point.Zero, cmd.Point);
            }
        }

        public class PathLineTo
        {
            [Fact]
            public void Works()
            {
                var primitive = new PathPrimitive();
                primitive.PathMoveTo(Point.Zero);
                primitive.PathLineTo(new Point(0, 10));
                primitive.PathLineTo(new Point(10, 10));
                primitive.PathLineTo(new Point(10, 0));

                Assert.Equal(4, primitive.Path.Count);
                {
                    Assert.IsType<MoveToCommand>(primitive.Path[0]);
                    var cmd = (MoveToCommand)primitive.Path[0];
                    Assert.Equal(Point.Zero, cmd.Point);
                }
                {
                    var cmd = (LineToCommand)primitive.Path[1];
                    Assert.Equal(new Point(0, 10), cmd.Point);
                }
                {
                    var cmd = (LineToCommand)primitive.Path[2];
                    Assert.Equal(new Point(10, 10), cmd.Point);
                }
                {
                    var cmd = (LineToCommand)primitive.Path[3];
                    Assert.Equal(new Point(10, 0), cmd.Point);
                }
            }
        }

        public class PathArcToFast
        {
            [Fact]
            public void Works()
            {
                var primitive = new PathPrimitive();
                primitive.PathMoveTo(Point.Zero);
                primitive.PathArcFast(new Point(10, 0), 10, 3, 6);

                {
                    var cmd1 = (ArcCommand)primitive.Path[1];
                    var center = cmd1.Center;
                    var amin = cmd1.Amin;
                    var amax = cmd1.Amax;
                    Assert.Equal(10, center.x, precision: 2);
                    Assert.Equal(0, center.y, precision: 2);
                    Assert.Equal(3, amin);
                    Assert.Equal(6, amax);
                }
            }

            [Fact]
            public void Draw()
            {
                var primitive = new PathPrimitive();
                primitive.PathMoveTo(Point.Zero);
                primitive.PathArcFast(new Point(10, 0), 10, 6, 9);
                primitive.PathStroke(1, Color.Black);

                Util.DrawPathPrimitive(primitive);
            }
        }
    }
}