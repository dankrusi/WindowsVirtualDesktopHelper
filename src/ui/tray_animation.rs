//! A short looping demo for the Tray icons page: open the hidden icons flyout, then drag
//! the helper's icons onto the taskbar as Previous, number, initial, Next.
//! It is its own component so each frame re-renders only this canvas, not the whole page.
use super::tray_preview::{bitmap_rect, DESKTOP_BLUE};
use std::time::Duration;
use windows_reactor::*;

/// `icons` holds the icons in their final order: Previous, number, initial, Next.
/// `dark` is the Settings window theme, which FontIcon glyphs follow.
#[derive(Clone, PartialEq)]
pub struct Input {
    pub dark: bool,
    pub icons: Vec<EncodedImage>,
}
pub struct TrayAnimation {
    time: f64,
    cursor: EncodedImage,
    animated: bool,
}
pub enum Message {
    Tick(f64),
}

const WIDTH: f64 = 520.;
const HEIGHT: f64 = 230.;
const TASKBAR_TOP: f64 = 182.;
const TASKBAR_HEIGHT: f64 = 48.;
const FRAME: f64 = 0.04;
const LOOP: f64 = 12.;
const REST: (f64, f64) = (430., 110.);
/// Wi-Fi, volume and battery sit in 32-wide cells from here; the clock follows them.
const SYSTEM_ICONS: f64 = 342.;
const ROW: f64 = 206.;
/// Dropped icons always land right next to Wi-Fi, like pinning a tray icon in Windows.
const DROP: (f64, f64) = (SYSTEM_ICONS - 16., ROW);
/// The Show hidden icons arrow before any icon is pinned: just left of Wi-Fi.
const CHEVRON: (f64, f64) = DROP;
/// The flyout opens above the arrow's starting place.
const FLYOUT: (f64, f64, f64, f64) = (CHEVRON.0 - 68., 78., 136., 96.);
/// Flyout cells (column, row) where each icon starts; deliberately out of order.
const SOURCE: [(f64, f64); 4] = [(0., 1.), (2., 0.), (1., 1.), (0., 0.)];
const OTHER_APPS: [((f64, f64), &str); 2] = [((1., 0.), "\u{E753}"), ((2., 1.), "\u{E8D6}")];
// Moments in the loop, in seconds.
const OPEN: f64 = 1.5;
const FIRST_DRAG: f64 = 1.9;
const DRAG: f64 = 1.8;
const CLOSE: f64 = FIRST_DRAG + 4. * DRAG;
const RETURNED: f64 = CLOSE + 1.1;
const FADE: f64 = LOOP - 0.6;

fn cell((column, row): (f64, f64)) -> (f64, f64) {
    (FLYOUT.0 + 28. + 40. * column, FLYOUT.1 + 28. + 40. * row)
}
/// Centre of pinned icon `index` when `pinned` icons (fractional while room opens) sit
/// between the arrow and Wi-Fi; the newest is always next to Wi-Fi.
fn pinned_x(index: usize, pinned: f64) -> f64 {
    DROP.0 - 32. * (pinned - 1. - index as f64)
}
fn progress(t: f64, start: f64, duration: f64) -> f64 {
    let x = ((t - start) / duration).clamp(0., 1.);
    x * x * (3. - 2. * x)
}
fn mix(a: (f64, f64), b: (f64, f64), x: f64) -> (f64, f64) {
    (a.0 + (b.0 - a.0) * x, a.1 + (b.1 - a.1) * x)
}

