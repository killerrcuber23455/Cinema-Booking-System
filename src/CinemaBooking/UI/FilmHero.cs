// the big film banner at the top of the films page (owner: divyesh)
// a blurred wash of the poster fills the width, the sharp poster stands on the right, the title and details on the left.
// arrows change the film with a soft cross fade. windows forms has no blur, so the blur is the poster made tiny then stretched
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public class FilmHero : Control
    {
        public const int BASE_HEIGHT = 400;

        // below this width the sharp poster is left out, so the text has room
        private const int POSTER_FROM_WIDTH = 760;

        private const int BASE_POSTER_MARGIN = 44;
        private const int BASE_TEXT_GAP = 48;
        private const int BASE_ARROW_SIZE = 38;
        private const int BASE_BUTTON_WIDTH = 210;
        private const int BASE_BUTTON_HEIGHT = 46;
        private const int MAX_DESCRIPTION_LINES = 3;

        // how strong the blurred poster shows, and how much of the fade is done each tick
        private const float WASH_ALPHA = 0.55f;
        private const float FADE_STEP = 0.07f;

        private const string ICON_LEFT = "";
        private const string ICON_RIGHT = "";

        // a tiny picture only used to measure text
        private static readonly Bitmap MEASURE_BITMAP = new Bitmap(1, 1);

        private readonly Func<Film, Image?> _poster_for;
        private readonly Action<Film> _open_film;
        private readonly GlowButton btnBook = new GlowButton();
        private readonly System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer();
        private readonly bool _is_animated = SystemInformation.UIEffectsEnabled;

        private Film[] _films = new Film[0];
        private int _index;
        private float _step = UiScale.SMALL_STEP;

        // the drawn background of the film now, and of the film before while it fades out
        private Bitmap? _backdrop;
        private Bitmap? _old_backdrop;
        private float _fade = 1f;

        // where the parts were placed by the last layout
        private Rectangle _poster_area;
        private RectangleF _title_area;
        private RectangleF _meta_area;
        private RectangleF _description_area;
        private Rectangle _previous_area;
        private Rectangle _next_area;
        private RectangleF _pager_text_area;
        private int _hovered_arrow;

        public FilmHero(Func<Film, Image?> poster_for, Action<Film> open_film)
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);

            _poster_for = poster_for;
            _open_film = open_film;
            BackColor = UiTheme.BACKGROUND;
            Height = BASE_HEIGHT;
            TabStop = true;

            btnBook.Text = "See showings";
            btnBook.BackColor = UiTheme.ACCENT_RED;
            btnBook.Font = UiTheme.BUTTON_FONT;
            btnBook.Click += (sender, e) => OpenCurrentFilm();
            Controls.Add(btnBook);

            _timer.Interval = 15;
            _timer.Tick += (sender, e) => FadeTick();
        }

        // the films the banner can show, the first one is shown at once
        public void SetFilms(Film[] films)
        {
            _films = films ?? new Film[0];
            _index = 0;
            DropBackdrops();
            btnBook.Visible = _films.Length > 0;
            LayoutParts();
            Invalidate();
        }

        public void SetStep(float step)
        {
            _step = step;
            Height = UiScale.Pixels(BASE_HEIGHT, step);
            btnBook.Font = FontCache.Scaled(FontCache.SEMIBOLD, UiTheme.SIZE_BODY, step);
            DropBackdrops();
            LayoutParts();
            Invalidate();
        }

        private Film? CurrentFilm
        {
            get { return _films.Length == 0 ? null : _films[_index]; }
        }

        private void OpenCurrentFilm()
        {
            Film? film = CurrentFilm;

            if (film != null)
            {
                _open_film(film);
            }
        }

        // moves to another film, the old picture fades out under the new one
        private void ShowFilm(int new_index)
        {
            if (_films.Length < 2)
            {
                return;
            }

            int wrapped = ((new_index % _films.Length) + _films.Length) % _films.Length;

            _old_backdrop?.Dispose();
            _old_backdrop = _backdrop;
            _backdrop = null;
            _index = wrapped;
            LayoutParts();

            if (_is_animated && _old_backdrop != null)
            {
                _fade = 0f;
                _timer.Start();
            }
            else
            {
                _fade = 1f;
                _old_backdrop?.Dispose();
                _old_backdrop = null;
            }

            Invalidate();
        }

        private void FadeTick()
        {
            _fade = Math.Min(1f, _fade + FADE_STEP);

            if (_fade >= 1f)
            {
                _timer.Stop();
                _old_backdrop?.Dispose();
                _old_backdrop = null;
            }

            Invalidate();
        }

        private void DropBackdrops()
        {
            _timer.Stop();
            _fade = 1f;
            _backdrop?.Dispose();
            _backdrop = null;
            _old_backdrop?.Dispose();
            _old_backdrop = null;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            _backdrop?.Dispose();
            _backdrop = null;
            LayoutParts();
        }

        private Font TitleFont
        {
            get { return FontCache.Scaled(FontCache.DISPLAY, UiTheme.SIZE_HERO, _step); }
        }

        private Font MetaFont
        {
            get { return FontCache.Scaled(FontCache.MEDIUM, UiTheme.SIZE_BODY, _step); }
        }

        private Font DescriptionFont
        {
            get { return FontCache.Scaled(FontCache.REGULAR, UiTheme.SIZE_BODY, _step); }
        }

        private bool ShowsPoster
        {
            get { return Width >= POSTER_FROM_WIDTH; }
        }

        // places the poster, the text, the button and the arrows for the film now
        private void LayoutParts()
        {
            Film? film = CurrentFilm;

            if (film == null || Width <= 0 || Height <= 0)
            {
                return;
            }

            int side = UiScale.Pixels(UiTheme.PAGE_SIDE_PADDING, _step);
            int poster_margin = UiScale.Pixels(BASE_POSTER_MARGIN, _step);

            int text_right = Width - side;
            if (ShowsPoster)
            {
                int poster_height = Height - (2 * poster_margin);
                int poster_width = poster_height * 2 / 3;
                _poster_area = new Rectangle(Width - side - poster_width, poster_margin, poster_width, poster_height);
                text_right = _poster_area.Left - UiScale.Pixels(BASE_TEXT_GAP, _step);
            }
            else
            {
                _poster_area = Rectangle.Empty;
            }

            float text_width = Math.Max(120, Math.Min(text_right - side, UiScale.Pixels(780, _step)));

            using (Graphics measure = Graphics.FromImage(MEASURE_BITMAP))
            {
                measure.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                SizeF title_size = measure.MeasureString(film.Title, TitleFont, (int)text_width);
                SizeF meta_size = measure.MeasureString(MetaText(film), MetaFont, (int)text_width);

                float line_height = DescriptionFont.GetHeight(measure);
                SizeF description_size = measure.MeasureString(film.Description ?? "", DescriptionFont, (int)text_width);
                float description_height = Math.Min(description_size.Height, line_height * MAX_DESCRIPTION_LINES);

                int button_width = UiScale.Pixels(BASE_BUTTON_WIDTH, _step) + (2 * UiTheme.GLOW_SPREAD);
                int button_height = UiScale.Pixels(BASE_BUTTON_HEIGHT, _step) + (2 * UiTheme.GLOW_SPREAD);

                float gap_small = UiScale.Pixels(UiTheme.SPACE_M, _step);
                float gap_large = UiScale.Pixels(UiTheme.SPACE_XL, _step);
                float block_height = title_size.Height + gap_small + meta_size.Height + gap_large
                    + description_height + gap_large + button_height;

                // the text block sits a little above the middle, the arrows go under it
                float top = Math.Max(UiScale.Pixels(UiTheme.SPACE_XL, _step), (Height - block_height) / 2f - UiScale.Pixels(18, _step));

                _title_area = new RectangleF(side, top, text_width, title_size.Height);
                _meta_area = new RectangleF(side, _title_area.Bottom + gap_small, text_width, meta_size.Height);
                _description_area = new RectangleF(side, _meta_area.Bottom + gap_large, text_width, description_height);

                btnBook.SetBounds(side - UiTheme.GLOW_SPREAD, (int)(_description_area.Bottom + gap_large) - UiTheme.GLOW_SPREAD,
                    button_width, button_height);
            }

            int arrow = UiScale.Pixels(BASE_ARROW_SIZE, _step);
            int arrow_top = Height - arrow - UiScale.Pixels(UiTheme.SPACE_XL, _step);
            _previous_area = new Rectangle(side, arrow_top, arrow, arrow);
            _pager_text_area = new RectangleF(_previous_area.Right, arrow_top, UiScale.Pixels(70, _step), arrow);
            _next_area = new Rectangle((int)_pager_text_area.Right, arrow_top, arrow, arrow);
        }

        private static string MetaText(Film film)
        {
            string age_text = film.AgeRating == 0 ? "All ages" : film.AgeRating + "+";
            return film.Genre + "   ·   " + film.Year + "   ·   " + film.DurationMinutes + " min   ·   " + age_text;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            Film? film = CurrentFilm;

            if (film == null || Width <= 0 || Height <= 0)
            {
                graphics.Clear(UiTheme.BACKGROUND);
                return;
            }

            if (_backdrop == null || _backdrop.Size != Size)
            {
                _backdrop?.Dispose();
                _backdrop = MakeBackdrop(film);
            }

            graphics.DrawImageUnscaled(_backdrop, 0, 0);

            // the film before fades out on top of the new one
            if (_old_backdrop != null && _fade < 1f)
            {
                DrawFaded(graphics, _old_backdrop, 1f - _fade);
            }

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            // the new text fades in with the new picture
            int text_alpha = (int)(255 * Math.Max(0.15f, _fade));

            using (StringFormat wrap = new StringFormat())
            using (SolidBrush title_brush = new SolidBrush(Color.FromArgb(text_alpha, UiTheme.TEXT)))
            using (SolidBrush meta_brush = new SolidBrush(Color.FromArgb(text_alpha, UiTheme.AMBER)))
            using (SolidBrush description_brush = new SolidBrush(Color.FromArgb(text_alpha, UiTheme.TEXT_MUTED)))
            {
                wrap.Trimming = StringTrimming.EllipsisWord;

                graphics.DrawString(film.Title, TitleFont, title_brush, _title_area, wrap);
                graphics.DrawString(MetaText(film), MetaFont, meta_brush, _meta_area, wrap);
                graphics.DrawString(film.Description ?? "", DescriptionFont, description_brush, _description_area, wrap);
            }

            if (_films.Length > 1)
            {
                DrawArrow(graphics, _previous_area, ICON_LEFT, _hovered_arrow == -1);
                DrawArrow(graphics, _next_area, ICON_RIGHT, _hovered_arrow == 1);

                using (StringFormat centre = new StringFormat())
                using (SolidBrush pager_brush = new SolidBrush(UiTheme.TEXT_MUTED))
                {
                    centre.Alignment = StringAlignment.Center;
                    centre.LineAlignment = StringAlignment.Center;
                    graphics.DrawString((_index + 1) + " of " + _films.Length,
                        FontCache.Scaled(FontCache.MEDIUM, UiTheme.SIZE_SMALL, _step), pager_brush, _pager_text_area, centre);
                }
            }

            if (Focused && ShowFocusCues)
            {
                using (Pen ring = new Pen(UiTheme.TEXT_FAINT))
                {
                    graphics.DrawRectangle(ring, 0, 0, Width - 1, Height - 1);
                }
            }
        }

        private void DrawArrow(Graphics graphics, Rectangle area, string glyph, bool is_hovered)
        {
            RectangleF circle = new RectangleF(area.X + 0.5f, area.Y + 0.5f, area.Width - 1.5f, area.Height - 1.5f);

            if (is_hovered)
            {
                using (SolidBrush fill = new SolidBrush(Color.FromArgb(200, UiTheme.MENU_HOVER)))
                {
                    graphics.FillEllipse(fill, circle);
                }
            }

            using (Pen edge = new Pen(is_hovered ? UiTheme.TEXT_MUTED : UiTheme.LINE))
            {
                graphics.DrawEllipse(edge, circle);
            }

            TextRenderer.DrawText(graphics, glyph, UiTheme.ICON_FONT, area, UiTheme.TEXT, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        private static void DrawFaded(Graphics graphics, Image picture, float alpha)
        {
            using (ImageAttributes see_through = new ImageAttributes())
            {
                ColorMatrix matrix = new ColorMatrix();
                matrix.Matrix33 = alpha;
                see_through.SetColorMatrix(matrix);
                graphics.DrawImage(picture, new Rectangle(0, 0, picture.Width, picture.Height),
                    0, 0, picture.Width, picture.Height, GraphicsUnit.Pixel, see_through);
            }
        }

        // draws the background of one film once, the paint then only copies it
        private Bitmap MakeBackdrop(Film film)
        {
            Bitmap backdrop = new Bitmap(Width, Height);

            using (Graphics graphics = Graphics.FromImage(backdrop))
            {
                graphics.Clear(UiTheme.BACKGROUND);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;

                Image? poster = _poster_for(film);

                if (poster != null)
                {
                    DrawBlurredWash(graphics, poster);
                }

                // dark on the left where the text is, the poster colour shows through on the right
                Rectangle whole = new Rectangle(0, 0, Width, Height);
                using (LinearGradientBrush side_shade = new LinearGradientBrush(whole, Color.Black, Color.Black, LinearGradientMode.Horizontal))
                {
                    ColorBlend blend = new ColorBlend();
                    blend.Colors = new Color[]
                    {
                        Color.FromArgb(250, UiTheme.BACKGROUND),
                        Color.FromArgb(215, UiTheme.BACKGROUND),
                        Color.FromArgb(110, UiTheme.BACKGROUND)
                    };
                    blend.Positions = new float[] { 0f, 0.45f, 1f };
                    side_shade.InterpolationColors = blend;
                    graphics.FillRectangle(side_shade, whole);
                }

                // the bottom melts into the page, so the banner has no hard edge
                int fade_height = Math.Max(1, Height / 3);
                Rectangle bottom = new Rectangle(0, Height - fade_height, Width, fade_height);
                using (LinearGradientBrush bottom_shade = new LinearGradientBrush(
                    new Rectangle(bottom.X, bottom.Y - 1, bottom.Width, bottom.Height + 1),
                    Color.FromArgb(0, UiTheme.BACKGROUND), UiTheme.BACKGROUND, LinearGradientMode.Vertical))
                {
                    graphics.FillRectangle(bottom_shade, bottom);
                }

                if (poster != null && ShowsPoster && _poster_area.Width > 0)
                {
                    DrawPoster(graphics, poster);
                }
            }

            return backdrop;
        }

        // the poster made tiny and stretched back looks blurred, it fills the whole banner
        private void DrawBlurredWash(Graphics graphics, Image poster)
        {
            int tiny_width = 24;
            int tiny_height = Math.Max(1, tiny_width * poster.Height / Math.Max(1, poster.Width));

            using (Bitmap tiny = new Bitmap(tiny_width, tiny_height))
            {
                using (Graphics tiny_graphics = Graphics.FromImage(tiny))
                {
                    tiny_graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    tiny_graphics.DrawImage(poster, 0, 0, tiny_width, tiny_height);
                }

                // cover the banner: as wide as the banner, centred up and down
                int cover_height = Width * tiny_height / tiny_width;
                Rectangle cover = new Rectangle(0, (Height - cover_height) / 2, Width, cover_height);

                InterpolationMode old_mode = graphics.InterpolationMode;
                graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;

                using (ImageAttributes wash = new ImageAttributes())
                {
                    ColorMatrix matrix = new ColorMatrix();
                    matrix.Matrix33 = WASH_ALPHA;
                    wash.SetColorMatrix(matrix);
                    // the edge pixels are repeated, so the stretched edge is not dark
                    wash.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(tiny, cover, 0, 0, tiny_width, tiny_height, GraphicsUnit.Pixel, wash);
                }

                graphics.InterpolationMode = old_mode;
            }
        }

        // the sharp poster with rounded corners, a coloured light behind it from the poster's own colour
        private void DrawPoster(Graphics graphics, Image poster)
        {
            RectangleF area = _poster_area;
            float radius = UiScale.Pixels(14, _step);

            Color poster_colour = UiTheme.ACCENT_RED;
            Bitmap? poster_bitmap = poster as Bitmap;
            if (poster_bitmap != null)
            {
                poster_colour = ColorMath.Average(poster_bitmap);
            }

            GlowPainter.DrawGlow(graphics, area, radius, Color.Black, 6, UiScale.Pixels(26, _step));
            GlowPainter.DrawGlow(graphics, area, radius, ColorMath.Blend(poster_colour, Color.White, 0.2), 5, UiScale.Pixels(18, _step));

            using (GraphicsPath path = GlowPainter.RoundedPath(area, radius))
            {
                graphics.SetClip(path);
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(poster, CoverRect(poster.Size, _poster_area));
                graphics.ResetClip();

                using (Pen edge = new Pen(Color.FromArgb(40, Color.White)))
                {
                    graphics.DrawPath(edge, path);
                }
            }
        }

        // the picture made big enough to cover the area, cut evenly on the sides that are too long
        internal static Rectangle CoverRect(Size picture, Rectangle area)
        {
            if (picture.Width <= 0 || picture.Height <= 0)
            {
                return area;
            }

            double scale = Math.Max((double)area.Width / picture.Width, (double)area.Height / picture.Height);
            int width = (int)Math.Ceiling(picture.Width * scale);
            int height = (int)Math.Ceiling(picture.Height * scale);
            return new Rectangle(area.X + ((area.Width - width) / 2), area.Y + ((area.Height - height) / 2), width, height);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int hovered = 0;
            if (_films.Length > 1 && _previous_area.Contains(e.Location))
            {
                hovered = -1;
            }
            else if (_films.Length > 1 && _next_area.Contains(e.Location))
            {
                hovered = 1;
            }

            bool is_on_poster = _poster_area.Contains(e.Location);
            Cursor = hovered != 0 || is_on_poster ? Cursors.Hand : Cursors.Default;

            if (hovered != _hovered_arrow)
            {
                _hovered_arrow = hovered;
                Invalidate(_previous_area);
                Invalidate(_next_area);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hovered_arrow = 0;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (_films.Length > 1 && _previous_area.Contains(e.Location))
            {
                ShowFilm(_index - 1);
            }
            else if (_films.Length > 1 && _next_area.Contains(e.Location))
            {
                ShowFilm(_index + 1);
            }
            else if (_poster_area.Contains(e.Location))
            {
                OpenCurrentFilm();
            }
        }

        // left and right arrows change the film when the banner has the keyboard focus
        protected override bool IsInputKey(Keys keyData)
        {
            return keyData == Keys.Left || keyData == Keys.Right || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Left)
            {
                ShowFilm(_index - 1);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Right)
            {
                ShowFilm(_index + 1);
                e.Handled = true;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
                DropBackdrops();
            }

            base.Dispose(disposing);
        }
    }
}
