using System;

namespace LightHouse.Features.Computer.LEO.Mails
{
    /// <summary>
    /// Style générique pour le rendu TMP (couleurs, tailles, interlignage, séparateur).
    /// Extrait de MailGenerator pour être réutilisable indépendamment (MailText, MailBuilder, éditeur...).
    ///
    /// ⚠️ BREAKING CHANGE mineur : si du code existant référence explicitement
    /// `MailGenerator.MailStyle` (plutôt que `MailStyle` tout court), il faudra
    /// mettre à jour ces références. Tout code DANS le namespace
    /// LightHouse.Features.Computer.LEO.Mails qui utilisait `MailStyle` sans
    /// préfixe continue de fonctionner sans changement.
    /// </summary>
    [Serializable]
    public struct MailStyle
    {
        public string Primary;   // titres
        public string Accent;    // sous-titres / infos
        public string Positive;  // ok/gains
        public string Negative;  // erreurs/alertes
        public string Body;      // texte principal

        public int TitlePct;     // % taille titre
        public int BodyPct;      // % taille corps
        public float LineHeight; // % line-height

        public string Divider;   // "—" ou "• • •", etc.

        public static MailStyle Default => new MailStyle
        {
            Primary = "#1E88E5",
            Accent = "#8EACBB",
            Positive = "#00C853",
            Negative = "#FF5252",
            Body = "#E0E0E0",
            TitlePct = 150,
            BodyPct = 100,
            LineHeight = 110f,
            Divider = "────────────────"
        };
    }
}