#[derive(Clone, Copy, Debug, PartialEq)]
enum Icon {
    InFlyout,
    Dragging(f64, f64),
    Pinned(f64),
}
#[derive(Debug, PartialEq)]
struct Scene {
    cursor: (f64, f64),
    pressed: bool,
    flyout: f64,
    chevron: f64,
    chevron_hover: bool,
    pinned_opacity: f64,
    icons: [Icon; 4],
}
fn scene(t: f64) -> Scene {
    let mut cursor = mix(REST, CHEVRON, progress(t, 0.6, 0.8));
    let mut pressed = (1.4..1.6).contains(&t);
    // Room for each icon opens while it is dragged in, before it is dropped.
    let pinned: f64 = (0..4)
        .map(|i| progress(t, FIRST_DRAG + DRAG * i as f64 + 1., 0.5))
        .sum();
    let mut icons = [Icon::InFlyout; 4];
    for (i, icon) in icons.iter_mut().enumerate() {
        let start = FIRST_DRAG + DRAG * i as f64;
        let source = cell(SOURCE[i]);
        if t >= start {
            cursor = mix(
                if i == 0 { CHEVRON } else { DROP },
                source,
                progress(t, start, 0.6),
            );
        }
        if (start + 0.6..start + 0.8).contains(&t) {
            pressed = true;
        }
        if t >= start + 0.8 {
            cursor = mix(source, DROP, progress(t, start + 0.8, 0.8));
            if t < start + 1.6 {
                pressed = true;
                *icon = Icon::Dragging(cursor.0, cursor.1);
            } else {
                *icon = Icon::Pinned(pinned_x(i, pinned));
            }
        }
    }
    if t >= CLOSE + 0.3 {
        cursor = mix(DROP, REST, progress(t, CLOSE + 0.3, 0.8));
    }
    // At the end the pinned icons fade where they are and the arrow slides back to Wi-Fi.
    let reset = progress(t, FADE, 0.6);
    Scene {
        cursor,
        pressed,
        flyout: progress(t, OPEN, 0.3) * (1. - progress(t, CLOSE, 0.3)),
        chevron: CHEVRON.0 - 32. * pinned * (1. - reset),
        chevron_hover: (1.2..FIRST_DRAG).contains(&t),
        pinned_opacity: 1. - reset,
        icons,
    }
}
/// The next moment worth drawing, skipping stretches where nothing moves.
fn next_time(t: f64) -> f64 {
    if t < 0.6 {
        0.6
    } else if (RETURNED..FADE).contains(&t) {
        FADE
    } else if t + FRAME >= LOOP {
        0.
    } else {
        t + FRAME
    }
}

fn animations_enabled() -> bool {
    use windows_sys::Win32::UI::WindowsAndMessaging::*;
    let mut enabled = 1i32;
    let ok = unsafe {
        SystemParametersInfoW(
            SPI_GETCLIENTAREAANIMATION,
            0,
            (&mut enabled as *mut i32).cast(),
            0,
        )
    };
    ok == 0 || enabled != 0
}

