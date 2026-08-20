using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LightHouse.Features.Computer.LEO.Mails
{
    /// <summary>
    /// Helpers de formatage purs (pas de rich text ici, juste des conversions).
    /// </summary>
    public static class MailFormat
    {
        public static string Money(float amount, string currencySymbol = "$")
            => string.Format(CultureInfo.InvariantCulture, "{0}{1:N0}", currencySymbol, amount);
    }

    /// <summary>
    /// Builder fluent pour composer du texte riche TMP (gras, italique, couleurs, tailles,
    /// puces, sections conditionnelles, boucles...) sans manipuler de string interpolée à la main.
    ///
    /// Objectif : rendre l'écriture d'un nouveau template de mail rapide et lisible, tout en
    /// gardant l'échappement HTML (&lt; &gt; &amp;) systématique et centralisé.
    ///
    /// Usage typique :
    /// <code>
    /// var text = MailText.Create(style)
    ///     .Bold("Dear Keeper,").NewLine(2)
    ///     .Line("Summary below:")
    ///     .Bullet(b => b.Raw("Errors: ").Negative("3", bold: true))
    ///     .If(hasBonus, b => b.Positive("Bonus unlocked!", bold: true))
    ///     .ToString();
    /// </code>
    /// </summary>
    public sealed class MailText
    {
        private readonly StringBuilder _sb = new StringBuilder();
        private readonly MailStyle _style;

        public MailStyle Style => _style;

        private MailText(MailStyle style) => _style = style;

        public static MailText Create(MailStyle? style = null) => new MailText(style ?? MailStyle.Default);

        #region ---------- Echappement ----------
        public static string Escape(string s) => s?
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
        #endregion

        #region ---------- Primitives ----------
        /// <summary>Insère du TMP brut, SANS échappement. À utiliser uniquement pour des tags TMP volontaires.</summary>
        public MailText Raw(string raw)
        {
            _sb.Append(raw);
            return this;
        }

        /// <summary>Insère du texte échappé, sans retour à la ligne.</summary>
        public MailText Text(string text)
        {
            _sb.Append(Escape(text));
            return this;
        }

        /// <summary>Insère du texte échappé suivi d'un &lt;br&gt;.</summary>
        public MailText Line(string text = "")
        {
            if (!string.IsNullOrEmpty(text)) _sb.Append(Escape(text));
            _sb.Append("<br>");
            return this;
        }

        public MailText NewLine(int count = 1)
        {
            for (int i = 0; i < count; i++) _sb.Append("<br>");
            return this;
        }
        #endregion

        #region ---------- Emphase ----------
        public MailText Bold(string text)
        {
            _sb.Append("<b>").Append(Escape(text)).Append("</b>");
            return this;
        }

        public MailText Italic(string text)
        {
            _sb.Append("<i>").Append(Escape(text)).Append("</i>");
            return this;
        }

        public MailText BoldItalic(string text)
        {
            _sb.Append("<b><i>").Append(Escape(text)).Append("</i></b>");
            return this;
        }

        public MailText Underline(string text)
        {
            _sb.Append("<u>").Append(Escape(text)).Append("</u>");
            return this;
        }

        public MailText Strike(string text)
        {
            _sb.Append("<s>").Append(Escape(text)).Append("</s>");
            return this;
        }
        #endregion

        #region ---------- Couleurs ----------
        public MailText Color(string text, string hexColor, bool bold = false)
        {
            if (bold) _sb.Append("<b>");
            _sb.Append($"<color={hexColor}>").Append(Escape(text)).Append("</color>");
            if (bold) _sb.Append("</b>");
            return this;
        }

        public MailText Primary(string text, bool bold = false) => Color(text, _style.Primary, bold);
        public MailText Accent(string text, bool bold = false) => Color(text, _style.Accent, bold);
        public MailText Positive(string text, bool bold = false) => Color(text, _style.Positive, bold);
        public MailText Negative(string text, bool bold = false) => Color(text, _style.Negative, bold);
        public MailText BodyColor(string text, bool bold = false) => Color(text, _style.Body, bold);
        #endregion

        #region ---------- Taille / alignement ----------
        public MailText Size(string text, int pct)
        {
            _sb.Append($"<size={pct}%>").Append(Escape(text)).Append("</size>");
            return this;
        }

        /// <summary>align: "left", "right" ou "center".</summary>
        public MailText Align(string text, string align)
        {
            _sb.Append($"<align={align}>").Append(Escape(text)).Append("</align>");
            return this;
        }

        /// <summary>Titre de section : taille + gras + couleur primaire, suivi d'un &lt;br&gt;.</summary>
        public MailText SectionTitle(string text, int pct = 115) => SectionTitle(text, _style.Primary, pct);

        /// <summary>Titre de section avec couleur explicite (ex: Positive/Negative pour une note conditionnelle).</summary>
        public MailText SectionTitle(string text, string colorHex, int pct = 115)
        {
            _sb.Append($"<size={pct}%><b><color={colorHex}>")
               .Append(Escape(text))
               .Append("</color></b></size><br>");
            return this;
        }
        #endregion

        #region ---------- Puces ----------
        /// <summary>Puce simple avec texte échappé.</summary>
        public MailText Bullet(string text)
        {
            _sb.Append("• ").Append(Escape(text)).Append("<br>");
            return this;
        }

        /// <summary>Puce dont le contenu est composé librement (mix gras/couleur/etc.).</summary>
        public MailText Bullet(Action<MailText> content)
        {
            _sb.Append("• ");
            content(this);
            _sb.Append("<br>");
            return this;
        }
        #endregion

        #region ---------- Structure ----------
        public MailText Divider()
        {
            _sb.Append($"<br><alpha=#55>{_style.Divider}</alpha><br>");
            return this;
        }

        public MailText Header(string subjectLeft, string fromRight)
        {
            _sb.Append(
$@"<line-height={_style.LineHeight}%><size={_style.BodyPct}%><color={_style.Body}>
<align=left><size={_style.TitlePct}%><b><color={_style.Primary}>{Escape(subjectLeft)}</color></b></size></align>
<align=right><i><color={_style.Accent}>{Escape(fromRight)}</color></i></align>
<align=left>");
            return this;
        }

        public MailText Footer(string signature)
        {
            _sb.Append($"<br><br>Respectfully,<br>{Escape(signature)}</color>");
            return this;
        }
        #endregion

        #region ---------- Pourcentage coloré ----------
        /// <summary>
        /// 0..goodThreshold-1 = Negative, goodThreshold..okThreshold-1 = Accent, au-dessus = Positive.
        /// (mêmes seuils que l'ancien Rt.PercentColored : 60% / 80%)
        /// </summary>
        public MailText Percent(float pct, float positiveThreshold = 80f, float accentThreshold = 60f)
        {
            string col = pct >= positiveThreshold ? _style.Positive : (pct >= accentThreshold ? _style.Accent : _style.Negative);
            _sb.Append($"<b><color={col}>{pct:0}%</color></b>");
            return this;
        }
        #endregion

        #region ---------- Composition conditionnelle / boucles ----------
        /// <summary>Branche la composition selon une condition, sans casser la chaîne fluente.</summary>
        public MailText If(bool condition, Action<MailText> then, Action<MailText> otherwise = null)
        {
            if (condition) then?.Invoke(this);
            else otherwise?.Invoke(this);
            return this;
        }

        /// <summary>Répète `body` pour chaque élément. Si la liste est vide/nulle, écrit `emptyText` (si fourni).</summary>
        public MailText Each<T>(IEnumerable<T> items, Action<MailText, T> body, string emptyText = null)
        {
            bool any = false;
            if (items != null)
            {
                foreach (var item in items)
                {
                    body(this, item);
                    any = true;
                }
            }
            if (!any && !string.IsNullOrEmpty(emptyText)) Italic(emptyText).Raw("<br>");
            return this;
        }
        #endregion

        public override string ToString() => _sb.ToString();
    }

    /// <summary>
    /// Assemble un MailDatas complet (expéditeur, sujet, arrivée, fichiers, header/body/footer)
    /// sans répéter le boilerplate présent dans chaque template de MailGenerator.
    ///
    /// Usage typique :
    /// <code>
    /// return MailBuilder.From("Coastal Weather Station", style)
    ///     .Subject("Weather Report – " + date)
    ///     .Arrival(day, time)
    ///     .Body(t => t.Bold("Dear " + keeper + ",").NewLine(2). ... )
    ///     .Build();
    /// </code>
    /// </summary>
    public sealed class MailBuilder
    {
        private readonly MailStyle _style;
        private readonly string _expeditorLabel;
        private string _subject = "";
        private byte _arrivalDay;
        private float _arrivalTime = 9f;
        private MailAttachedFile[] _files;
        private Action<MailText> _bodyBuilder;
        private bool _includeHeader = true;
        private bool _includeFooter = true;
        private string _signature;

        private MailBuilder(string expeditorLabel, MailStyle style)
        {
            _expeditorLabel = expeditorLabel;
            _signature = expeditorLabel;
            _style = style;
        }

        public static MailBuilder From(string expeditorLabel, MailStyle? style = null)
            => new MailBuilder(expeditorLabel, style ?? MailStyle.Default);

        public MailBuilder Subject(string subject) { _subject = subject; return this; }
        public MailBuilder Arrival(byte day, float time) { _arrivalDay = day; _arrivalTime = time; return this; }
        public MailBuilder Attach(MailAttachedFile[] files) { _files = files; return this; }
        public MailBuilder Signature(string signature) { _signature = signature; return this; }

        /// <summary>Désactive le header auto-généré (titre + expéditeur), pour un mail 100% custom.</summary>
        public MailBuilder NoHeader() { _includeHeader = false; return this; }

        /// <summary>Désactive le footer auto-généré ("Respectfully, ...").</summary>
        public MailBuilder NoFooter() { _includeFooter = false; return this; }

        /// <summary>Corps du mail : reçoit un MailText déjà initialisé avec le style courant.</summary>
        public MailBuilder Body(Action<MailText> build) { _bodyBuilder = build; return this; }

        public MailDatas Build()
        {
            var t = MailText.Create(_style);

            if (_includeHeader) t.Header(_subject, $"From : {_expeditorLabel}");
            _bodyBuilder?.Invoke(t);
            if (_includeFooter) t.Footer(_signature);

            return new MailDatas
            {
                ExpeditorName = _expeditorLabel,
                MailObject = _subject,
                ArrivalDay = _arrivalDay,
                ArrivalTime = _arrivalTime,
                MailMessage = t.ToString(),
                Files = _files
            };
        }
    }
}