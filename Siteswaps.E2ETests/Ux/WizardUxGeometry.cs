using FluentAssertions;
using Microsoft.Playwright;

namespace Siteswaps.E2ETests.Ux;

/// <summary>Shared geometry and contrast checks for wizard UX contracts.</summary>
internal static class WizardUxGeometry
{
    public const float MinTouchPx = 40f;

    public const double MinBodyTextContrast = 4.5;

    public static async Task EnsureMobileViewportAsync(IPage page)
    {
        await page.SetViewportSizeAsync(390, 844);
    }

    public static async Task SwipeHorizontallyAsync(ILocator locator, int deltaX)
    {
        await locator.EvaluateAsync(
            @"((el, dx) => {
                const rect = el.getBoundingClientRect();
                const y = rect.top + Math.min(40, Math.max(8, rect.height / 2));
                const startX = dx < 0
                    ? rect.left + Math.min(rect.width - 8, Math.max(80, rect.width * 0.8))
                    : rect.left + Math.min(rect.width - 80, Math.max(8, rect.width * 0.2));
                const endX = startX + dx;
                const fire = (type, x, touchesAlive) => {
                    const touch = new Touch({
                        identifier: 1,
                        target: el,
                        clientX: x,
                        clientY: y,
                        pageX: x,
                        pageY: y,
                        radiusX: 2.5,
                        radiusY: 2.5,
                        rotationAngle: 0,
                        force: 1
                    });
                    el.dispatchEvent(new TouchEvent(type, {
                        bubbles: true,
                        cancelable: true,
                        touches: touchesAlive ? [touch] : [],
                        targetTouches: touchesAlive ? [touch] : [],
                        changedTouches: [touch]
                    }));
                };
                fire('touchstart', startX, true);
                fire('touchmove', startX + dx * 0.4, true);
                fire('touchmove', endX, true);
                fire('touchend', endX, false);
            })",
            deltaX
        );
    }

    public static async Task AssertMinTouchTargetAsync(ILocator locator, string controlName)
    {
        var box = await locator.BoundingBoxAsync();
        box.Should().NotBeNull($"{controlName} must be laid out");
        box!
            .Width.Should()
            .BeGreaterThanOrEqualTo(
                MinTouchPx,
                because: $"{controlName} width must be >= {MinTouchPx}px"
            );
        box.Height.Should()
            .BeGreaterThanOrEqualTo(
                MinTouchPx,
                because: $"{controlName} height must be >= {MinTouchPx}px"
            );
    }

    public static async Task AssertWebkitThumbMinSizeAsync(IPage page, string inputSelector)
    {
        var size = await page.EvaluateAsync<float[]>(
            @"((selector) => {
                const input = document.querySelector(selector);
                if (!input) {
                    return [0, 0];
                }
                const style = getComputedStyle(input, '::-webkit-slider-thumb');
                return [parseFloat(style.width) || 0, parseFloat(style.height) || 0];
            })",
            inputSelector
        );

        size[0]
            .Should()
            .BeGreaterThanOrEqualTo(
                MinTouchPx,
                because: "dual-range thumb width must be touch-sized"
            );
        size[1]
            .Should()
            .BeGreaterThanOrEqualTo(
                MinTouchPx,
                because: "dual-range thumb height must be touch-sized"
            );
    }

    public static async Task<bool> ResultsActionsCoverLastCardAsync(IPage page)
    {
        return await page.EvaluateAsync<bool>(
            @"() => {
                const card = document.querySelector('.wizard-results-grid .pz-siteswap-card:last-child');
                const actions = document.querySelector('.wizard-results-actions');
                if (!card || !actions) {
                    return false;
                }
                const cardRect = card.getBoundingClientRect();
                const actionsRect = actions.getBoundingClientRect();
                const verticallyOverlaps = cardRect.bottom > actionsRect.top + 1
                    && cardRect.top < actionsRect.bottom - 1;
                return verticallyOverlaps;
            }"
        );
    }

    public static async Task<bool> StickyNavCoversElementAsync(IPage page, string cssSelector)
    {
        return await page.EvaluateAsync<bool>(
            @"((selector) => {
                const el = document.querySelector(selector);
                const nav = document.querySelector('.wizard-nav');
                if (!el || !nav) {
                    return false;
                }
                const elRect = el.getBoundingClientRect();
                const navRect = nav.getBoundingClientRect();
                return elRect.bottom > navRect.top + 1 && elRect.top < navRect.bottom - 1;
            })",
            cssSelector
        );
    }

    public static async Task<double> ContrastRatioAsync(IPage page, string selector)
    {
        return await page.EvaluateAsync<double>(
            @"((selector) => {
                const el = document.querySelector(selector);
                if (!el) {
                    return 0;
                }
                const style = getComputedStyle(el);
                const fg = parseColor(style.color);
                let bg = parseColor(style.backgroundColor);
                let node = el.parentElement;
                while (node && bg.a < 0.99) {
                    const parentBg = parseColor(getComputedStyle(node).backgroundColor);
                    bg = blend(bg, parentBg);
                    node = node.parentElement;
                }
                const l1 = relativeLuminance(fg);
                const l2 = relativeLuminance(bg);
                const lighter = Math.max(l1, l2);
                const darker = Math.min(l1, l2);
                return (lighter + 0.05) / (darker + 0.05);

                function parseColor(value) {
                    const canvas = document.createElement('canvas');
                    const ctx = canvas.getContext('2d');
                    ctx.fillStyle = '#000';
                    ctx.fillStyle = value;
                    const computed = ctx.fillStyle;
                    if (typeof computed === 'string' && computed.startsWith('#')) {
                        const hex = computed.length === 4
                            ? '#' + computed[1] + computed[1] + computed[2] + computed[2] + computed[3] + computed[3]
                            : computed;
                        return {
                            r: parseInt(hex.slice(1, 3), 16),
                            g: parseInt(hex.slice(3, 5), 16),
                            b: parseInt(hex.slice(5, 7), 16),
                            a: 1
                        };
                    }
                    const m = String(computed).match(/rgba?\((\d+),\s*(\d+),\s*(\d+)(?:,\s*([\d.]+))?\)/);
                    if (!m) {
                        return { r: 0, g: 0, b: 0, a: 1 };
                    }
                    return {
                        r: Number(m[1]),
                        g: Number(m[2]),
                        b: Number(m[3]),
                        a: m[4] === undefined ? 1 : Number(m[4])
                    };
                }

                function blend(top, bottom) {
                    const a = top.a + bottom.a * (1 - top.a);
                    if (a === 0) {
                        return { r: 255, g: 255, b: 255, a: 1 };
                    }
                    return {
                        r: Math.round((top.r * top.a + bottom.r * bottom.a * (1 - top.a)) / a),
                        g: Math.round((top.g * top.a + bottom.g * bottom.a * (1 - top.a)) / a),
                        b: Math.round((top.b * top.a + bottom.b * bottom.a * (1 - top.a)) / a),
                        a
                    };
                }

                function relativeLuminance(color) {
                    const toLinear = (c) => {
                        const s = c / 255;
                        return s <= 0.03928 ? s / 12.92 : Math.pow((s + 0.055) / 1.055, 2.4);
                    };
                    return 0.2126 * toLinear(color.r)
                        + 0.7152 * toLinear(color.g)
                        + 0.0722 * toLinear(color.b);
                }
            })",
            selector
        );
    }

    public static async Task<string> ActiveElementSummaryAsync(IPage page)
    {
        return await page.EvaluateAsync<string>(
            @"() => {
                const el = document.activeElement;
                if (!el) {
                    return '';
                }
                const insideSheet = !!el.closest('.wizard-bottom-sheet');
                return `${el.tagName}:${insideSheet}`;
            }"
        );
    }
}