impl TrayAnimation {
    fn schedule(&self, context: &ComponentContext<Self>) {
        let next = next_time(self.time);
        let wait = if next == 0. { FRAME } else { next - self.time };
        // Tasks belong to this component and stop when the Tray icons page closes.
        context.spawn_background(move |_| {
            std::thread::sleep(Duration::from_secs_f64(wait));
            Message::Tick(next)
        });
    }
}
impl Component for TrayAnimation {
    type Input = Input;
    type Message = Message;
    fn create(_: &Input, context: &ComponentContext<Self>) -> Self {
        let animated = animations_enabled();
        let result = Self {
            // Without animations, show the finished arrangement.
            time: if animated { 0. } else { RETURNED },
            cursor: EncodedImage::new(cursor_bitmap()),
            animated,
        };
        if animated {
            result.schedule(context);
        }
        result
    }
    fn update(&mut self, message: Message, context: &ComponentContext<Self>) {
        let Message::Tick(time) = message;
        self.time = time;
        self.schedule(context);
    }
    fn view(&self, input: &Input, _: &mut ViewContext<Self>) -> View {
        let s = scene(self.time);
        let (fg, bar, flyout_bg, hover) = if input.dark {
            (
                Color::rgb(240, 240, 240),
                Color::rgb(32, 32, 32),
                Color::rgb(44, 44, 44),
                Color::rgb(60, 60, 60),
            )
        } else {
            (
                Color::rgb(25, 25, 25),
                Color::rgb(243, 243, 243),
                Color::rgb(249, 249, 249),
                Color::rgb(225, 225, 225),
            )
        };
        let glyph = |glyph: &str, size: f64| {
            // FontIcon has no font size and clips its glyph when given a smaller box,
            // so scale the whole glyph down with a Viewbox instead.
            Viewbox::new()
                .width(size)
                .height(size)
                .stretch(Stretch::Uniform)
                .horizontal_alignment(HorizontalAlignment::Center)
                .vertical_alignment(VerticalAlignment::Center)
                .slots([SlotView::new(
                    ViewboxSlot::Child,
                    FontIcon::new().glyph(glyph),
                )])
        };
        let mut layers: Vec<View> = vec![
            Border::new()
                .width(WIDTH)
                .height(HEIGHT)
                .background(DESKTOP_BLUE)
                .into(),
            Border::new()
                .canvas_top(TASKBAR_TOP)
                .width(WIDTH)
                .height(TASKBAR_HEIGHT)
                .background(bar)
                .into(),
            Border::new()
                .canvas_left(s.chevron - 16.)
                .canvas_top(CHEVRON.1 - 16.)
                .width(32.)
                .height(32.)
                .corner_radius(4.)
                .background(if s.chevron_hover { hover } else { bar })
                .content(glyph("\u{E70E}", 16.)),
        ];
        for (i, icon) in ["\u{E701}", "\u{E767}", "\u{E83F}"].into_iter().enumerate() {
            layers.push(
                Border::new()
                    .canvas_left(SYSTEM_ICONS + 32. * i as f64)
                    .canvas_top(TASKBAR_TOP)
                    .width(32.)
                    .height(TASKBAR_HEIGHT)
                    .content(glyph(icon, 14.)),
            );
        }
        layers.push(
            Border::new()
                .canvas_left(446.)
                .canvas_top(TASKBAR_TOP)
                .width(64.)
                .height(TASKBAR_HEIGHT)
                .content(
                    TextBlock::new()
                        .text("12:00")
                        .font_size(12.)
                        .foreground(fg)
                        .horizontal_alignment(HorizontalAlignment::Center)
                        .vertical_alignment(VerticalAlignment::Center),
                ),
        );
        layers.push(
            Border::new()
                .canvas_left(FLYOUT.0)
                .canvas_top(FLYOUT.1)
                .width(FLYOUT.2)
                .height(FLYOUT.3)
                .corner_radius(8.)
                .background(flyout_bg)
                .border_brush(Color::rgb(128, 128, 128))
                .border_thickness(0.5)
                .opacity(s.flyout)
                .into(),
        );
        for (position, icon) in OTHER_APPS {
            let (x, y) = cell(position);
            layers.push(
                Border::new()
                    .canvas_left(x - 12.)
                    .canvas_top(y - 12.)
                    .width(24.)
                    .height(24.)
                    .opacity(s.flyout)
                    .content(glyph(icon, 16.)),
            );
        }
        // The helper's icons draw last so a dragged icon passes over everything else.
        for (i, image) in input.icons.iter().enumerate().take(4) {
            let ((x, y), opacity, scale) = match s.icons[i] {
                Icon::InFlyout => (cell(SOURCE[i]), s.flyout, 1.),
                Icon::Dragging(x, y) => ((x + 4., y + 4.), 1., 1.15),
                Icon::Pinned(x) => ((x, ROW), s.pinned_opacity, 1.),
            };
            layers.push(
                Border::new()
                    .canvas_left(x - 12.)
                    .canvas_top(y - 12.)
                    .width(24.)
                    .height(24.)
                    .opacity(opacity)
                    .scale(scale)
                    .content(
                        Image::new()
                            .source_data(image.clone())
                            .width(20.)
                            .height(20.)
                            .stretch(Stretch::Uniform),
                    ),
            );
        }
        layers.push(
            Border::new()
                .canvas_left(s.cursor.0 - 11.)
                .canvas_top(s.cursor.1 - 11.)
                .width(22.)
                .height(22.)
                .corner_radius(11.)
                .border_brush(Color::rgb(255, 255, 255))
                .border_thickness(2.)
                .opacity(if s.pressed && self.animated { 0.8 } else { 0. })
                .into(),
        );
        layers.push(
            Image::new()
                .source_data(self.cursor.clone())
                .canvas_left(s.cursor.0)
                .canvas_top(s.cursor.1)
                .width(14.)
                .height(21.)
                .opacity(if self.animated { 1. } else { 0. })
                .into(),
        );
        Border::new()
            .corner_radius(8.)
            .border_thickness(1.)
            .border_brush(ThemeBrush::CardStroke)
            .content(
                Viewbox::new()
                    .max_height(230.)
                    .stretch(Stretch::Uniform)
                    .slots([SlotView::new(
                        ViewboxSlot::Child,
                        Canvas::new().width(WIDTH).height(HEIGHT).keyed_children(
                            layers.into_iter().enumerate().map(|(i, v)| (i as u64, v)),
                        ),
                    )]),
            )
    }
}

