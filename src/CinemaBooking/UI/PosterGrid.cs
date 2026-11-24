// the grid of film posters on the films page, drawn as one control so a resize or a search is quick (owner: divyesh)
// the grid adds columns when the window gets wider. the posters come in one after the other when the list changes,
// and a poster lifts with a soft red light under the mouse. PosterGridLayout does the sums, so they are tested
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public class PosterGrid : Control
    {
        // sizes at the small step, they grow with the window step
        private const int BASE_MIN_CARD_WIDTH = 180;
        private const int BASE_GAP = 24;
        private const int BASE_TEXT_BLOCK = 64;
        private const int BASE_TOP_ROOM = 12;
        private const int BASE_BOTTOM_ROOM = 56;
        private const int BASE_LIFT = 6;
        private const int BASE_SLIDE = 18;
        private const int BASE_RADIUS = 10;

        // the posters come in one after the other: each starts this much later and takes this long
        private const double APPEAR_DELAY_MS = 45;
        private const double APPEAR_LENGTH_MS = 380;

        // how far the hover light moves to its target each tick
        private const float HOVER_EASE = 0.25f;

        private readonly Func<Film, Image?> _poster_for;
        private readonly Action<Film> _open_film;
        private readonly System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer();
        private readonly bool _is_animated = SystemInformation.UIEffectsEnabled;

        private Film[] _films = new Film[0];
        private Bitmap?[] _thumbs = new Bitmap?[0];
        private float[] _hover = new float[0];
        private string _empty_text = "";
        private float _step = UiScale.SMALL_STEP;

        private int _hovered_index = -1;
        private int _focus_index = -1;
        private DateTime _appear_start = DateTime.MinValue;
        private bool _is_appearing;

        // the sizes worked out by the last layout
        private int _columns = 1;
        private int _card_width;
        private int _poster_height;
        private int _card_height;
        private int _gap;
        private int _side;
        private int _top_room;
        private int _thumb_width = -1;

        public PosterGrid(Func<Film, Image?> poster_for, Action<Film> open_film)
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);

            _poster_for = poster_for;
            _open_film = open_film;
            BackColor = UiTheme.BACKGROUND;
            TabStop = true;

            _timer.Interval = 15;
            _timer.Tick += (sender, e) => AnimateTick();
        }

        // the films to show, the empty text is shown when there are none
        public void SetFilms(Film[] films, string empty_text)
        {
            DropThumbs();
            _films = films ?? new Film[0];
            _thumbs = new Bitmap?[_films.Length];
            _hover = new float[_films.Length];
            _empty_text = empty_text;
            _hovered_index = -1;
            _focus_index = _films.Length > 0 ? Math.Min(Math.Max(_focus_index, 0), _films.Length - 1) : -1;

            if (_is_animated && _films.Length > 0)
            {
                _appear_start = DateTime.Now;
                _is_appearing = true;
                _timer.Start();
            }
            else
            {
                _is_appearing = false;
            }

            Relayout();
            Invalidate();
        }

        public void SetStep(float step)
        {
            _step = step;
            Relayout();
            Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Relayout();
        }

        // works out the columns and card sizes for the width now, and sets the height the rows need
        private void Relayout()
        {
            _side = UiScale.Pixels(UiTheme.PAGE_SIDE_PADDING, _step);
            _gap = UiScale.Pixels(BASE_GAP, _step);
            _top_room = UiScale.Pixels(BASE_TOP_ROOM, _step);

            int available = Math.Max(0, Width - (2 * _side));
            _columns = PosterGridLayout.ColumnsFor(available, UiScale.Pixels(BASE_MIN_CARD_WIDTH, _step), _gap);
            _card_width = PosterGridLayout.CardWidth(available, _columns, _gap);
            _poster_height = PosterGridLayout.PosterHeight(_card_width);
            _card_height = _poster_height + UiScale.Pixels(BASE_TEXT_BLOCK, _step);

            // new card size, so the small copies of the posters are made again at the new size
            if (_card_width != _thumb_width)
            {
                DropThumbs();
                _thumbs = new Bitmap?[_films.Length];
                _thumb_width = _card_width;
            }

            int needed = _films.Length == 0
                ? UiScale.Pixels(120, _step)
                : _top_room + PosterGridLayout.TotalHeight(_films.Length, _columns, _card_height, _gap)
                    + UiScale.Pixels(BASE_BOTTOM_ROOM, _step);

            if (Height != needed)
            {
                Height = needed;
            }
        }

        // a small copy of the poster at the card size, made once, so drawing is fast
        private Bitmap? ThumbFor(int index)
        {
            if (_thumbs[index] != null)
            {
                return _thumbs[index];
            }

            Image? poster = _poster_for(_films[index]);

            if (poster == null || _card_width <= 0 || _poster_height <= 0)
            {
                return null;
            }

            Bitmap thumb = new Bitmap(_card_width, _poster_height);
            using (Graphics graphics = Graphics.FromImage(thumb))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.Clear(UiTheme.CARD);
                graphics.DrawImage(poster, FilmHero.CoverRect(poster.Size, new Rectangle(0, 0, _card_width, _poster_height)));
            }

            _thumbs[index] = thumb;
            return thumb;
        }

        private void DropThumbs()
        {
            for (int index = 0; index < _thumbs.Length; index++)
            {
                _thumbs[index]?.Dispose();
                _thumbs[index] = null;
            }
        }

        // how far card number index has come in, 0 not yet shown and 1 fully in, eased so it slows at the end
        private float AppearFor(int index)
        {
            if (!_is_appearing)
            {
                return 1f;
            }

            double elapsed = (DateTime.Now - _appear_start).TotalMilliseconds - (index * APPEAR_DELAY_MS);
            double progress = Math.Clamp(elapsed / APPEAR_LENGTH_MS, 0.0, 1.0);
            return (float)(1.0 - Math.Pow(1.0 - progress, 3));
        }

        private void AnimateTick()
        {
            bool is_moving = false;

            for (int index = 0; index < _hover.Length; index++)
            {
                float target = index == _hovered_index || (Focused && index == _focus_index) ? 1f : 0f;
                float now = _hover[index];

                if (Math.Abs(target - now) > 0.01f)
                {
                    _hover[index] = _is_animated ? now + ((target - now) * HOVER_EASE) : target;
                    is_moving = true;
                }
                else if (now != target)
                {
                    _hover[index] = target;
                    is_moving = true;
                }
            }

            if (_is_appearing)
            {
                double total = (_films.Length * APPEAR_DELAY_MS) + APPEAR_LENGTH_MS;
                if ((DateTime.Now - _appear_start).TotalMilliseconds >= total)
                {
                    _is_appearing = false;
                }

                is_moving = true;
            }

            if (!is_moving)
            {
                _timer.Stop();
            }

            Invalidate();
        }

        private void StartAnimating()
        {
            if (!_timer.Enabled)
            {
                _timer.Start();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            graphics.Clear(BackColor);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;

            if (_films.Length == 0)
            {
                TextRenderer.DrawText(graphics, _empty_text, FontCache.Scaled(FontCache.REGULAR, UiTheme.SIZE_BODY, _step),
                    new Rectangle(_side, _top_room, Math.Max(10, Width - (2 * _side)), UiScale.Pixels(60, _step)),
                    UiTheme.TEXT_MUTED, TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak);
                return;
            }

            Font title_font = FontCache.Scaled(FontCache.SEMIBOLD, UiTheme.SIZE_CARD_TITLE, _step);
            Font meta_font = FontCache.Scaled(FontCache.REGULAR, UiTheme.SIZE_SMALL, _step);
            Font badge_font = FontCache.Scaled(FontCache.SEMIBOLD, UiTheme.SIZE_SMALL, _step);

            for (int index = 0; index < _films.Length; index++)
            {
                Rectangle cell = PosterGridLayout.CardBounds(index, _columns, _card_width, _card_height, _gap);
                cell.Offset(_side, _top_room);

                if (!cell.IntersectsWith(Rectangle.Inflate(e.ClipRectangle, 0, UiScale.Pixels(BASE_SLIDE, _step) * 2)))
                {
                    continue;
                }

                float appear = AppearFor(index);
                if (appear <= 0f)
                {
                    continue;
                }

                DrawCard(graphics, index, cell, appear, title_font, meta_font, badge_font);
            }
        }

        private void DrawCard(Graphics graphics, int index, Rectangle cell, float appear, Font title_font, Font meta_font, Font badge_font)
        {
            Film film = _films[index];
            float hover = _hover[index];

            // the poster lifts under the mouse, and slides up into place when the list changes
            float lift = (UiScale.Pixels(BASE_LIFT, _step) * hover) - (UiScale.Pixels(BASE_SLIDE, _step) * (1f - appear));
            RectangleF poster_area = new RectangleF(cell.X, cell.Y - lift, _card_width, _poster_height);
            float radius = UiScale.Pixels(BASE_RADIUS, _step);

            if (hover > 0.02f)
            {
                DrawSoftLight(graphics, poster_area, radius, UiTheme.ACCENT_RED, hover * appear);
            }

            using (GraphicsPath path = GlowPainter.RoundedPath(poster_area, radius))
            {
                Region old_clip = graphics.Clip;
                graphics.SetClip(path, CombineMode.Intersect);

                Bitmap? thumb = ThumbFor(index);
                if (thumb != null)
                {
                    DrawPicture(graphics, thumb, poster_area, appear);
                }
                else
                {
                    using (SolidBrush empty_fill = new SolidBrush(Color.FromArgb((int)(255 * appear), UiTheme.CARD)))
                    {
                        graphics.FillRectangle(empty_fill, poster_area);
                    }
                }

                // under the mouse the bottom of the poster darkens and says what a click does
                if (hover > 0.02f)
                {
                    DrawHoverShade(graphics, poster_area, hover * appear);
                }

                graphics.Clip = old_clip;
                old_clip.Dispose();

                using (Pen edge = new Pen(Color.FromArgb((int)(36 * appear), Color.White)))
                {
                    graphics.DrawPath(edge, path);
                }

                if (Focused && ShowFocusCues && index == _focus_index)
                {
                    using (Pen ring = new Pen(UiTheme.TEXT, 2f))
                    {
                        graphics.DrawPath(ring, path);
                    }
                }
            }

            DrawAgeBadge(graphics, film, poster_area, badge_font, appear);

            // the title and the details under the poster, they stay in place while the poster lifts
            int text_alpha = (int)(255 * appear);
            float text_top = cell.Y + _poster_height + UiScale.Pixels(UiTheme.SPACE_M, _step);
            float title_height = title_font.GetHeight(graphics);

            using (StringFormat one_line = new StringFormat(StringFormatFlags.NoWrap))
            using (SolidBrush title_brush = new SolidBrush(Color.FromArgb(text_alpha, UiTheme.TEXT)))
            using (SolidBrush meta_brush = new SolidBrush(Color.FromArgb(text_alpha, UiTheme.TEXT_MUTED)))
            {
                one_line.Trimming = StringTrimming.EllipsisCharacter;
                graphics.DrawString(film.Title, title_font, title_brush,
                    new RectangleF(cell.X, text_top, _card_width, title_height + 2), one_line);
                graphics.DrawString(film.Genre + "  ·  " + film.Year + "  ·  " + film.DurationMinutes + " min", meta_font, meta_brush,
                    new RectangleF(cell.X, text_top + title_height + UiScale.Pixels(UiTheme.SPACE_XS, _step), _card_width, meta_font.GetHeight(graphics) + 2),
                    one_line);
            }
        }

        private static void DrawPicture(Graphics graphics, Image picture, RectangleF area, float alpha)
        {
            if (alpha >= 0.99f)
            {
                graphics.DrawImage(picture, area);
                return;
            }

            using (ImageAttributes see_through = new ImageAttributes())
            {
                ColorMatrix matrix = new ColorMatrix();
                matrix.Matrix33 = alpha;
                see_through.SetColorMatrix(matrix);

                PointF[] corners =
                {
                    new PointF(area.Left, area.Top),
                    new PointF(area.Right, area.Top),
                    new PointF(area.Left, area.Bottom)
                };
                graphics.DrawImage(picture, corners, new RectangleF(0, 0, picture.Width, picture.Height), GraphicsUnit.Pixel, see_through);
            }
        }

        // a soft coloured light around the poster, stronger as the hover grows
        private void DrawSoftLight(Graphics graphics, RectangleF area, float radius, Color colour, float strength)
        {
            int layers = 6;
            float spread = UiScale.Pixels(16, _step);

            for (int layer = layers; layer >= 1; layer--)
            {
                float grow = spread * layer / layers;
                RectangleF layer_area = RectangleF.Inflate(area, grow, grow);
                int alpha = (int)(20 * strength);

                using (GraphicsPath path = GlowPainter.RoundedPath(layer_area, radius + grow))
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, colour)))
                {
                    graphics.FillPath(brush, path);
                }
            }
        }

        private void DrawHoverShade(Graphics graphics, RectangleF poster_area, float strength)
        {
            RectangleF shade_area = new RectangleF(poster_area.X, poster_area.Y + (poster_area.Height * 0.45f),
                poster_area.Width, poster_area.Height * 0.55f + 1);

            using (LinearGradientBrush shade = new LinearGradientBrush(shade_area,
                Color.FromArgb(0, Color.Black), Color.FromArgb((int)(220 * strength), Color.Black), LinearGradientMode.Vertical))
            {
                graphics.FillRectangle(shade, shade_area);
            }

            Font action_font = FontCache.Scaled(FontCache.SEMIBOLD, UiTheme.SIZE_BODY, _step);
            float pad = UiScale.Pixels(UiTheme.SPACE_L, _step);
            float slide = (1f - strength) * UiScale.Pixels(10, _step);
            float action_height = action_font.GetHeight(graphics);

            using (SolidBrush action_brush = new SolidBrush(Color.FromArgb((int)(255 * strength), UiTheme.TEXT)))
            using (SolidBrush bar_brush = new SolidBrush(Color.FromArgb((int)(255 * strength), UiTheme.ACCENT_RED)))
            {
                float text_top = poster_area.Bottom - pad - action_height + slide;
                graphics.FillRectangle(bar_brush, poster_area.X + pad, text_top - UiScale.Pixels(8, _step), UiScale.Pixels(22, _step), UiScale.Pixels(3, _step));
                graphics.DrawString("See showings", action_font, action_brush, poster_area.X + pad, text_top);
            }
        }

        // the age rating in a small dark tag on the top left of the poster, 16 and over in red
        private void DrawAgeBadge(Graphics graphics, Film film, RectangleF poster_area, Font badge_font, float appear)
        {
            string age_text = film.AgeRating == 0 ? "All ages" : film.AgeRating + "+";
            Color age_colour = film.AgeRating >= 16 ? UiTheme.ACCENT_RED : UiTheme.AMBER;

            SizeF text_size = graphics.MeasureString(age_text, badge_font);
            float pad_x = UiScale.Pixels(8, _step);
            float pad_y = UiScale.Pixels(3, _step);
            float inset = UiScale.Pixels(10, _step);
            RectangleF badge = new RectangleF(poster_area.X + inset, poster_area.Y + inset,
                text_size.Width + (2 * pad_x), text_size.Height + (2 * pad_y));

            using (GraphicsPath path = GlowPainter.RoundedPath(badge, badge.Height / 2f))
            using (SolidBrush fill = new SolidBrush(Color.FromArgb((int)(200 * appear), UiTheme.BACKGROUND)))
            using (SolidBrush text_brush = new SolidBrush(Color.FromArgb((int)(255 * appear), age_colour)))
            {
                graphics.FillPath(fill, path);
                graphics.DrawString(age_text, badge_font, text_brush, badge.X + pad_x, badge.Y + pad_y);
            }
        }

        // the card under a point of this control, or -1
        private int IndexAtPoint(Point point)
        {
            Point in_grid = new Point(point.X - _side, point.Y - _top_room);
            return PosterGridLayout.IndexAt(in_grid, _films.Length, _columns, _card_width, _card_height, _gap);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int index = IndexAtPoint(e.Location);

            if (index != _hovered_index)
            {
                _hovered_index = index;
                Cursor = index >= 0 ? Cursors.Hand : Cursors.Default;
                StartAnimating();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hovered_index = -1;
            StartAnimating();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            int index = IndexAtPoint(e.Location);

            if (e.Button == MouseButtons.Left && index >= 0)
            {
                _open_film(_films[index]);
            }
        }

        // the arrows move between posters, enter or space opens the film
        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    return true;
                default:
                    return base.IsInputKey(keyData);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (_films.Length == 0)
            {
                return;
            }

            int moved = _focus_index;
            switch (e.KeyCode)
            {
                case Keys.Left:
                    moved--;
                    break;
                case Keys.Right:
                    moved++;
                    break;
                case Keys.Up:
                    moved -= _columns;
                    break;
                case Keys.Down:
                    moved += _columns;
                    break;
                case Keys.Enter:
                case Keys.Space:
                    if (_focus_index >= 0)
                    {
                        _open_film(_films[_focus_index]);
                    }
                    e.Handled = true;
                    return;
                default:
                    return;
            }

            _focus_index = Math.Clamp(moved, 0, _films.Length - 1);
            e.Handled = true;
            StartAnimating();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            if (_focus_index < 0 && _films.Length > 0)
            {
                _focus_index = 0;
            }

            StartAnimating();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            StartAnimating();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
                DropThumbs();
            }

            base.Dispose(disposing);
        }
    }
}
