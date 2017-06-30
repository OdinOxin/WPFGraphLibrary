using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;

namespace MirrorConfigClient.Graph
{
    public abstract class GeometryUtils
    {
        public const int ACCURACY = 5;

        public enum Side
        {
            Top,
            Left,
            Right,
            Bottom,
        }

        public static double Distance(Point p1, Point p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }

        public static double Distance(double X1, double X2, double Y1, double Y2)
        {
            return Math.Sqrt((X1 - X2) * (X1 - X2) + (Y1 - Y2) * (Y1 - Y2));
        }

        public static double Length(Line line)
        {
            return Distance(line.X1, line.X2, line.Y1, line.Y2);
        }

        public static string ToString(Line line)
        {
            return string.Format("( {0} / {1} ) -> ( {2} / {3} )", line.X1, line.Y1, line.X2, line.Y2);
        }

        public static Point? LineIntersection(Line a, Line b)
        {
            double aA = a.Y2 - a.Y1;
            double aB = a.X1 - a.X2;
            double aC = aA * a.X1 + aB * a.Y1;

            double bA = b.Y2 - b.Y1;
            double bB = b.X1 - b.X2;
            double bC = bA * b.X1 + bB * b.Y1;

            double det = aA * bB - bA * aB;
            if (det == 0)
                return null;
            Point intersection = new Point((bB * aC - aB * bC) / det, (aA * bC - bA * aC) / det);
            if (Math.Round(Math.Min(a.X1, a.X2), ACCURACY) <= Math.Round(intersection.X, ACCURACY) && Math.Round(intersection.X, ACCURACY) <= Math.Round(Math.Max(a.X1, a.X2), ACCURACY)
                && Math.Round(Math.Min(a.Y1, a.Y2), ACCURACY) <= Math.Round(intersection.Y, ACCURACY) && Math.Round(intersection.Y, ACCURACY) <= Math.Round(Math.Max(a.Y1, a.Y2), ACCURACY)
                && Math.Round(Math.Min(b.X1, b.X2), ACCURACY) <= Math.Round(intersection.X, ACCURACY) && Math.Round(intersection.X, ACCURACY) <= Math.Round(Math.Max(b.X1, b.X2), ACCURACY)
                && Math.Round(Math.Min(b.Y1, b.Y2), ACCURACY) <= Math.Round(intersection.Y, ACCURACY) && Math.Round(intersection.Y, ACCURACY) <= Math.Round(Math.Max(b.Y1, b.Y2), ACCURACY))
                return intersection;
            return null;
        }

        public static Dictionary<Side, Point> RectIntersection(Rect rect, Line line)
        {
            Dictionary<Side, Point> sides = new Dictionary<Side, Point>();
            Line tmp = new Line() { X1 = rect.TopLeft.X, Y1 = rect.TopLeft.Y, X2 = rect.TopRight.X, Y2 = rect.TopRight.Y };
            Point? intersection = GeometryUtils.LineIntersection(line, tmp);
            if (intersection != null)
                sides.Add(Side.Top, intersection.Value);
            tmp = new Line() { X1 = rect.TopRight.X, Y1 = rect.TopRight.Y, X2 = rect.BottomRight.X, Y2 = rect.BottomRight.Y };
            intersection = GeometryUtils.LineIntersection(line, tmp);
            if (intersection != null)
                sides.Add(Side.Right, intersection.Value);
            tmp = new Line() { X1 = rect.BottomLeft.X, Y1 = rect.BottomLeft.Y, X2 = rect.BottomRight.X, Y2 = rect.BottomRight.Y };
            intersection = GeometryUtils.LineIntersection(line, tmp);
            if (intersection != null)
                sides.Add(Side.Bottom, intersection.Value);
            tmp = new Line() { X1 = rect.TopLeft.X, Y1 = rect.TopLeft.Y, X2 = rect.BottomLeft.X, Y2 = rect.BottomLeft.Y };
            intersection = GeometryUtils.LineIntersection(line, tmp);
            if (intersection != null)
                sides.Add(Side.Left, intersection.Value);
            return sides;
        }

        public static double Angle(Line a, Line b)
        {
            return Math.Acos(((a.X2 - a.X1) * (b.X2 - b.X1) + (a.Y2 - a.Y1) * (b.Y2 - b.Y1)) / (Distance(a.X1, a.X2, a.Y1, a.Y2) * Distance(b.X1, b.X2, b.Y1, b.Y2)));
        }
    }
}