/// A classic arrow pointer, white with a dark outline, drawn at 2x for a 14 x 21 DIP image.
fn cursor_bitmap() -> Vec<u8> {
    const W: i32 = 28;
    const H: i32 = 42;
    let shape = [
        (1.5, 1.5),
        (1.5, 33.5),
        (9.5, 26.),
        (15., 38.5),
        (20., 36.5),
        (14.5, 24.5),
        (24.5, 24.5),
    ];
    let inside = |x: f64, y: f64| {
        let mut hit = false;
        for i in 0..shape.len() {
            let (ax, ay) = shape[i];
            let (bx, by) = shape[(i + 1) % shape.len()];
            if (ay > y) != (by > y) && x < ax + (y - ay) * (bx - ax) / (by - ay) {
                hit = !hit;
            }
        }
        hit
    };
    // Points at least `r` inside every edge form the white fill.
    let inner = |x: f64, y: f64| {
        let r = 2.2;
        (0..8).all(|k| {
            let a = k as f64 * std::f64::consts::FRAC_PI_4;
            inside(x + r * a.cos(), y + r * a.sin())
        })
    };
    let mut pixels = Vec::with_capacity((W * H) as usize);
    for py in 0..H {
        for px in 0..W {
            let (mut outer_hits, mut inner_hits) = (0u32, 0u32);
            for sy in 0..4 {
                for sx in 0..4 {
                    let x = px as f64 + (sx as f64 + 0.5) / 4.;
                    let y = py as f64 + (sy as f64 + 0.5) / 4.;
                    if inside(x, y) {
                        outer_hits += 1;
                        if inner(x, y) {
                            inner_hits += 1;
                        }
                    }
                }
            }
            // Premultiplied ARGB: coverage is alpha, the white fill is the colour.
            let alpha = outer_hits * 255 / 16;
            let white = inner_hits * 255 / 16;
            pixels.push(alpha << 24 | white << 16 | white << 8 | white);
        }
    }
    bitmap_rect(W, H, &pixels)
}

#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn demo_opens_the_flyout_and_pins_icons_in_order() {
        let start = scene(0.);
        assert_eq!(start.icons, [Icon::InFlyout; 4]);
        assert_eq!(start.flyout, 0.);
        assert_eq!(start.cursor, REST);
        // The arrow starts right next to Wi-Fi.
        assert_eq!(start.chevron + 16., SYSTEM_ICONS);
        let open = scene(FIRST_DRAG);
        assert_eq!(open.flyout, 1.);
        assert_eq!(open.cursor, CHEVRON);
        let second = scene(FIRST_DRAG + DRAG + 1.2);
        assert!(matches!(second.icons[0], Icon::Pinned(_)));
        assert!(matches!(second.icons[1], Icon::Dragging(..)));
        assert_eq!(second.icons[2], Icon::InFlyout);
        let done = scene(RETURNED);
        assert_eq!(done.flyout, 0.);
        assert_eq!(done.cursor, REST);
        // Previous, number, initial, Next from left to right, the last next to Wi-Fi,
        // with the arrow pushed left of them all.
        let xs: Vec<f64> = done
            .icons
            .iter()
            .map(|icon| match icon {
                Icon::Pinned(x) => *x,
                other => panic!("not pinned: {other:?}"),
            })
            .collect();
        assert_eq!(xs, [DROP.0 - 96., DROP.0 - 64., DROP.0 - 32., DROP.0]);
        assert_eq!(done.chevron, DROP.0 - 128.);
        let restart = scene(LOOP);
        assert_eq!(restart.pinned_opacity, 0.);
        assert_eq!(restart.chevron, CHEVRON.0);
    }
    #[test]
    fn room_opens_before_each_drop() {
        // Halfway through the first drag the arrow has not moved yet...
        assert_eq!(scene(FIRST_DRAG + 1.).chevron, CHEVRON.0);
        // ...and it has made a full icon of room just before the drop.
        assert_eq!(scene(FIRST_DRAG + 1.55).chevron, CHEVRON.0 - 32.);
        // An already pinned icon slides left while the next one comes in.
        let before = scene(FIRST_DRAG + DRAG + 0.9);
        let after = scene(FIRST_DRAG + DRAG + 1.55);
        assert_eq!(before.icons[0], Icon::Pinned(DROP.0));
        assert_eq!(after.icons[0], Icon::Pinned(DROP.0 - 32.));
    }
    #[test]
    fn idle_stretches_are_skipped_and_the_loop_restarts() {
        assert_eq!(next_time(0.), 0.6);
        assert_eq!(next_time(RETURNED), FADE);
        assert_eq!(next_time(LOOP - FRAME / 2.), 0.);
        assert!((next_time(2.) - (2. + FRAME)).abs() < 1e-9);
    }
    #[test]
    fn cursor_has_a_white_fill_and_dark_outline() {
        let bytes = cursor_bitmap();
        let pixel = |x: usize, y: usize| {
            let at = 138 + (y * 28 + x) * 4;
            [bytes[at], bytes[at + 1], bytes[at + 2], bytes[at + 3]]
        };
        assert_eq!(pixel(6, 20), [255, 255, 255, 255]);
        assert_eq!(pixel(1, 20)[..3], [0, 0, 0]);
        assert_eq!(pixel(27, 0)[3], 0);
    }
}
