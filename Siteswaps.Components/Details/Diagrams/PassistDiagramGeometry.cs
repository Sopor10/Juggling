using System.Globalization;

namespace Siteswaps.Components.Details.Diagrams;

/// <summary>
/// Geometry helpers for passist-style causal/ladder SVG paths
/// (port of CausalDiagramWidget.arrow from helbling/passist).
/// </summary>
public static class PassistDiagramGeometry
{
    public const decimal XOffset = 55m;
    public const decimal YOffset = 70m;
    public const decimal RowHeight = 100m;
    public const decimal NodeRadius = 13m;
    public const decimal ArrowHeadLength = 20m;

    public static decimal ColumnWidth(decimal timeStretchFactor) => 70m / timeStretchFactor;

    public static decimal X(decimal time, decimal dx) => XOffset + time * dx;

    public static decimal Y(int line) => YOffset + line * RowHeight;

    public static (decimal Width, decimal Height) ViewSize(
        int steps,
        int jugglerCount,
        decimal timeStretchFactor
    )
    {
        var dx = ColumnWidth(timeStretchFactor);
        var width = steps * dx + 50;
        var height =
            (jugglerCount - (jugglerCount > 1 ? 1m : 1.4m)) * RowHeight + 2 * YOffset;
        return (width, height);
    }

    public static string BuildArrowPath(
        decimal time,
        int step,
        int fromLine,
        int toLine,
        decimal timeStretchFactor
    )
    {
        var dx = ColumnWidth(timeStretchFactor);
        var dy = RowHeight;
        var inv = CultureInfo.InvariantCulture;

        string Xy(decimal t, decimal shorten, decimal towardsX, decimal towardsY, int line)
        {
            var px = X(t, dx);
            var py = Y(line);
            if (shorten != 0)
            {
                var ddx = (double)(towardsX - px);
                var ddy = (double)(towardsY - py);
                var len = Math.Sqrt(ddx * ddx + ddy * ddy);
                if (len > 0.001)
                {
                    px += shorten * (decimal)(ddx / len);
                    py += shorten * (decimal)(ddy / len);
                }
            }

            return $"{px.ToString(inv)},{py.ToString(inv)}";
        }

        var time2 = time + step;
        var absStep = Math.Abs(step);

        if (
            fromLine != toLine
            || absStep >= timeStretchFactor * 0.7m && absStep <= timeStretchFactor
        )
        {
            return "M"
                + Xy(time, NodeRadius, X(time2, dx), Y(toLine), fromLine)
                + " L"
                + Xy(time2, NodeRadius + ArrowHeadLength, X(time, dx), Y(fromLine), toLine);
        }

        var dirX = X(time2, dx) > X(time, dx) ? 1m : -1m;
        var dirY = fromLine != 0 ? 1m : -1m;
        var offsetX = dirX * dy / 2;
        var offsetY = dirY * dy / 2;

        if (step == 0)
        {
            offsetX /= 2;
            dirX /= 2;
        }
        else if (absStep < timeStretchFactor)
        {
            offsetX = 0;
            dirX = 0;
        }

        var controlPoint1 =
            $"{(X(time, dx) + offsetX).ToString(inv)},{(Y(fromLine) + offsetY).ToString(inv)}";
        var controlPoint2 =
            $"{(X(time2, dx) - offsetX).ToString(inv)},{(Y(fromLine) + offsetY).ToString(inv)}";

        return "M"
            + Xy(time, NodeRadius, X(time, dx) + dirX, Y(fromLine) + dirY, fromLine)
            + "C"
            + controlPoint1
            + " "
            + controlPoint2
            + " "
            + Xy(
                time2,
                NodeRadius + ArrowHeadLength,
                X(time2, dx) - dirX,
                Y(fromLine) + dirY,
                toLine
            );
    }
}
